using System;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
public sealed partial class VoxArena
{
    IEnumerator PrototypeSoak()
    {
        int checks=0;bool passed=true;string folder=Path.Combine(Directory.GetCurrentDirectory(),"Docs","Screenshots","PD14-Three-Prototypes",arenaMap.ToString());Directory.CreateDirectory(folder);
        if(Array.IndexOf(Environment.GetCommandLineArgs(),"--weapon-art-review")>=0){folder=Path.Combine(Directory.GetCurrentDirectory(),"Docs","Screenshots","Weapons-20261105","Combat",arenaMap.ToString());Directory.CreateDirectory(folder);}
        void Check(bool ok,string label){checks++;passed&=ok;Debug.Log("PROTOTYPE_CHECK "+label+" "+(ok?"PASS":"FAIL"));}
        for(int id=0;id<3;id++)
        {
            TestSetupSelectable(id,(id+1)%3);smoke=false;inMenu=false;muted=true;spectatorMode=false;targetLock=false;
            player.root.position=new Vector3(70,FighterGroundY,-70);enemy.root.position=new Vector3(70,FighterGroundY,-45);player.facing=Vector3.forward;
            float begin=Time.realtimeSinceStartup,travel=0,peak=0;double previousFrame=Time.realtimeSinceStartupAsDouble;var frameTimes=new List<float>();int fired=0,captures=0;int[] firedBySlot=new int[4];Vector3 previous=player.root.position,previousEnemy=enemy.root.position;float enemyTravel=0,stillTime=0,maxStill=0;bool finite=true;
            while(Time.realtimeSinceStartup-begin<15)
            {
                yield return null;float t=Time.realtimeSinceStartup-begin,dt=Mathf.Min(Time.deltaTime,.04f);
                double now=Time.realtimeSinceStartupAsDouble;if(t>1)frameTimes.Add((float)((now-previousFrame)*1000));previousFrame=now;roundTime+=dt;
                player.hp=player.spec.health;enemy.hp=enemy.spec.health;
                Vector3 toward=(enemy.root.position-player.root.position).normalized;player.facing=id==0?Vector3.forward:Vector3.ProjectOnPlane(toward,Vector3.up).normalized;
                player.boostHeld=t%5<2;player.jumpHeld=t>4&&t<4.5f;
                if(t>4&&t<4.15f&&player.jumpHeight<=.01f){player.jumpOriginY=player.root.position.y;player.jumpVelocity=jumpSpeed*player.spec.jumpImpulseScale;}
                Move(player,t<7?Vector3.forward:Vector3.right,dt);player.weaponSlot=1+(Mathf.FloorToInt(t/3)%4);int beforeShots=projectilesCreated;
                Attack(player,enemy,id==0?player.facing:toward);int spawned=projectilesCreated-beforeShots;fired+=spawned;firedBySlot[player.weaponSlot-1]+=spawned;
                EnemyAI(enemy,player,dt);UpdateCombat(dt);UpdateObstacles(dt);UpdateSparks(dt);Animate(player);Animate(enemy);
                float enemyStep=Vector3.Distance(previousEnemy,enemy.root.position);enemyTravel+=enemyStep;previousEnemy=enemy.root.position;stillTime=enemyStep<.002f?stillTime+dt:0;maxStill=Mathf.Max(maxStill,stillTime);
                travel+=Vector3.Distance(previous,player.root.position);previous=player.root.position;peak=Mathf.Max(peak,player.jumpHeight);
                finite&=Finite(player.root.position)&&Finite(enemy.root.position);
                cameraYaw=0;cameraPitch=DefaultThirdPersonPitch;
                yield return new WaitForEndOfFrame();
                if(captures<2&&t>(captures==0?6:12))
                {
                    Texture2D image=ScreenCapture.CaptureScreenshotAsTexture();File.WriteAllBytes(Path.Combine(folder,(id+1)+"-"+player.spec.name+"-"+captures+".png"),image.EncodeToPNG());Destroy(image);captures++;
                }
            }
            frameTimes.Sort();float sum=0;foreach(float ms in frameTimes)sum+=ms;if(frameTimes.Count>0)Debug.Log("PROTOTYPE_PERF "+arenaMap+" "+player.spec.name+" frames="+frameTimes.Count+" avgMs="+(sum/frameTimes.Count).ToString("F2")+" p95Ms="+frameTimes[(int)(frameTimes.Count*.95f)].ToString("F2"));
            Check(frameTimes.Count>120,"actual rendered timing sample "+id);
            Debug.Log("PROTOTYPE_AI_TRAVEL "+id+" distance="+enemyTravel.ToString("F2")+" longestStill="+maxStill.ToString("F2"));
            Check(maxStill<1.8f,"actual rival avoids sustained movement stalls "+id);
            Check(enemyTravel>5,"actual rival AI follows moving opponent "+id);Check(finite,"finite movement "+id);Check(travel>30,"actual movement/boost "+id);Check(peak>.6f,"actual jump "+id);Check(fired>5&&Array.TrueForAll(firedBySlot,n=>n>0),"four-slot actual firing "+id+" counts="+string.Join(",",firedBySlot));Check(captures==2,"actual rear gameplay captures "+id);
        }
        Debug.Log(passed?"PROTOTYPE_SOAK_PASS "+checks+" checks":"PROTOTYPE_SOAK_FAIL "+checks+" checks");Application.Quit(passed?0:2);
    }
}
