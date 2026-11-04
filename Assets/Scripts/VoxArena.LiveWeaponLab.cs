using System;
using System.Collections;
using System.IO;
using UnityEngine;
public sealed partial class VoxArena
{
    void DrawLiveWeaponLab(Fighter f)
    {
        for(int i=0;i<4;i++)if(GUI.Button(new Rect(199+i*176,313,169,25),(f.weaponSlot==i+1?"[":"")+(i+1)+" "+f.weaponProfiles[i].name+(f.weaponSlot==i+1?"]":"")))f.weaponSlot=i+1;
        if(GUI.Button(new Rect(904,313,170,25),"RESET THIS WEAPON")){int i=WeaponIndex(f);WeaponProfile p=Loadout[i];f.weaponProfiles[i]=new WeaponProfile(p.name,p.mount,p.capacity,p.damage,p.interval,p.speed,p.radius,p.range);}
        WeaponProfile active=EquippedProfile(f);float y=351;
        LiveWeaponSlider("DAMAGE",ref active.damage,1,100,ref y);LiveWeaponSlider("FIRE INTERVAL (s)",ref active.interval,.1f,4,ref y);
        LiveWeaponSlider("PROJECTILE SPEED",ref active.speed,40,260,ref y);LiveWeaponSlider("PROJECTILE RADIUS",ref active.radius,.05f,.6f,ref y);LiveWeaponSlider("RANGE",ref active.range,20,200,ref y);
    }
    void LiveWeaponSlider(string name,ref float value,float min,float max,ref float y)
    {
        GUI.Label(new Rect(209,y,350,24),name+"  "+value.ToString("0.00"));value=GUI.HorizontalSlider(new Rect(555,y+6,480,22),value,min,max);y+=40;
    }
    void LiveWeaponLabChecks(Action<bool,string> check)
    {
        bool prior=smoke;TestSetupSelectable(0,1);smoke=false;player.root.position=new Vector3(100,FighterGroundY,100);enemy.root.position=player.root.position+Vector3.forward*30;player.facing=Vector3.forward;
        for(int i=0;i<4;i++)
        {
            player.weaponSlot=i+1;WeaponProfile p=EquippedProfile(player);float rivalDamage=enemy.weaponProfiles[i].damage;
            p.damage=11+i;p.interval=.5f+i*.1f;p.speed=150+i*10;p.radius=.15f+i*.02f;p.range=55+i*5;player.weaponTimer=player.reloadTime=0;player.magazine=p.capacity;int n=shots.Count;
            FireEquippedWeapon(player,enemy,Vector3.forward);Shot actual=shots.Count>n?shots[shots.Count-1]:null;
            check(actual!=null&&Mathf.Abs(actual.damage-p.damage)<.001f&&Mathf.Abs(actual.velocity.magnitude-p.speed*bulletSpeedScale)<.01f,"runtime profile changes actual damage and velocity slot "+i);
            check(actual!=null&&Mathf.Abs(actual.radius-p.radius*bulletSizeScale)<.001f&&Mathf.Abs(actual.life-p.range/(p.speed*bulletSpeedScale))<.001f&&Mathf.Abs(player.weaponTimer-p.interval*weaponCooldownScale/Mathf.Max(.2f,player.spec.rightArmHandling))<.001f,"runtime profile changes actual radius range and cooldown slot "+i);
            check(actual!=null&&AuthoredProjectileFits(actual),"authored projectile keeps imported forward axis and transverse physical envelope slot "+i);
            check(enemy.weaponProfiles[i].damage==rivalDamage&&Loadout[i].damage==rivalDamage,"runtime weapon edit leaves rival and saved trial profile unchanged slot "+i);
        }
        player.weaponProfiles=NewWeaponProfiles();check(Mathf.Abs(player.weaponProfiles[0].damage-8.05f)<.001f&&player.weaponProfiles[1].damage!=12,"runtime reset restores four default profiles");
        player.root.position=new Vector3(120,FighterGroundY,-120);player.facing=Vector3.forward;player.velocity=Vector3.zero;player.boostHeld=false;player.jumpHeight=player.jumpVelocity=0;player.jumpHeld=false;player.spec.leftSpeedFactor=1;Vector3 start=player.root.position;Move(player,Vector3.left,.05f);float original=Vector3.Distance(start,player.root.position);
        player.root.position=start;player.velocity=Vector3.zero;player.spec.leftSpeedFactor=.5f;Move(player,Vector3.left,.05f);float adjusted=Vector3.Distance(start,player.root.position);
        check(original>.1f&&Mathf.Abs(adjusted-original*.5f)<.01f,"live left movement factor changes actual travel without altering right factor");
        smoke=prior;TestSetup();
    }
    IEnumerator CaptureLiveWeaponLab()
    {
        string dir=Path.Combine(Directory.GetCurrentDirectory(),"Docs","Screenshots","PD14-Live-Weapons");Directory.CreateDirectory(dir);TestSetupSelectable(0,1);smoke=false;inMenu=false;muted=true;testLabOpen=true;testCategory=5;testPickupTools=false;
        for(int side=0;side<2;side++)
        {
            testSide=side;TestFighter.weaponSlot=side+1;for(int frame=0;frame<4;frame++)yield return null;yield return new WaitForEndOfFrame();Texture2D image=ScreenCapture.CaptureScreenshotAsTexture();File.WriteAllBytes(Path.Combine(dir,side==0?"pilot-rifle.png":"rival-cannon.png"),image.EncodeToPNG());Destroy(image);
        }
        testSide=0;testCategory=4;testScroll=new Vector2(0,159);float equip=player.spec.shieldEquipMovePenalty,guard=player.spec.shieldGuardMovePenalty;
        for(int f=0;f<4;f++)yield return null;yield return new WaitForEndOfFrame();Texture2D legs=ScreenCapture.CaptureScreenshotAsTexture();File.WriteAllBytes(Path.Combine(dir,"pilot-legs-no-silent-clamp.png"),legs.EncodeToPNG());Destroy(legs);
        bool unchanged=player.spec.shieldEquipMovePenalty==equip&&player.spec.shieldGuardMovePenalty==guard;
        Debug.Log(unchanged?"LIVE_WEAPON_LAB_CAPTURE_PASS 3 actual images / opening legs preserves shield penalties":"LIVE_WEAPON_LAB_CAPTURE_FAIL silent slider clamp");Application.Quit(unchanged?0:2);
    }
}
