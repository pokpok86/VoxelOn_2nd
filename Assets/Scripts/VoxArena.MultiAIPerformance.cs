using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Profiling;

public sealed partial class VoxArena
{
    static float Percentile(List<float> sorted,float q)
    {
        if(sorted.Count==0)return 0;
        return sorted[Mathf.Clamp(Mathf.CeilToInt((sorted.Count-1)*q),0,sorted.Count-1)];
    }

    IEnumerator MultiAIPerformanceTest()
    {
        const float stageSeconds=6f,warmupSeconds=1f;
        int[] populations={2,4,8,16,32};
        var additional=new List<Fighter>();
        bool valid=true;
        int originalDifficulty=difficulty;
        difficulty=1;smoke=true;spectatorMode=false;paused=false;inMenu=false;
        Screen.SetResolution(1600,900,FullScreenMode.Windowed);
        Debug.Log($"MULTIAI_ENV cpu={SystemInfo.processorType} cpuCores={SystemInfo.processorCount} gpu={SystemInfo.graphicsDeviceName} gpuMemoryMB={SystemInfo.graphicsMemorySize} ramMB={SystemInfo.systemMemorySize} resolution={Screen.width}x{Screen.height} targetFPS={Application.targetFrameRate} vsync={QualitySettings.vSyncCount} map={arenaMap} physics=custom-movement-and-AABB-collision skills=not-implemented gpuFrameTime=not-exposed drawCalls=not-exposed");

        for(int stage=0;stage<populations.Length;stage++)
        {
            foreach(Fighter old in additional)
            {
                RemoveMatchParticipant(old);
            }
            additional.Clear();
            ApplySelectableCharacter(player,stage%CraftCatalog.Selectable.Length);
            ApplySelectableCharacter(enemy,(stage+1)%CraftCatalog.Selectable.Length);
            ResetRound();
            inMenu=false;paused=false;spectatorMode=false;
            int count=populations[stage];
            for(int i=2;i<count;i++)
            {
                Fighter f=AddMatchParticipant($"Load AI {i+1:00}",i%CraftCatalog.Selectable.Length,i&1);
                additional.Add(f);
            }

            var actors=new List<Fighter>(fighterRoster);
            if(actors.Count!=count){valid=false;Debug.LogError($"MULTIAI_ROSTER_FAIL expected={count} actual={actors.Count}");}
            bool targetsValid=true;
            for(int i=0;i<actors.Count;i++)
            {
                float angle=(i/(float)count)*Mathf.PI*2f;
                actors[i].teamId=i&1;
                actors[i].root.position=new Vector3(Mathf.Cos(angle)*20f,FighterGroundY,Mathf.Sin(angle)*20f);
                Fighter target=Opponent(actors[i]);
                if(target==null||target.teamId==actors[i].teamId)targetsValid=false;
                else FaceOpponent(actors[i],target);
            }
            if(!targetsValid){valid=false;Debug.LogError($"MULTIAI_TARGET_FAIL actors={count}");}
            Debug.Log($"MULTIAI_STAGE_START actors={count} teams=2 allAI=true roster={fighterRoster.Count} seconds={stageSeconds} warmup={warmupSeconds} targetPairs=nearest-hostile");

            var frameMs=new List<float>();var aiMs=new List<float>();var collisionMs=new List<float>();var combatMs=new List<float>();
            int peakShots=0,peakSparks=0;double start=Time.realtimeSinceStartupAsDouble,previous=start;
            while(Time.realtimeSinceStartupAsDouble-start<stageSeconds)
            {
                yield return null;
                double now=Time.realtimeSinceStartupAsDouble;float elapsed=(float)(now-start),frame=(float)(now-previous);previous=now;
                float dt=Mathf.Clamp(frame,.001f,.05f);roundTime+=dt;
                long tick=System.Diagnostics.Stopwatch.GetTimestamp();
                foreach(Fighter f in actors)
                {
                    Fighter target=Opponent(f);
                    if(target==null){valid=false;continue;}
                    EnemyAI(f,target,dt);
                }
                float aiElapsed=(float)((System.Diagnostics.Stopwatch.GetTimestamp()-tick)*1000.0/System.Diagnostics.Stopwatch.Frequency);
                tick=System.Diagnostics.Stopwatch.GetTimestamp();UpdateObstacles(dt);
                float collisionElapsed=(float)((System.Diagnostics.Stopwatch.GetTimestamp()-tick)*1000.0/System.Diagnostics.Stopwatch.Frequency);
                foreach(Fighter f in actors)f.hp=f.spec.health;
                tick=System.Diagnostics.Stopwatch.GetTimestamp();UpdateCombat(dt);
                float combatElapsed=(float)((System.Diagnostics.Stopwatch.GetTimestamp()-tick)*1000.0/System.Diagnostics.Stopwatch.Frequency);
                UpdateSparks(dt);foreach(Fighter f in actors){Animate(f);f.hp=f.spec.health;}
                peakShots=Mathf.Max(peakShots,shots.Count);peakSparks=Mathf.Max(peakSparks,sparks.Count);
                if(elapsed>=warmupSeconds){frameMs.Add(frame*1000f);aiMs.Add(aiElapsed);collisionMs.Add(collisionElapsed);combatMs.Add(combatElapsed);}
            }
            frameMs.Sort();aiMs.Sort();collisionMs.Sort();combatMs.Sort();
            if(frameMs.Count<60){valid=false;Debug.LogError($"MULTIAI_SAMPLE_INSUFFICIENT actors={count} frames={frameMs.Count}");}
            float average=0;foreach(float v in frameMs)average+=v;if(frameMs.Count>0)average/=frameMs.Count;
            float p95=Percentile(frameMs,.95f),p99=Percentile(frameMs,.99f);
            long mono=Profiler.GetMonoUsedSizeLong()/1048576,allocated=Profiler.GetTotalAllocatedMemoryLong()/1048576;
            Debug.Log($"MULTIAI_SAMPLE actors={count} frames={frameMs.Count} avgFrameMs={average:0.00} averageFPS={(average>0?1000/average:0):0.0} p95FrameMs={p95:0.00} p99FrameMs={p99:0.00} worstFrameMs={Percentile(frameMs,1):0.00} aiAvgMs={Mean(aiMs):0.00} aiP95Ms={Percentile(aiMs,.95f):0.00} collisionAvgMs={Mean(collisionMs):0.00} collisionP95Ms={Percentile(collisionMs,.95f):0.00} combatAvgMs={Mean(combatMs):0.00} combatP95Ms={Percentile(combatMs,.95f):0.00} monoUsedMB={mono} totalAllocatedMB={allocated} obstacles={obstacles.Count} peakShots={peakShots} peakSparks={peakSparks} targetPairs=nearest-hostile gpuFrameTime=not-exposed drawCalls=not-exposed rigidbodyPhysics=not-used");
            if(count==8||count==32)
            {
                string captureFolder=Path.Combine("Docs","Screenshots","SystemDesign-20261106","MultiAI",arenaMap.ToString());
                Directory.CreateDirectory(captureFolder);
                yield return new WaitForEndOfFrame();
                Texture2D image=ScreenCapture.CaptureScreenshotAsTexture();
                string path=Path.Combine(Directory.GetCurrentDirectory(),captureFolder,$"AI-{count:00}.png");
                File.WriteAllBytes(path,image.EncodeToPNG());Destroy(image);Debug.Log($"MULTIAI_SCREENSHOT {path}");
            }
        }
        foreach(Fighter f in additional)
        {
            RemoveMatchParticipant(f);
        }
        additional.Clear();difficulty=originalDifficulty;
        Debug.Log(valid?"VOXSKY_MULTIAI_MEASUREMENTS_COMPLETE stages=5 counts=2,4,8,16,32; performance thresholds are informational":"VOXSKY_MULTIAI_MEASUREMENTS_INCOMPLETE");
        Application.Quit(valid?0:2);
    }

    static float Mean(List<float> values)
    {
        if(values.Count==0)return 0;float sum=0;foreach(float value in values)sum+=value;return sum/values.Count;
    }
}
