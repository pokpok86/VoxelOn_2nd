using System;
using System.Collections;
using System.IO;
using UnityEngine;
public sealed partial class VoxArena
{
    IEnumerator CornerSoak()
    {
        string folder=Path.Combine(Directory.GetCurrentDirectory(),"Docs","Screenshots","PD14-Corner-Recovery");Directory.CreateDirectory(folder);
        bool pass=true;int checks=0;
        void Check(bool ok,string label){checks++;pass&=ok;Debug.Log("CORNER_CHECK "+label+" "+(ok?"PASS":"FAIL"));}
        for(int frame=0;frame<3;frame++)for(int scenario=0;scenario<2;scenario++)
        {
            TestSetupSelectable((frame+1)%3,frame);smoke=false;inMenu=false;muted=true;spectatorMode=true;targetLock=false;cameraYaw=35;
            int first=obstacles.Count;Transform a=null,b=null;
            if(scenario==1)
            {
                // Temporary test-only high L-shaped cover; ordinary Canyon stays unchanged.
                Bounds one=new Bounds(new Vector3(105,GroundSurfaceY+4,98),new Vector3(22,8,2));
                Bounds two=new Bounds(new Vector3(98,GroundSurfaceY+4,105),new Vector3(2,8,22));obstacles.Add(one);obstacles.Add(two);
                a=PlaceEnvironment("FoundryCargo",null,new Vector3(105,GroundSurfaceY,98),new Vector3(22,8,2));b=PlaceEnvironment("FoundryCargo",null,new Vector3(98,GroundSurfaceY,105),new Vector3(2,8,22));
                enemy.root.position=new Vector3(100+enemy.Radius,FighterGroundY,100+enemy.Radius);player.root.position=new Vector3(84,FighterGroundY,84);
            }
            else {enemy.root.position=new Vector3(-ArenaHalfWidth+enemy.Radius+.4f,FighterGroundY,-ArenaHalfWidth+enemy.Radius+.4f);player.root.position=new Vector3(-90,FighterGroundY,-90);}
            enemy.chassisForward=(player.root.position-enemy.root.position).normalized;enemy.facing=enemy.chassisForward;
            Vector3 start=enemy.root.position,previous=start;float travel=0,maxStill=0,still=0;bool finite=true,clear=false;
            Check(!Occupied(start,enemy.Radius*.99f),"valid starting clearance frame="+frame+" case="+scenario);
            float begin=Time.realtimeSinceStartup;
            while(Time.realtimeSinceStartup-begin<8)
            {
                yield return null;float dt=Mathf.Min(Time.deltaTime,.04f);roundTime+=dt;player.hp=player.spec.health;enemy.hp=enemy.spec.health;
                EnemyAI(enemy,player,dt);UpdateCombat(dt);UpdateSparks(dt);Animate(player);Animate(enemy);
                float step=Vector3.Distance(previous,enemy.root.position);travel+=step;previous=enemy.root.position;still=step<.002f?still+dt:0;maxStill=Mathf.Max(maxStill,still);
                finite&=Finite(enemy.root.position);clear|=!ObstacleOnSegment(enemy.root.position,player.root.position,.2f,out _);
            }
            Debug.Log("CORNER_TRACE frame="+frame+" case="+scenario+" travel="+travel.ToString("F2")+" displacement="+Vector3.Distance(start,enemy.root.position).ToString("F2")+" longestStill="+maxStill.ToString("F2")+" clear="+clear);
            Check(finite,"finite corner movement frame="+frame+" case="+scenario);Check(travel>12&&Vector3.Distance(start,enemy.root.position)>6,"escape corner frame="+frame+" case="+scenario);
            Check(clear,"recover clear shot frame="+frame+" case="+scenario);Check(!Occupied(enemy.root.position,enemy.Radius*.99f),"no final wall penetration frame="+frame+" case="+scenario);
            yield return new WaitForEndOfFrame();Texture2D image=ScreenCapture.CaptureScreenshotAsTexture();File.WriteAllBytes(Path.Combine(folder,(frame+1)+"-case-"+scenario+".png"),image.EncodeToPNG());Destroy(image);
            if(obstacles.Count>first)obstacles.RemoveRange(first,obstacles.Count-first);if(a!=null)Destroy(a.gameObject);if(b!=null)Destroy(b.gameObject);
        }
        Debug.Log(pass?"CORNER_SOAK_PASS "+checks+" checks":"CORNER_SOAK_FAIL "+checks+" checks");Application.Quit(pass?0:2);
    }
}
