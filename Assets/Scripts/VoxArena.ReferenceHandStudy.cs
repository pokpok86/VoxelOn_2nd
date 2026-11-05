using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
public sealed partial class VoxArena
{
    IEnumerator ReferenceHandStudy()
    {
        string dir=Path.Combine(Directory.GetCurrentDirectory(),"Docs","Screenshots","ReferenceHands-20261106","Unity-v003-Angles");Directory.CreateDirectory(dir);EnsureWeaponStudio();
        GameObject[] current={wispModelSource,bastionModelSource,tarantulaModelSource};int checks=0,captures=0;bool pass=true;
        void Check(bool ok,string text){checks++;pass&=ok;Debug.Log("REFERENCE_HAND_CHECK "+text+" "+(ok?"PASS":"FAIL"));}
        activeBalance=BalanceData.Defaults();foreach(BalanceFrame frame in activeBalance.frames){frame.slots[0]=frame.slots[1]="Military_AR";}
        for(int frame=0;frame<3;frame++)
        {
            if(frame==0)wispModelSource=referenceHandsBaseline[frame];else if(frame==1)bastionModelSource=referenceHandsBaseline[frame];else tarantulaModelSource=referenceHandsBaseline[frame];
            TestSetupSelectable(frame,(frame+1)%3);smoke=false;inMenu=false;Setup();Animate(player);yield return null;yield return new WaitForEndOfFrame();yield return Shot(frame+"-Before-Box-Hand.png",0,true,false);
            if(frame==0)wispModelSource=current[frame];else if(frame==1)bastionModelSource=current[frame];else tarantulaModelSource=current[frame];
            TestSetupSelectable(frame,(frame+1)%3);smoke=false;inMenu=false;Setup();Animate(player);yield return null;yield return new WaitForEndOfFrame();
            bool sockets=player.gripRig[0].gripSocket!=null&&player.gripRig[1].gripSocket!=null;bool poses=player.handPoses!=null&&player.handPoses[0]!=null&&player.handPoses[1]!=null;
            Check(sockets,"real palm grip sockets both hands frame "+frame);Check(poses,"Grip/Fist morph channels both hands frame "+frame);if(!sockets||!poses)continue;
            ReferenceHandPose handPose=player.handPoses[0];Transform hand=player.gripRig[0].hand;
            Check(handPose.skin.sharedMesh.vertexCount>500&&handPose.skin.sharedMesh.vertexCount<5000,"detailed but bounded hand geometry frame "+frame);
            Check(handPose.skin.GetBlendShapeWeight(handPose.grip)>99&&player.handPoses[1].skin.GetBlendShapeWeight(player.handPoses[1].grip)>99,"guns automatically close both hands into grip pose frame "+frame);
            Check(Vector3.Distance(player.weaponGripPoints[0].position,HandGripPosition(player.gripRig[0]))<.01f&&Vector3.Distance(player.weaponGripPoints[1].position,HandGripPosition(player.gripRig[1]))<.01f,"gun handles align inside both palms frame "+frame);
            Check(Vector3.Distance(player.weaponVisuals[0].lossyScale,player.weaponVisuals[1].lossyScale)<.001f,"same gun has equal physical scale in both hands frame "+frame);
            yield return Shot(frame+"-After-Guns-Actor.png",0,false,false);yield return Shot(frame+"-After-Actual-Gun-Grip.png",0,true,true);yield return Shot(frame+"-After-Actual-Gun-Grip-Inner.png",0,true,true);yield return Shot(frame+"-After-Actual-Gun-Grip-Side.png",0,true,true);
            Vector3 socket=player.gripRig[0].gripSocket.position;
            handPose.skin.SetBlendShapeWeight(handPose.grip,0);handPose.skin.SetBlendShapeWeight(handPose.fist,0);Vector3[] relaxed=ReferenceHandVertices(handPose.skin,hand);Bounds relaxedBounds=PointsBounds(relaxed);yield return Shot(frame+"-After-Relaxed-Hand.png",0,true,false);
            handPose.skin.SetBlendShapeWeight(handPose.grip,100);Vector3[] gripped=ReferenceHandVertices(handPose.skin,hand);float gripDelta=MaxPointDelta(relaxed,gripped);Check(gripDelta>.04f,"actual grip morph bends finger vertices frame "+frame);yield return Shot(frame+"-After-Grip-Hand.png",0,true,false);
            handPose.skin.SetBlendShapeWeight(handPose.grip,0);handPose.skin.SetBlendShapeWeight(handPose.fist,100);Vector3[] closed=ReferenceHandVertices(handPose.skin,hand);Bounds fistBounds=PointsBounds(closed);
            Check(MaxPointDelta(relaxed,closed)>.08f&&fistBounds.size.y<relaxedBounds.size.y*.80f,"fist folds fingers into a compact shape frame "+frame);
            Check(Vector3.Distance(socket,player.gripRig[0].gripSocket.position)<.001f,"finger poses do not move wrist/palm connection frame "+frame);
            Check(Finite(fistBounds.size)&&fistBounds.size.magnitude>.15f&&fistBounds.size.magnitude<1.5f,"fist retains meter-scale proportions frame "+frame);yield return Shot(frame+"-After-Fist-Hand.png",0,true,false);
            activeBalance.frames[frame].slots[0]="UNARMED";TestSetupSelectable(frame,(frame+1)%3);smoke=false;inMenu=false;Setup();player.weaponSlot=1;Animate(player);FireEquippedWeapon(player,enemy,Vector3.forward);
            float until=Time.realtimeSinceStartup+.12f;while(Time.realtimeSinceStartup<until){yield return null;TickWeapons(player,Mathf.Min(Time.deltaTime,.04f));Animate(player);}
            ReferenceHandPose punching=player.handPoses[0];Check(punching.skin.GetBlendShapeWeight(punching.fist)>90&&player.punchTimer>0,"real punch animation closes selected empty hand frame "+frame);yield return Shot(frame+"-After-Live-Punch.png",0,false,false);
            Check(Finite(player.gripRig[0].hand.position)&&Vector3.Distance(player.gripRig[0].hand.position,player.leftArm.position)<3,"punch keeps arm rig finite frame "+frame);activeBalance.frames[frame].slots[0]="Military_AR";
        }
        wispModelSource=current[0];bastionModelSource=current[1];tarantulaModelSource=current[2];activeBalance=null;
        Debug.Log((pass?"REFERENCE_HAND_STUDY_PASS ":"REFERENCE_HAND_STUDY_FAIL ")+checks+" checks; "+captures+" actual images");Application.Quit(pass?0:2);
        void Setup(){player.root.position=new Vector3(80,FighterGroundY,70);player.facing=player.chassisForward=Vector3.forward;player.velocity=Vector3.zero;aimDirection=Vector3.forward;roundTime=0;enemy.root.position=player.root.position+Vector3.forward*30;foreach(Transform t in player.root.GetComponentsInChildren<Transform>(true))t.gameObject.layer=9;}
        IEnumerator Shot(string name,int side,bool isolated,bool withGun)
        {
            Transform hand=player.gripRig[side].hand;var enabled=new Dictionary<Renderer,bool>();
            foreach(Renderer renderer in player.body.GetComponentsInChildren<Renderer>(true))
            {
                enabled[renderer]=renderer.enabled;
                if(!isolated)continue;bool show=renderer is SkinnedMeshRenderer skin&&SkinUsesHand(skin,hand);if(withGun&&renderer.transform.IsChildOf(player.weaponVisuals[side]))show=true;renderer.enabled=renderer.enabled&&show;
            }
            yield return null;yield return new WaitForEndOfFrame();weaponStudioCamera.cullingMask=1<<9;
            if(!isolated){weaponStudioCamera.orthographicSize=2.6f;weaponStudioCamera.transform.position=player.root.position+new Vector3(-5,2.5f,8);weaponStudioCamera.transform.LookAt(player.root.position+Vector3.up*.6f);}
            else if(withGun)
            {
                Vector3 center=HandGripPosition(player.gripRig[side]);float outward=Mathf.Sign(Vector3.Dot(hand.position-player.root.position,player.root.right));if(name.Contains("Inner"))outward=-outward;float front=name.Contains("Side")?.15f:1.1f;weaponStudioCamera.orthographicSize=.55f;weaponStudioCamera.transform.position=center+player.root.right*(outward*.9f)+Vector3.up*.35f+player.facing*front;weaponStudioCamera.transform.LookAt(center);
            }
            else
            {
                Bounds bounds=player.handPoses!=null&&player.handPoses[side]!=null?PointsBounds(ReferenceHandVertices(player.handPoses[side].skin,hand)):RenderedHandBounds(player,hand);
                Vector3 center=hand.TransformPoint(bounds.center);weaponStudioCamera.orthographicSize=.50f;weaponStudioCamera.transform.position=center+hand.rotation*new Vector3(-1,.4f,2);weaponStudioCamera.transform.LookAt(center,hand.up);
            }
            weaponStudioCamera.Render();SaveWeaponImage(Path.Combine(dir,name));captures++;foreach(var entry in enabled)entry.Key.enabled=entry.Value;
        }
    }
    static bool SkinUsesHand(SkinnedMeshRenderer skin,Transform hand)
    {
        int index=Array.IndexOf(skin.bones,hand);if(index<0)return false;foreach(BoneWeight w in skin.sharedMesh.boneWeights)if(w.boneIndex0==index&&w.weight0>.5f||w.boneIndex1==index&&w.weight1>.5f)return true;return false;
    }
    static Bounds PointsBounds(Vector3[] points)
    {
        Bounds bounds=new Bounds();bool first=true;foreach(Vector3 p in points){if(first){bounds=new Bounds(p,Vector3.zero);first=false;}else bounds.Encapsulate(p);}return bounds;
    }
    static float MaxPointDelta(Vector3[] a,Vector3[] b){float distance=0;for(int i=0;i<a.Length;i++)distance=Mathf.Max(distance,Vector3.Distance(a[i],b[i]));return distance;}
    static Vector3[] ReferenceHandVertices(SkinnedMeshRenderer skin,Transform hand)
    {
        Mesh mesh=skin.sharedMesh;Vector3[] vertices=mesh.vertices;
        for(int shape=0;shape<mesh.blendShapeCount;shape++)
        {
            float amount=skin.GetBlendShapeWeight(shape);if(amount<=0)continue;int frame=mesh.GetBlendShapeFrameCount(shape)-1;Vector3[] delta=new Vector3[vertices.Length];mesh.GetBlendShapeFrameVertices(shape,frame,delta,null,null);float weight=amount/mesh.GetBlendShapeFrameWeight(shape,frame);for(int i=0;i<vertices.Length;i++)vertices[i]+=delta[i]*weight;
        }
        BoneWeight[] weights=mesh.boneWeights;Matrix4x4[] bind=mesh.bindposes;
        for(int i=0;i<vertices.Length;i++)
        {
            BoneWeight w=weights[i];Vector3 p=vertices[i],world=Vector3.zero;
            if(w.weight0>0)world+=(skin.bones[w.boneIndex0].localToWorldMatrix*bind[w.boneIndex0]).MultiplyPoint3x4(p)*w.weight0;
            if(w.weight1>0)world+=(skin.bones[w.boneIndex1].localToWorldMatrix*bind[w.boneIndex1]).MultiplyPoint3x4(p)*w.weight1;
            if(w.weight2>0)world+=(skin.bones[w.boneIndex2].localToWorldMatrix*bind[w.boneIndex2]).MultiplyPoint3x4(p)*w.weight2;
            if(w.weight3>0)world+=(skin.bones[w.boneIndex3].localToWorldMatrix*bind[w.boneIndex3]).MultiplyPoint3x4(p)*w.weight3;
            vertices[i]=hand.InverseTransformPoint(world);
        }
        return vertices;
    }
}
