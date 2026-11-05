using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
public sealed partial class VoxArena
{
    IEnumerator AIStallStudy()
    {
        bool before=Array.IndexOf(Environment.GetCommandLineArgs(),"--before-ai-stall")>=0;string folder=Path.Combine(Directory.GetCurrentDirectory(),"Docs","Screenshots","AI-Stall-20261105",before?"Before":"After");Directory.CreateDirectory(folder);
        bool pass=true;int checks=0;void Check(bool ok,string name){checks++;pass&=ok;Debug.Log("AI_STALL_CHECK "+name+" "+(ok?"PASS":"FAIL"));}
        for(int id=0;id<3;id++)for(int scenario=0;scenario<3;scenario++)
        {
            TestSetupSelectable((id+1)%3,id);smoke=false;inMenu=false;paused=false;muted=true;spectatorMode=true;targetLock=false;difficulty=0;aiMoveEnabled=aiJumpEnabled=aiAttackEnabled=aiBoostEnabled=true;
            int first=obstacles.Count;var art=new List<Transform>();float h=GroundSurfaceY+10;
            if(scenario==0){enemy.root.position=new Vector3(ArenaLimit-enemy.Radius-.035f,FighterGroundY,20);player.root.position=enemy.root.position+Vector3.forward*25;enemy.orbitSign=-1;enemy.orbitTime=1.5f;}
            else if(scenario==1){Wall(new Vector3(75,h,60),new Vector3(38,20,2));enemy.root.position=new Vector3(75,FighterGroundY,59-enemy.Radius-.035f);player.root.position=new Vector3(75,FighterGroundY,76);}
            else {Wall(new Vector3(75,h,59),new Vector3(24,20,2));Wall(new Vector3(62,h,70),new Vector3(2,20,24));Wall(new Vector3(88,h,70),new Vector3(2,20,24));enemy.root.position=new Vector3(75,FighterGroundY,60+enemy.Radius+.035f);player.root.position=new Vector3(75,FighterGroundY,48);}
            enemy.facing=enemy.chassisForward=Vector3.ProjectOnPlane(player.root.position-enemy.root.position,Vector3.up).normalized;ResetCapsuleTreads(enemy);
            if(scenario==0){Vector3 escape=EscapeHeading(enemy,Vector3.right),end=enemy.root.position+escape*2.5f;Check(escape.sqrMagnitude>.1f&&Mathf.Abs(end.x)<=ArenaLimit-enemy.Radius+.001f&&Mathf.Abs(end.z)<=ArenaLimit-enemy.Radius+.001f,"boundary escape stays inside playable arena "+id);}
            Check(!Occupied(enemy.root.position,enemy.Radius*.99f),"valid physical starting clearance "+id+"/"+scenario);
            Vector3 start=enemy.root.position,previous=start;float travel=0,still=0,longest=0,maximum=0;bool finite=true,clear=false;
            // 30 simulation seconds at fixed50Hz, rendered every8ticks. Not a wall-clock/FPS benchmark.
            for(int tick=0;tick<1500;tick++)
            {
                const float dt=.02f;roundTime+=dt;player.hp=player.spec.health;enemy.hp=enemy.spec.health;EnemyAI(enemy,player,dt);UpdateCombat(dt);UpdateObstacles(dt);UpdateSparks(dt);Animate(player);Animate(enemy);
                float step=Vector3.ProjectOnPlane(enemy.root.position-previous,Vector3.up).magnitude;travel+=step;still=step<.001f?still+dt:0;longest=Mathf.Max(longest,still);maximum=Mathf.Max(maximum,Vector3.ProjectOnPlane(enemy.root.position-start,Vector3.up).magnitude);previous=enemy.root.position;finite&=Finite(enemy.root.position);clear|=!ObstacleOnSegment(enemy.root.position,player.root.position,.2f,out _);
                if(tick%8==0)yield return null;
            }
            Debug.Log("AI_STALL_TRACE frame="+id+" scenario="+scenario+" travel="+travel.ToString("F2")+" maxDisplacement="+maximum.ToString("F2")+" longestStill="+longest.ToString("F2")+" clear="+clear+" finalState="+enemy.aiState);
            Check(finite,"finite movement "+id+"/"+scenario);Check(travel>20&&maximum>8,"actual route progress "+id+"/"+scenario);Check(longest<1.8f,"no persistent planar freeze "+id+"/"+scenario);Check(clear,"recovers line of sight "+id+"/"+scenario);Check(!Occupied(enemy.root.position,enemy.Radius*.99f),"no final collision penetration "+id+"/"+scenario);
            yield return new WaitForEndOfFrame();Texture2D image=ScreenCapture.CaptureScreenshotAsTexture();File.WriteAllBytes(Path.Combine(folder,(id+1)+"-case-"+scenario+".png"),image.EncodeToPNG());Destroy(image);
            obstacles.RemoveRange(first,obstacles.Count-first);foreach(Transform t in art)if(t!=null)Destroy(t.gameObject);
            void Wall(Vector3 center,Vector3 size){obstacles.Add(new Bounds(center,size));art.Add(PlaceEnvironment("FoundryCargo",null,new Vector3(center.x,GroundSurfaceY,center.z),size));}
        }
        Debug.Log(pass?"AI_STALL_STUDY_PASS "+checks+" checks / 9 scenarios / 270 simulation seconds":"AI_STALL_STUDY_FAIL "+checks+" checks");Application.Quit(pass?0:2);
    }
}
