using System;
using System.IO;
using System.Linq;
using System.Collections;
using UnityEngine;

public sealed partial class VoxArena
{
    IEnumerator FamilyArtReview()
    {
        if(FamilyJointReviewRequested){yield return FamilyJointReview();yield break;}
        string folder=Path.Combine(Directory.GetCurrentDirectory(),"Docs","Validation","WispFamily-20261112");Directory.CreateDirectory(folder);
        balanceDirectoryOverride=Path.Combine(Path.GetTempPath(),"VoxelOn-WispFamily-Review-"+DateTime.Now.ToString("yyyyMMdd-HHmmss"));
        familyLoadouts=new FamilyLoadoutFile();bool pass=true;int checks=0;
        void Check(bool ok,string text){checks++;pass&=ok;Debug.Log("WISP_FAMILY_CHECK "+(ok?"PASS ":"FAIL ")+text);}
        TestSetupSelectable(0,1);smoke=false;inMenu=true;paused=true;muted=true;spectatorMode=false;
        player.root.position=new Vector3(100,FighterGroundY,100);player.root.rotation=Quaternion.identity;player.facing=Vector3.forward;
        Check(WispFamilyAssets.Available&&familyRigs.ContainsKey(player),"real game fighter uses exported Blender family");
        string[] ids=new[]{"BD01","HD01","W-01_LUMEN","W-02_BREAKER","W-03_RATTLE","W-04_EDGE","W-05_AEGIS"}.Concat(FamilyBackpacks).Concat(FamilyLegs).ToArray();
        foreach(string id in ids){var d=WispFamilyAssets.Data(id);Check(d.groups.Length>0&&d.groups.All(g=>g.positions.Length>0&&g.normals.Length==g.positions.Length&&g.triangles.All(v=>v>=0&&v<g.positions.Length/3)),"valid actual mesh buffers "+id);}
        FamilyLoadout c=FamilyConfig(player);c.enabled=true;
        Camera study=new GameObject("Wisp family runtime review camera").AddComponent<Camera>();study.enabled=false;study.orthographic=true;study.orthographicSize=2.1f;study.cullingMask=1<<9;study.clearFlags=CameraClearFlags.SolidColor;study.backgroundColor=new Color(.055f,.07f,.095f);
        RenderTexture rt=new RenderTexture(720,960,24);study.targetTexture=rt;Texture2D png=new Texture2D(720,960,TextureFormat.RGB24,false);
        void Capture(string name,bool rear=false)
        {
            FamilyRig rig=familyRigs[player];foreach(Transform t in rig.root.GetComponentsInChildren<Transform>(true))t.gameObject.layer=9;
            Bounds b=new Bounds(rig.root.position,Vector3.zero);bool first=true;foreach(Renderer r in rig.root.GetComponentsInChildren<Renderer>())if(r.enabled){if(first){b=r.bounds;first=false;}else b.Encapsulate(r.bounds);}
            study.orthographicSize=Mathf.Max(1.8f,b.size.y*.59f,b.size.x*.76f);study.transform.position=b.center+new Vector3(rear?-5:5,2,rear?-8:8);study.transform.LookAt(b.center);study.Render();
            RenderTexture.active=rt;png.ReadPixels(new Rect(0,0,720,960),0,0);png.Apply();File.WriteAllBytes(Path.Combine(folder,name+".png"),png.EncodeToPNG());RenderTexture.active=null;
            foreach(Transform t in rig.root.GetComponentsInChildren<Transform>(true))t.gameObject.layer=0;
        }
        for(int i=0;i<FamilyLegs.Length;i++)
        {
            c.legs=FamilyLegs[i];c.backpack=FamilyBackpacks[i];BuildRobot(player);UpdateFamilyArt(player);yield return null;
            var rig=familyRigs[player];Check(rig.legs.data.id==c.legs&&rig.backpack.data.id==c.backpack,"mounted actual lower/backpack "+i);
            float connection=(rig.upper.position-rig.legs.root.TransformPoint(Vector3.up*rig.legs.data.waistHeight)).magnitude;Check(connection<.001f,"waist connection continuous "+i);
            Capture("0"+(i+1)+"-"+c.legs);Capture("0"+(i+1)+"-Back",true);
        }
        c.legs=FamilyLegs[0];c.backpack=FamilyBackpacks[4];BuildRobot(player);UpdateFamilyArt(player);Capture("05-RAMJET",true);
        for(int i=1;i<FamilyWeapons.Length;i++)
        {
            c.weapons[0]=FamilyWeapons[i];BuildRobot(player);UpdateFamilyArt(player);var rig=familyRigs[player];
            Check(rig.weaponIds[0]==FamilyWeapons[i]||player.weaponProfiles[0].isUnarmed,"weapon model selection "+i);
            Capture("Weapon-"+FamilyWeapons[i]);
        }
        c.weapons[0]="AUTO";var profile=player.weaponProfiles[0];bool empty=profile.isUnarmed;profile.isUnarmed=true;BuildRobot(player);Check(familyRigs[player].weapons[0]==null,"removing weapon removes new visible mesh");profile.isUnarmed=empty;
        bool shield=profile.isShield;profile.isShield=true;BuildRobot(player);Check(familyRigs[player].weaponIds[0]=="W-05_AEGIS","shield maps to shield model");player.guardBlend=0;UpdateFamilyArt(player);Quaternion lowered=familyRigs[player].weapons[0].root.rotation;player.guardBlend=1;UpdateFamilyArt(player);Check(Quaternion.Angle(lowered,familyRigs[player].weapons[0].root.rotation)>60,"guard rotates actual shield");profile.isShield=shield;player.guardBlend=0;
        c.legs=FamilyLegs[0];c.backpack=FamilyBackpacks[0];BuildRobot(player);inMenu=false;aimDirection=Vector3.forward;UpdateFamilyArt(player);
        var shoulderRig=familyRigs[player];
        Check(shoulderRig.weapons[2]==null&&shoulderRig.weapons[3]==null&&shoulderRig.muzzles[2]==null&&shoulderRig.muzzles[3]==null,"both pending backpack weapons and muzzles are absent");
        for(int slot=2;slot<4;slot++)
        {
            Check(player.weaponProfiles[slot].isUnarmed&&player.weaponProfiles[slot].shoulderBlocked&&player.weaponArtIds[slot]=="UNARMED","pending shoulder is effectively empty "+slot);
            player.weaponSlot=slot+1;int before=shots.Count;FireEquippedWeapon(player,enemy,Vector3.forward);Check(shots.Count==before,"empty shoulder cannot fire "+slot);
        }
        player.weaponSlot=1;SelectWeaponOrdinal(player,2);Check(player.weaponSlot<=2,"numeric selection skips both pending shoulder slots");player.weaponSlot=1;
        Vector3 muzzle;Check(TryFamilyMuzzle(player,out muzzle)&&Finite(muzzle)&&Vector3.Distance(WeaponMuzzle(player),muzzle)<.001f,"shots use authored actual muzzle");
        player.velocity=Vector3.forward*player.spec.speed;player.jumpHeight=2;player.jumpVelocity=0;roundTime=.3f;UpdateFamilyArt(player);Quaternion airPose=familyRigs[player].legs.nodes["L_Hip"].localRotation;roundTime=.9f;UpdateFamilyArt(player);Check(Quaternion.Angle(airPose,familyRigs[player].legs.nodes["L_Hip"].localRotation)<.01f,"biped stops stride while airborne");player.jumpHeight=0;player.velocity=Vector3.zero;
        SaveFamilyConfig();Check(File.Exists(FamilyLoadoutPath)&&JsonUtility.FromJson<FamilyLoadoutFile>(File.ReadAllText(FamilyLoadoutPath)).designs.Count>0,"independent appearance settings save and deserialize in isolated folder");
        enemy.root.position=player.root.position+new Vector3(0,0,25);paused=false;inMenu=false;cameraYaw=180;cameraPitch=14;UpdateCamera(10);
        for(int i=0;i<8;i++)yield return null;yield return new WaitForEndOfFrame();Texture2D screen=ScreenCapture.CaptureScreenshotAsTexture();File.WriteAllBytes(Path.Combine(folder,"Gameplay.png"),screen.EncodeToPNG());Destroy(screen);
        inMenu=true;OpenBalanceEditor();balanceTab=6;BalanceSelectPreview();for(int i=0;i<8;i++)yield return null;yield return new WaitForEndOfFrame();screen=ScreenCapture.CaptureScreenshotAsTexture();File.WriteAllBytes(Path.Combine(folder,"Customization.png"),screen.EncodeToPNG());Destroy(screen);
        rt.Release();Destroy(rt);Destroy(png);Destroy(study.gameObject);
        File.WriteAllText(Path.Combine(folder,"Result.txt"),(pass?"PASS":"FAIL")+" / "+checks+" actual runtime checks\n"+DateTime.Now.ToString("O"));
        Debug.Log((pass?"WISP_FAMILY_REVIEW_PASS ":"WISP_FAMILY_REVIEW_FAIL ")+checks+" checks");Application.Quit(pass?0:2);
    }
}
