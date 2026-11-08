using System;
using UnityEngine;
public sealed partial class VoxArena
{
    [Header("PD combat trial settings — not final balance")]
    [Range(.85f,1f)] public float normalTravelScale=.95f;
    [Range(0,2)] public float weakAssistDegrees=1.0f;
    sealed class WeaponState { public int ammo; public float reload,idle,cooldown,readyFlash; }
    sealed class WeaponProfile
    {
        public bool isShield,isBeamShield,isUnarmed,blocksShoulderWeapons,shoulderBlocked,allowsShoulderDuplicate=true;public float partialReload=2.5f,emptyReload=4f,idleReload=2.2f,spread,blast,weight,ammoWeight;
        public string trait="DAMAGE";public string[] additionalTraits;public float lockStrength,lockPenalty,guardReduction;
        public string name,mount;public int capacity;public float damage,interval,speed,radius,range;
        public WeaponProfile(string n,string m,int c,float d,float i,float s,float r,float distance){name=n;mount=m;blast=n=="ROCKET"?2:0;capacity=c;damage=d;interval=i;speed=s;radius=r;range=distance;}
    }
    static readonly WeaponProfile[] Loadout={
        new WeaponProfile("RIFLE","LEFT HAND",30,14*1.15f,.18f/.65f,264,.24f,82),
        new WeaponProfile("CANNON","RIGHT HAND",8,48*1.15f,.65f/.65f,220,.29f,92),
        new WeaponProfile("ROCKET","LEFT SHOULDER",12,26*1.15f,.42f/.65f,200,.28f,100),
        new WeaponProfile("RAIL","RIGHT SHOULDER",4,80*1.15f,1.1f/.65f,360,.20f,130)
    };
    static WeaponProfile[] NewWeaponProfiles()
    {
        WeaponProfile[] profiles=new WeaponProfile[Loadout.Length];for(int i=0;i<profiles.Length;i++){WeaponProfile p=Loadout[i];profiles[i]=new WeaponProfile(p.name,p.mount,p.capacity,p.damage,p.interval,p.speed,p.radius,p.range){weight=p.weight,ammoWeight=p.ammoWeight};}return profiles;
    }
    WeaponProfile EquippedProfile(Fighter f)=>f.weaponProfiles[WeaponIndex(f)];
    static WeaponState[] NewWeaponStates(WeaponProfile[] profiles=null)
    {
        WeaponState[] result=new WeaponState[4];for(int i=0;i<4;i++)result[i]=new WeaponState{ammo=(profiles??Loadout)[i].capacity};return result;
    }
    int WeaponIndex(Fighter f)=>Mathf.Clamp(f.weaponSlot-1,0,3);
    static bool HasWeaponTrait(WeaponProfile profile,string id)=>profile!=null&&(profile.trait==id||Array.IndexOf(profile.additionalTraits??Array.Empty<string>(),id)>=0);
    float ShieldMovementMultiplier(Fighter f)
    {
        if(!HasEquippedGuard(f))return 1f;
        float multiplier=1f;
        foreach(WeaponProfile profile in f.weaponProfiles)
        {
            if(!profile.isShield)continue;
            // Carrying a physical shield has a noticeable load; active guard costs
            // more mobility. Beam shields stay light in both poses.
            float weight=Mathf.Max(0,profile.weight);
            float item=profile.isBeamShield
                ?(f.guarding?Mathf.Clamp(.94f-weight*.001f,.88f,.94f):Mathf.Clamp(.985f-weight*.0005f,.96f,.985f))
                :(f.guarding?Mathf.Clamp(.70f-weight*.0015f,.52f,.70f):Mathf.Clamp(.84f-weight*.002f,.66f,.84f));
            multiplier=Mathf.Min(multiplier,item);
        }
        return multiplier;
    }
    float GuardReduction(Fighter f,Vector3 source)
    {
        if(!f.guarding||!HasEquippedGuard(f))return 0;
        Vector3 incoming=Vector3.ProjectOnPlane(source-f.root.position,Vector3.up);
        if(Vector3.Dot(incoming,f.facing)<=0)return 0;
        float reduction=0;foreach(WeaponProfile profile in f.weaponProfiles)if(profile.isShield)reduction=Mathf.Max(reduction,profile.guardReduction*(profile.isBeamShield?.5f:1f));
        return Mathf.Clamp01(reduction);
    }
    void SelectWeaponOrdinal(Fighter f,int ordinal)
    {
        for(int i=0,count=0;i<4;i++)if(!f.weaponProfiles[i].isShield&&!f.weaponProfiles[i].shoulderBlocked){if(count++==ordinal){f.weaponSlot=i+1;return;}}
    }
    float ReloadSeconds(Fighter f)=>f.magazine<=0?EquippedProfile(f).emptyReload:EquippedProfile(f).partialReload;
    bool StartWeaponReload(Fighter f)
    {
        if(EquippedProfile(f).isShield||EquippedProfile(f).isUnarmed||f.reloadTime>0||f.magazine>=EquippedProfile(f).capacity)return false;
        f.reloadTime=ReloadSeconds(f);return true;
    }
    void TickWeapons(Fighter f,float dt)
    {
        if(EquippedProfile(f).isShield)SelectWeaponOrdinal(f,0);
        f.punchTimer=Mathf.Max(0,f.punchTimer-dt);
        if(Vector3.ProjectOnPlane(f.velocity,Vector3.up).magnitude<1&&f.jumpHeight<.02f)f.aimSettled+=dt;else f.aimSettled=0;
        for(int i=0;i<4;i++)
        {
            if(f.weaponProfiles[i].isShield||f.weaponProfiles[i].isUnarmed)continue;
            WeaponState w=f.weapons[i];w.cooldown=Mathf.Max(0,w.cooldown-dt);w.readyFlash=Mathf.Max(0,w.readyFlash-dt);w.idle+=dt;
            if(w.reload>0){w.reload=Mathf.Max(0,w.reload-dt);if(w.reload==0){w.ammo=f.weaponProfiles[i].capacity;w.readyFlash=.8f;w.idle=0;}}
            else if(!smoke&&w.ammo<f.weaponProfiles[i].capacity&&w.idle>=f.weaponProfiles[i].idleReload)w.reload=w.ammo<=0?f.weaponProfiles[i].emptyReload:f.weaponProfiles[i].partialReload;
        }
    }
    void FireEquippedWeapon(Fighter f,Fighter target,Vector3 direction)
    {
        if(EquippedProfile(f).isShield||EquippedProfile(f).shoulderBlocked||Finished||f.lockTime>0||f.rushTime>0||f.reloadTime>0||f.magazine<=0||f.weaponTimer>0||!InArc(f,direction))return;
        WeaponProfile p=EquippedProfile(f);
        if(p.isUnarmed){if(WeaponIndex(f)<2)Punch(f,target);return;}
        if(WeaponIndex(f)>=2)AimShoulderForShot(f,WeaponIndex(f),direction);
        f.weaponTimer=p.interval*weaponCooldownScale/Mathf.Max(.2f,f.weaponSlot%2==1?f.spec.leftArmHandling:f.spec.rightArmHandling);
        f.magazine--;f.shotIdleTime=0;
        Vector3 fired=AssistAim(f,target,direction);
        float spread=(f.spec.aimSpreadDegrees+p.spread)/Mathf.Max(.2f,f.weaponSlot%2==1?f.spec.leftArmStability:f.spec.rightArmStability);
        fired=Quaternion.Euler(0,UnityEngine.Random.Range(-spread,spread),0)*fired;
        Shot shot=Fire(f,fired,p.damage,p.speed,p.radius,p.range,HasWeaponTrait(p,"LOCK")&&(f!=player||targetLock)?p.lockStrength*p.range/Mathf.Max(1,p.speed):0,p.blast,loadoutVisual:true);
        if(shot!=null)shot.visual.localScale*=1.15f; // Cosmetic size is independent of collision radius.
        PlaySound(SoundKind.Fire,f.root.position,.32f,f.weaponSlot==1?1.25f:.85f);
        if(f.magazine<=0)StartWeaponReload(f);
    }
    void ChooseAIWeapon(Fighter f,float distance)
    {
        int preferred=distance>65?3:distance>35?2:distance<18?0:1;
        // Commit briefly; switching every frame would remove readable attack pauses.
        if(f.weaponProfiles[preferred].isShield||f.weaponProfiles[preferred].shoulderBlocked||f.weaponProfiles[preferred].isUnarmed&&preferred>=2)preferred=0;
        if(f.weaponTimer>0)return;
        if(f.reloadTime>0||f.magazine<=0)
        {
            if(f.aiWeaponReadyAt<=0){f.aiWeaponReadyAt=f.aiClock+(difficulty==0?1f:difficulty==1?.7f:.4f);return;}
            if(f.aiClock<f.aiWeaponReadyAt)return;
        }

        if(!EquippedProfile(f).isShield&&(!EquippedProfile(f).isUnarmed||WeaponIndex(f)<2)&&f.ActiveWeapon.reload<=0&&f.magazine>0&&Mathf.FloorToInt(f.aiClock*2)%5!=0)return;
        for(int j=0;j<4;j++){int index=(preferred+j)%4;if(!f.weaponProfiles[index].isShield&&!f.weaponProfiles[index].shoulderBlocked&&(!f.weaponProfiles[index].isUnarmed||index<2)&&f.weapons[index].reload<=0&&f.weapons[index].ammo>0){f.weaponSlot=index+1;f.aiWeaponReadyAt=0;return;}}
    }
    void DrawWeaponLoadout()
    {
        if(inMenu)return;
        Panel(new Rect(16,526,226,178),new Color(.025f,.045f,.075f,.78f));
        for(int i=0;i<4;i++)
        {
            WeaponState w=player.weapons[i];WeaponProfile p=player.weaponProfiles[i];
            float x=22,y=531+i*42;bool active=player.weaponSlot==i+1;bool unavailable=p.shoulderBlocked;
            float fraction=p.isShield?0f:p.isUnarmed?1f:p.capacity>0?Mathf.Clamp01((float)w.ammo/p.capacity):0f;
            Panel(new Rect(x,y,214,38),active?new Color(.035f,.23f,.29f,.98f):new Color(.035f,.065f,.09f,.86f));
            if(active)
            {
                Panel(new Rect(x,y,4,38),Cyan);
                Panel(new Rect(x+4,y,210,1),new Color(.35f,.96f,1f,.8f));
            }
            Color numberColor=active?Color.white:unavailable?new Color(.38f,.46f,.50f):new Color(.68f,.77f,.82f);GUI.color=numberColor;Text(x+8,y+8,25,(i+1).ToString(),label);GUI.color=Color.white;
            Panel(new Rect(x+39,y+12,103,8),new Color(.17f,.23f,.27f));
            if(!unavailable&&!p.isShield)Panel(new Rect(x+39,y+12,103*fraction,8),w.reload>0?Orange:active?Cyan:new Color(.44f,.65f,.68f));
            string state=unavailable?"LOCKED":p.isShield?"GUARD":p.isUnarmed?"MELEE":w.reload>0?"RLD "+w.reload.ToString("F1")+"s":w.ammo+"/"+p.capacity;
            GUI.color=unavailable?new Color(.48f,.55f,.58f):p.isShield?new Color(.63f,.82f,1f):w.reload>0?Orange:active?Color.white:new Color(.70f,.78f,.81f);Text(x+148,y+8,62,state,small);GUI.color=Color.white;
        }
    }
    void CombatRevisionChecks(Action<bool,string> check)
    {
        bool prior=smoke;smoke=false;TestSetupSelectable(0,1);player.root.position=new Vector3(100,FighterGroundY,100);enemy.root.position=player.root.position+Vector3.forward*30;player.facing=Vector3.forward;
        for(int i=0;i<4;i++)
        {
            player.weaponSlot=i+1;int count=shots.Count,ammo=player.magazine;FireEquippedWeapon(player,enemy,Vector3.forward);
            check(player.magazine==ammo-1&&shots.Count==count+1,"four-slot live shot spends independent ammo "+i);
            player.weaponTimer=0;player.magazine=1;FireEquippedWeapon(player,enemy,Vector3.forward);
            check(player.magazine==0&&Mathf.Abs(player.reloadTime-4f)<.01f,"four-slot empty reload uses 4 seconds "+i);
        }
        player.weaponSlot=1;TickWeapons(player,2.6f);check(player.weapons[0].ammo==0&&player.weapons[3].ammo==0,"all empty weapons remain unavailable before 4 seconds");
        TickWeapons(player,1.5f);check(player.weapons[0].ammo==30&&player.weapons[1].ammo==8&&player.weapons[2].ammo==12&&player.weapons[3].ammo==4,"inactive slots reload independently to their capacities");
        player.magazine=15;StartWeaponReload(player);check(Mathf.Abs(player.reloadTime-2.5f)<.01f,"partial reload uses 2.5 seconds");int n=shots.Count;FireEquippedWeapon(player,enemy,Vector3.forward);check(shots.Count==n,"reloading weapon cannot fire");
        player.weaponSlot=2;FireEquippedWeapon(player,enemy,Vector3.forward);check(shots.Count==n+1&&player.weapons[0].reload>0,"another weapon can fire while first weapon reloads");
        float health=player.hp;player.hp=1;float d=Loadout[0].damage;check(Mathf.Abs(d-16.1f)<.001f&&Loadout[0].interval>.27f,"trial weapon damage/frequency are explicit and do not depend on low HP");player.hp=health;
        smoke=prior;TestSetup();
    }
}
