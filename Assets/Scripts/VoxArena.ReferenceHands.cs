using System;
using UnityEngine;
public sealed partial class VoxArena
{
    sealed class ReferenceHandPose {public SkinnedMeshRenderer skin;public int grip,fist;}
    static int ReferenceShapeIndex(Mesh mesh,string name)
    {
        for(int i=0;i<mesh.blendShapeCount;i++){string current=mesh.GetBlendShapeName(i);if(current==name||current.EndsWith("."+name,StringComparison.Ordinal))return i;}return -1;
    }
    void ConnectReferenceHandPoses(Fighter f)
    {
        f.handPoses=new ReferenceHandPose[2];f.handPoseInitialized=false;
        foreach(SkinnedMeshRenderer skin in f.body.GetComponentsInChildren<SkinnedMeshRenderer>())
        for(int side=0;side<2;side++)if(skin.name.Contains("ArticulatedGlove_"+(side==0?"Left":"Right")))
        {
            int grip=ReferenceShapeIndex(skin.sharedMesh,"Grip"),fist=ReferenceShapeIndex(skin.sharedMesh,"Fist");if(grip>=0&&fist>=0)f.handPoses[side]=new ReferenceHandPose{skin=skin,grip=grip,fist=fist};
        }
    }
    static Vector3 HandGripPosition(ArmGrip arm)=>arm.gripSocket!=null?arm.gripSocket.position:arm.hand.position;
    Quaternion ReferenceHandRotation(Fighter f,int side,Quaternion facing)
    {
        ArmGrip arm=f.gripRig[side];bool gun=arm.gripSocket!=null&&!f.weaponProfiles[side].isShield&&!f.weaponProfiles[side].isUnarmed;
        return facing*(gun?Quaternion.AngleAxis(side==0?90:-90,Vector3.forward):Quaternion.identity)*arm.handFacing;
    }
    void AnimateReferenceHandPoses(Fighter f)
    {
        if(f.handPoses==null)return;float step=Mathf.Min(Time.deltaTime,.05f)*1200;
        for(int side=0;side<2;side++)
        {
            ReferenceHandPose pose=f.handPoses[side];if(pose==null)continue;WeaponProfile profile=f.weaponProfiles[side];
            float grip=profile.isUnarmed?0:profile.isShield?65:100;float fist=profile.isUnarmed?(f.punchTimer>0&&f.punchSide==side?100:20):0;
            float oldGrip=pose.skin.GetBlendShapeWeight(pose.grip),oldFist=pose.skin.GetBlendShapeWeight(pose.fist);
            float newGrip=f.handPoseInitialized?Mathf.MoveTowards(oldGrip,grip,step):grip,newFist=f.handPoseInitialized?Mathf.MoveTowards(oldFist,fist,step):fist;
            if(Mathf.Abs(newGrip-oldGrip)>.001f)pose.skin.SetBlendShapeWeight(pose.grip,newGrip);
            if(Mathf.Abs(newFist-oldFist)>.001f)pose.skin.SetBlendShapeWeight(pose.fist,newFist);
        }
        f.handPoseInitialized=true;
    }
}
