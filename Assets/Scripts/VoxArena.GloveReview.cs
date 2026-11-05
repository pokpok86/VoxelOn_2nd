using System;
using System.Collections;
using System.IO;
using UnityEngine;
public sealed partial class VoxArena
{
    IEnumerator GloveReview()
    {
        string dir=Path.Combine(Directory.GetCurrentDirectory(),"Docs","Screenshots","GloveHands-20261105","Final");Directory.CreateDirectory(dir);EnsureWeaponStudio();
        GameObject[] actual={wispModelSource,bastionModelSource,tarantulaModelSource};GameObject[] current=referenceHandsBaseline;int checks=0,captures=0;bool pass=true;
        void Check(bool ok,string text){checks++;pass&=ok;Debug.Log("GLOVE_CHECK "+text+" "+(ok?"PASS":"FAIL"));}
        for(int frame=0;frame<3;frame++)
        {
            Vector3 oldPivot=Vector3.zero;Bounds oldBounds=new Bounds();
            for(int phase=0;phase<2;phase++)
            {
                GameObject model=phase==0?gloveReviewBaseline[frame]:current[frame];if(frame==0)wispModelSource=model;else if(frame==1)bastionModelSource=model;else tarantulaModelSource=model;
                TestSetupSelectable(frame,(frame+1)%3);smoke=false;inMenu=false;player.root.position=new Vector3(80,FighterGroundY,70);player.facing=player.chassisForward=Vector3.forward;aimDirection=Vector3.forward;roundTime=0;Animate(player);
                foreach(Transform t in player.root.GetComponentsInChildren<Transform>(true))t.gameObject.layer=9;foreach(Transform w in player.weaponVisuals)w.gameObject.SetActive(false);if(player.guardShield!=null)player.guardShield.gameObject.SetActive(false);
                yield return null;yield return new WaitForEndOfFrame();Transform hand=player.gripRig[0].hand;Bounds bounds=RenderedHandBounds(player,hand);
                if(phase==0){oldPivot=hand.position;oldBounds=bounds;}
                else
                {
                    Check(Vector3.Distance(hand.position,oldPivot)<.001f,"hand joint preserved frame "+frame);
                    float ratio=bounds.size.x*bounds.size.y*bounds.size.z/(oldBounds.size.x*oldBounds.size.y*oldBounds.size.z);Debug.Log("GLOVE_METRIC "+frame+" old="+oldBounds.size.ToString("F4")+" new="+bounds.size.ToString("F4")+" volumeRatio="+ratio);
                    Check(Finite(bounds.size)&&ratio>1.5f&&ratio<3.5f,"actual baked hand volume grows without scale explosion frame "+frame);
                }
                string prefix=(frame+1)+"-"+(phase==0?"Before":"After");weaponStudioCamera.cullingMask=1<<9;weaponStudioCamera.orthographicSize=2.5f;
                weaponStudioCamera.transform.position=player.root.position+new Vector3(0,1.2f,8);weaponStudioCamera.transform.LookAt(player.root.position+Vector3.up*.5f);weaponStudioCamera.Render();SaveWeaponImage(Path.Combine(dir,prefix+"-front.png"));captures++;
                weaponStudioCamera.transform.position=player.root.position+new Vector3(8,1.2f,0);weaponStudioCamera.transform.LookAt(player.root.position+Vector3.up*.5f);weaponStudioCamera.Render();SaveWeaponImage(Path.Combine(dir,prefix+"-side.png"));captures++;
                // Isolate the glove for close-up review; the torso can otherwise occlude the far hand.
                foreach(SkinnedMeshRenderer renderer in player.body.GetComponentsInChildren<SkinnedMeshRenderer>())
                {
                    int index=Array.IndexOf(renderer.bones,hand);bool isHand=false;
                    if(index>=0)foreach(BoneWeight weight in renderer.sharedMesh.boneWeights)if(weight.boneIndex0==index&&weight.weight0>.5f||weight.boneIndex1==index&&weight.weight1>.5f){isHand=true;break;}
                    renderer.enabled=isHand;
                }
                Vector3 center=hand.TransformPoint(bounds.center);weaponStudioCamera.orthographicSize=.48f;weaponStudioCamera.transform.position=center+new Vector3(-1,.4f,2);weaponStudioCamera.transform.LookAt(center);weaponStudioCamera.Render();SaveWeaponImage(Path.Combine(dir,prefix+"-hand.png"));captures++;
            }
        }
        wispModelSource=actual[0];bastionModelSource=actual[1];tarantulaModelSource=actual[2];
        Debug.Log((pass?"GLOVE_REVIEW_PASS ":"GLOVE_REVIEW_FAIL ")+checks+" checks; "+captures+" images");Application.Quit(pass?0:2);
    }
    Bounds RenderedHandBounds(Fighter f,Transform hand)
    {
        Bounds result=new Bounds();bool first=true;
        foreach(SkinnedMeshRenderer skin in f.body.GetComponentsInChildren<SkinnedMeshRenderer>())
        {
            int bone=Array.IndexOf(skin.bones,hand);if(bone<0)continue;Vector3[] vertices=skin.sharedMesh.vertices;BoneWeight[] weights=skin.sharedMesh.boneWeights;Matrix4x4[] bind=skin.sharedMesh.bindposes;
            for(int i=0;i<vertices.Length;i++)
            {
                BoneWeight w=weights[i];if(!(w.boneIndex0==bone&&w.weight0>.5f||w.boneIndex1==bone&&w.weight1>.5f))continue;
                Vector3 world=Vector3.zero;
                if(w.weight0>0)world+=(skin.bones[w.boneIndex0].localToWorldMatrix*bind[w.boneIndex0]).MultiplyPoint3x4(vertices[i])*w.weight0;
                if(w.weight1>0)world+=(skin.bones[w.boneIndex1].localToWorldMatrix*bind[w.boneIndex1]).MultiplyPoint3x4(vertices[i])*w.weight1;
                if(w.weight2>0)world+=(skin.bones[w.boneIndex2].localToWorldMatrix*bind[w.boneIndex2]).MultiplyPoint3x4(vertices[i])*w.weight2;
                if(w.weight3>0)world+=(skin.bones[w.boneIndex3].localToWorldMatrix*bind[w.boneIndex3]).MultiplyPoint3x4(vertices[i])*w.weight3;
                Vector3 p=hand.InverseTransformPoint(world);if(first){result=new Bounds(p,Vector3.zero);first=false;}else result.Encapsulate(p);
            }
        }
        return result;
    }
}
