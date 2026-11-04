using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed partial class VoxArena
{
    // Real-time rendered integration exercise. Scripted aim and restored HP are
    // deliberate load controls, not a measurement of player accuracy or balance.
    IEnumerator WispSoak()
    {
        yield return new WaitForEndOfFrame();
        UnityEngine.Random.InitState(41104+(int)arenaMap);TestSetupSelectable(0,0);ResetRound();inMenu=false;smoke=false;muted=true;difficulty=0;
        spectatorMode=false;targetLock=false;view.orthographic=false;view.fieldOfView=60;
        player.root.position=new Vector3(60,FighterGroundY,0);enemy.root.position=new Vector3(60,FighterGroundY,25);
        if(System.Array.IndexOf(System.Environment.GetCommandLineArgs(),"--rain-soak")>=0){forcedWeather=2;forcedDay=1;SelectRoundClimate();}
        UpdateCamera(10); // Match the normal match-start camera snap after test spawn placement.
        var frames=new List<float>();double begin=Time.realtimeSinceStartupAsDouble,last=begin;
        int fired=0,partialReloads=0,emptyReloads=0,boostFrames=0,jumpStarts=0,peakShots=0,nextCapture=0;
        float playerTravel=0,enemyTravel=0,peakJump=0,lostSeconds=0,maxLostSeconds=0;bool mousePitchRange=true;bool capturedHeadClear=true;bool ammoOk=true,energyOk=true,finite=true,cameraOk=true,flamesOk=true;
        float[] jumpAt={4,17,30,40};float[] captureAt={12,28,42};
        string map=arenaMap==ArenaMapKind.OutdoorCanyon?"Canyon":"Foundry";
        Debug.Log("WISP_SOAK_START "+map+" seconds=45 scriptedAim=true hpRestored=true audio=muted resolution="+Screen.width+"x"+Screen.height+" vsync="+QualitySettings.vSyncCount+" targetFPS="+Application.targetFrameRate);
        while(Time.realtimeSinceStartupAsDouble-begin<45)
        {
            yield return null;
            double now=Time.realtimeSinceStartupAsDouble;float elapsed=(float)(now-begin),frame=(float)(now-last);last=now;
            if(elapsed>2)frames.Add(frame*1000);float dt=Mathf.Clamp(frame,.001f,.05f);roundTime+=dt;
            player.hp=player.spec.health;enemy.hp=enemy.spec.health;
            Vector3 p0=player.root.position,e0=enemy.root.position;
            Vector3 toward=Vector3.ProjectOnPlane(enemy.root.position-player.root.position,Vector3.up).normalized;
            bool orbit=elapsed>=12&&elapsed<24||elapsed>=36;
            cameraYaw=orbit?(elapsed-12)*30:Mathf.Atan2(toward.x,toward.z)*Mathf.Rad2Deg;
            cameraPitch=DefaultThirdPersonPitch+Mathf.Sin(elapsed*.3f)*3;
            Vector3 forward=Quaternion.Euler(0,cameraYaw,0)*Vector3.forward,right=Vector3.Cross(Vector3.up,forward);
            player.facing=forward;
            Vector3 input=CameraRelativeMove(forward,right,Mathf.Sin(elapsed*.6f)>.2f?1:0,elapsed%9<6?1:-1,true);
            player.boostHeld=elapsed%5<2;player.guarding=elapsed%11>9;
            player.jumpHeld=jumpStarts>0&&elapsed-jumpAt[jumpStarts-1]<.6f;
            if(jumpStarts<jumpAt.Length&&elapsed>=jumpAt[jumpStarts]&&player.jumpHeight<=.02f)
            {
                player.jumpOriginY=player.root.position.y;player.jumpVelocity=jumpSpeed*player.spec.jumpImpulseScale/Mathf.Sqrt(player.spec.jumpWeightFactor);
                jumpStarts++;player.jumpHeld=true;
            }
            float beforeReload=player.reloadTime;
            Move(player,input,dt);int beforeAmmo=player.magazine;
            bool firing=elapsed<12||elapsed>=24&&elapsed<28||elapsed>=33&&elapsed<36;
            aimDirection=toward;canShoot=InArc(player,aimDirection);
            if(firing)Attack(player,enemy,aimDirection);
            fired+=Mathf.Max(0,beforeAmmo-player.magazine);
            if(beforeReload<=0&&player.reloadTime>0){if(player.magazine==0)emptyReloads++;else partialReloads++;}
            EnemyAI(enemy,player,dt);FaceOpponent(enemy,player,dt);UpdateObstacles(dt);UpdateCombat(dt);UpdateSalvage(dt);UpdateSparks(dt);Animate(player);Animate(enemy);
            player.hp=player.spec.health;enemy.hp=enemy.spec.health;
            playerTravel+=Vector3.Distance(p0,player.root.position);enemyTravel+=Vector3.Distance(e0,enemy.root.position);
            peakJump=Mathf.Max(peakJump,player.jumpHeight);peakShots=Mathf.Max(peakShots,shots.Count);if(player.boosting)boostFrames++;
            ammoOk&=player.magazine>=0&&player.magazine<=30;
            energyOk&=player.boostEnergy>=0&&player.boostEnergy<=player.spec.boostCapacity+.01f;
            finite&=Finite(player.root.position)&&Finite(enemy.root.position);
            flamesOk&=player.jumpHeld&&player.jumpHeight>.08f||!player.thruster.gameObject.activeSelf;
            yield return new WaitForEndOfFrame(); // LateUpdate has applied the real shoulder camera.
            finite&=Finite(view.transform.position);
            float actualPitch=-Mathf.Asin(Mathf.Clamp(view.transform.forward.y,-1,1))*Mathf.Rad2Deg;
            mousePitchRange&=actualPitch>=-5.1f&&actualPitch<=45.1f;
            Vector3 torso=view.WorldToViewportPoint(player.root.position+Vector3.up*.2f);
            bool framed=torso.z>0&&torso.x>.04f&&torso.x<.96f&&torso.y>.16f&&torso.y<.85f;
            lostSeconds=framed?0:lostSeconds+frame;maxLostSeconds=Mathf.Max(maxLostSeconds,lostSeconds);
            foreach(Bounds b in cameraSolids)cameraOk&=!b.Contains(view.transform.position);
            foreach(Bounds b in obstacles)cameraOk&=!b.Contains(view.transform.position);
            if(nextCapture<captureAt.Length&&elapsed>=captureAt[nextCapture])
            {
                bool headClearNow=!VisibleWispHeadBlocksReticle();capturedHeadClear&=headClearNow;
                Debug.Log("SOAK_HEAD_RETICLE "+map+" t="+captureAt[nextCapture]+" clear="+headClearNow);
                if(!SaveGameplayUIFrame("WISP-soak-"+captureAt[nextCapture].ToString("00")+"s.png"))yield break;
                nextCapture++;
            }
        }
        bool passed=true;int checks=0;
        void Check(bool ok,string name){checks++;passed&=ok;Debug.Log("SOAK_CHECK "+name+" "+(ok?"PASS":"FAIL"));}
        Check(frames.Count>300,"real-time rendered frame sample");Check(finite,"robot and camera coordinates finite");
        Check(ammoOk&&fired>35,"real gun fire preserves magazine bounds");
        Check(emptyReloads>0,"automatic empty reload exercised");Check(partialReloads>0,"automatic partial idle reload exercised");
        Check(energyOk&&boostFrames>50,"held boost drains and restores bounded energy");
        Check(jumpStarts==4&&peakJump>3.1f,"four real-time jumps reach required height");
        Check(flamesOk,"exhaust stays absent without airborne held Space");
        Check(cameraOk,"real camera remains outside solid geometry throughout exercise");
        Check(playerTravel>40&&enemyTravel>10,"player and Practice AI continue moving");
        Check(!targetLock&&!CanStartLockOn(player),"WISP remains manual aim without target lock");
        Check(maxLostSeconds<.25f,"robot torso remains framed above HUD during close-wall corrections");
        Check(mousePitchRange,"camera stays within mouse pitch range instead of forced ground view");
        Check(capturedHeadClear,"player head does not visibly obscure the reticle in captured gameplay frames");
        Check(nextCapture==3,"three nonblank actual gameplay frames captured");
        frames.Sort();float sum=0;foreach(float ms in frames)sum+=ms;
        if(frames.Count>0)Debug.Log("WISP_SOAK_METRICS "+map+" frames="+frames.Count+" avgMs="+(sum/frames.Count).ToString("F2")+
            " p95Ms="+frames[(int)(frames.Count*.95f)].ToString("F2")+" fired="+fired+" emptyReloads="+emptyReloads+" partialReloads="+partialReloads+
            " playerTravel="+playerTravel.ToString("F1")+" enemyTravel="+enemyTravel.ToString("F1")+" peakTerrainClearance="+peakJump.ToString("F2")+" peakShots="+peakShots+" maxTorsoLostSeconds="+maxLostSeconds.ToString("F3"));
        Debug.Log(passed?"VOXELON_SOAK_PASS "+map+" "+checks+" checks":"VOXELON_SOAK_FAIL "+map);
        Application.Quit(passed?0:2);
    }
    static bool Finite(Vector3 v)=>!float.IsNaN(v.x)&&!float.IsNaN(v.y)&&!float.IsNaN(v.z)&&!float.IsInfinity(v.x)&&!float.IsInfinity(v.y)&&!float.IsInfinity(v.z);
}
