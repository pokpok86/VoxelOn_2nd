using System;
using System.Collections;
using System.IO;
using UnityEngine;
public sealed partial class VoxArena
{
    IEnumerator EffectCapture()
    {
        string dir=Path.Combine(Directory.GetCurrentDirectory(),"Docs","Screenshots","PD14-Effects-Iteration04");Directory.CreateDirectory(dir);
        int captures=0;bool pass=true;TestSetupSelectable(0,1);smoke=false;inMenu=false;muted=true;targetLock=false;spectatorMode=false;cameraYaw=0;cameraPitch=DefaultThirdPersonPitch;
        player.root.position=new Vector3(0,FighterGroundY,0);enemy.root.position=new Vector3(10,FighterGroundY,20);player.facing=Vector3.forward;Animate(player);Animate(enemy);
        player.shieldEnergy=0;Hit(player,enemy.root.position,45,player.leftArm.position);
        float waitUntil=Time.realtimeSinceStartup+2;bool emitted=false;
        while(Time.realtimeSinceStartup<waitUntil){Move(player,Vector3.zero,Time.deltaTime);Animate(player);yield return null;emitted=visualParticles.Exists(p=>p.smoke&&p.age>.72f);if(emitted)break;}
        pass&=emitted;Debug.Log("EFFECT_CHECK actual delayed arm smoke emitted "+emitted);
                yield return new WaitForEndOfFrame();Save("01-arm-damage-smoke.png");pass&=player.partDamage[2]>0;
        foreach(VisualParticle puff in visualParticles)if(puff.smoke)
        {
            Renderer renderer=puff.visual.GetComponentInChildren<Renderer>();Bounds bounds=renderer.bounds;
            Debug.Log("SMOKE_RENDER_EVIDENCE center="+bounds.center.ToString("F3")+" size="+bounds.size.ToString("F3")+" camera="+view.WorldToViewportPoint(bounds.center).ToString("F3")+" active="+renderer.enabled);
            Vector3 before=view.transform.position;Quaternion rotation=view.transform.rotation;
            view.transform.position=bounds.center+new Vector3(-1,1,-2);view.transform.LookAt(bounds.center);SaveScene("01b-arm-smoke-closeup.png");view.transform.SetPositionAndRotation(before,rotation);break;
        }
        Debris cover=debris[0];player.root.position=cover.origin+new Vector3(0,0,-12);player.root.position=new Vector3(player.root.position.x,FighterGroundY,player.root.position.z);player.facing=Vector3.forward;Animate(player);
        Vector3 impact=obstacles[0].center-Vector3.forward*obstacles[0].extents.z;DamageDestructibleCover(impact,10);
        for(int i=0;i<15;i++)yield return null;
        yield return new WaitForEndOfFrame();Save("02-cover-crack.png");pass&=cover.cracks.Count>0;
        DamageDestructibleCover(impact,10000);
        for(int i=0;i<8;i++)yield return null;
        yield return new WaitForEndOfFrame();Save("03-cover-break.png");pass&=cover.destroyed&&coverFragments.Count>=16;
        TestSetupSelectable(0,1);smoke=false;inMenu=false;player.root.position=new Vector3(0,FighterGroundY,0);enemy.root.position=new Vector3(0,FighterGroundY,15);player.facing=Vector3.forward;Animate(player);Animate(enemy);
        player.jumpOriginY=player.root.position.y;player.jumpVelocity=jumpSpeed*player.spec.jumpImpulseScale;player.jumpHeld=true;
        for(int i=0;i<30;i++){Move(player,Vector3.zero,1f/120);roundTime+=1f/120;Animate(player);yield return null;}
        yield return new WaitForEndOfFrame();Save("04-air-held-space-exhaust.png");pass&=player.boostVisual.gameObject.activeSelf;
        player.jumpHeld=false;player.root.position=new Vector3(0,FighterGroundY,0);player.jumpHeight=player.jumpVelocity=0;Animate(player);
        enemy.shieldEnergy=0;Hit(enemy,player.root.position,10000,enemy.corePart.position);FinishPresentation();
        for(int i=0;i<12;i++)yield return null;
        yield return new WaitForEndOfFrame();Save("05-robot-module-break.png");pass&=enemy.deadVisual&&flyingModules.Count==4;
        ResetRound();pass&=!player.deadVisual&&!enemy.deadVisual&&flyingModules.Count==0;
        Debug.Log(pass?"EFFECT_CAPTURE_PASS "+captures+" actual images; damage crack cover break exhaust module break reset":"EFFECT_CAPTURE_FAIL");Application.Quit(pass?0:2);
        void SaveScene(string name)
        {
            RenderTexture target=new RenderTexture(768,768,24);Texture2D image=new Texture2D(768,768,TextureFormat.RGB24,false);RenderTexture previous=RenderTexture.active;
            view.targetTexture=target;view.Render();RenderTexture.active=target;image.ReadPixels(new Rect(0,0,768,768),0,0);image.Apply();File.WriteAllBytes(Path.Combine(dir,name),image.EncodeToPNG());view.targetTexture=null;RenderTexture.active=previous;target.Release();Destroy(target);Destroy(image);captures++;
        }
        void Save(string name){Texture2D image=ScreenCapture.CaptureScreenshotAsTexture();File.WriteAllBytes(Path.Combine(dir,name),image.EncodeToPNG());Destroy(image);captures++;}
    }
}
