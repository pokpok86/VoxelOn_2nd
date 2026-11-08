using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
public sealed partial class VoxArena
{
    sealed class LowerSkinSample {public Vector3[] vertices;public BoneWeight[] weights;public Matrix4x4[] bind;public Transform[] bones;public bool[] lower;}
    IEnumerator ChassisMotionStudy()
    {
        string revision=Array.IndexOf(Environment.GetCommandLineArgs(),"--gait-review")>=0?"Iteration33":Array.IndexOf(Environment.GetCommandLineArgs(),"--after-motion")>=0?"After":"Before";
        string folder=Path.Combine(Directory.GetCurrentDirectory(),"Docs","Screenshots","PD14-Chassis-Motion",revision);Directory.CreateDirectory(folder);
        bool pass=true;int checks=0,captures=0;RenderTexture target=new RenderTexture(768,768,24);Texture2D image=new Texture2D(768,768,TextureFormat.RGB24,false);
        Transform studio=SpawnPDPart("Pond",null,new Vector3(0,GroundSurfaceY-.008f,0),new Vector3(7,1,7));SetPartMaterial(studio,Mat(new Color(.18f,.20f,.23f)));foreach(Transform t in studio.GetComponentsInChildren<Transform>())t.gameObject.layer=9;
        for(int id=1;id<3;id++)
        {
            TestSetupSelectable(id,0);smoke=false;inMenu=false;paused=true;spectatorMode=false;ApplyClimate(WeatherKind.Clear,false);player.root.position=new Vector3(30,FighterGroundY,-50);player.facing=player.chassisForward=Vector3.forward;player.velocity=Vector3.zero;player.boostHeld=player.jumpHeld=false;
            foreach(Transform t in player.root.GetComponentsInChildren<Transform>(true))t.gameObject.layer=9;foreach(Transform t in player.weaponVisuals)t.gameObject.SetActive(false);if(player.guardShield!=null)player.guardShield.gameObject.SetActive(false);enemy.root.gameObject.SetActive(false);enemy.shadow.gameObject.SetActive(false);
            var samples=new List<LowerSkinSample>();foreach(SkinnedMeshRenderer skin in player.body.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                Transform[] bones=skin.bones;bool[] lower=new bool[bones.Length];bool any=false;for(int i=0;i<bones.Length;i++){lower[i]=bones[i]==player.legsPart||bones[i].IsChildOf(player.legsPart);any|=lower[i];}
                if(any)samples.Add(new LowerSkinSample{vertices=skin.sharedMesh.vertices,weights=skin.sharedMesh.boneWeights,bind=skin.sharedMesh.bindposes,bones=bones,lower=lower});
            }
            view.targetTexture=target;view.orthographic=true;view.orthographicSize=3.6f;view.cullingMask=1<<9;view.clearFlags=CameraClearFlags.SolidColor;view.backgroundColor=new Color(.13f,.16f,.20f);
            float start=Time.realtimeSinceStartup,nextSample=0,minGap=float.PositiveInfinity,maxGap=float.NegativeInfinity;int nextCapture=0,count=0;bool finite=true;Animate(player);Save("idle");
            while(Time.realtimeSinceStartup-start<6)
            {
                yield return null;float elapsed=Time.realtimeSinceStartup-start,dt=Mathf.Min(Time.deltaTime,.04f);roundTime+=dt;Vector3 input=elapsed<2?Vector3.forward:elapsed<4?Vector3.right:Vector3.back;Move(player,input,dt);Animate(player);
                if(elapsed>=nextSample)
                {
                    nextSample+=.12f;float minY=float.PositiveInfinity;
                    foreach(LowerSkinSample sample in samples)
                    {
                        Matrix4x4[] world=new Matrix4x4[sample.bones.Length];for(int b=0;b<world.Length;b++)world[b]=sample.bones[b].localToWorldMatrix*sample.bind[b];
                        for(int v=0;v<sample.vertices.Length;v++)
                        {
                            BoneWeight w=sample.weights[v];if(!sample.lower[w.boneIndex0])continue;Vector3 p=world[w.boneIndex0].MultiplyPoint3x4(sample.vertices[v]);minY=Mathf.Min(minY,p.y);finite&=Finite(p);
                        }
                    }
                    float gap=minY-GroundSurfaceY;minGap=Mathf.Min(minGap,gap);maxGap=Mathf.Max(maxGap,gap);count++;
                }
                if(nextCapture<3&&elapsed>1.5f+nextCapture*2){Save("moving-"+nextCapture);nextCapture++;}
            }
            Debug.Log("CHASSIS_MOTION_METRIC "+player.spec.name+" samples="+count+" minSoleGap="+minGap.ToString("F4")+" maxSoleGap="+maxGap.ToString("F4"));
            Check(finite&&count>30,"finite actual lower skin poses "+id);Check(minGap>=-.06f,"moving sole penetration stays below six centimetres "+id);Check(maxGap<=.18f,"at least one support stays near ground during normal travel "+id);
            void Save(string pose){studio.position=new Vector3(player.root.position.x,GroundSurfaceY-.008f,player.root.position.z);Vector3 center=player.root.position+Vector3.down*.2f;view.transform.position=center+new Vector3(5,2.4f,8).normalized*8+Vector3.up*.18f;view.transform.LookAt(center);view.Render();RenderTexture.active=target;image.ReadPixels(new Rect(0,0,768,768),0,0);image.Apply();File.WriteAllBytes(Path.Combine(folder,(id+1)+"-"+player.spec.name+"-"+pose+".png"),image.EncodeToPNG());captures++;}
            void Check(bool ok,string label){checks++;pass&=ok;Debug.Log("CHASSIS_MOTION_CHECK "+label+" "+(ok?"PASS":"FAIL"));}
        }
        RenderTexture.active=null;view.targetTexture=null;target.Release();Destroy(target);Destroy(image);Debug.Log(pass?"CHASSIS_MOTION_PASS "+checks+" checks / "+captures+" actual images":"CHASSIS_MOTION_FAIL "+checks+" checks");Application.Quit(pass?0:2);
    }
}
