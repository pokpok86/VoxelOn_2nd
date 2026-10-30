using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed partial class VoxArena
{
    IEnumerator PerformanceTest()
    {
        // Real rendered frames, not the accelerated simulation used by SmokeTest.
        // HP is restored to sustain load; audio and preference writes remain suppressed.
        int[] pilots={3,4,6},rivals={3,4,7};
        bool valid=true;
        Debug.Log($"PERF_ENV gpu={SystemInfo.graphicsDeviceName} cpu={SystemInfo.processorType} resolution={Screen.width}x{Screen.height} targetFPS={Application.targetFrameRate} vsync={QualitySettings.vSyncCount} audio=off");
        for(int phase=0;phase<pilots.Length;phase++)
        {
            ApplyCharacter(player,pilots[phase]); ApplyCharacter(enemy,rivals[phase]); ResetRound(); inMenu=false;
            var frameTimes=new List<float>(); int peakShots=0,peakSparks=0;
            double start=Time.realtimeSinceStartupAsDouble,previous=start;
            while(Time.realtimeSinceStartupAsDouble-start<9)
            {
                yield return null;
                double now=Time.realtimeSinceStartupAsDouble;
                float elapsed=(float)(now-start),frame=(float)(now-previous); previous=now;
                if(elapsed>1) frameTimes.Add(frame*1000);
                float dt=Mathf.Clamp(frame,.001f,.05f); roundTime+=dt;
                UpdateObstacles(dt);
                Vector3 toward=(enemy.root.position-player.root.position).normalized;
                float distance=Vector3.Distance(player.root.position,enemy.root.position);
                Vector3 movement=distance>player.spec.range*.45f?toward:Vector3.Cross(toward,Vector3.up);
                player.boostHeld=elapsed%3<1;
                Move(player,movement,dt); FaceOpponent(player,enemy);
                aimDirection=AssistAim(player,enemy,toward); assistEngaged=CanAssist(player,enemy,toward); canShoot=InArc(player,aimDirection);
                Attack(player,enemy,aimDirection); Special(player,enemy,aimDirection);
                EnemyAI(dt); UpdateCombat(dt); UpdateSparks(dt); Animate(player); Animate(enemy);
                peakShots=Mathf.Max(peakShots,shots.Count); peakSparks=Mathf.Max(peakSparks,sparks.Count);
                player.hp=player.spec.health; enemy.hp=enemy.spec.health;
                hitConfirmTime=Mathf.Max(0,hitConfirmTime-dt);
            }
            frameTimes.Sort(); float sum=0; foreach(float ms in frameTimes) sum+=ms;
            valid&=frameTimes.Count>120;
            if(frameTimes.Count>0)
            {
                float average=sum/frameTimes.Count;
                float p95=frameTimes[Mathf.Min(frameTimes.Count-1,(int)(frameTimes.Count*.95f))];
                float p99=frameTimes[Mathf.Min(frameTimes.Count-1,(int)(frameTimes.Count*.99f))];
                Debug.Log($"PERF_SAMPLE {player.spec.name}/{enemy.spec.name} frames={frameTimes.Count} avgMs={average:0.00} fps={1000/average:0.0} p95Ms={p95:0.00} p99Ms={p99:0.00} worstMs={frameTimes[frameTimes.Count-1]:0.00} peakShots={peakShots} peakSparks={peakSparks}");
            }
            yield return new WaitForEndOfFrame();
            if(phase==1) SavePreview("PerformancePreview.png");
        }
        ReturnToHangar();
        Debug.Log(valid?"VOXSKY_PERF_COMPLETE":"VOXSKY_PERF_INSUFFICIENT_FRAMES");
        Application.Quit(valid?0:2);
    }
}
