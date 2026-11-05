using UnityEngine;
public sealed partial class VoxArena
{
    sealed class ArmGrip {public Transform shoulder,elbow,hand,gripSocket;public Quaternion elbowRest,handFacing;}
    void ConnectArmGrips(Fighter f)
    {
        f.gripRig=null;if(!f.simpleShell)return;f.gripRig=new ArmGrip[2];
        for(int side=0;side<2;side++)
        {
            string name=side==0?"Left":"Right";Transform upper=side==0?f.leftArm:f.rightArm,elbow=upper.Find(name+"_elbow"),hand=elbow!=null?elbow.Find(name+"_hand"):null;
            if(hand!=null)f.gripRig[side]=new ArmGrip{shoulder=upper,elbow=elbow,hand=hand,gripSocket=hand.Find(name+"_GRIP_SOCKET"),elbowRest=elbow.localRotation,handFacing=Quaternion.Inverse(f.root.rotation)*hand.rotation};
        }
        ConnectReferenceHandPoses(f);
    }
    static void SolveTwoBone(Transform upper,Transform joint,Transform end,Vector3 goal,Vector3 bend)
    {
        Vector3 a=upper.position,first=joint.position-a,second=end.position-joint.position,d=goal-a;float l1=first.magnitude,l2=second.magnitude;
        if(l1<.0001f||l2<.0001f||d.sqrMagnitude<.0001f)return;
        float length=Mathf.Clamp(d.magnitude,Mathf.Abs(l1-l2)+.001f,l1+l2-.001f);Vector3 axis=d.normalized,normal=Vector3.ProjectOnPlane(bend,axis).normalized;
        if(normal.sqrMagnitude<.01f)normal=Vector3.ProjectOnPlane(Vector3.up,axis).normalized;
        float along=(l1*l1-l2*l2+length*length)/(2*length),height=Mathf.Sqrt(Mathf.Max(0,l1*l1-along*along));Vector3 at=a+axis*along+normal*height;
        upper.rotation=Quaternion.FromToRotation(first,at-a)*upper.rotation;
        joint.rotation=Quaternion.FromToRotation(end.position-joint.position,goal-joint.position)*joint.rotation;
    }
    void AnimateArmGrips(Fighter f)
    {
        if(f.gripRig==null||f.weaponVisuals==null||f.deadVisual)return;
        Vector3 aim=f==player&&!spectatorMode&&!inMenu&&Finite(aimDirection)&&Vector3.Dot(aimDirection,f.facing)>.15f?aimDirection.normalized:f.facing;
        Quaternion facing=Quaternion.LookRotation(aim,Vector3.up);
        for(int side=0;side<2;side++)
        {
            ArmGrip arm=f.gripRig[side];if(arm==null)continue;
            {
                arm.elbow.localRotation=arm.elbowRest;float length=Vector3.Distance(arm.shoulder.position,arm.elbow.position)+Vector3.Distance(arm.elbow.position,arm.hand.position);float sign=side==0?-1:1;
                Quaternion handRotation=ReferenceHandRotation(f,side,facing);
                arm.hand.rotation=handRotation;
                Vector3 target=arm.shoulder.position+aim*(length*.55f)-Vector3.up*(length*.56f)-f.root.right*(sign*length*.12f);
                if(arm.gripSocket!=null&&!f.weaponProfiles[side].isShield&&!f.weaponProfiles[side].isUnarmed)
                {
                    Vector3 gripGoal=arm.shoulder.position+aim*(length*.82f+.10f)-Vector3.up*(length*.28f)-f.root.right*(sign*length*.05f);
                    target=gripGoal-(arm.gripSocket.position-arm.hand.position);
                }
                if(side==1&&HasEquippedGuard(f))
                {
                    Vector3 guard=arm.shoulder.position+f.root.forward*(length*.90f)-Vector3.up*(length*.18f)-f.root.right*(length*.18f);
                    target=Vector3.Lerp(target,guard,f.guardBlend);
                }
                if(f.punchTimer>0&&f.punchSide==side)
                {
                    float extension=Mathf.Sin((1-Mathf.Clamp01(f.punchTimer/.32f))*Mathf.PI);
                    target=Vector3.Lerp(target,arm.shoulder.position+aim*(length*.98f)-Vector3.up*(length*.12f),extension);
                }
                SolveTwoBone(arm.shoulder,arm.elbow,arm.hand,target,f.root.right*sign-Vector3.up*.15f);
                arm.hand.rotation=ReferenceHandRotation(f,side,facing);
            }
            if(f.weaponProfiles[side].isShield||f.weaponProfiles[side].isUnarmed)continue;
            Transform weapon=f.weaponVisuals[side];weapon.rotation=facing;
            if(f.modernWeaponArt&&f.weaponGripPoints[side]!=null)weapon.position+=HandGripPosition(arm)-f.weaponGripPoints[side].position;
            else weapon.position=arm.hand.position-weapon.TransformVector(new Vector3(0,-.23f,-.08f));
        }
        for(int i=2;i<4;i++)
        {
            if(!f.modernWeaponArt){f.weaponVisuals[i].rotation=facing;continue;}
            if(WeaponIndex(f)==i&&!f.weaponProfiles[i].isUnarmed)AimShoulderForShot(f,i,aim);
            else PlaceShoulderWeapon(f,i);
        }
        AnimateReferenceHandPoses(f);
        if(HasEquippedGuard(f)&&f.guardShield!=null)f.guardShield.rotation=Quaternion.Slerp(f.guardShield.rotation,f.root.rotation,f.guardBlend);
    }
    void PlaceShoulderWeapon(Fighter f,int slot)
    {
        Transform shoulder=slot==2?f.leftArm:f.rightArm,weapon=f.weaponVisuals[slot];float sign=slot==2?-1:1;
        weapon.rotation=shoulder.rotation*f.shoulderWeaponRest[slot-2];weapon.position=shoulder.position+shoulder.rotation*f.shoulderWeaponOffsets[slot-2];
        if(!f.weaponProfiles[slot].isUnarmed&&f.weaponMuzzlePoints[slot]!=null&&ObstacleOnSegment(f.root.position,f.weaponMuzzlePoints[slot].position,f.weaponProfiles[slot].radius*bulletSizeScale,out _))
        {
            float safeSide=Mathf.Max(.1f,Mathf.Min(.80f,f.Radius-f.weaponProfiles[slot].radius*bulletSizeScale-.06f));
            weapon.position+=f.root.right*(sign*safeSide-Vector3.Dot(weapon.position-f.root.position,f.root.right));
        }
    }
    void AimShoulderForShot(Fighter f,int slot,Vector3 direction)
    {
        if(!f.modernWeaponArt||slot<2||slot>3||f.weaponProfiles[slot].isUnarmed)return;
        int side=slot-2;Transform shoulder=side==0?f.leftArm:f.rightArm;Quaternion facing=Quaternion.LookRotation(direction.normalized,Vector3.up);
        // The mount stays a child of the actual shoulder. Active aim poses that joint;
        // passive mounts inherit arm/guard motion instead of floating at a world angle.
        shoulder.rotation=facing*Quaternion.Inverse(f.shoulderWeaponRest[side]);
        if(f.gripRig!=null&&f.gripRig[side]!=null)
        {
            ArmGrip arm=f.gripRig[side];
            Quaternion carried=arm.gripSocket!=null?Quaternion.LookRotation((f.facing*.15f-Vector3.up).normalized,f.facing):facing;
            arm.hand.rotation=ReferenceHandRotation(f,side,carried);
            if(!f.weaponProfiles[side].isShield&&!f.weaponProfiles[side].isUnarmed)
            {
                Transform gun=f.weaponVisuals[side];gun.rotation=carried;gun.position+=HandGripPosition(arm)-f.weaponGripPoints[side].position;
            }
        }
        PlaceShoulderWeapon(f,slot);
    }
}
