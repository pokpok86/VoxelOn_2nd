using System;
using System.Linq;
using UnityEngine;

public sealed partial class VoxArena
{
    bool gunSecondaryHeld;
    static bool RetiredShieldId(string id)=>id=="WISP_GuardShield"||id=="Future_Shield"||id=="Future_HoloShield"||id=="Future_Drone";
    static bool HandWeaponAvailable(BalanceWeapon w)=>w!=null&&!RetiredShieldId(w.id)&&(w.kind=="firearm"||w.kind=="unarmed");
    static string GunSecondaryAction(BalanceWeapon w)
    {
        if(w.kind!="firearm")return w.secondaryActionId;
        if(w.id=="Military_RL"&&(w.secondaryActionId=="NONE"||w.secondaryActionId=="LOCK_ON"))return "BRACE";
        if(w.secondaryActionId!="NONE")return w.secondaryActionId;
        if(w.id=="Future_Launcher")return "LOCK_ON";
        return "FOCUS";
    }
    bool GunBracing(Fighter f)=>f==player&&!AuxSelected(f)&&gunSecondaryHeld&&EquippedProfile(f).secondaryActionId=="BRACE"&&f.jumpHeight<=.02f&&f.jumpVelocity<=0;
    bool GunSteady(Fighter f)=>f==player&&!AuxSelected(f)&&gunSecondaryHeld&&(EquippedProfile(f).secondaryActionId=="FOCUS"||GunBracing(f))&&f.jumpHeight<=.02f&&Vector3.ProjectOnPlane(f.velocity,Vector3.up).magnitude<.5f;
    string GunActionHint(Fighter f)
    {
        if(AuxSelected(f))return AuxLockAvailable(f)?"RMB 호밍 포드 락온":"펄스 빔건 · 수동 조준";
        string action=EquippedProfile(f).secondaryActionId;
        return action=="BRACE"?"RMB 고정 포격 · 지상 정지 / 반동 감소":action=="FOCUS"?"RMB 정밀 사격 · 정지 시 탄퍼짐 감소":action=="LOCK_ON"?"RMB 락온":"RMB 특수 동작 없음";
    }
    void ApplyGunRecoil(Fighter f,WeaponProfile p,Vector3 fired,bool stable)
    {
        if(f==player&&p.recoil>0)
        {
            cameraShakeTime=Mathf.Max(cameraShakeTime,.3f);
            cameraShakeStrength=Mathf.Max(cameraShakeStrength,Mathf.Clamp(p.recoil,0,100)*.0012f*(stable&&p.secondaryActionId=="BRACE"?.3f:1f));
        }
        if(p.secondaryActionId=="BRACE"&&!stable)f.velocity-=Vector3.ProjectOnPlane(fired,Vector3.up).normalized*1.5f;
    }
    BalanceWeapon[] CurrentHandGuns()=>balanceDraft.weapons.Where(HandWeaponAvailable).ToArray();
    void SelectHandGun(int hand,int direction)
    {
        BalanceDesign design=EditingDesign();if(design==null||design.isEmpty)return;
        BalanceWeapon[] allowed=CurrentHandGuns();int current=Array.FindIndex(allowed,w=>w.id==design.slots[hand]);
        int next=(Mathf.Max(0,current)+direction+allowed.Length)%allowed.Length;
        FamilyConfig(player).weapons[hand]="AUTO";SaveFamilyConfig();balanceMountIndex=hand;
        CycleLoadoutWeapon(design,allowed,next);
        balanceMessage=allowed[next].name+" 장착 · "+GunActionLabel(GunSecondaryAction(allowed[next]));
    }
    static string GunActionLabel(string action)=>action=="BRACE"?"우클릭: 고정 포격":action=="LOCK_ON"?"우클릭: 락온":action=="FOCUS"?"우클릭: 정밀 사격":"특수 동작 없음";
    void DrawHandGunSelector(int hand)
    {
        BalanceDesign design=EditingDesign();BalanceWeapon[] allowed=CurrentHandGuns();if(design==null||design.isEmpty)return;
        BalanceWeapon weapon=allowed.FirstOrDefault(w=>w.id==design.slots[hand]);bool empty=weapon==null||weapon.kind=="unarmed";
        Panel(new Rect(700,186,536,340),new Color(.035f,.055f,.075f));
        if(!empty)GUI.DrawTexture(new Rect(758,190,420,330),FamilyPartPreview(weapon.id),ScaleMode.ScaleToFit,false);
        else GUI.Label(new Rect(758,300,420,70),"미장착 · 주먹 공격",new GUIStyle(GUI.skin.label){fontSize=24,alignment=TextAnchor.MiddleCenter});
        if(GUI.Button(new Rect(704,322,48,66),"◀"))SelectHandGun(hand,-1);
        if(GUI.Button(new Rect(1182,322,48,66),"▶"))SelectHandGun(hand,1);
        GUI.Label(new Rect(708,536,520,36),empty?"미장착":weapon.name,new GUIStyle(GUI.skin.label){fontSize=23,alignment=TextAnchor.MiddleCenter});
        GUI.Label(new Rect(708,576,520,25),empty?"왼쪽 클릭: 주먹":GunActionLabel(GunSecondaryAction(weapon)),new GUIStyle(small){alignment=TextAnchor.MiddleCenter});
        if(!empty&&GUI.Button(new Rect(850,614,230,32),"이 무기 수치 편집")){balanceWeaponIndex=Array.IndexOf(balanceDraft.weapons,weapon);balanceTab=1;BalanceSelectPreview();}
    }
    void SaveGunValidationImage(RenderTexture rt,string name)
    {
        RenderTexture prior=RenderTexture.active;var image=new Texture2D(rt.width,rt.height,TextureFormat.RGB24,false);
        try{RenderTexture.active=rt;image.ReadPixels(new Rect(0,0,rt.width,rt.height),0,0);image.Apply();string folder=System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(),"Docs","Validation","HandGuns-20261118");System.IO.Directory.CreateDirectory(folder);System.IO.File.WriteAllBytes(System.IO.Path.Combine(folder,name+".png"),image.EncodeToPNG());}
        finally{RenderTexture.active=prior;Destroy(image);}
    }
    void HandGunChecks(Action<bool,string> check)
    {
        BalanceDocument saved=activeBalance;bool oldSmoke=smoke,oldMenu=inMenu;FamilyLoadoutFile oldFamily=familyLoadouts;
        try
        {
            activeBalance=BalanceData.Defaults();familyLoadouts=new FamilyLoadoutFile();
            BalanceDesign design=activeBalance.designs[0];activeBalance.activeDesignId=design.id;
            foreach(BalanceWeapon weapon in activeBalance.weapons.Where(w=>w.kind=="firearm"))
            {
                design.slots[0]=weapon.id;design.slots[1]="UNARMED";
                TestSetupSelectable(0,1);smoke=false;inMenu=false;player.weaponSlot=1;player.facing=Vector3.forward;player.weaponTimer=player.reloadTime=0;
                player.magazine=player.weaponProfiles[0].capacity;int ammo=player.magazine;ClearCombat();
                FireEquippedWeapon(player,enemy,Vector3.forward);
                int expected=weapon.id=="Military_SG"||weapon.id=="Future_Shotgun"?7:weapon.id=="Military_FL"?3:1;
                check(player.magazine==ammo-1,"hand gun spends one magazine round "+weapon.id);
                check(authoredCombatEffects.Count(e=>e.kind==0)==1,"one trigger creates one muzzle effect even for spread patterns "+weapon.id);
                check(shots.Count==expected&&beams.Count==0,"actual per-gun firing mode "+weapon.id);
                check(Mathf.Abs(shots.Sum(x=>x.damage)-weapon.Get("damage"))<.001f,"firing pattern preserves configured total damage "+weapon.id);
                check(player.weaponProfiles[0].secondaryActionId!="NONE","gun has explicit secondary action "+weapon.id);
                check(familyRigs[player].weaponIds[0]==weapon.id&&familyRigs[player].muzzles[0]!=null,"AUTO displays actual equipped gun with authored muzzle "+weapon.id);
                RenderTexture model=FamilyPartPreview(weapon.id);check(model!=null&&model.IsCreated(),"actual standalone gun preview "+weapon.id);
                SaveGunValidationImage(model,weapon.id);
                if(weapon.id=="Military_GL"){Vector3 velocity=shots[0].velocity;UpdateCombat(.02f);check(shots.Count>0&&shots[0].velocity.y<velocity.y,"grenade is affected by gravity");}
                if(weapon.id=="Military_FL")check(shots.All(x=>x.life*x.velocity.magnitude<=14.01f),"flame is limited to close range");
            }
            design.slots[0]="Military_RL";TestSetupSelectable(0,1);smoke=false;player.weaponSlot=1;player.jumpHeight=player.jumpVelocity=0;player.velocity=Vector3.zero;gunSecondaryHeld=true;
            check(GunBracing(player)&&GunSteady(player)&&!CanStartLockOn(player),"bazooka right click braces without acquiring lock");
            player.jumpHeight=2;check(!GunBracing(player)&&!GunSteady(player),"airborne bazooka cannot gain grounded brace bonus");
            player.jumpHeight=player.jumpVelocity=0;player.velocity=Vector3.zero;player.reloadTime=player.weaponTimer=0;player.magazine=4;player.facing=Vector3.forward;smoke=false;
            gunSecondaryHeld=false;FireEquippedWeapon(player,enemy,Vector3.forward);float kick=player.velocity.magnitude;
            player.velocity=Vector3.zero;player.weaponTimer=player.reloadTime=0;gunSecondaryHeld=true;FireEquippedWeapon(player,enemy,Vector3.forward);
            check(kick>1&&player.velocity.magnitude<.01f,"grounded bracing removes bazooka physical kickback");
            foreach(string id in new[]{"WISP_GuardShield","Future_Shield","Future_HoloShield"})
            {
                design.slots[0]=id;TestSetupSelectable(0,1);
                check(player.weaponProfiles[0].isUnarmed&&!HasEquippedGuard(player)&&!HandWeaponAvailable(activeBalance.weapons.First(w=>w.id==id)),"retired shield cannot appear or guard "+id);
            }
        }
        finally{gunSecondaryHeld=false;activeBalance=saved;familyLoadouts=oldFamily;smoke=oldSmoke;inMenu=oldMenu;ClearCombat();TestSetupSelectable(0,1);}
    }

}
