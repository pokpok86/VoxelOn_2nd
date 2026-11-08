using System;
using System.Collections;
using System.IO;
using UnityEngine;
public sealed partial class VoxArena
{
    IEnumerator ControlReview()
    {
        bool before=Array.IndexOf(Environment.GetCommandLineArgs(),"--before-controls")>=0;
        string dir=Path.Combine(Directory.GetCurrentDirectory(),"Docs","Screenshots","Controls-20261105",before?"Before":"After");Directory.CreateDirectory(dir);
        if(Array.IndexOf(Environment.GetCommandLineArgs(),"--weapon-art-review")>=0){dir=Path.Combine(Directory.GetCurrentDirectory(),"Docs","Screenshots","Weapons-20261105","Controls");Directory.CreateDirectory(dir);}
        bool pass=true;int checks=0;void Check(bool ok,string name){checks++;pass&=ok;Debug.Log("CONTROL_CHECK "+name+" "+(ok?"PASS":"FAIL"));}
        TestSetupSelectable(1,0);smoke=false;inMenu=false;paused=false;targetLock=false;player.root.position=new Vector3(80,FighterGroundY,70);enemy.root.position=player.root.position+Vector3.forward*30;player.facing=Vector3.forward;
        view.orthographic=false;view.transform.position=player.root.position-Vector3.forward*10+Vector3.up*2;view.transform.LookAt(enemy.root.position+Vector3.up*.7f);
        Vector3 softAim=(enemy.root.position-WeaponMuzzle(player)).normalized;assistEngaged=CanWeakAssist(player,enemy,softAim);
        Check(assistEngaged&&!targetLock&&TryGetTargetScreenRect(out Rect softRect)&&ShouldDrawSoftAssistReticle(true),"unlocked radar assist has a separate subtle target cue");
        yield return null;yield return new WaitForEndOfFrame();Texture2D softCue=ScreenCapture.CaptureScreenshotAsTexture();File.WriteAllBytes(Path.Combine(dir,"soft-aim-assist.png"),softCue.EncodeToPNG());Destroy(softCue);
        for(int id=1;id<3;id++)
        {
            TestSetupSelectable(id,0);smoke=false;inMenu=false;paused=false;targetLock=true;player.root.position=new Vector3(80,FighterGroundY,70);enemy.root.position=player.root.position+new Vector3(20,0,20);cameraYaw=-40;cameraPitch=12;
            UpdateThirdPersonCamera(1);Check(Mathf.Abs(Mathf.DeltaAngle(-40,cameraYaw))<=6.01f,"lock camera bounds turn after a long frame "+id);Check(Mathf.Abs(cameraPitch-12)<.01f,"lock acquisition preserves pilot pitch "+id);
            targetLock=false;UpdateThirdPersonCamera(.02f);
            player.root.position=new Vector3(80,FighterGroundY,70);enemy.root.position=player.root.position+new Vector3(20,0,20);cameraYaw=45;cameraPitch=DefaultThirdPersonPitch;targetLock=true;UpdateThirdPersonCamera(.02f);
            bool framed=TryGetTargetScreenRect(out Rect lockRect)&&lockRect.width>20&&lockRect.height>20&&lockRect.center.x>0&&lockRect.center.y>0;
            Check(framed,"lock cursor and character brackets share imported rendered bounds "+id);
            if(framed){yield return null;yield return new WaitForEndOfFrame();Texture2D lockImage=ScreenCapture.CaptureScreenshotAsTexture();File.WriteAllBytes(Path.Combine(dir,"lock-frame-"+player.spec.name+".png"),lockImage.EncodeToPNG());Destroy(lockImage);}
            targetLock=false;
            view.transform.position=player.root.position-Vector3.forward*10+Vector3.up*2;view.transform.LookAt(enemy.root.position+Vector3.up*1.2f);
            Vector3 ahead=enemy.root.position;enemy.root.position=player.root.position-Vector3.forward*20;
            Check(!TryAcquireTargetLock(player,enemy),"offscreen target cannot start camera lock "+id);enemy.root.position=ahead;
            Bounds cover=new Bounds((player.root.position+enemy.root.position)*.5f+Vector3.up,Vector3.one*5);obstacles.Add(cover);
            Check(!TryAcquireTargetLock(player,enemy),"occluded target cannot start camera lock "+id);obstacles.RemoveAt(obstacles.Count-1);
        }
        TestSetupSelectable(0,1);smoke=false;inMenu=false;targetLock=false;player.root.position=new Vector3(80,FighterGroundY,70);enemy.root.position=player.root.position+Vector3.forward*30;player.facing=Vector3.forward;Animate(player);
        view.orthographic=false;view.transform.position=player.root.position-Vector3.forward*10+Vector3.up*2;view.transform.LookAt(enemy.root.position);
        Vector3 direct=(enemy.root.position-WeaponMuzzle(player)).normalized;bool acquire=CanWeakAssist(player,enemy,Quaternion.AngleAxis(1.7f,Vector3.up)*direct);bool retain=CanWeakAssist(player,enemy,Quaternion.AngleAxis(2.1f,Vector3.up)*direct);
        Check(acquire&&retain,"shot assist retains a candidate across the narrow acquisition edge");Check(!CanWeakAssist(player,enemy,Quaternion.AngleAxis(3,Vector3.up)*direct),"shot assist still releases outside its small outer cone");
        float yaw=cameraYaw,pitch=cameraPitch;CanWeakAssist(player,enemy,Quaternion.AngleAxis(1.7f,Vector3.up)*direct);Vector3 corrected=WeakAssist(player,enemy,Quaternion.AngleAxis(2.1f,Vector3.up)*direct);
        Check(Vector3.Angle(corrected,Quaternion.AngleAxis(2.1f,Vector3.up)*direct)<=1.001f&&cameraYaw==yaw&&cameraPitch==pitch,"retained shot assistance stays at one degree without camera pull");
        Bounds wall=new Bounds((player.root.position+enemy.root.position)*.5f,Vector3.one*5);obstacles.Add(wall);Check(!CanWeakAssist(player,enemy,direct),"occlusion immediately drops the retained shot candidate");obstacles.RemoveAt(obstacles.Count-1);
        for(int id=0;id<3;id++)
        {
            TestSetupSelectable(id,(id+1)%3);smoke=false;inMenu=false;paused=false;muted=true;targetLock=false;roundTime=0;player.root.position=new Vector3(80,FighterGroundY,70);player.facing=player.chassisForward=Vector3.forward;player.velocity=Vector3.zero;aimDirection=Vector3.forward;
            foreach(Transform t in player.root.GetComponentsInChildren<Transform>(true))t.gameObject.layer=9;
            Animate(player);yield return null;yield return new WaitForEndOfFrame();Save(id,"aim");
            Transform elbow=player.leftArm.Find("Left_elbow"),hand=elbow!=null?elbow.Find("Left_hand"):null;
            float bend=hand==null?0:Vector3.Angle(elbow.position-player.leftArm.position,hand.position-elbow.position);Debug.Log("GRIP_METRIC "+id+" elbowBend="+bend+" barrelDot="+Vector3.Dot(player.weaponVisuals[0].forward,player.facing));
            Check(hand!=null&&bend>15&&Vector3.Dot(player.weaponVisuals[0].forward,player.facing)>.99f,"rifle is aimed forward with a bent elbow "+id);
            if(id==2)
            {
                Vector3[] last=new Vector3[4];for(int i=0;i<4;i++)last[i]=player.quadrupedAnkles[i].position;
                float start=Time.realtimeSinceStartup,slip=0,contactTime=0;int samples=0;
                while(Time.realtimeSinceStartup-start<4)
                {
                    yield return null;float dt=Mathf.Min(Time.deltaTime,.04f);player.root.position+=Vector3.forward*2*dt;player.velocity=Vector3.forward*2;roundTime+=dt;Animate(player);
                    for(int i=0;i<4;i++){Vector3 at=player.quadrupedAnkles[i].position;if(at.y<GroundSurfaceY+.18f&&last[i].y<GroundSurfaceY+.18f){slip+=Vector3.ProjectOnPlane(at-last[i],Vector3.up).magnitude;contactTime+=dt;samples++;}last[i]=at;}
                }
                float speed=slip/Mathf.Max(.001f,contactTime);Debug.Log("QUAD_STANCE_METRIC contactSamples="+samples+" meanSlipSpeed="+speed);Check(samples>30&&speed<.8f,"slow quadruped stance feet remain planted rather than sliding with the hull");
                yield return new WaitForEndOfFrame();Save(id,"walking");
                player.velocity=Vector3.zero;Vector3 resting=player.quadrupedAnkles[0].position;roundTime+=20;for(int n=0;n<4;n++)Animate(player);
                Check(Vector3.Distance(resting,player.quadrupedAnkles[0].position)<.005f,"standing quadruped does not keep cycling with elapsed round time");
                Vector3 oldScale=player.body.localScale;player.body.localScale=oldScale*1.1f;Animate(player);bool grounded=true;
                foreach(Transform foot in player.quadrupedAnkles)grounded&=Finite(foot.position)&&Mathf.Abs(foot.position.y-GroundSurfaceY-.075f*foot.lossyScale.y)<.01f;
                Check(grounded,"live model scaling recalibrates foot anchors and preserves grounded ankles");player.body.localScale=oldScale;Animate(player);
            }
        }
        TestSetupSelectable(2,0);smoke=false;inMenu=false;testLabOpen=true;testCategory=6;testSide=0;for(int f=0;f<4;f++)yield return null;yield return new WaitForEndOfFrame();Texture2D lab=ScreenCapture.CaptureScreenshotAsTexture();File.WriteAllBytes(Path.Combine(dir,"quad-live-settings.png"),lab.EncodeToPNG());Destroy(lab);testLabOpen=false;
        Debug.Log(pass?"CONTROL_REVIEW_PASS "+checks+" checks":"CONTROL_REVIEW_FAIL "+checks+" checks");Application.Quit(pass?0:2);
        void Save(int id,string pose)
        {
            RenderTexture target=new RenderTexture(768,768,24);Texture2D image=new Texture2D(768,768,TextureFormat.RGB24,false);view.targetTexture=target;view.cullingMask=1<<9;view.orthographic=true;view.orthographicSize=3.6f;view.clearFlags=CameraClearFlags.SolidColor;view.backgroundColor=new Color(.13f,.16f,.20f);
            Vector3 at=player.root.position+Vector3.down*.2f;view.transform.position=at+new Vector3(5,2.4f,8).normalized*8;view.transform.LookAt(at);view.Render();RenderTexture previous=RenderTexture.active;RenderTexture.active=target;image.ReadPixels(new Rect(0,0,768,768),0,0);image.Apply();File.WriteAllBytes(Path.Combine(dir,(id+1)+"-"+player.spec.name+"-"+pose+".png"),image.EncodeToPNG());RenderTexture.active=previous;view.targetTexture=null;target.Release();Destroy(target);Destroy(image);
        }
    }
}
