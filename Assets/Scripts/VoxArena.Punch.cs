using UnityEngine;
public sealed partial class VoxArena
{
    void Punch(Fighter attacker,Fighter target)
    {
        WeaponProfile p=EquippedProfile(attacker);if(!p.isUnarmed||WeaponIndex(attacker)>=2||attacker.punchTimer>0)return;
        int side=WeaponIndex(attacker);attacker.weaponTimer=p.interval*weaponCooldownScale/Mathf.Max(.2f,side==0?attacker.spec.leftArmHandling:attacker.spec.rightArmHandling);attacker.punchTimer=.32f;attacker.punchSide=side;
        Schedule(attacker,.12f,()=>
        {
            if(attacker.hp<=0||target==null||target.hp<=0||attacker.knockdownTime>0)return;
            Vector3 delta=target.root.position-attacker.root.position;
            if(delta.magnitude>p.range+target.Radius||Mathf.Abs(delta.y)>1.8f||Vector3.Dot(attacker.facing,delta.normalized)<.6f)return;
            Vector3 origin=attacker.gripRig!=null&&attacker.gripRig[side]!=null?attacker.gripRig[side].hand.position:attacker.root.position;
            if(ObstacleOnSegment(origin,target.root.position,p.radius,out _))return;
            Hit(target,attacker.root.position,p.damage,target.corePart.position,attacker);Flash(origin,TeamColor(attacker),.28f,.1f);
        });
    }
}

