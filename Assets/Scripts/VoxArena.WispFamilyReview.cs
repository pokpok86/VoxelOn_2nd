using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed partial class VoxArena
{
    IEnumerator FamilyArtReview()
    {
        if(FamilyJointReviewRequested){yield return FamilyJointReview();yield break;}
        string folder=Path.Combine(Directory.GetCurrentDirectory(),"Docs","Validation","PartSelection-20261118");Directory.CreateDirectory(folder);
        balanceDirectoryOverride=Path.Combine(Path.GetTempPath(),"VoxelOn-WispFamily-Review-"+DateTime.Now.ToString("yyyyMMdd-HHmmss"));
        familyLoadouts=new FamilyLoadoutFile();bool pass=true;int checks=0;
        void Check(bool ok,string text){checks++;pass&=ok;Debug.Log("WISP_FAMILY_CHECK "+(ok?"PASS ":"FAIL ")+text);}
        TestSetupSelectable(0,1);smoke=false;inMenu=true;paused=true;muted=true;spectatorMode=false;
        player.root.position=new Vector3(100,FighterGroundY,100);player.root.rotation=Quaternion.identity;player.facing=Vector3.forward;
        Check(WispFamilyAssets.Available&&familyRigs.ContainsKey(player),"real game fighter uses exported Blender family");
        string[] ids=new[]{"BD01","HD01","W-01_LUMEN","W-02_BREAKER","W-03_RATTLE","W-04_EDGE","W-05_AEGIS"}.Concat(FamilyHeads).Concat(FamilyBodies).Concat(FamilyBackpacks).Concat(FamilyLegs).ToArray();
        foreach(string id in ids){if(DronePackIndex(id)>=0){Check(droneBackpackSources[DronePackIndex(id)]!=null,"authored drone pack source "+id);continue;}var d=WispFamilyAssets.Data(id);Check(d.groups.Length>0&&d.groups.All(g=>g.positions.Length>0&&g.normals.Length==g.positions.Length&&g.triangles.All(v=>v>=0&&v<g.positions.Length/3)),"valid actual mesh buffers "+id);}
        foreach(string id in FamilyHeads.Skip(1).Concat(FamilyBodies.Skip(1)).Concat(FamilyBackpacks.Skip(5)).Concat(FamilyLegs.Skip(4)))
            Check(FamilyPartLabel(id).StartsWith("신규 ·",StringComparison.Ordinal),"new cosmetic option has a readable localized label "+id);
        string legacyKey=(activeBalance?.activeDesignId??"DESIGN_01")+"/"+player.spec.name;
        familyLoadouts=JsonUtility.FromJson<FamilyLoadoutFile>("{\"version\":1,\"designs\":[{\"key\":\""+legacyKey+"\",\"legs\":\"L-01_STRIDER\",\"backpack\":\"B-01_SKYWING\"}]}");
        FamilyLoadout legacyConfig=FamilyConfig(player);Check(legacyConfig.head==FamilyHeads[0]&&legacyConfig.body==FamilyBodies[0],"older saved appearance loadouts keep default head/core when new fields are absent");
        familyLoadouts=JsonUtility.FromJson<FamilyLoadoutFile>("{\"version\":1,\"designs\":[{\"key\":\""+legacyKey+"\",\"head\":\"HD01\",\"body\":\"BD01\",\"legs\":\"L-01_STRIDER\",\"backpack\":\"B-01_SKYWING\",\"weapons\":[\"AUTO\",\"W-01_LUMEN\",\"AUTO\",\"AUTO\"]}]}");
        legacyConfig=FamilyConfig(player);Check(legacyConfig.head=="HD01"&&legacyConfig.body=="BD01"&&legacyConfig.legs=="L-01_STRIDER"&&legacyConfig.backpack=="B-01_SKYWING"&&legacyConfig.weapons[1]=="W-01_LUMEN","older saved appearance IDs remain readable without rewriting preserved art choices");
        familyLoadouts=new FamilyLoadoutFile();FamilyLoadout c=FamilyConfig(player);c.enabled=true;
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
            if(c.legs=="VIS_L03_TARANTULAN")
            {
                Check(rig.legs.data.nodes.Count(n=>n.motion=="quadHip")==4&&rig.legs.data.nodes.Count(n=>n.motion=="quadKnee")==4&&rig.legs.data.nodes.Count(n=>n.motion=="quadAnkle")==4,"new TARANTULAN has four animated hip/knee/ankle chains");
                player.velocity=player.facing*player.spec.speed;roundTime=.1f;UpdateFamilyArt(player);
                var before=new Dictionary<string,Quaternion>();foreach(string tag in new[]{"LF","LB","RF","RB"})foreach(string joint in new[]{"Hip","Knee","Ankle"})before[tag+"_"+joint]=rig.legs.nodes[tag+"_"+joint].localRotation;
                // Cross the alternating support half-cycle so both diagonal pairs must respond.
                roundTime=.50f;UpdateFamilyArt(player);bool allJointsAnimate=true;
                foreach(string name in before.Keys)allJointsAnimate&=Quaternion.Angle(before[name],rig.legs.nodes[name].localRotation)>.25f;
                Check(allJointsAnimate,"all twelve authored quadruped joints respond to movement phase");player.velocity=Vector3.zero;
            }
            Capture("0"+(i+1)+"-"+c.legs);Capture("0"+(i+1)+"-Back",true);
        }
        foreach(string id in FamilyHeads.Skip(1))
        {
            c.head=id;BuildRobot(player);UpdateFamilyArt(player);var rig=familyRigs[player];
            Check(rig.head.data.id==id&&rig.head.data.category=="head"&&rig.head.data.groups.Length>0,"mounted authored head candidate "+id);
            Capture("Head-"+id);Capture("Head-"+id+"-Back",true);
        }
        foreach(string id in FamilyBodies.Skip(1))
        {
            c.body=id;BuildRobot(player);UpdateFamilyArt(player);var rig=familyRigs[player];
            Check(rig.body.data.id==id&&rig.body.data.category=="body"&&rig.body.data.groups.Length>0,"mounted authored core candidate "+id);
            Check(rig.body.nodes.ContainsKey("LeftShoulder")&&rig.body.nodes.ContainsKey("RightShoulder")&&rig.body.data.groups.Any(g=>g.node=="LeftShoulder")&&rig.body.data.groups.Any(g=>g.node=="RightShoulder"),"core shoulder armor follows both existing arm joints "+id);
            Check(Mathf.Abs(rig.body.data.waistBottom+.13411f)<.002f&&Mathf.Abs(rig.body.data.neckTop-.07734f)<.002f&&Vector3.Distance(WispFamilyAssets.V(rig.body.data.leftElbow),new Vector3(-.18074f,-.11144f,0))<.002f,"core keeps shared waist, neck and elbow interfaces "+id);
            Capture("Core-"+id);Capture("Core-"+id+"-Back",true);
        }
        // Exercise every new head/core pairing together: individually valid parts can
        // still collide, hide the sensor, or produce a broken combined silhouette.
        foreach(string headId in FamilyHeads.Skip(1))
        foreach(string bodyId in FamilyBodies.Skip(1))
        {
            c.head=headId;c.body=bodyId;BuildRobot(player);UpdateFamilyArt(player);yield return null;
            var rig=familyRigs[player];
            Check(rig.head.data.id==headId&&rig.body.data.id==bodyId,"combined head/core loadout resolves both IDs "+headId+" + "+bodyId);
            Bounds comboBounds=new Bounds(rig.root.position,Vector3.zero);bool firstRenderer=true;int visibleMeshes=0;
            foreach(Renderer renderer in rig.root.GetComponentsInChildren<Renderer>())if(renderer.enabled)
            {if(firstRenderer){comboBounds=renderer.bounds;firstRenderer=false;}else comboBounds.Encapsulate(renderer.bounds);visibleMeshes++;}
            Check(!firstRenderer&&visibleMeshes>=8&&Finite(comboBounds.size)&&comboBounds.size.x>.2f&&comboBounds.size.y>.5f&&comboBounds.size.z>.2f,"combined head/core keeps finite visible assembly bounds "+headId+" + "+bodyId);
            Capture("Combo-"+headId+"-"+bodyId);Capture("Combo-"+headId+"-"+bodyId+"-Back",true);
        }
        // Reuse the same family kit on the other selectable silhouettes. Keep
        // WISP's complete matrix above and sample every pairing again per frame so
        // scale/chassis integration regressions are visible in saved screenshots.
        foreach(int frame in new[]{1,2})
        {
            TestSetupSelectable(frame,1);smoke=false;inMenu=true;paused=true;muted=true;
            player.root.position=new Vector3(100,FighterGroundY,100);player.root.rotation=Quaternion.identity;player.facing=Vector3.forward;
            c=FamilyConfig(player);c.enabled=true;c.legs=FamilyLegs[frame==1?4:5];c.backpack=FamilyBackpacks[0];
            foreach(string headId in FamilyHeads.Skip(1))
            foreach(string bodyId in FamilyBodies.Skip(1))
            {
                c.head=headId;c.body=bodyId;BuildRobot(player);UpdateFamilyArt(player);yield return null;
                var rig=familyRigs[player];
                Check(rig.head.data.id==headId&&rig.body.data.id==bodyId&&rig.legs.data.id==c.legs,"shared head/core fits selectable frame "+player.spec.name+" "+headId+" + "+bodyId);
                float frameSeam=Vector3.Distance(rig.upper.position,rig.legs.root.TransformPoint(Vector3.up*rig.legs.data.waistHeight));
                Check(frameSeam<.001f,"selectable frame lower/core waist seam remains closed "+player.spec.name+" "+headId+" + "+bodyId);
                Bounds frameBounds=new Bounds(rig.root.position,Vector3.zero);bool firstFrameRenderer=true;int frameMeshes=0;
                foreach(Renderer renderer in rig.root.GetComponentsInChildren<Renderer>())if(renderer.enabled)
                {if(firstFrameRenderer){frameBounds=renderer.bounds;firstFrameRenderer=false;}else frameBounds.Encapsulate(renderer.bounds);frameMeshes++;}
                Check(!firstFrameRenderer&&frameMeshes>=8&&Finite(frameBounds.size)&&frameBounds.size.x>.2f&&frameBounds.size.y>.5f&&frameBounds.size.z>.2f,"selectable frame retains finite visible family assembly "+player.spec.name+" "+headId+" + "+bodyId);
                Capture("Frame-"+player.spec.name+"-"+headId+"-"+bodyId);Capture("Frame-"+player.spec.name+"-"+headId+"-"+bodyId+"-Back",true);
            }
        }
        c.head=FamilyHeads[0];c.body=FamilyBodies[0];BuildRobot(player);UpdateFamilyArt(player);
        c.legs=FamilyLegs[0];c.backpack=FamilyBackpacks[4];BuildRobot(player);UpdateFamilyArt(player);Capture("05-RAMJET",true);
        for(int i=1;i<FamilyWeapons.Length;i++)
        {
            c.weapons[0]=FamilyWeapons[i];BuildRobot(player);UpdateFamilyArt(player);var rig=familyRigs[player];
            Check(rig.weaponIds[0]==player.weaponArtIds[0]||player.weaponProfiles[0].isUnarmed,"actual equipped gun takes priority over preserved legacy cosmetic selection "+i);
            Capture("Weapon-"+FamilyWeapons[i]);
        }
        c.weapons[0]="AUTO";var profile=player.weaponProfiles[0];bool empty=profile.isUnarmed;profile.isUnarmed=true;BuildRobot(player);Check(familyRigs[player].weapons[0]==null,"removing weapon removes new visible mesh");profile.isUnarmed=empty;
        bool shield=profile.isShield;profile.isShield=true;BuildRobot(player);Check(familyRigs[player].weaponIds[0]=="W-05_AEGIS","shield maps to shield model");player.guardBlend=0;UpdateFamilyArt(player);Quaternion lowered=familyRigs[player].weapons[0].root.rotation;player.guardBlend=1;UpdateFamilyArt(player);Check(Quaternion.Angle(lowered,familyRigs[player].weapons[0].root.rotation)>60,"guard rotates actual shield");profile.isShield=shield;player.guardBlend=0;
        c.head=FamilyHeads[0];c.body=FamilyBodies[0];c.legs=FamilyLegs[0];c.backpack=FamilyBackpacks[0];BuildRobot(player);inMenu=false;aimDirection=Vector3.forward;UpdateFamilyArt(player);
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
        SaveFamilyConfig();FamilyLoadoutFile saved=File.Exists(FamilyLoadoutPath)?JsonUtility.FromJson<FamilyLoadoutFile>(File.ReadAllText(FamilyLoadoutPath)):null;
        FamilyLoadout savedDesign=saved?.designs?.FirstOrDefault(d=>d!=null&&d.key==c.key);
        Check(savedDesign!=null&&savedDesign.head==c.head&&savedDesign.body==c.body&&savedDesign.legs==c.legs&&savedDesign.backpack==c.backpack,"independent appearance settings save and deserialize in isolated folder");
        enemy.root.position=player.root.position+new Vector3(0,0,25);paused=false;inMenu=false;cameraYaw=180;cameraPitch=14;UpdateCamera(10);
        for(int i=0;i<8;i++)yield return null;yield return new WaitForEndOfFrame();Texture2D screen=ScreenCapture.CaptureScreenshotAsTexture();File.WriteAllBytes(Path.Combine(folder,"Gameplay.png"),screen.EncodeToPNG());Destroy(screen);
        inMenu=true;OpenBalanceEditor();balanceTab=6;BalanceSelectPreview();
        for(int category=0;category<4;category++)
        {
            string before=FamilyCategoryValue(FamilyConfig(player),category);
            SelectFamilyPart(category,1);string after=FamilyCategoryValue(FamilyConfig(player),category);
            Check(before!=after,"arrow advances selected part category "+category);
            FamilyRig visible=familyRigs[player];
            string actual=category==0?visible.head.data.id:category==1?visible.body.data.id:category==2?visible.legs.data.id:visible.backpack.data.id;
            Check(actual==after,"arrow rebuilds actual robot mesh category "+category);
            Check(FamilyPartPreview(after)!=null&&FamilyPartPreview(after).IsCreated(),"isolated actual part render category "+category);
            SelectFamilyPart(category,-1);
            Check(FamilyCategoryValue(FamilyConfig(player),category)==before,"previous arrow restores selection category "+category);
        }
        familyArtCategory=2;
        yield return null;yield return new WaitForEndOfFrame();
        Texture2D selectorScreen=ScreenCapture.CaptureScreenshotAsTexture();File.WriteAllBytes(Path.Combine(folder,"ArrowSelector.png"),selectorScreen.EncodeToPNG());Destroy(selectorScreen);
        string[][] cardGroups={FamilyHeads,FamilyBodies,FamilyLegs,FamilyBackpacks,FamilyWeapons,FamilyWeapons};
        for(int category=0;category<cardGroups.Length;category++)foreach(string id in cardGroups[category])Check(DronePackIndex(id)>=0?FamilyPartPreview(id)!=null:Resources.Load<Texture2D>("PartSelectionCards/"+id)!=null,"actual rendered selection card texture available "+id);
        familyArtCategory=0;
        string[] expectedKeys=new string[3],expectedHeads=new string[3],expectedBodies=new string[3],expectedLegs=new string[3],expectedBackpacks=new string[3],expectedWeapons=new string[3];
        for(int frame=0;frame<3;frame++)
        {
            TestSetupSelectable(frame,1);smoke=false;inMenu=true;FamilyLoadout independent=FamilyConfig(player);independent.enabled=true;
            independent.head=expectedHeads[frame]=FamilyHeads[frame+1];independent.body=expectedBodies[frame]=FamilyBodies[frame+1];
            independent.legs=expectedLegs[frame]=frame==0?FamilyLegs[0]:FamilyLegs[frame==1?4:5];
            independent.backpack=expectedBackpacks[frame]=FamilyBackpacks[5+frame%2];independent.weapons[0]=expectedWeapons[frame]=FamilyWeapons[frame+1];expectedKeys[frame]=independent.key;
            SaveFamilyConfig();
        }
        FamilyLoadoutFile independentRoundTrip=JsonUtility.FromJson<FamilyLoadoutFile>(File.ReadAllText(FamilyLoadoutPath));bool isolatedPresets=true;
        for(int frame=0;frame<3;frame++)
        {
            FamilyLoadout loaded=independentRoundTrip?.designs?.FirstOrDefault(d=>d!=null&&d.key==expectedKeys[frame]);
            isolatedPresets&=loaded!=null&&loaded.weapons!=null&&loaded.weapons.Length>0&&loaded.head==expectedHeads[frame]&&loaded.body==expectedBodies[frame]&&loaded.legs==expectedLegs[frame]&&loaded.backpack==expectedBackpacks[frame]&&loaded.weapons[0]==expectedWeapons[frame];
        }
        Check(isolatedPresets,"three selectable frames keep separate cosmetic presets after save and reload");
        rt.Release();Destroy(rt);Destroy(png);Destroy(study.gameObject);
        File.WriteAllText(Path.Combine(folder,"Result.txt"),(pass?"PASS":"FAIL")+" / "+checks+" actual runtime checks\n"+DateTime.Now.ToString("O"));
        Debug.Log((pass?"WISP_FAMILY_REVIEW_PASS ":"WISP_FAMILY_REVIEW_FAIL ")+checks+" checks");Application.Quit(pass?0:2);
    }
}
