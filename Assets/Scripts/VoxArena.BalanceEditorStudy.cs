using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
public sealed partial class VoxArena
{
    IEnumerator BalanceEditorStudy()
    {
        string folder=Path.Combine(Directory.GetCurrentDirectory(),"Docs","Screenshots","Balance-Editor-20261105","Final");Directory.CreateDirectory(folder);
        string disk=Path.Combine(Directory.GetCurrentDirectory(),"Logs","Balance-Editor-Test-"+DateTime.UtcNow.ToString("yyyyMMdd-HHmmss"));Directory.CreateDirectory(disk);balanceDirectoryOverride=disk;
        int checks=0,captures=0;bool pass=true;void Check(bool ok,string name){checks++;pass&=ok;Debug.Log("BALANCE_CHECK "+name+" "+(ok?"PASS":"FAIL"));}
        void Reject(Action action,string label){bool rejected=false;try{action();}catch(Exception){rejected=true;}Check(rejected,label);}
        BalanceDocument defaults=BalanceData.Defaults();Check(defaults.weapons.Length==34&&defaults.parts.Length==18&&defaults.frames.Length==3,"34 equipment entries, 18 parts, 3 loadouts");
        BalanceDocument draft=BalanceData.ValidateAndNormalize(defaults);string path=Path.Combine(disk,"roundtrip.json");BalanceFiles.Save(path,draft);Check(File.Exists(path)&&BalanceFiles.Load(path).frames.Length==3,"first save is immediately readable from disk");
        draft.weapons.First(w=>w.id=="Military_AR").Set("damage",11);BalanceFiles.Save(path,draft);Check(BalanceFiles.Load(path).weapons.First(w=>w.id=="Military_AR").Get("damage")==11&&BalanceFiles.Load(path+".bak").weapons.First(w=>w.id=="Military_AR").Get("damage")==8.05f,"overwrite stores changed value and preserves previous file");
        string original=File.ReadAllText(path);BalanceDocument invalid=BalanceData.Copy(draft);invalid.weapons[0].Set("damage",float.NaN);Reject(()=>BalanceFiles.Save(path,invalid),"NaN save rejected");Check(File.ReadAllText(path)==original,"failed save leaves existing file intact");
        invalid=BalanceData.Copy(draft);invalid.weapons[0].Set("capacity",2.5f);Reject(()=>BalanceData.ValidateAndNormalize(invalid),"fractional magazine rejected");
        invalid=BalanceData.Copy(draft);invalid.formatVersion=99;Reject(()=>BalanceData.ValidateAndNormalize(invalid),"future format rejected without conversion");
        invalid=BalanceData.Copy(draft);invalid.weapons[0].id=invalid.weapons[1].id;Reject(()=>BalanceData.ValidateAndNormalize(invalid),"duplicate equipment IDs rejected");
        invalid=BalanceData.Copy(draft);invalid.weapons[0].numbers[0].key="unknown";Reject(()=>BalanceData.ValidateAndNormalize(invalid),"unknown numeric keys rejected");
        invalid=BalanceData.Copy(draft);invalid.frames[0].slots[0]="Missing";Reject(()=>BalanceData.ValidateAndNormalize(invalid),"missing equipment rejected");
        invalid=BalanceData.Copy(draft);invalid.frames.First(f=>f.id=="BASTION").slots[1]="WISP_GuardShield";Reject(()=>BalanceData.ValidateAndNormalize(invalid),"WISP guard cannot silently equip on another rig");
        invalid=BalanceData.Copy(draft);invalid.weapons.First(w=>w.id=="Military_RL").shoulders=false;Reject(()=>BalanceData.ValidateAndNormalize(invalid),"incompatible shoulder assignment rejected");
        invalid=BalanceData.Copy(draft);invalid.weapons[0].Set("range",1000);Reject(()=>BalanceData.ValidateAndNormalize(invalid),"out-of-range value rejected");
        OpenBalanceEditor();balanceFileName="../escape";Reject(()=>BalancePath(),"profile filename cannot escape its folder");balanceFileName="PD-Test";
        balanceDraft=BalanceData.Copy(draft);balanceDraft.notes="한글 메모 / round trip";BalanceWeapon rifle=balanceDraft.weapons.First(w=>w.id=="Military_AR");rifle.name="시험 소총";rifle.Set("capacity",7);rifle.Set("interval",.5f);rifle.Set("speed",160);rifle.Set("range",99);rifle.Set("radius",.2f);rifle.Set("partialReload",1.1f);rifle.Set("emptyReload",1.7f);rifle.Set("idleReload",1.3f);rifle.Set("weight",12.5f);
        BalanceFrame frame=balanceDraft.frames.First(f=>f.id=="WISP");frame.slots[0]=frame.slots[1]="Military_AR";
        balanceDraft.parts.First(p=>p.id=="WISP_CORE").numbers.First(n=>n.key=="health").value=777;
        balanceDraft.parts.First(p=>p.id=="WISP_HEAD").numbers.First(n=>n.key=="radarRange").value=123;
        balanceDraft.parts.First(p=>p.id=="WISP_LEFT_ARM").numbers.First(n=>n.key=="leftArmHandling").value=1.25f;
        BalanceSave();balanceDraft.weapons[0].Set("damage",99);BalanceLoad();Check(balanceDraft.weapons.First(w=>w.id=="Military_AR").Get("damage")==11&&balanceDraft.notes.Contains("한글"),"load restores exact numeric data and Korean notes");
        Check(balanceDraft.weapons.First(w=>w.id=="Military_AR").Get("weight")==12.5f,"future weight option is persisted without inventing a movement rule");
        TestSetupSelectable(0,1);smoke=false;inMenu=true;selectedCraft=0;BalanceApply();Check(player.spec.health==777&&player.spec.radarRange==123&&player.hp==777,"applied core and head values reach actual fighter");
        Check(player.weaponProfiles[0].name=="시험 소총","Korean equipment name reaches runtime HUD");
        Check(GalleryEquipmentStatus("Military_AR")=="CURRENT FRAME / EQUIPPED SLOTS 1, 2","gallery lists both actually equipped hand slots");
        Check(GalleryEquipmentStatus("Military_GL")=="ART PREVIEW / NOT EQUIPPED ON CURRENT FRAME","gallery does not label a replaced default weapon as equipped");
        Check(player.weaponProfiles[0].capacity==7&&player.weaponProfiles[1].capacity==7&&player.magazine==7,"same gun independently equipped in both hands with configured capacities");
        Check(player.weaponArtIds[0]=="Military_AR"&&player.weaponArtIds[1]=="Military_AR"&&player.weaponVisuals[1].GetComponentsInChildren<Renderer>().Length>0&&!player.guardShield.gameObject.activeSelf,"right gun replaces visible native shield");
        Check(CraftCatalog.Selectable[0].health!=777&&Loadout[0].damage==8.05f,"catalog presets and original weapon data remain unchanged");
        player.weaponProfiles[0].damage=12;Check(player.weaponProfiles[1].damage==11&&enemy.weaponProfiles[0].damage==11,"slot and fighter profile copies are independent");player.weaponProfiles[0].damage=11;
        inMenu=false;player.root.position=new Vector3(80,FighterGroundY,70);player.facing=Vector3.forward;aimDirection=Vector3.forward;enemy.root.position=player.root.position+Vector3.forward*20;Animate(player);int before=shots.Count;FireEquippedWeapon(player,enemy,Vector3.forward);
        Shot shot=shots.Count>before?shots[shots.Count-1]:null;Check(shot!=null&&shot.damage==11&&Mathf.Abs(shot.velocity.magnitude-160*bulletSpeedScale)<.01f&&player.magazine==6,"configured damage, velocity and magazine affect a real shot");
        Check(Mathf.Abs(player.weaponTimer-.5f*weaponCooldownScale/1.25f)<.001f,"left-hand handling controls its actual fire interval");
        player.weaponTimer=0;player.magazine=4;StartWeaponReload(player);Check(Mathf.Abs(player.reloadTime-1.1f)<.001f,"configured partial reload starts");TickWeapons(player,1.11f);Check(player.magazine==7&&player.reloadTime==0,"partial reload actually completes to configured capacity");
        player.magazine=1;player.weaponTimer=0;FireEquippedWeapon(player,enemy,Vector3.forward);Check(player.magazine==0&&Mathf.Abs(player.reloadTime-1.7f)<.001f,"configured empty reload starts after last shot");
        player.weaponSlot=2;player.magazine=3;player.shotIdleTime=0;TickWeapons(player,1.31f);Check(Mathf.Abs(player.reloadTime-1.1f)<.001f&&player.weapons[0].ammo==0,"inactive empty slot and partial idle reload remain independent");TickWeapons(player,1.11f);Check(player.weapons[0].ammo==7&&player.weapons[1].ammo==7,"both independently reloading slots complete");
        inMenu=false;balanceEditorOpen=false;cameraYaw=0;cameraPitch=DefaultThirdPersonPitch;UpdateCamera(10);yield return null;yield return new WaitForEndOfFrame();SaveUI("00-custom-profile-live-hud.png");balanceEditorOpen=true;
        inMenu=true;balanceEditorOpen=false;weaponGalleryOpen=true;ShowWeaponPreview(Array.FindIndex(WeaponCatalog().models,w=>w.id=="Military_AR"));yield return null;yield return new WaitForEndOfFrame();SaveUI("00b-gallery-actual-equipped-slots.png");weaponGalleryOpen=false;balanceEditorOpen=true;
        inMenu=true;balanceTab=0;balanceWeaponIndex=Array.FindIndex(balanceDraft.weapons,w=>w.id=="Military_AR");BalanceSelectPreview();yield return null;yield return new WaitForEndOfFrame();SaveUI("01-weapon-options.png");
        balanceTab=1;balancePartIndex=Array.FindIndex(balanceDraft.parts,p=>p.id=="WISP_LEGS");yield return null;yield return new WaitForEndOfFrame();SaveUI("02-part-options.png");
        balanceTab=2;yield return null;yield return new WaitForEndOfFrame();SaveUI("03-independent-loadout.png");
        balanceTab=3;yield return null;yield return new WaitForEndOfFrame();SaveUI("04-save-load.png");
        frame=balanceDraft.frames.First(f=>f.id=="WISP");frame.slots[1]="WISP_GuardShield";BalanceApply();inMenu=false;balanceEditorOpen=false;player.guarding=true;player.guardBlend=1;Animate(player);
        Check(HasEquippedGuard(player)&&player.guardShield.gameObject.activeSelf&&player.weaponVisuals[1].GetComponentsInChildren<Renderer>().Length==0,"equipped shield excludes right gun geometry");
        player.weaponSlot=2;before=shots.Count;int ammo=player.magazine;FireEquippedWeapon(player,enemy,Vector3.forward);Check(shots.Count==before&&player.magazine==ammo&&!StartWeaponReload(player),"shield neither fires bullets nor reloads");
        Check(Vector3.Dot(player.guardShield.forward,player.root.forward)>.99f,"equipped shield keeps its forward guard pose");
        yield return CaptureEquipmentPose("05-shield-equipped.png");
        inMenu=true;frame.slots[1]="Military_AR";BalanceApply();inMenu=false;Animate(player);Check(!HasEquippedGuard(player)&&!player.guardShield.gameObject.activeSelf,"swapping back to a gun removes shield and guard capability");yield return CaptureEquipmentPose("06-two-hand-guns.png");
        inMenu=true;frame.slots[0]="UNARMED";frame.slots[2]=frame.slots[3]="UNARMED";BalanceApply();inMenu=false;player.root.position=new Vector3(80,FighterGroundY,70);player.facing=Vector3.forward;aimDirection=Vector3.forward;enemy.root.position=player.root.position+Vector3.forward*(player.Radius+enemy.Radius+.1f);enemy.spec.armor=0;enemy.shieldEnergy=0;Animate(player);
        player.weaponSlot=1;float hp=enemy.hp;before=shots.Count;ammo=player.magazine;Vector3 restHand=player.gripRig[0].hand.position;FireEquippedWeapon(player,enemy,Vector3.forward);TickWeapons(player,.16f);Animate(player);
        Check(player.punchTimer>0&&Vector3.Distance(restHand,player.gripRig[0].hand.position)>.12f,"empty left hand extends in a real punch motion");UpdateCombat(.16f);Check(Mathf.Abs(hp-enemy.hp-15)<.01f&&shots.Count==before&&player.magazine==ammo,"unarmed attack damages nearby opponent without bullets or ammo use");yield return CaptureEquipmentPose("07-left-punch.png");
        ClearCombat();player.punchTimer=player.weaponTimer=0;enemy.hp=hp;Bounds wall=new Bounds(player.root.position+Vector3.forward*1.5f,new Vector3(8,8,.3f));obstacles.Add(wall);FireEquippedWeapon(player,enemy,Vector3.forward);UpdateCombat(.16f);Check(enemy.hp==hp,"punch cannot hit through a wall");obstacles.RemoveAt(obstacles.Count-1);ClearCombat();
        player.punchTimer=player.weaponTimer=0;enemy.root.position=player.root.position-Vector3.forward*3;FireEquippedWeapon(player,enemy,Vector3.forward);UpdateCombat(.16f);Check(enemy.hp==hp,"punch does not damage an opponent behind the fighter");ClearCombat();
        player.punchTimer=player.weaponTimer=0;player.weaponSlot=3;FireEquippedWeapon(player,enemy,Vector3.forward);Check(player.punchTimer==0&&shots.Count==0,"empty shoulder slot produces no hand attack");
        inMenu=true;frame.slots[0]="Military_AR";frame.slots[1]="UNARMED";BalanceApply();inMenu=false;player.weaponSlot=2;player.facing=Vector3.forward;enemy.root.position=player.root.position+Vector3.forward*3;Animate(player);restHand=player.gripRig[1].hand.position;FireEquippedWeapon(player,enemy,Vector3.forward);TickWeapons(player,.16f);Animate(player);Check(player.punchSide==1&&Vector3.Distance(restHand,player.gripRig[1].hand.position)>.12f,"empty right hand uses right-side punch animation");yield return CaptureEquipmentPose("08-right-punch.png");
        activeBalance=null;balanceDraft=BalanceData.Defaults();balanceEditorOpen=false;inMenu=true;ApplySelectableCharacter(player,0);ApplySelectableCharacter(enemy,1);ResetRound();balanceDirectoryOverride=null;
        Debug.Log((pass?"BALANCE_EDITOR_STUDY_PASS ":"BALANCE_EDITOR_STUDY_FAIL ")+checks+" checks; "+captures+" images; isolated files "+disk);Application.Quit(pass?0:2);
        void SaveUI(string name){Texture2D image=ScreenCapture.CaptureScreenshotAsTexture();File.WriteAllBytes(Path.Combine(folder,name),image.EncodeToPNG());Destroy(image);captures++;}
        IEnumerator CaptureEquipmentPose(string name)
        {
            player.root.position=new Vector3(80,FighterGroundY,70);player.facing=Vector3.forward;aimDirection=Vector3.forward;foreach(Transform t in player.root.GetComponentsInChildren<Transform>(true))t.gameObject.layer=9;Animate(player);yield return null;yield return new WaitForEndOfFrame();EnsureWeaponStudio();int mask=weaponStudioCamera.cullingMask;Vector3 pos=weaponStudioCamera.transform.position;Quaternion rot=weaponStudioCamera.transform.rotation;
            weaponStudioCamera.cullingMask=1<<9;weaponStudioCamera.orthographicSize=2.5f;weaponStudioCamera.transform.position=player.root.position+new Vector3(-5,2.5f,8);weaponStudioCamera.transform.LookAt(player.root.position+Vector3.up*.6f);weaponStudioCamera.Render();SaveWeaponImage(Path.Combine(folder,name));captures++;weaponStudioCamera.cullingMask=mask;weaponStudioCamera.orthographicSize=1.2f;weaponStudioCamera.transform.SetPositionAndRotation(pos,rot);
        }
    }
}
