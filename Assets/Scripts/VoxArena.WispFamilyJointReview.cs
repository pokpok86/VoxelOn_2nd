using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed partial class VoxArena
{
    sealed class JointMeshSample {public Transform transform;public Vector3[] vertices;}
    IEnumerator FamilyJointReview()
    {
        string folder=Path.Combine(Directory.GetCurrentDirectory(),"Docs","Validation","WispJoints-20261112");Directory.CreateDirectory(folder);
        balanceDirectoryOverride=Path.Combine(Path.GetTempPath(),"VoxelOn-JointReview-"+DateTime.Now.ToString("yyyyMMdd-HHmmss"));
        familyLoadouts=new FamilyLoadoutFile();bool pass=true;int checks=0;
        var report=new List<string>();var metrics=new List<string>{"lower,phase,samples,maxElbowGap,minGroundGap,maxGroundGap,maxSideSwing,maxHingeAxisDegrees,finite"};
        void Check(bool ok,string text){checks++;pass&=ok;string line=(ok?"PASS ":"FAIL ")+text;report.Add(line);Debug.Log("WISP_JOINT_CHECK "+line);}
        TestSetupSelectable(0,1);smoke=false;inMenu=false;paused=true;muted=true;spectatorMode=false;targetLock=false;ApplyClimate(WeatherKind.Clear,false);
        enemy.root.gameObject.SetActive(false);enemy.shadow.gameObject.SetActive(false);
        var camera=new GameObject("Actual game joint review camera").AddComponent<Camera>();camera.enabled=false;camera.orthographic=true;camera.orthographicSize=2.3f;camera.cullingMask=1<<9;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.055f,.07f,.095f);
        RenderTexture rt=new RenderTexture(600,800,24);camera.targetTexture=rt;Texture2D image=new Texture2D(600,800,TextureFormat.RGB24,false);
        FamilyLoadout c=FamilyConfig(player);string[] phases={"Idle","Forward","Backward","Left","Right","Jump","Landing","AimUp","AimDown","Guard"};
        for(int lower=0;lower<FamilyLegs.Length;lower++)
        {
            TestSetupSelectable(lower==1?1:lower==2?2:0,0);smoke=false;inMenu=false;paused=true;enemy.root.gameObject.SetActive(false);enemy.shadow.gameObject.SetActive(false);c=FamilyConfig(player);
            c.legs=FamilyLegs[lower];c.backpack=FamilyBackpacks[0];c.weapons[0]="W-01_LUMEN";c.weapons[1]="W-01_LUMEN";
            BuildRobot(player);FamilyRig rig=familyRigs[player];
            var meshes=rig.legs.root.GetComponentsInChildren<MeshFilter>().Select(m=>new JointMeshSample{transform=m.transform,vertices=m.sharedMesh.vertices}).ToArray();
            foreach(Transform t in rig.root.GetComponentsInChildren<Transform>(true))t.gameObject.layer=9;
            for(int phase=0;phase<phases.Length;phase++)
            {
                Vector3 origin=new Vector3(80,0,70);origin.y=SurfaceHeight(origin)+FighterSupportOffset;
                player.root.position=origin;player.velocity=Vector3.zero;player.facing=player.chassisForward=Vector3.forward;player.weaponSlot=1;player.jumpHeight=player.jumpVelocity=0;player.jumpHeld=player.boostHeld=player.guarding=false;
                if(phase==5){player.jumpVelocity=JumpLaunchSpeed(player,player.spec.jumpImpulseScale);player.jumpHeld=true;}
                if(phase==6){player.jumpHeight=2;player.jumpVelocity=-5;player.root.position+=Vector3.up*2;}
                if(phase==9){player.weaponProfiles[1].isShield=true;player.weaponProfiles[1].guardReduction=.5f;BuildRobot(player);rig=familyRigs[player];meshes=rig.legs.root.GetComponentsInChildren<MeshFilter>().Select(m=>new JointMeshSample{transform=m.transform,vertices=m.sharedMesh.vertices}).ToArray();foreach(Transform t in rig.root.GetComponentsInChildren<Transform>(true))t.gameObject.layer=9;player.guarding=true;}
                Vector3 direction=phase==1?Vector3.forward:phase==2?Vector3.back:phase==3?Vector3.left:phase==4?Vector3.right:Vector3.zero;
                aimDirection=phase==7?new Vector3(0,.7f,1).normalized:phase==8?new Vector3(0,-.6f,1).normalized:Vector3.forward;
                float hingeError=0,elbowGap=0,minGap=float.PositiveInfinity,maxGap=float.NegativeInfinity,sideSwing=0;bool finite=true,landed=false;int samples=0;Quaternion? airbornePose=null;bool airFrozen=true;
                for(int tick=0;tick<64;tick++)
                {
                    roundTime+=.025f;Move(player,direction,.025f);Animate(player);
                    finite&=Finite(player.root.position)&&Finite(rig.root.position);
                    for(int side=0;side<2;side++)if(rig.weapons[side]!=null){Transform arm=rig.body.nodes[side==0?"LeftShoulder":"RightShoulder"];hingeError=Mathf.Max(hingeError,Vector3.Angle(arm.right,rig.weapons[side].root.right));elbowGap=Mathf.Max(elbowGap,Vector3.Distance(rig.weapons[side].root.position,arm.TransformPoint(rig.elbows[side])));}
                    if(lower==0){float a=rig.legs.nodes["L_Hip"].localEulerAngles.z;sideSwing=Mathf.Max(sideSwing,Mathf.Abs(Mathf.DeltaAngle(0,a)));}
                    if(phase==5&&player.jumpHeight>.1f&&(lower==0||lower==2)){Transform n=rig.legs.nodes.Values.FirstOrDefault(t=>t.name.EndsWith("Hip"));if(n!=null){if(airbornePose.HasValue)airFrozen&=Quaternion.Angle(n.localRotation,airbornePose.Value)<.02f;airbornePose=n.localRotation;}}
                    if(phase==6&&player.jumpHeight<.02f&&player.jumpVelocity<=0)landed=true;
                    if(tick%8==0)
                    {
                        float lowest=float.PositiveInfinity;foreach(var mesh in meshes){Matrix4x4 matrix=mesh.transform.localToWorldMatrix;foreach(Vector3 v in mesh.vertices){Vector3 p=matrix.MultiplyPoint3x4(v);finite&=Finite(p);lowest=Mathf.Min(lowest,p.y-SurfaceHeight(p));}}
                        minGap=Mathf.Min(minGap,lowest);maxGap=Mathf.Max(maxGap,lowest);samples++;
                        if(lower==0&&phase==1)Capture("Walk-"+(tick/8).ToString("00"));
                    }
                    if(tick%8==0)yield return null;
                }
                Check(finite&&elbowGap<.002f&&hingeError<.2f,"finite pose, attached elbow centers and aligned hinge axes / "+c.legs+" / "+phases[phase]);
                if(phase>=1&&phase<=4&&lower!=3)Check(minGap>-.07f&&maxGap<.20f,"support feet stay near ground / "+c.legs+" / "+phases[phase]);
                if(lower==0&&(phase==3||phase==4))Check(sideSwing>1,"biped hip responds laterally / "+phases[phase]);
                if(phase==5&&(lower==0||lower==2))Check(airFrozen&&airbornePose.HasValue,"stride frozen during actual boost ascent / "+c.legs);
                if(phase==6)Check(landed,"actual landing completes / "+c.legs);
                metrics.Add(c.legs+","+phases[phase]+","+samples+","+elbowGap.ToString("F5")+","+minGap.ToString("F4")+","+maxGap.ToString("F4")+","+sideSwing.ToString("F2")+","+hingeError.ToString("F3")+","+finite);
                Capture((lower+1)+"-"+phases[phase]);
                void Capture(string name)
                {
                    Vector3 center=rig.root.position+Vector3.up*1.65f;camera.transform.position=center+new Vector3(5,1.5f,8);camera.transform.LookAt(center);camera.Render();RenderTexture.active=rt;image.ReadPixels(new Rect(0,0,600,800),0,0);image.Apply();File.WriteAllBytes(Path.Combine(folder,name+".png"),image.EncodeToPNG());RenderTexture.active=null;
                }
            }
            player.weaponProfiles[1].isShield=false;
        }
        rt.Release();Destroy(rt);Destroy(image);Destroy(camera.gameObject);
        File.WriteAllLines(Path.Combine(folder,"Metrics.csv"),metrics);File.WriteAllLines(Path.Combine(folder,"Checks.txt"),report);File.WriteAllText(Path.Combine(folder,"Result.txt"),(pass?"PASS":"FAIL")+" / "+checks+" real-runtime joint checks");
        Debug.Log((pass?"WISP_JOINT_REVIEW_PASS ":"WISP_JOINT_REVIEW_FAIL ")+checks+" checks");Application.Quit(pass?0:2);
    }
}
