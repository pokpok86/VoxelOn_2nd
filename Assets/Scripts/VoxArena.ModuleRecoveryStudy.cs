using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
public sealed partial class VoxArena
{
    // Exercise real lethal damage, detached skin bones and repeated round restoration
    // on every current rig, rather than assuming the WISP result covers tracks/4 legs.
    IEnumerator ModuleRecoveryStudy()
    {
        string dir=Path.Combine(Directory.GetCurrentDirectory(),"Docs","Screenshots","PD14-Module-Recovery","Iteration02");Directory.CreateDirectory(dir);
        bool pass=true;int checks=0,captures=0;var silhouette=new Dictionary<string,bool[]>();
        RenderTexture target=new RenderTexture(768,768,24);Texture2D image=new Texture2D(768,768,TextureFormat.RGB24,false);
        for(int id=0;id<3;id++)
        {
            TestSetupSelectable(id,(id+1)%3);smoke=false;inMenu=false;paused=false;muted=true;targetLock=false;spectatorMode=false;Pose();
            foreach(Transform t in player.root.GetComponentsInChildren<Transform>(true))t.gameObject.layer=9;
            yield return null;yield return new WaitForEndOfFrame();Vector3[] original=SkinWorld();Save("before",3.6f);
            for(int cycle=0;cycle<2;cycle++)
            {
                player.shieldEnergy=0;Hit(player,enemy.root.position,10000,player.corePart.position);FinishPresentation();
                Check(player.deadVisual&&flyingModules.Count==4,"real lethal damage detaches four modules "+id+"/"+cycle);
                var positions=new List<Vector3>();foreach(FlyingModule m in flyingModules)positions.Add(m.part.position);
                float until=Time.realtimeSinceStartup+.7f;while(Time.realtimeSinceStartup<until)yield return null;
                bool moved=flyingModules.Count==4;for(int j=0;j<flyingModules.Count;j++)moved&=Finite(flyingModules[j].part.position)&&Vector3.Distance(positions[j],flyingModules[j].part.position)>.1f;
                Check(moved,"detached module bones actually fall and remain finite "+id+"/"+cycle);if(cycle==0)Save("defeated",4.8f);
                ResetRound();Pose();yield return null;yield return new WaitForEndOfFrame();
                Check(!player.deadVisual&&player.hp==player.spec.health&&flyingModules.Count==0&&visualParticles.Count==0,"round restores health hierarchy and clears effects "+id+"/"+cycle);
                Vector3[] restored=SkinWorld();float error=0;bool finite=restored.Length==original.Length;
                for(int v=0;v<restored.Length&&v<original.Length;v++){finite&=Finite(restored[v]);error=Mathf.Max(error,Vector3.Distance(original[v],restored[v]));}
                Debug.Log("MODULE_RESTORE_METRIC frame="+id+" cycle="+cycle+" vertices="+restored.Length+" maxWorldError="+error.ToString("F6"));
                Check(finite&&restored.Length>1000&&error<.003f,"actual restored skin returns within three millimetres "+id+"/"+cycle);
                if(cycle==0)Save("restored",3.6f);yield return null;
            }
        }
        target.Release();Destroy(target);Destroy(image);Debug.Log(pass?"MODULE_RECOVERY_PASS "+checks+" checks / "+captures+" actual images":"MODULE_RECOVERY_FAIL "+checks+" checks");Application.Quit(pass?0:2);
        void Check(bool ok,string name){checks++;pass&=ok;Debug.Log("MODULE_RECOVERY_CHECK "+name+" "+(ok?"PASS":"FAIL"));}
        void Pose(){roundTime=0;player.root.position=new Vector3(0,FighterGroundY,0);player.facing=player.chassisForward=Vector3.forward;player.velocity=Vector3.zero;player.jumpHeld=false;ResetCapsuleTreads(player);Animate(player);}
        Vector3[] SkinWorld()
        {
            var points=new List<Vector3>();Mesh mesh=new Mesh();
            foreach(SkinnedMeshRenderer renderer in player.body.GetComponentsInChildren<SkinnedMeshRenderer>())
            {renderer.BakeMesh(mesh);foreach(Vector3 vertex in mesh.vertices)points.Add(renderer.transform.TransformPoint(vertex));mesh.Clear();}
            Destroy(mesh);return points.ToArray();
        }
        void Save(string phase,float size)
        {
            view.targetTexture=target;view.orthographic=true;view.orthographicSize=size;view.cullingMask=1<<9;view.clearFlags=CameraClearFlags.SolidColor;view.backgroundColor=new Color(.13f,.16f,.20f);
            Vector3 at=new Vector3(0,FighterGroundY-.2f,0);view.transform.position=at+new Vector3(5,2.4f,8).normalized*8;view.transform.LookAt(at);view.Render();RenderTexture prior=RenderTexture.active;RenderTexture.active=target;
            image.ReadPixels(new Rect(0,0,768,768),0,0);image.Apply();
            if(phase=="before"||phase=="restored")
            {
                Color32[] pixels=image.GetPixels32();Color32 bg=pixels[0];bool[] mask=new bool[pixels.Length];int area=0;
                for(int i=0;i<mask.Length;i++){Color32 c=pixels[i];mask[i]=Mathf.Abs(c.r-bg.r)+Mathf.Abs(c.g-bg.g)+Mathf.Abs(c.b-bg.b)>45;if(mask[i])area++;}
                if(phase=="before")silhouette[player.spec.name]=mask;
                else {bool[] expected=silhouette[player.spec.name];int intersection=0,union=0;for(int i=0;i<mask.Length;i++){if(mask[i]&&expected[i])intersection++;if(mask[i]||expected[i])union++;}float iou=union==0?0:(float)intersection/union;Debug.Log("MODULE_RENDER_METRIC "+player.spec.name+" foreground="+area+" silhouetteIoU="+iou.ToString("F5"));Check(area>8000&&iou>.97f,"rendered restored silhouette matches actual before image "+player.spec.name);}
            }
            File.WriteAllBytes(Path.Combine(dir,player.spec.name+"-"+phase+".png"),image.EncodeToPNG());RenderTexture.active=prior;view.targetTexture=null;captures++;
        }
    }
}
