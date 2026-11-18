using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public sealed partial class VoxArena
{
    bool profileDroneSoak;float profileDroneMs,profileAuxMs,profileShotMs,profileFxMs,profileCoverMs;
    static float ProfileElapsed(long start)=>(float)((System.Diagnostics.Stopwatch.GetTimestamp()-start)*1000.0/System.Diagnostics.Stopwatch.Frequency);
    bool DroneSoakRequested=>Array.IndexOf(Environment.GetCommandLineArgs(),"--drone-soak")>=0;
    IEnumerator DroneEquipmentSoak()
    {
        balanceDirectoryOverride=Path.Combine(Path.GetTempPath(),"VoxelOn-Drone-Soak-"+DateTime.Now.ToString("yyyyMMdd-HHmmss"));
        profileDroneSoak=true;activeBalance=BalanceData.Defaults();familyLoadouts=new FamilyLoadoutFile();bool valid=true;int checks=0;muted=true;
        void Check(bool condition,string detail){checks++;valid&=condition;Debug.Log("DRONE_SOAK_CHECK "+(condition?"PASS ":"FAIL ")+detail);}
        CollisionBroadphaseChecks(Check);
        var added=new List<Fighter>();var report=new List<string>{"actors,simulatedSeconds,cpuTickMeanMs,cpuTickP95Ms,peakShots,peakAuthoredEffects,deployments,weaponMaterials"};
        foreach(int count in new[]{8,16})
        {
            TestSetupSelectable(0,1);smoke=false;inMenu=false;paused=true;timedFragMode=false;
            for(int i=2;i<count;i++)added.Add(AddMatchParticipant("Equipment soak "+i,i%3,i%2));
            var actors=fighterRoster.ToArray();Check(actors.Length==count,"expected real roster "+count);
            for(int i=0;i<actors.Length;i++)
            {
                Fighter f=actors[i];FamilyLoadout config=FamilyConfig(f);config.backpack=i%4==3?"B-02_CROSSLANCE":DronePackIds[i%4];config.auxiliary=new[]{"AUX_MISSILE_POD","AUX_PULSE_GUN"};BuildRobot(f);
                // This diagnostic deliberately mixes gear on actors sharing an AI
                // template. Freeze each installed instance's configuration.
                if(familyRigs.TryGetValue(f,out FamilyRig rig))rig.config=JsonUtility.FromJson<FamilyLoadout>(JsonUtility.ToJson(rig.config));
                float angle=i*2*Mathf.PI/count;f.root.position=new Vector3(Mathf.Cos(angle)*25,SurfaceHeight(new Vector3(Mathf.Cos(angle)*25,0,Mathf.Sin(angle)*25))+FighterSupportOffset,Mathf.Sin(angle)*25);f.teamId=i%2;
            }
            int peakShots=0,peakEffects=0,deployments=0;var ticks=new List<float>();var aiTicks=new List<float>();var coverTicks=new List<float>();var animationTicks=new List<float>();var droneTicks=new List<float>();var auxTicks=new List<float>();var shotTicks=new List<float>();var effectTicks=new List<float>();bool finite=true;int createdBefore=projectilesCreated;
            for(int step=0;step<600;step++)
            {
                const float dt=.05f;profileCoverMs=0;long tick=System.Diagnostics.Stopwatch.GetTimestamp();
                foreach(Fighter f in actors)
                {
                    f.hp=f.spec.health;f.invulnerableTime=1;Fighter target=Opponent(f);EnemyAI(f,target,dt);
                    if(droneRuntime.TryGetValue(f,out DroneRuntime drone)&&drone.phase==DronePhase.Docked&&DeployDrone(f))deployments++;
                    if(auxiliaryRuntime.TryGetValue(f,out AuxiliaryRuntime auxiliary))
                    {SelectAuxiliary(f,(step/40)%2);FireAuxiliary(f,target,(target.root.position-WeaponMuzzle(f)).normalized);}
                }
                aiTicks.Add(ProfileElapsed(tick));coverTicks.Add(profileCoverMs);UpdateCombat(dt);UpdateSparks(dt);long animationStart=System.Diagnostics.Stopwatch.GetTimestamp();
                foreach(Fighter f in actors){Animate(f);finite&=Finite(f.root.position);}
                foreach(DroneRuntime drone in droneRuntime.Values)if(drone.visual!=null)finite&=Finite(drone.visual.position);
                animationTicks.Add(ProfileElapsed(animationStart));droneTicks.Add(profileDroneMs);auxTicks.Add(profileAuxMs);shotTicks.Add(profileShotMs);effectTicks.Add(profileFxMs);
                ticks.Add((float)((System.Diagnostics.Stopwatch.GetTimestamp()-tick)*1000.0/System.Diagnostics.Stopwatch.Frequency));
                peakShots=Mathf.Max(peakShots,shots.Count);peakEffects=Mathf.Max(peakEffects,authoredCombatEffects.Count);
                if(step%2==0)yield return null;
            }
            foreach(var samples in new[]{aiTicks,coverTicks,animationTicks,droneTicks,auxTicks,shotTicks,effectTicks})samples.Sort();
            Debug.Log("DRONE_SOAK_BREAKDOWN actors="+count+" AI+equipmentMeanMs="+Mean(aiTicks)+" AI+equipmentP95Ms="+Percentile(aiTicks,.95f)+" coverMeanMs="+Mean(coverTicks)+" coverP95Ms="+Percentile(coverTicks,.95f)+" animationMeanMs="+Mean(animationTicks)+" animationP95Ms="+Percentile(animationTicks,.95f)+" dronesMeanMs="+Mean(droneTicks)+" dronesP95Ms="+Percentile(droneTicks,.95f)+" auxMeanMs="+Mean(auxTicks)+" shotsMeanMs="+Mean(shotTicks)+" effectsMeanMs="+Mean(effectTicks));
            ticks.Sort();Check(finite,"finite actor/drone positions for 30 simulated seconds "+count);
            Check(projectilesCreated>createdBefore&&peakShots>0,"actual hand, auxiliary and drone combat executed "+count);
            Check(deployments>=count/2,"drone deployments executed across mixed equipment "+count);
            Check(peakEffects<=64,"authored transient effect cap survives sustained combat "+count);
            string row=count+",30,"+Mean(ticks).ToString("F3",System.Globalization.CultureInfo.InvariantCulture)+","+Percentile(ticks,.95f).ToString("F3",System.Globalization.CultureInfo.InvariantCulture)+","+peakShots+","+peakEffects+","+deployments+","+weaponArtMaterials.Count;report.Add(row);Debug.Log("DRONE_SOAK_PROFILE "+row);
            ClearCombat();foreach(Fighter f in added)RemoveMatchParticipant(f);added.Clear();yield return null;yield return null;
            Check(shots.Count==0&&authoredCombatEffects.Count==0,"reset removes projectiles and transient authored effects "+count);
            Check(droneRuntime.Keys.All(f=>f==portraitModel||fighterRoster.Contains(f))&&auxiliaryRuntime.Keys.All(f=>f==portraitModel||fighterRoster.Contains(f)),"removed actors leave no equipment controllers "+count);
            Check(droneRuntime.Values.All(d=>d.phase==DronePhase.Docked&&d.visual.parent==d.frame&&(d.fieldVisual==null||!d.fieldVisual.gameObject.activeSelf)),"all remaining drones dock and jammer fields switch off after reset "+count);
        }
        string folder=Path.Combine(Directory.GetCurrentDirectory(),"Docs","Validation","Overnight-20261119","Iteration11");Directory.CreateDirectory(folder);File.WriteAllLines(Path.Combine(folder,"DroneEquipmentSoak-"+arenaMap+".csv"),report);
        Debug.Log(valid?"VOXSKY_DRONE_SOAK_PASS "+checks+" checks map="+arenaMap:"VOXSKY_DRONE_SOAK_FAIL");Application.Quit(valid?0:2);
    }
}
