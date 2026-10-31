using System.Collections.Generic;
using UnityEngine;

public sealed partial class VoxArena
{
    enum SalvageKind { Throwable, Club, Shield }
    sealed class Salvage
    {
        public SalvageKind kind;
        public Transform visual;
        public Vector3 spawn;
        public bool held,spent;
        public float flightTime;
        public Vector3 velocity;
    }
    readonly List<Salvage> salvage=new List<Salvage>();
    public float pickupRadius=3.6f, thrownSpeed=27f, thrownDamage=22f;
    public float meleeRange=4.1f, meleeDamage=24f, meleeCooldown=.7f, shieldDurability=75f;
    Salvage heldSalvage;
    float meleeTimer,shieldRemaining;
    bool meleeHitDone;

    void AddSalvage(SalvageKind kind,Vector3 p)
    {
        Color c=kind==SalvageKind.Throwable?new Color(.64f,.57f,.42f):
            kind==SalvageKind.Club?new Color(.53f,.59f,.66f):new Color(.32f,.49f,.55f);
        Vector3 size=kind==SalvageKind.Throwable?new Vector3(.8f,.8f,.8f):
            kind==SalvageKind.Club?new Vector3(.28f,2.7f,.3f):new Vector3(2.1f,2.5f,.22f);
        Transform t=Box("Salvage "+kind,null,p,size,c);
        salvage.Add(new Salvage{kind=kind,visual=t,spawn=p});
    }
    void BuildSalvage()
    {
        AddSalvage(SalvageKind.Throwable,new Vector3(-11,1.6f,-10));
        AddSalvage(SalvageKind.Club,new Vector3(-9,2.4f,-14));
        AddSalvage(SalvageKind.Shield,new Vector3(-18,2.4f,-10));
        AddSalvage(SalvageKind.Throwable,new Vector3(12,1.6f,10));
        AddSalvage(SalvageKind.Club,new Vector3(9,2.4f,14));
        AddSalvage(SalvageKind.Shield,new Vector3(18,2.4f,10));
    }
    void ResetSalvage()
    {
        heldSalvage=null; meleeTimer=0; shieldRemaining=0;
        foreach(Salvage s in salvage)
        {
            s.held=false; s.spent=false; s.flightTime=0; s.velocity=Vector3.zero;
            s.visual.SetParent(null,true); s.visual.position=s.spawn;
            s.visual.rotation=Quaternion.identity; s.visual.gameObject.SetActive(true);
        }
    }
    void TryPickupSalvage()
    {
        if(player.weaponSlot!=3 || heldSalvage!=null || player.jumpHeight>.2f) return;
        Salvage nearest=null; float best=pickupRadius*pickupRadius;
        foreach(Salvage s in salvage)
        {
            if(s.held || s.spent || s.flightTime>0) continue;
            float d=(s.visual.position-player.root.position).sqrMagnitude;
            if(d<best) {best=d;nearest=s;}
        }
        if(nearest==null) return;
        heldSalvage=nearest; nearest.held=true; shieldRemaining=shieldDurability;
        nearest.visual.SetParent(player.rightArm,false);
        nearest.visual.localPosition=new Vector3(0,-.8f,1f);
        nearest.visual.localRotation=Quaternion.identity;
    }
    void UseSalvage(Vector3 aim)
    {
        if(player.weaponSlot!=3 || meleeTimer>0 || player.knockdownTime>0) return;
        meleeTimer=meleeCooldown;
        if(heldSalvage!=null && heldSalvage.kind==SalvageKind.Throwable)
        {
            Salvage thrown=heldSalvage; heldSalvage=null; thrown.held=false;
            thrown.visual.SetParent(null,true);
            Vector3 dir=aim.sqrMagnitude>.001f?aim.normalized:player.facing;
            thrown.visual.position=player.root.position+Vector3.up*.9f+dir*1.5f;
            thrown.velocity=dir*thrownSpeed+Vector3.up*3f; thrown.flightTime=3f;
            return;
        }
        if(heldSalvage!=null && heldSalvage.kind==SalvageKind.Shield) return;
        float damage=heldSalvage!=null?meleeDamage*1.6f:meleeDamage;
        Vector3 to=enemy.root.position-player.root.position;
        if(to.magnitude<=meleeRange+enemy.Radius && Vector3.Dot(player.facing,to.normalized)>.2f &&
            !ObstacleOnSegment(player.root.position+Vector3.up,enemy.root.position+Vector3.up,.1f,out _))
        {
            Hit(enemy,player.root.position,damage);
            Flash(enemy.root.position,TeamColor(player),.5f,.15f);
        }
    }
    void UpdateSalvage(float dt)
    {
        meleeTimer=Mathf.Max(0,meleeTimer-dt);
        foreach(Salvage s in salvage)
        {
            if(s.flightTime<=0 || s.spent) continue;
            s.flightTime-=dt;
            Vector3 before=s.visual.position;
            s.velocity+=Vector3.down*17f*dt;
            Vector3 next=before+s.velocity*dt;
            bool blocked=ObstacleOnSegment(before,next,.15f,out _);
            bool hit=(next-enemy.root.position-Vector3.up*.8f).magnitude<enemy.Radius+1f;
            if(hit && !blocked) Hit(enemy,before,thrownDamage);
            if(hit || blocked || next.y<1.4f || s.flightTime<=0)
            {
                s.spent=true; s.visual.gameObject.SetActive(false);
            }
            else {s.visual.position=next; s.visual.Rotate(180f*dt,100f*dt,0);}
        }
    }
    bool AbsorbSalvageShield(Fighter target,float damage)
    {
        if(target!=player || heldSalvage==null || heldSalvage.kind!=SalvageKind.Shield ||
            player.weaponSlot!=3 || !Input.GetMouseButton(1)) return false;
        shieldRemaining-=damage;
        if(shieldRemaining<=0)
        {
            heldSalvage.spent=true; heldSalvage.visual.gameObject.SetActive(false); heldSalvage=null;
        }
        return true;
    }
}