using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public sealed partial class VoxArena
{
    IEnumerator CaptureGameplayUI()
    {
        yield return new WaitForSecondsRealtime(.2f);
        yield return new WaitForEndOfFrame();
        if(!SaveGameplayUIFrame("WISP-only-hangar.png")) yield break;
        TestSetupSelectable(0,0); inMenu=false; smoke=false; spectatorMode=false;
        player.root.position=new Vector3(0,FighterGroundY,0); enemy.root.position=new Vector3(0,FighterGroundY,30);
        player.facing=Vector3.forward; player.velocity=Vector3.zero; canShoot=true; targetLock=false;
        cameraYaw=0; cameraPitch=DefaultThirdPersonPitch; view.orthographic=false; view.fieldOfView=60;
        aimMarker.gameObject.SetActive(false); Animate(player); Animate(enemy); UpdateCamera(10);
        yield return new WaitForSecondsRealtime(.4f);
        yield return new WaitForEndOfFrame();
        if(!SaveGameplayUIFrame("WISP-crosshair-camera.png")) yield break;
        player.guarding=true;
        for(int frame=0;frame<35;frame++) { Animate(player); yield return null; }
        yield return new WaitForEndOfFrame();
        if(!SaveGameplayUIFrame("WISP-guard-gameplay.png")) yield break;
        if(System.Array.IndexOf(System.Environment.GetCommandLineArgs(),"--camera-review")>=0)
        {
            TestSetupSelectable(0,0);inMenu=false;smoke=false;spectatorMode=false;
            player.root.position=new Vector3(100,FighterGroundY,100);enemy.root.position=new Vector3(110,FighterGroundY,125);
            player.facing=Vector3.forward;player.velocity=Vector3.zero;targetLock=false;cameraYaw=0;cameraPitch=DefaultThirdPersonPitch;
            Bounds wall=new Bounds(new Vector3(100,3,96.5f),new Vector3(20,8,2));obstacles.Add(wall);
            Transform art=PlaceEnvironment("FoundryCargo",null,new Vector3(100,GroundSurfaceY,96.5f),new Vector3(20,7.91f,2));
            for(int frame=0;frame<80;frame++){Animate(player);yield return null;}
            yield return new WaitForEndOfFrame();
            if(!SaveGameplayUIFrame("CameraFix-center-close.png"))yield break;
            Debug.Log("CAMERA_REVIEW pitch="+(-Mathf.Asin(view.transform.forward.y)*Mathf.Rad2Deg)+" requested="+cameraPitch+" headAlpha="+cameraBodyOpacity);
            cameraPitch=-5;
            for(int frame=0;frame<80;frame++){Animate(player);yield return null;}
            yield return new WaitForEndOfFrame();
            if(!SaveGameplayUIFrame("CameraFix-center-look-up.png"))yield break;
            Debug.Log("CAMERA_REVIEW pitch="+(-Mathf.Asin(view.transform.forward.y)*Mathf.Rad2Deg)+" requested="+cameraPitch+" headAlpha="+cameraBodyOpacity);
            obstacles.RemoveAt(obstacles.Count-1);if(art!=null)Destroy(art.gameObject);
        }
        Application.Quit(0);
    }
    bool SaveGameplayUIFrame(string filename)
    {
        Texture2D screenshot=ScreenCapture.CaptureScreenshotAsTexture();
        if(screenshot==null) { Debug.LogError("WISP_GAMEPLAY_UI_CAPTURE_FAIL"); Application.Quit(2); return false; }
        // Hidden/minimized player windows can return an all-black image despite
        // a successful API call. Reject those instead of publishing false evidence.
        Color[] pixels=screenshot.GetPixels();float minLight=1,maxLight=0;
        for(int p=0;p<pixels.Length;p+=97) { float light=pixels[p].grayscale;minLight=Mathf.Min(minLight,light);maxLight=Mathf.Max(maxLight,light); }
        if(maxLight-minLight<.05f) { Destroy(screenshot);Debug.LogError("WISP_GAMEPLAY_UI_CAPTURE_FAIL blank image");Application.Quit(2);return false; }
        string path=Path.Combine(Directory.GetCurrentDirectory(),"Docs","Models",(arenaMap==ArenaMapKind.OutdoorCanyon?"Canyon-":"Foundry-")+filename);
        File.WriteAllBytes(path,screenshot.EncodeToPNG()); Destroy(screenshot);
        Debug.Log("WISP_GAMEPLAY_UI_CAPTURE_PASS "+path); return true;
    }
    bool HeadOverlapsCenterReticle(Fighter fighter)
    {
        Vector2 min=Vector2.one*float.PositiveInfinity,max=Vector2.one*float.NegativeInfinity;
        bool projected=false;
        foreach(SkinnedMeshRenderer skin in fighter.body.GetComponentsInChildren<SkinnedMeshRenderer>())
        {
            Vector3[] vertices=skin.sharedMesh.vertices; BoneWeight[] weights=skin.sharedMesh.boneWeights;
            Matrix4x4[] bind=skin.sharedMesh.bindposes;
            for(int v=0;v<vertices.Length;v++)
            {
                int boneIndex=weights[v].boneIndex0;
                if(!skin.bones[boneIndex].IsChildOf(fighter.headPart)) continue;
                Vector3 point=view.WorldToViewportPoint((skin.bones[boneIndex].localToWorldMatrix*bind[boneIndex]).MultiplyPoint3x4(vertices[v]));
                if(point.z<=0) continue;
                min=Vector2.Min(min,new Vector2(point.x,point.y)); max=Vector2.Max(max,new Vector2(point.x,point.y)); projected=true;
            }
        }
        if(!projected) return false;
        // Include the complete HUD reticle, not just its center dot.
        return new Rect(min.x-18f/1280,min.y-18f/720,max.x-min.x+36f/1280,max.y-min.y+36f/720).Contains(new Vector2(.5f,.5f));
    }
    IEnumerator CapturePrototypeVisuals()
    {
        yield return new WaitForEndOfFrame();
        string root=Path.Combine(Directory.GetCurrentDirectory(),"Docs","VisualReferences","Implemented");
        Directory.CreateDirectory(root);
        const int width=768,height=432;
        RenderTexture target=new RenderTexture(width,height,24,RenderTextureFormat.ARGB32);
        Texture2D image=new Texture2D(width,height,TextureFormat.RGB24,false);
        RenderTexture previousTarget=view.targetTexture,previousActive=RenderTexture.active;
        bool previousOrtho=view.orthographic; float previousFov=view.fieldOfView;
        Color previousBackground=view.backgroundColor; CameraClearFlags previousClear=view.clearFlags;
        view.targetTexture=target; view.orthographic=false; view.fieldOfView=40;
        view.clearFlags=CameraClearFlags.SolidColor; view.backgroundColor=new Color(.035f,.055f,.08f);
        aimMarker.gameObject.SetActive(false);
        enemy.root.gameObject.SetActive(false); enemy.shadow.gameObject.SetActive(false);
        Vector3 center=new Vector3(0,1.1f,0),focus=center+Vector3.up*.65f;
        Vector3[] directions={Vector3.forward,Vector3.right,Vector3.back,Vector3.left};
        string[] views={"front","right","rear","left"}; int written=0;
        for(int frame=0;frame<CraftCatalog.Selectable.Length;frame++)
        {
            ApplySelectableCharacter(player,frame); player.root.position=center; player.root.rotation=Quaternion.identity;
            player.facing=Vector3.forward; player.velocity=Vector3.zero; player.boosting=false;
            player.thruster.gameObject.SetActive(false); player.jumpFlame.gameObject.SetActive(false);
            player.shield.gameObject.SetActive(false); player.saber.gameObject.SetActive(false);
            Animate(player);
            for(int angle=0;angle<directions.Length;angle++)
            {
                view.transform.position=center+directions[angle]*8.7f+Vector3.up*3f;
                view.transform.LookAt(focus); view.Render(); RenderTexture.active=target;
                image.ReadPixels(new Rect(0,0,width,height),0,0); image.Apply();
                string path=Path.Combine(root,(frame+1).ToString("00")+"-"+player.spec.name.ToLowerInvariant()+"-"+views[angle]+".png");
                File.WriteAllBytes(path,image.EncodeToPNG()); written++;
            }
        }
        if(wispModelSource!=null&&IsArticulatedWisp)
        {
            ApplySelectableCharacter(player,0); player.root.position=center; player.facing=Vector3.forward;
            inMenu=false;
            string[] poses={"stand","stride-left","stride-right","jump","guard"};
            for(int pose=0;pose<poses.Length;pose++)
            {
                roundTime=pose==1?.10f:pose==2?.30f:0f;
                player.velocity=pose==1||pose==2?Vector3.forward*player.spec.speed:Vector3.zero;
                player.jumpHeight=pose==3?2f:0f; player.jumpVelocity=pose==3?4f:0f;
                player.guarding=pose==4; player.guardBlend=pose==4?1:0;
                player.root.position=center+Vector3.up*player.jumpHeight; Animate(player);
                view.transform.position=center+(pose==3?new Vector3(6f,4.2f,9f):new Vector3(6f,2.2f,7f));
                view.transform.LookAt(center+Vector3.up*(pose==3?1.3f:.2f));
                view.Render(); RenderTexture.active=target;
                image.ReadPixels(new Rect(0,0,width,height),0,0); image.Apply();
                File.WriteAllBytes(Path.Combine(Directory.GetCurrentDirectory(),"Docs","Models","WISP-v003-unity-"+poses[pose]+".png"),image.EncodeToPNG());
            }
            Debug.Log("WISP_V003_MOTION_CAPTURE_PASS 5 actual player poses");
            player.guarding=false; player.guardBlend=0;
            player.root.position=center; player.velocity=Vector3.zero; player.jumpHeight=player.jumpVelocity=0; Animate(player);
            Bounds wallBounds=new Bounds(center+new Vector3(0,2,-3.5f),new Vector3(20,8,2));
            Transform wall=Box("Camera regression wall",null,wallBounds.center,wallBounds.size,new Color(.3f,.34f,.4f));
            obstacles.Add(wallBounds);
            bool savedSmoke=smoke,savedMenu=inMenu; smoke=false; inMenu=false;
            targetLock=false; cameraYaw=0; cameraPitch=DefaultThirdPersonPitch; view.fieldOfView=60;
            view.transform.position=center+new Vector3(0,2,-9);
            for(int step=0;step<90;step++) UpdateCamera(.025f);
            view.Render(); RenderTexture.active=target; image.ReadPixels(new Rect(0,0,width,height),0,0); image.Apply();
            File.WriteAllBytes(Path.Combine(Directory.GetCurrentDirectory(),"Docs","Models","WISP-camera-wall.png"),image.EncodeToPNG());
            smoke=savedSmoke; inMenu=savedMenu; obstacles.RemoveAt(obstacles.Count-1); Destroy(wall.gameObject);
            Debug.Log("WISP_WALL_CAMERA_CAPTURE_PASS");
        }
        RenderTexture.active=previousActive; view.targetTexture=previousTarget;
        view.orthographic=previousOrtho; view.fieldOfView=previousFov;
        view.clearFlags=previousClear; view.backgroundColor=previousBackground;
        Destroy(image); target.Release(); Destroy(target);
        Debug.Log("VOXSKY_VISUAL_TARGETS_PASS "+written+" images saved to "+root);
        Application.Quit(0);
    }
    void SavePreview(string name)
    {
        string path=Path.GetFullPath(Path.Combine(Application.dataPath,"../../"+name));
        Texture2D preview=ScreenCapture.CaptureScreenshotAsTexture();
        if(preview==null) throw new System.Exception("Screen capture unavailable");
        File.WriteAllBytes(path,preview.EncodeToPNG()); Destroy(preview); Debug.Log("VOXSKY_PREVIEW "+path);
    }
    IEnumerator CaptureDocumentationGallery()
    {
        yield return new WaitForEndOfFrame();
        string root=Path.Combine(Directory.GetCurrentDirectory(),"docs","Screenshots");
        string robots=Path.Combine(root,"Robots"),arena=Path.Combine(root,"Arena");
        Directory.CreateDirectory(robots); Directory.CreateDirectory(arena);
        const int width=768,height=432;
        RenderTexture target=new RenderTexture(width,height,24,RenderTextureFormat.ARGB32);
        Texture2D image=new Texture2D(width,height,TextureFormat.RGB24,false);
        RenderTexture previousTarget=view.targetTexture,previousActive=RenderTexture.active;
        bool previousOrtho=view.orthographic; float previousFov=view.fieldOfView;
        Color previousBackground=view.backgroundColor; CameraClearFlags previousClear=view.clearFlags;
        view.targetTexture=target; view.orthographic=false; view.fieldOfView=40;
        view.clearFlags=CameraClearFlags.SolidColor; view.backgroundColor=new Color(.035f,.055f,.08f);
        aimMarker.gameObject.SetActive(false);
        enemy.root.gameObject.SetActive(false); enemy.shadow.gameObject.SetActive(false);
        player.root.gameObject.SetActive(true); player.shadow.gameObject.SetActive(true);
        Vector3 center=new Vector3(0,1.1f,0),focus=center+Vector3.up*.7f;
        Vector3[] directions={Vector3.forward,Vector3.back,Vector3.left,Vector3.right};
        string[] views={"front","back","left","right"};
        int written=0;
        for(int frame=0;frame<CraftCatalog.All.Length;frame++)
        {
            ApplyCharacter(player,frame); player.root.position=center; player.root.rotation=Quaternion.identity;
            player.facing=Vector3.forward; player.velocity=Vector3.zero; player.boosting=false;
            player.thruster.gameObject.SetActive(false); player.jumpFlame.gameObject.SetActive(false);
            player.shield.gameObject.SetActive(false); player.saber.gameObject.SetActive(false);
            player.gun.gameObject.SetActive(true); player.fist.gameObject.SetActive(false);
            Animate(player);
            string slug=CraftCatalog.All[frame].name.ToLowerInvariant();
            for(int angle=0;angle<directions.Length;angle++)
            {
                view.transform.position=center+directions[angle]*8.7f+Vector3.up*3f;
                view.transform.LookAt(focus);
                view.Render();
                RenderTexture.active=target; image.ReadPixels(new Rect(0,0,width,height),0,0); image.Apply();
                string path=Path.Combine(robots,(frame+1).ToString("00")+"-"+slug+"-"+views[angle]+".png");
                File.WriteAllBytes(path,image.EncodeToPNG()); written++;
            }
        }
        // Capture the same arena from four compass positions without either robot
        // obscuring the terrain, skyline, cover distribution or perimeter props.
        player.root.gameObject.SetActive(false); player.shadow.gameObject.SetActive(false);
        view.fieldOfView=55; view.farClipPlane=600;
        Vector3[] compass={Vector3.forward,Vector3.right,Vector3.back,Vector3.left};
        string[] sectors={"north","east","south","west"};
        for(int i=0;i<compass.Length;i++)
        {
            view.transform.position=new Vector3(0,36,0)+compass[i]*78f;
            view.transform.LookAt(new Vector3(0,0,0));
            view.Render();
            RenderTexture.active=target; image.ReadPixels(new Rect(0,0,width,height),0,0); image.Apply();
            string path=Path.Combine(arena,"arena-"+sectors[i]+"-sector.png");
            File.WriteAllBytes(path,image.EncodeToPNG()); written++;
        }
        RenderTexture.active=previousActive; view.targetTexture=previousTarget;
        view.orthographic=previousOrtho; view.fieldOfView=previousFov;
        view.clearFlags=previousClear; view.backgroundColor=previousBackground;
        Destroy(image); target.Release(); Destroy(target);
        Debug.Log("VOXSKY_GALLERY_PASS "+written+" images: 8 frames x 4 angles + 4 arena sectors at "+root);
        Application.Quit(0);
    }
    void TestSetup(int pilot=0,int rival=0)
    {
        ApplyCharacter(player,pilot); ApplyCharacter(enemy,rival); ResetRound(); inMenu=false;
        player.root.position=new Vector3(100,1.1f,100); enemy.root.position=new Vector3(100,1.1f,125);
        FaceOpponent(player,enemy); FaceOpponent(enemy,player);
    }
    void TestSetupSelectable(int pilot=0,int rival=0)
    {
        ApplySelectableCharacter(player,pilot); ApplySelectableCharacter(enemy,rival); ResetRound(); inMenu=false;
        player.root.position=new Vector3(100,1.1f,100); enemy.root.position=new Vector3(100,1.1f,125);
        FaceOpponent(player,enemy); FaceOpponent(enemy,player);
    }
    void SaveWeaponStudy()
    {
        // Render the actual projectile meshes at native world scale, not concept artwork.
        Camera study=new GameObject("Projectile study camera").AddComponent<Camera>();
        study.enabled=false; study.orthographic=true; study.orthographicSize=1.12f;
        study.clearFlags=CameraClearFlags.SolidColor; study.backgroundColor=new Color(.035f,.055f,.08f);
        study.cullingMask=1<<9;
        Vector3 stage=new Vector3(200,60,200);
        study.transform.position=stage+new Vector3(2,1.5f,3); study.transform.LookAt(stage);
        RenderTexture rt=new RenderTexture(256,192,16); study.targetTexture=rt;
        Texture2D atlas=new Texture2D(1024,384,TextureFormat.RGB24,false);
        CraftSpec original=player.spec;
        RenderTexture previous=RenderTexture.active;
        for(int i=0;i<8;i++)
        {
            player.spec=CraftCatalog.All[i];
            Shot sample=new Shot { owner=player,radius=player.spec.projectileRadius,tracking=i==3?1:0,wave=i==6?1:0 };
            Transform model=BuildProjectileVisual(sample,stage);
            foreach(Transform part in model.GetComponentsInChildren<Transform>()) part.gameObject.layer=9;
            study.Render(); RenderTexture.active=rt;
            atlas.ReadPixels(new Rect(0,0,256,192),i%4*256,(1-i/4)*192);
            model.gameObject.SetActive(false); Destroy(model.gameObject);
        }
        player.spec=original; RenderTexture.active=previous; atlas.Apply();
        File.WriteAllBytes(Path.GetFullPath(Path.Combine(Application.dataPath,"../../WeaponStudy.png")),atlas.EncodeToPNG());
        study.targetTexture=null; rt.Release(); Destroy(rt); Destroy(study.gameObject); Destroy(atlas);
    }
    void TestStep(float seconds)
    {
        for(float elapsed=0;elapsed<seconds-.00001f;)
        {
            float dt=Mathf.Min(.025f,seconds-elapsed);
            Move(player,Vector3.zero,dt); Move(enemy,Vector3.zero,dt);
            UpdateCombat(dt); UpdateSparks(dt); elapsed+=dt;
        }
    }
    IEnumerator SmokeTest()
    {
        int count=0; bool passed=true;
        void Check(bool ok,string name) { count++; passed&=ok; Debug.Log("CHECK "+name+" "+(ok?"PASS":"FAIL")); }
        inMenu=true;
        yield return new WaitForEndOfFrame(); SavePreview("MenuPreview.png");
        showTuning=true;
        yield return new WaitForEndOfFrame(); SavePreview("SettingsPreview.png");
        showTuning=false;
        ToggleGuide();
        yield return new WaitForEndOfFrame(); SavePreview("GuidePreview.png");
        CloseGuide();
        Vector3 before=enemy.root.position;
        yield return null;
        Check(inMenu&&before==enemy.root.position,"menu freezes battle");
        Check(CraftCatalog.All.Length==8,"legacy eight-frame catalog retained");
        Check(CraftCatalog.Selectable.Length==4 &&
            CraftCatalog.Selectable[0].name=="WISP" && CraftCatalog.Selectable[1].name=="BASTION" &&
            CraftCatalog.Selectable[2].name=="TARANTULA" && CraftCatalog.Selectable[3].name=="HOUND",
            "four prototype definitions remain archived for future unlocks");
        Check(CraftCatalog.UnlockedCount==1&&CraftCatalog.Selectable[0].name=="WISP"&&player.spec.name=="WISP"&&enemy.spec.name=="WISP",
            "hangar and initial rival expose WISP only");
        Check(!CraftCatalog.Selectable[0].supportsLockOn && CraftCatalog.Selectable[1].supportsLockOn &&
            CraftCatalog.Selectable[2].supportsLockOn && !CraftCatalog.Selectable[3].supportsLockOn,
            "lock-on availability matches official WISP/BASTION/TARANTULA/HOUND concept");
        TestSetupSelectable(0,1);
        Check(!CanStartLockOn(player) && CanStartLockOn(enemy),
            "WISP cannot enter lock-on while BASTION can");
        targetLock=true;
        Check(!TryAcquireTargetLock(player,enemy) && !targetLock,
            "WISP right-click cannot acquire a target lock");
        Check(TryAcquireTargetLock(enemy,player) && targetLock,
            "BASTION right-click can acquire a target lock");
        TestSetupSelectable(2,3);
        Check(CanStartLockOn(player) && !CanStartLockOn(enemy),
            "TARANTULA can enter lock-on while HOUND cannot");
        Check(TryAcquireTargetLock(player,enemy) && targetLock,
            "TARANTULA right-click can acquire a target lock");
        Check(!TryAcquireTargetLock(enemy,player) && !targetLock,
            "HOUND right-click cannot acquire a target lock");
        Check(CanFireSpecialWithCurrentLock(enemy),
            "HOUND special remains usable without a lock-on sensor");
        TestSetupSelectable(0,1);
        if(wispModelSource!=null)
            Check(player.headPart.name=="Head"&&(IsArticulatedWisp?player.body.GetComponentInChildren<SkinnedMeshRenderer>(true)!=null:player.headPart.GetComponentInChildren<Renderer>(true)!=null)&&
                player.legsPart.Find("Left_thigh/Left_knee/Left_ankle")!=null&&
                player.headPart.parent==player.corePart.parent&&player.headPart.IsChildOf(player.body),
                "WISP uses artist-authored head and compact articulated leg structure");
        else
            Check(player.headPart.localScale.x>1.8f && player.legsPart.localScale.y<.6f &&
                player.corePart.localScale.x<1f && player.headPart.localScale.x/player.corePart.localScale.x>2f,
                "WISP has oversized head, compact torso and short SD limbs");
        Check(enemy.treadLinks!=null && enemy.treadLinks.Length==2 && enemy.treadLinks[0].Length>=20,
            "BASTION has two continuous animated track runs");
        Check(enemy.corePart.Find("Bastion square sensor turret")!=null &&
            enemy.leftArm.Find("Bastion shoulder missile tray")!=null && enemy.pelvisPart.gameObject.activeSelf==false,
            "BASTION reads as a tracked siege chassis with twin command pods and missile trays");
        Vector3 firstTread=enemy.treadLinks[0][0].localPosition;
        enemy.velocity=Vector3.forward*4f; AnimateTreads(enemy,.1f);
        Check((enemy.treadLinks[0][0].localPosition-firstTread).sqrMagnitude>.01f,
            "BASTION tread shoes scroll with forward movement");
        TestSetupSelectable(2,3);
        Check(player.quadrupedLegs!=null && player.quadrupedLegs.Length==4 && player.quadrupedKnees.Length==4 &&
            player.spec.jumpHeightScale>1.4f && jumpMaxHeight*player.spec.jumpHeightScale>9f,
            "TARANTULA four-leg rig and raised jump ceiling");
        Check(player.armsPart!=null&&!player.armsPart.gameObject.activeSelf&&player.weaponPart.parent==player.legsPart&&player.gun.gameObject.activeInHierarchy,
            "TARANTULA keeps its gun on a visible chassis mount");
        Check(player.legsPart.Find("Tarantula rear abdomen shell")!=null &&
            player.headPart.Find("Tarantula optic array")!=null &&
            player.quadrupedLegs[0].Find("Upper leg armor").localRotation!=Quaternion.identity,
            "TARANTULA has a low spider carapace, sensor cluster and splayed limbs");
        Check(enemy.quadrupedLegs!=null && enemy.quadrupedLegs.Length==4 && enemy.spec.name=="HOUND",
            "HOUND canine four-leg rig");
        Check(enemy.headPart.Find("Hound muzzle")!=null && enemy.headPart.Find("Hound upright ear")!=null &&
            enemy.legsPart.Find("Hound tail tip")!=null &&
            enemy.quadrupedLegs[0].Find("Knee articulation/Ankle articulation/Padded paw")!=null,
            "HOUND has canine muzzle, upright ears, segmented tail and paws");
        Vector3 jumpStart=new Vector3(0,FighterGroundY,0);
        bool clearJumpPath=!ObstacleOnSegment(jumpStart,jumpStart+Vector3.up*9f,player.Radius,out _);
        for(int radius=10;radius<=120&&!clearJumpPath;radius+=10)
            for(int angle=0;angle<16&&!clearJumpPath;angle++)
            {
                float radians=angle*Mathf.PI/8f;
                Vector3 candidate=new Vector3(Mathf.Cos(radians)*radius,FighterGroundY,Mathf.Sin(radians)*radius);
                clearJumpPath=!ObstacleOnSegment(candidate,candidate+Vector3.up*9f,player.Radius,out _);
                if(clearJumpPath) jumpStart=candidate;
            }
        player.root.position=jumpStart; player.jumpOriginY=jumpStart.y;
        player.jumpVelocity=jumpSpeed*player.spec.jumpImpulseScale/Mathf.Sqrt(Mathf.Max(.5f,player.spec.jumpWeightFactor));
        float tarantulaPeak=0f;
        for(int tick=0;tick<120;tick++) { Move(player,Vector3.zero,.02f); tarantulaPeak=Mathf.Max(tarantulaPeak,player.jumpHeight); }
        Check(clearJumpPath&&tarantulaPeak>4.5f&&tarantulaPeak<jumpMaxHeight*player.spec.jumpHeightScale,
            "TARANTULA reaches the higher jump arc in motion");
        TestSetupSelectable(0,1);
        Check(ValidTuning(float.NaN)==1&&ValidTuning(float.PositiveInfinity)==1&&ValidTuning(-9)==.5f&&ValidTuning(9)==2,"invalid saved tuning is sanitized");
        for(int rival=0;rival<CraftCatalog.Selectable.Length;rival++)
        {
            rivalSelection=rival; selectedCraft=rival; BeginMatch();
            Check(enemyCraft==0&&selectedCraft==0&&enemy.spec!=CraftCatalog.Selectable[0]&&
                enemy.spec.name=="WISP"&&player.spec.name=="WISP","locked player and rival selection resolves to WISP "+rival);
        }
        rivalSelection=-1;
        bool validRandom=true;
        for(int roll=0;roll<32;roll++) { int candidate=ResolveRival(); validRandom&=candidate>=0&&candidate<CraftCatalog.UnlockedCount; }
        Check(validRandom,"random rival selects only unlocked frames");
        // Engine foundation regressions: team-aware roster and explicit AI actor.
        TestSetup();
        Check(fighterRoster.Count==2 && Opponent(player)==enemy && Opponent(enemy)==player,
            "roster preserves current 1v1 opponents");
        float farthestCover=0; int nearbyCover=0;
        foreach(Debris piece in debris)
        {
            float radius=new Vector2(piece.origin.x,piece.origin.z).magnitude;
            farthestCover=Mathf.Max(farthestCover,radius);
            if(radius<50) nearbyCover++;
        }
        Check(debris.Count>=24&&farthestCover>180&&nearbyCover<=6,"ground cover is distributed beyond the central combat pad");
        int savedRivalTeam=enemy.teamId;
        enemy.teamId=player.teamId;
        Check(Opponent(player)==null && Opponent(enemy)==null,"same-team fighters are not opponents");
        enemy.teamId=savedRivalTeam;
        // A third fighter can join the roster without becoming the fixed rival.
        Fighter extraHostile=new Fighter { teamId=1,root=new GameObject("Roster probe").transform };
        extraHostile.root.position=player.root.position+Vector3.right*2;
        RegisterFighter(extraHostile);
        Check(fighterRoster.Count==3 && Opponent(player)==extraHostile,
            "nearest hostile is selected from a multi-fighter roster");
        UnregisterFighter(extraHostile);
        Destroy(extraHostile.root.gameObject);
        Check(fighterRoster.Count==2 && Opponent(player)==enemy,"unregister restores 1v1 roster");
        Fighter originalPilot=player,originalRival=enemy;
        EnemyAI(player,enemy,.04f);
        Check(player==originalPilot && enemy==originalRival && player.aiClock>0 && enemy.aiClock==0,
            "AI can control pilot without swapping global fighter identities");
        var primaryKinds=new HashSet<PrimaryKind>(); var specialKinds=new HashSet<SpecialKind>(); var boostKinds=new HashSet<BoostKind>();
        int[] initial={2,1,1,2,7,1,1,1},total={3,1,6,2,7,1,3,1};
        for(int i=0;i<8;i++)
        {
            TestSetup(i);
            CraftSpec spec=player.spec; primaryKinds.Add(spec.primary); specialKinds.Add(spec.special); boostKinds.Add(spec.boost);
            Check(player.hp==spec.health && Mathf.Abs(player.shieldEnergy-spec.ShieldMax)<.01f,"frame initialization "+spec.name);
            enemy.root.position=new Vector3(175,1.1f,100);
            player.facing=Vector3.forward;
            int created=projectilesCreated;
            Attack(player,enemy,Vector3.forward);
            Check(projectilesCreated-created==initial[i] && player.weaponTimer>0,"initial pattern "+spec.name);
            int first=projectilesCreated;
            Attack(player,enemy,Vector3.forward);
            Check(first==projectilesCreated,"primary cooldown "+spec.name);
            if(shots.Count>0) Check(Mathf.Abs(shots[0].velocity.magnitude-spec.projectileSpeed)<.01f &&
                Mathf.Abs(shots[0].radius-spec.projectileRadius)<.01f,"bullet speed and radius "+spec.name);
            if(shots.Count>0) Check(shots[0].visual.GetComponentsInChildren<Renderer>().Length>=1&&shots[0].visual.GetComponent<Collider>()==null,"weapon visual uses analytical collision "+spec.name);
            TestStep(.55f);
            Check(projectilesCreated-created==total[i],"complete sequence "+spec.name);
            TestSetup(i);
            Special(player,enemy,Vector3.forward);
            int payload=shots.Count+pending.Count+beams.Count+(player.rushTime>0?1:0);
            Check(player.specialTimer>0&&payload>0,"special activates "+spec.name);
            Special(player,enemy,Vector3.forward);
            Check(payload==shots.Count+pending.Count+beams.Count+(player.rushTime>0?1:0),"special cooldown "+spec.name);
            TestStep(.3f);
            Check(!float.IsNaN(player.root.position.x)&&player.hp>0,"special simulates "+spec.name);
        }
        Check(primaryKinds.Count==8&&specialKinds.Count==8&&boostKinds.Count==3,"distinct weapon and boost concepts");
        TestSetup(0);
        player.facing=Vector3.forward;
        Attack(player,enemy,Vector3.back);
        Check(shots.Count==0&&player.weaponTimer==0,"out of arc primary blocked");
        bulletSpeedScale=1.4f; bulletSizeScale=1.5f; weaponCooldownScale=1.3f;
        Attack(player,enemy,Vector3.forward);
        Check(Mathf.Abs(shots[0].velocity.magnitude-player.spec.projectileSpeed*1.4f)<.01f&&
            Mathf.Abs(shots[0].radius-player.spec.projectileRadius*1.5f)<.01f&&
            Mathf.Abs(player.weaponTimer-player.spec.weaponCooldown*1.3f)<.01f,"menu weapon tuning affects simulation");
        bulletSpeedScale=bulletSizeScale=weaponCooldownScale=1;
        TestSetup(1);
        before=player.root.position;
        Special(player,enemy,Vector3.forward);
        float targetShield=(enemy.hp+enemy.shieldEnergy);
        Move(player,Vector3.right,.2f); UpdateCombat(.2f);
        Check(Vector3.Distance(before,player.root.position)<.01f&&(enemy.hp+enemy.shieldEnergy)==targetShield,"siege charge locks movement and warns without damage");
        TestStep(.85f);
        Check((enemy.hp+enemy.shieldEnergy)<targetShield,"charged heavy beam damages target");
        TestStep(1.2f);
        Check(beams.Count==0&&player.lockTime==0,"beam finishes and releases movement");
        TestSetup(5);
        Special(player,enemy,Vector3.forward);
        TestStep(.9f);
        Check((enemy.hp+enemy.shieldEnergy)<enemy.spec.health+enemy.spec.ShieldMax&&beams.Count==0,"charge lance fires and expires");
        TestSetup(2);
        enemy.root.position=player.root.position+Vector3.forward*12;
        Special(player,enemy,Vector3.forward);
        Check(player.saber.gameObject.activeInHierarchy&&
            player.saber.GetComponentsInChildren<Renderer>(true).Length>0,
            "WISP saber rush activates the delivered sword model");
        TestStep(.6f);
        float afterRush=enemy.hp+enemy.shieldEnergy;
        Check(afterRush<enemy.spec.health+enemy.spec.ShieldMax && player.rushTime==0,"saber rush reaches and strikes");
        TestStep(.4f);
        Check(Mathf.Abs(enemy.hp+enemy.shieldEnergy-afterRush)<.01f,"saber cannot hit repeatedly");
        TestSetup(6);
        enemy.root.position=player.root.position+Vector3.forward*60;
        Special(player,enemy,Vector3.forward);
        TestStep(1.0f);
        bool waiting=shots.Count==3;
        foreach(Shot s in shots) waiting&=s.fuse>=0&&s.velocity==Vector3.zero&&s.warning!=null;
        Check(waiting,"homing lasers stop and telegraph explosion");
        enemy.root.position+=Vector3.right*60; targetShield=(enemy.hp+enemy.shieldEnergy);
        TestStep(1);
        Check(shots.Count==0&&(enemy.hp+enemy.shieldEnergy)==targetShield,"homing explosion can be evaded");
        TestSetup(7);
        Special(player,enemy,Vector3.forward);
        Check(shots.Count==5,"minefield deployment");
        enemy.root.position=shots[0].visual.position;
        TestStep(.9f);
        bool fuse=false; foreach(Shot s in shots) fuse|=s.fuse>=0;
        Check(fuse,"proximity mines warn before detonation");
        targetShield=(enemy.hp+enemy.shieldEnergy); TestStep(.6f);
        Check((enemy.hp+enemy.shieldEnergy)<targetShield,"mine explosion damage");
        TestSetup(2);
        float hull=player.hp,shield=player.shieldEnergy,fuel=player.boostEnergy;
        Hit(player,enemy.root.position,10);
        Check(player.hp==hull&&player.shieldEnergy<shield&&player.boostEnergy==fuel,"auto shield absorbs independently");
        shield=player.shieldEnergy;
        Move(player,Vector3.zero,2.9f);
        Check(player.shieldEnergy==shield,"shield recovery delay");
        Move(player,Vector3.zero,.6f);
        Check(player.shieldEnergy>shield,"shield automatically recovers");
        player.shieldEnergy=2; hull=player.hp;
        Hit(player,enemy.root.position,20);
        Check(player.shieldEnergy==0&&player.hp<hull,"shield overflow reaches hull");
        Move(player,Vector3.zero,10);
        Check(player.shieldEnergy==player.spec.ShieldMax&&player.hp<hull,"shield refills without healing hull");
        TestSetup(3);
        before=player.root.position; player.boostHeld=true;
        Move(player,Vector3.forward,.1f);
        float approach=Vector3.Distance(before,player.root.position);
        TestSetup(3); before=player.root.position; player.boostHeld=true;
        Move(player,Vector3.back,.1f);
        float retreat=Vector3.Distance(before,player.root.position);
        Check(approach>retreat&&approach>player.spec.boostSpeed*.1f,"approach boost faster than retreat");
        float energy=player.boostEnergy; player.boostHeld=false; before=player.root.position;
        Move(player,Vector3.forward,.1f);
        Check(Mathf.Abs(Vector3.Distance(before,player.root.position)-player.spec.speed*player.spec.forwardSpeedFactor*.1f)<.01f&&player.boostEnergy==energy,"cruise release restores normal directional speed");
        TestSetup(2); Vector3 boostStart=Vector3.zero; bool clearBoostPath=false;
        for(int radius=10;radius<=120&&!clearBoostPath;radius+=10)
            for(int angle=0;angle<16&&!clearBoostPath;angle++)
            {
                float radians=angle*Mathf.PI/8f;
                Vector3 candidate=new Vector3(Mathf.Cos(radians)*radius,FighterGroundY,Mathf.Sin(radians)*radius);
                bool clear=true;
                for(int step=0;step<=80&&clear;step++)
                    clear=!Occupied(candidate+Vector3.forward*(step*.1f),player.Radius);
                if(clear && !ObstacleOnSegment(candidate,candidate+Vector3.forward*8f,player.Radius,out _))
                { boostStart=candidate; clearBoostPath=true; }
            }
        Check(clearBoostPath,"boost release regression uses an obstacle-free path");
        player.root.position=boostStart; enemy.root.position=boostStart+Vector3.forward*25;
        player.facing=Vector3.forward; player.boostHeld=true; before=player.root.position; float pulseEnergy=player.boostEnergy;
        Move(player,Vector3.forward,.1f);
        Check(player.boosting&&Vector3.Distance(before,player.root.position)>player.spec.speed*.1f&&player.boostEnergy<pulseEnergy,"held boost accelerates and drains fuel");
        player.boostHeld=false; before=player.root.position; Move(player,Vector3.forward,.1f);
        float normalForwardSpeed=(player.spec.usePrototypeMovement?player.spec.prototypeBaseSpeed:player.spec.speed)*player.spec.forwardSpeedFactor;
        Check(!player.boosting&&Mathf.Abs(Vector3.Distance(before,player.root.position)-normalForwardSpeed*.1f)<.01f,
            "releasing boost returns to the prototype's normal directional speed");
        TestSetup(0); player.boostHeld=true; Move(player,Vector3.right,.1f);
        float firstSpeed=player.velocity.magnitude;
        Move(player,Vector3.right,.1f);
        Check(player.velocity.magnitude>firstSpeed,"glide accelerates smoothly");
        player.boostHeld=false; Move(player,Vector3.zero,.1f);
        Check(player.velocity.magnitude>0&&player.velocity.magnitude<firstSpeed,"glide has controlled drift");
        TestSetup(3); player.boostHeld=true; Move(player,Vector3.right,.1f); Animate(player);
        Check(player.thruster.gameObject.activeSelf&&Vector3.Dot(player.thruster.forward,player.velocity.normalized)<-.99f,"exhaust points opposite travel");
        player.boostEnergy=.01f; Move(player,Vector3.right,.1f);
        Check(player.boostLocked&&!player.boosting,"boost exhaustion");
        player.boostHeld=false; Move(player,Vector3.zero,1.5f);
        Check(!player.boostLocked&&player.boostEnergy>0,"boost release and recovery");
        // Variable-height jump and shared horizontal/vertical boost regression.
        TestSetup(0);
        Check(Mathf.Abs(player.spec.boostCapacity-240f)<.01f,"boost capacity increased by 50 percent");
        player.jumpVelocity=jumpSpeed; player.jumpHeld=false;
        for(int i=0;i<20;i++) Move(player,Vector3.zero,.025f);
        float tapHeight=player.jumpHeight;
        Check(tapHeight>1.7f&&tapHeight<2.6f,"a single jump clears low cover without an excessive leap");
        TestSetup(0);
        player.jumpOriginY=player.root.position.y; player.jumpVelocity=jumpSpeed; player.jumpHeld=true;
        float jumpFuel=player.boostEnergy;
        float heldMaxHeight=0, fuelAtHeldMax=jumpFuel;
        for(int i=0;i<240;i++)
        {
            Move(player,Vector3.zero,.025f);
            if(player.jumpHeight>heldMaxHeight) { heldMaxHeight=player.jumpHeight; fuelAtHeldMax=player.boostEnergy; }
        }
        Check(heldMaxHeight>tapHeight && heldMaxHeight<=jumpMaxHeight+.02f && fuelAtHeldMax<jumpFuel,
            "held jump climbs higher, spends fuel, and stays below the height cap");
        TestSetup(0);
        player.jumpVelocity=jumpSpeed; player.jumpHeld=true; player.boostHeld=true;
        float combinedFuel=player.boostEnergy;
        Move(player,Vector3.forward,.1f);
        Check(player.boostEnergy<combinedFuel-(player.spec.boostDrain*.1f),"horizontal and vertical boost share fuel");
        player.jumpHeld=false; player.boostHeld=false;
        float airborneHeight=player.jumpHeight;
        for(int i=0;i<160;i++) Move(player,Vector3.zero,.025f);
        Check(player.jumpHeight<airborneHeight || player.jumpHeight==0,"released jump falls under gravity");
        TestSetup(0);
        Bounds cornerTest=new Bounds(new Vector3(100,2.6f,100),new Vector3(4,3,4)); obstacles.Add(cornerTest);
        Check(!Occupied(new Vector3(102.9f,2,102.9f),player.Radius),"rounded collision footprint does not snag diagonal corner gaps");
        obstacles.RemoveAt(obstacles.Count-1);
        TestSetup(0); player.root.position=new Vector3(100,3.1f,100); player.jumpOriginY=1.1f;
        player.jumpHeight=2; player.jumpVelocity=0; player.jumpHeld=false;
        int landingTicks=0;
        while(player.root.position.y>1.101f && landingTicks<100) { Move(player,Vector3.zero,.02f); landingTicks++; }
        Check(landingTicks<=22,"falling jump lands quickly under stronger gravity");
        TestSetup(0); player.jumpOriginY=player.root.position.y; player.jumpVelocity=jumpSpeed; player.jumpHeld=true;
        Move(player,Vector3.zero,.04f); Animate(player);
        Check(player.thruster.gameObject.activeSelf&&player.jumpFlame.gameObject.activeSelf,
            "ascending Space jump shows a bright downward booster exhaust");
        player.jumpHeld=false;
        TestSetup(3); before=player.root.position; Move(player,Vector3.right,1);
        Check(player.root.position.x>100,"open space has no square boundary");
        Vector3 rockBefore=debris[0].visual.position; UpdateObstacles(3);
        Check(Vector3.Distance(rockBefore,debris[0].visual.position)<.001f&&
            Mathf.Abs(obstacles[0].min.y-GroundSurfaceY)<.01f&&
            Mathf.Abs(obstacles[0].max.y-(debris[0].origin.y+debris[0].size.y*.5f))<.01f,
            "ground cover base and collision bounds match the visible floor and roof");
        player.root.position=obstacles[0].center; ResolveOverlap(player);
        Check(!Occupied(player.root.position,player.Radius),"drifting asteroid overlap has an escape");
        TestSetup(3);
        int added=obstacles.Count;
        obstacles.Add(new Bounds(new Vector3(100,1.1f,110),new Vector3(6,8,4)));
        player.boostHeld=true; Move(player,Vector3.forward,1);
        Check(player.root.position.z<107&& !Occupied(player.root.position,player.Radius),"boost cannot tunnel through asteroid");
        player.root.position=new Vector3(100,1.1f,100); player.facing=Vector3.forward;
        Fire(player,Vector3.forward,40,60,.3f,70);
        targetShield=(enemy.hp+enemy.shieldEnergy); UpdateCombat(.5f);
        Check((enemy.hp+enemy.shieldEnergy)==targetShield&&shots.Count==0,"asteroid blocks projectile before opponent");
        obstacles.RemoveAt(added);
        // Heavy frames should climb the deliberately low access ramp, and a
        // landed fighter must remain on a rooftop instead of being ejected.
        TestSetup(1); player.root.position=new Vector3(-28,1.1f,20); player.facing=Vector3.right;
        float highestRampPosition=player.root.position.y;
        for(int tick=0;tick<120;tick++)
        {
            Move(player,Vector3.right,.025f);
            highestRampPosition=Mathf.Max(highestRampPosition,player.root.position.y);
        }
        Check(highestRampPosition>3.0f&&player.root.position.x>3.5f,
            "heavy frame climbs the ramp and crosses the raised deck without sticking");
        TestSetup(0);
        Bounds testRoof=new Bounds(new Vector3(100,.4f,108),new Vector3(6,3,6));
        obstacles.Add(testRoof); player.root.position=new Vector3(100,1.1f,103.5f); player.jumpOriginY=player.root.position.y; player.jumpHeld=true; player.jumpVelocity=jumpSpeed;
        for(int tick=0;tick<60;tick++)
            Move(player,player.root.position.z<107.5f?Vector3.forward:Vector3.zero,.02f);
        player.velocity=Vector3.zero;
        player.jumpHeld=false;
        for(int tick=0;tick<180;tick++) Move(player,Vector3.zero,.02f);
        Vector3 landed=player.root.position;
        Check(Mathf.Abs(landed.y-(testRoof.max.y+FighterSupportOffset))<.12f&&landed.x>testRoof.min.x+player.Radius&&
            landed.x<testRoof.max.x-player.Radius&&landed.z>testRoof.min.z+player.Radius&&
            landed.z<testRoof.max.z-player.Radius&&!Occupied(landed,player.Radius),"fighter can land and stay on an obstacle roof");
        obstacles.RemoveAt(obstacles.Count-1); TestSetup();
        // Put the practice rival at a blocked corner and verify it finds an open route.
        difficulty=0; TestSetup(0,1); enemy.root.position=new Vector3(100,1.1f,98); player.root.position=new Vector3(100,1.1f,130);
        obstacles.Add(new Bounds(new Vector3(100,2.6f,103),new Vector3(12,3,4)));
        obstacles.Add(new Bounds(new Vector3(106,2.6f,100),new Vector3(4,3,12)));
        Vector3 aiStart=enemy.root.position;
        for(int tick=0;tick<240;tick++) EnemyAI(enemy,player,.025f);
        Check(Vector3.Distance(aiStart,enemy.root.position)>12&&enemy.aiMove.sqrMagnitude>.1f,"practice AI keeps moving and escapes a blocked corner");
        obstacles.RemoveRange(obstacles.Count-2,2); TestSetup();
        TestSetup();
        enemy.root.position=new Vector3(38,1.1f,38); player.root.position=new Vector3(32,1.1f,32);
        float traveled=0;
        for(int i=0;i<150&&!Finished;i++)
        {
            Vector3 p=enemy.root.position; EnemyAI(.04f); UpdateCombat(.04f);
            traveled+=Vector3.Distance(p,enemy.root.position);
        }
        Check(traveled>20&&Mathf.Abs(enemy.root.position.x)+Mathf.Abs(enemy.root.position.z)>50,"AI keeps moving near former map corner");
        TestSetupSelectable(0,1); player.facing=Vector3.forward;
        for(int i=0;i<12;i++) UpdatePlayerHeading(player,Vector3.right,Vector3.forward,false,false,.025f);
        Animate(player);
        Check(Vector3.Angle(player.facing,Vector3.forward)<1&&Vector3.Angle(player.root.forward,Vector3.forward)<1,
            "no-lock WISP keeps mouse-camera heading while moving sideways");
        Vector3 cameraStrafe=CameraRelativeMove(Vector3.forward,Vector3.right,1,0);
        Check(Vector3.Angle(cameraStrafe,Vector3.right)<.1f&&Mathf.Abs(Vector3.Dot(cameraStrafe,Vector3.forward))<.001f,
            "no-lock lateral input remains a camera-relative sidestep");
        TestSetupSelectable(1,0); player.facing=Vector3.forward;
        for(int i=0;i<12;i++) UpdatePlayerHeading(player,Vector3.right,Vector3.forward,false,true,.025f);
        Check(Vector3.Angle(player.facing,Vector3.right)<1,
            "lock-capable frame still turns toward movement while lock is released");
        difficulty=2;
        TestSetup();
        player.velocity=Vector3.right*12; enemy.smoothedTargetVelocity=player.velocity;
        Vector3 lead=AimLead(enemy,player);
        Check(lead.x>0,"AI leads a moving target");
        TestSetup();
        Fire(player,Vector3.forward,5,30,.2f,70);
        float standingRisk=ThreatAt(enemy,Vector3.zero,1);
        enemy.aiReaction=0; EnemyAI(.02f);
        float evasionRisk=ThreatAt(enemy,enemy.aiMove*enemy.spec.boostSpeed,1);
        Check(standingRisk>0&&evasionRisk<standingRisk,"AI chooses safer motion against incoming fire");
        TestSetup(0,1);
        enemy.aiClock=8; enemy.specialTimer=0; player.root.position=new Vector3(100,1.1f,100); enemy.root.position=new Vector3(100,1.1f,135);
        EnemyAI(.02f);
        Check(enemy.specialTimer>0,"AI uses special weapon");
        difficulty=0;
        TestSetup(); enemy.aiClock=0; EnemyAI(.02f);
        Check(enemy.weaponTimer==0 && enemy.aiReaction>.5f,"practice gives opening time and slower reactions");
        TestSetup(); enemy.aiClock=4; EnemyAI(.02f);
        Check(enemy.weaponTimer>enemy.spec.weaponCooldown,"practice lengthens AI firing intervals");
        TestSetup(); float unshielded=player.hp; Hit(player,enemy.root.position,10);
        Check(player.shieldEnergy==0 && player.hp<unshielded,"standard frames take direct hull damage");
        TestSetup(); player.facing=Vector3.forward; enemy.velocity=Vector3.right*8;
        Vector3 assisted=AssistAim(player,enemy,Vector3.forward);
        Check(assisted.x>0&&InArc(player,assisted),"assist leads nearby moving target within firing arc");
        Check(AssistAim(player,enemy,Vector3.back)==Vector3.back,"assist never snaps backward aim");
        aimAssist=false; Check(AssistAim(player,enemy,Vector3.forward)==Vector3.forward,"assist can be disabled"); aimAssist=true;
        TestSetupSelectable(0,1); player.facing=Vector3.forward;
        enemy.root.position=player.root.position+new Vector3(3,0,20); enemy.velocity=Vector3.right*8;
        Check(!CanAssist(player,enemy,Vector3.forward)&&AssistAim(player,enemy,Vector3.forward)==Vector3.forward&&
            FollowupAim(player,enemy,Vector3.forward,.2f,1)==Vector3.forward,
            "WISP never applies target snapping, leading or followup aim assistance");
        Special(player,enemy,Vector3.forward);
        Check(Vector3.Angle(player.rushDirection,Vector3.forward)<.01f,
            "WISP saber rush follows manual aim instead of steering toward rival");
        int protectedFrames=0;
        for(int i=0;i<8;i++)
        {
            TestSetup(i);
            if(player.spec.ShieldMax>0) { protectedFrames++; Check(player.spec.health<=300,"shield reserved for fragile frame"); }
            Transform expectedParent=player.spec.name=="WISP"&&wispModelSource!=null&&IsArticulatedWisp?player.body.Find("WISP artist model v003/WISP_Rig_v003/Root"):player.body;
            Check(player.headPart.parent==expectedParent&&player.corePart.parent==expectedParent&&player.armsPart.parent==expectedParent&&player.legsPart.parent==expectedParent&&player.weaponPart.IsChildOf(player.spec.name=="WISP"&&wispModelSource!=null?player.leftArm:player.rightArm),"detachable module hierarchy "+i);
            if(i==0)
            {
                Transform head=player.headPart.Find("Helmet"),chest=player.corePart.Find("Armor chest");
                Transform shoulder=player.rightArm.Find("Armor shoulder"),foot=null;
                foreach(Transform part in player.leftLeg.GetComponentsInChildren<Transform>())
                    if(part.name=="Foot") { foot=part; break; }
                Check(head!=null&&head.localScale.x>=.75f&&head.localScale.y>=.63f&&
                    chest!=null&&chest.localScale.x>=1.35f&&
                    shoulder!=null&&shoulder.localScale.x>=.77f&&
                    foot!=null&&foot.localScale.x>=.63f&&foot.localScale.z>=.91f,
                    "Kestrel SD silhouette enlarges head, torso, shoulders and feet (head="+(head!=null?head.localScale.ToString():"missing")+" chest="+(chest!=null?chest.localScale.ToString():"missing")+" shoulder="+(shoulder!=null?shoulder.localScale.ToString():"missing")+" foot="+(foot!=null?foot.localScale.ToString():"missing")+")");
            }
        }
        Check(protectedFrames==2,"only two light frames retain shield");
        for(int i=0;i<CraftCatalog.Selectable.Length;i++)
        {
            TestSetupSelectable(i,(i+1)%CraftCatalog.Selectable.Length);
            Check(player.spec.name==CraftCatalog.Selectable[i].name && player.selectableIndex==i,
                "playable prototype selection "+CraftCatalog.Selectable[i].name);
            Check(framePortraits[i]!=null && framePortraits[i].IsCreated(),"prototype portrait rendered "+i);
            if(i==0 && wispModelSource!=null)
            {
                Transform hand=player.leftArm.Find("Left_elbow/Left_hand");
                Transform rightHand=player.rightArm.Find("Right_elbow/Right_hand");
                bool articulated=IsArticulatedWisp;
                Transform artist=player.body.Find(articulated?"WISP artist model v003":"WISP artist model v001");
                Transform moduleParent=articulated?artist.Find("WISP_Rig_v003/Root"):player.body;
                Renderer visor=articulated?artist.GetComponentInChildren<Renderer>(true):player.headPart.GetComponentInChildren<Renderer>(true);
                Transform oldChest=player.body.Find("Core module/Armor chest");
                Check(player.headPart.name=="Head"&&player.corePart.name=="Core"&&
                    player.armsPart.name=="Arms"&&player.legsPart.name=="Legs"&&
                    player.headPart.parent==moduleParent&&player.corePart.parent==moduleParent&&
                    player.armsPart.parent==moduleParent&&player.legsPart.parent==moduleParent,
                    "WISP uses artist FBX with separate head/core/arms/legs modules");
                Check(rightHand!=null&&hand!=null&&player.saber.parent==rightHand.Find("SWORD_SOCKET")&&
                    player.weaponPart.parent==(articulated?hand:player.leftArm)&&
                    player.leftLeg.Find("Left_knee/Left_ankle")!=null&&
                    player.rightLeg.Find("Right_knee/Right_ankle")!=null,
                    "WISP imported arm, leg, weapon and saber pivots remain connected");
                Check(player.saber.name=="WISP sword v001"&&
                    player.rightArm.Find("Right_elbow/Right_hand/SWORD_SOCKET/WISP sword v001")!=null&&
                    player.rightArm.Find("Right_elbow/RIGHT_SHIELD_SOCKET/WISP right shield (guard pose)")!=null&&
                    !player.saber.gameObject.activeSelf,
                    "WISP archived sword remains connected and right-hand shield is attached");
                Check(visor!=null&&visor.sharedMaterial.mainTexture==wispBaseColorTexture&&
                    wispRuntimeMaterial!=null&&wispRuntimeMaterial.GetTexture("_EmissionMap")==wispEmissionTexture,
                    "WISP base-color and emission texture atlas maps are assigned (base="+(visor!=null&&visor.sharedMaterial.mainTexture!=null?visor.sharedMaterial.mainTexture.name:"null")+" shader="+(wispRuntimeMaterial!=null&&wispRuntimeMaterial.shader!=null?wispRuntimeMaterial.shader.name:"null")+" main="+(wispRuntimeMaterial!=null&&wispRuntimeMaterial.GetTexture("_MainTex")!=null?wispRuntimeMaterial.GetTexture("_MainTex").name:"null")+" emission="+(wispRuntimeMaterial!=null&&wispRuntimeMaterial.GetTexture("_EmissionMap")!=null?wispRuntimeMaterial.GetTexture("_EmissionMap").name:"null")+")");
                Check(oldChest!=null&&oldChest.GetComponent<Renderer>().enabled==false,
                    "WISP duplicate procedural torso armor is hidden");
                Animate(player);
                Bounds wispBounds=new Bounds(); bool hasWispBounds=false;
                foreach(Transform module in articulated?new[]{artist}:new[]{player.armsPart,player.corePart,player.headPart,player.legsPart})
                    foreach(Renderer partRenderer in module.GetComponentsInChildren<Renderer>(true))
                        if(partRenderer.enabled&&partRenderer.gameObject.activeInHierarchy)
                        {
                            if(partRenderer is SkinnedMeshRenderer skin)
                            {
                                Vector3[] vertices=skin.sharedMesh.vertices;
                                BoneWeight[] weights=skin.sharedMesh.boneWeights;
                                Matrix4x4[] bind=skin.sharedMesh.bindposes;
                                for(int vertex=0;vertex<vertices.Length;vertex++)
                                {
                                    BoneWeight weight=weights[vertex];
                                    Vector3 world=(skin.bones[weight.boneIndex0].localToWorldMatrix*bind[weight.boneIndex0]).MultiplyPoint3x4(vertices[vertex]);
                                    if(!hasWispBounds) { wispBounds=new Bounds(world,Vector3.zero); hasWispBounds=true; }
                                    else wispBounds.Encapsulate(world);
                                }
                            }
                            else
                            {
                                if(!hasWispBounds) { wispBounds=partRenderer.bounds; hasWispBounds=true; }
                                else wispBounds.Encapsulate(partRenderer.bounds);
                            }
                        }
                Check(hasWispBounds&&Mathf.Abs(wispBounds.min.y-GroundSurfaceY)<.01f&&
                    Mathf.Abs(player.body.localPosition.y)<.001f,
                    "WISP sole vertices rest on the arena floor without idle hover (minY="+wispBounds.min.y.ToString("F3")+" floor="+GroundSurfaceY.ToString("F3")+")");
                Check(wispModelSource.name=="WISP_v005"?Mathf.Abs(wispBounds.size.y-3.778f)<.025f:wispBounds.size.y>3.8f&&wispBounds.size.y<4.6f,
                    "WISP imported visual keeps arena robot scale (height="+wispBounds.size.y.ToString("F3")+")");
                Vector3 restingShield=player.guardShield.position;
                player.guarding=true; player.guardBlend=1; Animate(player);
                Check(Vector3.Dot(player.guardShield.forward,player.root.forward)>.99f&&
                    Vector3.Dot(player.guardShield.position-restingShield,player.root.forward)>.1f&&
                    Quaternion.Angle(player.rightArm.localRotation,Quaternion.identity)>30f,
                    "WISP right arm raises shield forward with its face toward the aim direction");
                player.guarding=false; player.guardBlend=0; Animate(player);
                Check(Vector3.Distance(restingShield,player.guardShield.position)<.01f&&player.weaponPart.IsChildOf(player.leftArm),
                    "WISP shield returns to rest and gun remains mounted on left hand");
                if(articulated)
                {
                    Transform knee=player.leftLeg.Find("Left_knee");
                    SkinnedMeshRenderer driven=null;
                    foreach(SkinnedMeshRenderer skin in artist.GetComponentsInChildren<SkinnedMeshRenderer>())
                        foreach(BoneWeight weight in skin.sharedMesh.boneWeights)
                            if(weight.weight0>.99f&&skin.bones[weight.boneIndex0]==knee) { driven=skin; break; }
                    Check(driven!=null&&artist.GetComponentsInChildren<SkinnedMeshRenderer>().Length==112,
                        "WISP v003 has actual weighted mechanical joint meshes");
                    if(driven!=null)
                    {
                        Mesh a=new Mesh(),b=new Mesh(); driven.BakeMesh(a);
                        Quaternion rest=knee.localRotation; knee.localRotation=Quaternion.Euler(24,0,0)*rest;
                        driven.BakeMesh(b); float displacement=0;
                        for(int v=0;v<a.vertexCount;v++) displacement=Mathf.Max(displacement,Vector3.Distance(a.vertices[v],b.vertices[v]));
                        knee.localRotation=rest; Destroy(a); Destroy(b);
                        Check(displacement>.02f,"WISP knee rotation deforms the real skinned mesh");
                    }
                }
            }
        }
        TestSetup(0,1);
        TestSetup(); SetPaused(false); ToggleGuide();
        Check(showGuide&&paused,"guide pauses an active battle");
        CloseGuide(); Check(!showGuide&&!paused,"closing guide resumes previously active battle");
        SetPaused(true); ToggleGuide(); CloseGuide();
        Check(paused,"guide preserves a preexisting pause"); SetPaused(false);
        enemy.hp=0; ToggleGuide(); CloseGuide();
        Check(!paused&&Finished,"guide preserves result screen state"); TestSetup();
        foreach(bool boost in new[]{false,true})
        foreach(float reaction in new[]{0f,.2f,.4f})
        {
            TestSetup(0,3); player.facing=Vector3.forward; enemy.facing=Vector3.back;
            Attack(enemy,player,Vector3.back);
            for(int tick=0;tick<120;tick++)
            {
                player.boostHeld=boost&&tick*.02f>=reaction;
                Move(player,tick*.02f>=reaction?Vector3.right:Vector3.zero,.02f);
                Move(enemy,Vector3.zero,.02f); UpdateCombat(.02f); UpdateSparks(.02f);
            }
            Debug.Log($"MISSILE_EVASION boost={boost} reaction={reaction:0.0} damage={damageTaken:0.00}");
            if(boost&&reaction==.2f) Check(damageTaken==0,"practice missile volley can be evaded after a short reaction");
            if(boost&&reaction==.4f) Check(damageTaken>0,"late missile dodge still has consequences");
            yield return null;
        }
        TestSetup(0,3); enemy.facing=Vector3.back; Attack(enemy,player,Vector3.back); TestStep(.3f);
        Check(shots.Count==2&&shots.TrueForAll(shot=>shot.tracking==0),"practice missiles commit to terminal straight flight");
        difficulty=1; TestSetup(0,3); enemy.facing=Vector3.back; Attack(enemy,player,Vector3.back); TestStep(.3f);
        Check(shots.Count==2&&shots.TrueForAll(shot=>shot.tracking>0),"standard missiles retain guidance");
        difficulty=0; TestSetup(3,0); Attack(player,enemy,Vector3.forward); TestStep(.3f);
        Check(shots.Count==2&&shots.TrueForAll(shot=>shot.tracking>0),"player missile guidance is unchanged");
        // Reproducible single-volley range trials, not AI-vs-AI win-rate claims.
        foreach(int frame in new[]{0,1,2,3,4,5,6,7})
        foreach(float range in new[]{15f,25f,40f})
        foreach(float lateral in new[]{0f,12f})
        {
            if(range>CraftCatalog.All[frame].range) continue;
            float[] damage=new float[2];
            for(int assistedTrial=0;assistedTrial<2;assistedTrial++)
            {
                TestSetup(frame); enemy.root.position=player.root.position+Vector3.forward*range;
                enemy.velocity=Vector3.right*lateral; aimAssist=assistedTrial==1;
                Attack(player,enemy,AssistAim(player,enemy,Vector3.forward));
                for(int tick=0;tick<110;tick++)
                {
                    enemy.root.position+=enemy.velocity*.02f;
                    Move(player,Vector3.zero,.02f); UpdateCombat(.02f); UpdateSparks(.02f);
                }
                damage[assistedTrial]=damageDealt;
                yield return null;
            }
            Debug.Log($"RANGE_TRIAL frame={frame} range={range} lateral={lateral} manual={damage[0]:0.00} assist={damage[1]:0.00}");
        }
        aimAssist=true;
        TestSetup(); enemy.velocity=Vector3.right*12;
        Vector3 committed=AssistAim(player,enemy,Vector3.forward);
        Vector3 followup=FollowupAim(player,enemy,committed,.16f,1.15f);
        Check(Vector3.Angle(committed,followup)<=6.01f&&InArc(player,followup),"delayed center shot has bounded lead correction");
        Check(FollowupAim(enemy,player,enemy.facing,.16f,1.15f)==enemy.facing,"player followup assistance never buffs AI");
        TestSetup(); player.facing=Vector3.forward; canShoot=true;
        Check(CanAssist(player,enemy,Vector3.forward),"stationary target still displays active assistance");
        assistEngaged=true; player.weaponTimer=1;
        Check(AimStatus().StartsWith("PRIMARY COOLDOWN")&&!AimStatus().Contains("READY"),"fire interval is labeled cooldown and cannot falsely promise ready fire");
        enemy.root.position+=Vector3.forward*100;
        Check(AimStatus().Contains("OUT OF PRIMARY RANGE")&&!CanAssist(player,enemy,Vector3.forward),"range status and assistance agree");
        TestSetup(); canShoot=false;
        Check(AimStatus()=="OUTSIDE FIRING ARC","backward aim gives actionable feedback");
        TestSetup(); float damageBefore=enemy.hp;
        Hit(enemy,player.root.position,20);
        Check(hitConfirmTime>0&&Mathf.Abs(damageDealt-(damageBefore-enemy.hp))<.001f&&damageTaken==0,"confirmed damage uses actual armored hull loss");
        TestSetup(0,2); Hit(enemy,player.root.position,10);
        Check(lastHitShield&&enemy.hp==enemy.spec.health&&damageDealt>0,"shield hit confirmation distinguishes absorption");
        TestSetup(); Check(damageDealt==0&&hitConfirmTime==0&&!assistEngaged,"rematch clears hit feedback");
        TestSetup(); UpdateCamera(10);
        Check(!CameraOccludes(player),"clear pilot needs no cover marker");
        Vector3 sightPoint=player.root.position+Vector3.up*.6f-view.transform.forward*5;
        obstacles.Add(new Bounds(sightPoint,Vector3.one*3));
        Check(CameraOccludes(player),"orthographic occlusion detects cover in front of pilot");
        obstacles.RemoveAt(obstacles.Count-1);
        Vector3 savedCameraPosition=view.transform.position;
        Quaternion savedCameraRotation=view.transform.rotation;
        float savedNearClip=view.nearClipPlane;bool savedCameraOrtho=view.orthographic;
        TestSetupSelectable(0,1);
        Bounds cameraWall=new Bounds(new Vector3(100,3,96.5f),new Vector3(20,8,2));
        obstacles.Add(cameraWall);
        Vector3 cameraFocus=player.root.position+Vector3.up*1.4f;
        Vector3 constrained=ConstrainCamera(cameraFocus,cameraFocus+Vector3.back*8.5f,.4f);
        Check(constrained.z>cameraWall.max.z+.4f&&Vector3.Distance(cameraFocus,constrained)<2.2f,
            "camera retracts before close wall without forcing a minimum boom through it");
        bool savedSmoke=smoke; smoke=false; targetLock=false; cameraYaw=0; cameraPitch=DefaultThirdPersonPitch;
        view.transform.position=cameraFocus+Vector3.back*10;
        UpdateCamera(.016f);
        Check(Vector3.Distance(view.transform.position,ConstrainCamera(cameraFocus,view.transform.position,.4f))<.001f,
            "camera collision corrects smoothing immediately even when previous position is behind wall");
        for(int frame=0;frame<60;frame++) UpdateCamera(.025f);
        Vector3 pilotViewport=view.WorldToViewportPoint(player.root.position);
        Check(pilotViewport.z>0&&pilotViewport.x>0&&pilotViewport.x<1&&pilotViewport.y>0&&pilotViewport.y<1,
            "close-wall camera keeps pilot inside view");
        Check(!VisibleWispHeadBlocksReticle(),"wall-side camera leaves the reticle visible with close head transparency");
        smoke=savedSmoke; obstacles.RemoveAt(obstacles.Count-1);
        smoke=false;
        foreach(float pitch in new[]{-5f,DefaultThirdPersonPitch,45f})
        foreach(float yaw in new[]{0f,90f})
        {
            cameraPitch=pitch; cameraYaw=yaw;
            for(int frame=0;frame<80;frame++) UpdateCamera(.025f);
            Check(!HeadOverlapsCenterReticle(player),"WISP head and crosshair stay separated at camera pitch "+pitch+" yaw "+yaw);
        }
        smoke=savedSmoke;
        Vector3 perimeterFocus=new Vector3(0,5,-138.9f);
        Check(cameraSolids.Count>=4&&ConstrainCamera(perimeterFocus,perimeterFocus+Vector3.back*9,.4f).z>-140f,
            "camera remains inside visible arena boundary walls");
        TestSetup();
        view.transform.SetPositionAndRotation(savedCameraPosition,savedCameraRotation); view.nearClipPlane=savedNearClip;view.orthographic=savedCameraOrtho;
        float largest=CraftCatalog.All[1].size*CraftRadius;
        bool routes=true;
        // All four cardinal exits from the central open pad must fit the heaviest frame.
        foreach(Vector3 direction in new[]{Vector3.forward,Vector3.back,Vector3.left,Vector3.right})
            routes&=!ObstacleOnSegment(new Vector3(0,1.1f,0),new Vector3(0,1.1f,0)+direction*11,largest,out _);
        Check(routes,"heavy frame has four exits from central pad");
        bool separation=true;
        for(int a=0;a<debris.Count;a++) for(int b=a+1;b<debris.Count;b++)
        {
            Bounds expanded=obstacles[a]; expanded.Expand(new Vector3(largest*2,0,largest*2));
            separation&=!expanded.Intersects(obstacles[b]);
        }
        Check(separation,"primary cover leaves clearance for heavy frames");
        // Exercise complete rounds with movement, abilities, obstacle avoidance and resets.
        // This is a scripted pilot, not a claim about human win rates.
        for(int frame=0;frame<8;frame++)
        {
            ApplyCharacter(player,frame); ApplyCharacter(enemy,(frame+3)%8); ResetRound(); inMenu=false;
            float elapsed=0;
            for(int tick=0;tick<1200&&!Finished;tick++)
            {
                const float dt=.025f; elapsed+=dt; roundTime+=dt; UpdateObstacles(dt);
                Vector3 toward=(enemy.root.position-player.root.position).normalized;
                float distance=Vector3.Distance(player.root.position,enemy.root.position);
                Vector3 move=distance>player.spec.range*.48f?toward:Vector3.Cross(toward,Vector3.up);
                player.boostHeld=distance>player.spec.range*.65f && tick%120<60;
                Move(player,move,dt); FaceOpponent(player,enemy);
                Vector3 aim=AssistAim(player,enemy,toward);
                Attack(player,enemy,aim);
                if(tick%240==0) Special(player,enemy,aim);
                EnemyAI(dt); UpdateCombat(dt); UpdateSparks(dt); Animate(player); Animate(enemy);
                if(tick%120==0) yield return null;
            }
            Check(!float.IsNaN(player.hp)&&!float.IsNaN(enemy.root.position.x)&&!Occupied(player.root.position,player.Radius)&&!Occupied(enemy.root.position,enemy.Radius),"sustained ground duel "+frame);
            Debug.Log($"DUEL_SAMPLE {player.spec.name} vs {enemy.spec.name} duration={elapsed:0.0} pilotHP={player.hp:0.0} rivalHP={enemy.hp:0.0}");
            yield return null;
        }
        bool audioGood=soundClips.Count==System.Enum.GetValues(typeof(SoundKind)).Length;
        foreach(AudioClip clip in soundClips.Values)
        {
            float[] samples=new float[clip.samples]; clip.GetData(samples,0); float peak=0;
            foreach(float s in samples) { audioGood&=!float.IsNaN(s)&&!float.IsInfinity(s); peak=Mathf.Max(peak,Mathf.Abs(s)); }
            audioGood&=peak>.01f&&peak<=.30f;
        }
        Check(audioGood&&voices.Count==10&&view.GetComponent<AudioListener>()!=null,"all synthesized sound clips and playback voices");
        TestSetup(); muted=false; smoke=false;
        PlaySound(SoundKind.Fire,player.root.position,.4f);
        yield return null;
        bool playing=false; foreach(AudioSource voice in voices) playing|=voice.isPlaying;
        Check(playing,"audio playback starts");
        SetPaused(true); bool pauseSilent=true;
        foreach(AudioSource voice in voices) pauseSilent&=!voice.isPlaying;
        PlaySound(SoundKind.Laser,player.root.position,.5f);
        foreach(AudioSource voice in voices) pauseSilent&=!voice.isPlaying;
        Check(pauseSilent,"pause stops voices and blocks new sounds");
        SetPaused(false); effectsVolume=0; StopSounds();
        PlaySound(SoundKind.Fire,player.root.position,.4f);
        bool zeroSilent=true; foreach(AudioSource voice in voices) zeroSilent&=!voice.isPlaying;
        Check(zeroSilent,"zero effects volume prevents playback");
        effectsVolume=.7f; PlaySound(SoundKind.Fire,player.root.position,.4f);
        ToggleMute(); bool silent=true; foreach(AudioSource voice in voices) silent&=!voice.isPlaying;
        Check(muted&&silent,"mute stops audio"); muted=false; smoke=true;
        TestSetup(0,1); Attack(player,enemy,Vector3.forward); Special(enemy,player,Vector3.back);
        ReturnToHangar();
        Check(inMenu&&!paused&&shots.Count==0&&pending.Count==0&&beams.Count==0&&flashes.Count==0&&!aimMarker.gameObject.activeSelf,"hangar clears active and delayed battle effects");
        TestSetup(); Attack(player,enemy,Vector3.forward); enemy.hp=0; FinishPresentation();
        Check(finishCleaned&&shots.Count==0&&pending.Count==0&&!player.thruster.gameObject.activeSelf,"round end clears pending attacks and visual residue");
        BeginMatch();
        Check(!finishCleaned&&!inMenu&&player.hp==player.spec.health&&enemy.hp==enemy.spec.health&&aimMarker.gameObject.activeSelf,"new round restores presentation and health");
        // Yield between rebuilds so deferred Unity destruction is exercised, not hidden.
        bool repeatedRestarts=true;
        for(int restart=0;restart<12;restart++)
        {
            selectedCraft=restart%CraftCatalog.Selectable.Length;
            rivalSelection=(restart+3)%CraftCatalog.Selectable.Length; BeginMatch();
            Attack(player,enemy,(enemy.root.position-player.root.position).normalized);
            Special(enemy,player,(player.root.position-enemy.root.position).normalized);
            ReturnToHangar(); yield return null;
            repeatedRestarts&=shots.Count==0&&pending.Count==0&&beams.Count==0&&rings.Count==0&&flashes.Count==0&&sparks.Count==0;
            repeatedRestarts&=player.root.childCount==2&&enemy.root.childCount==2;
        }
        Check(repeatedRestarts,"repeated hangar cycles leave no attacks or old robot roots");
        rivalSelection=-1;
        selectedCraft=0; ApplyCharacter(player,0); ApplyCharacter(enemy,1); ResetRound();
        inMenu=false; wins=losses=0;
        player.root.position=new Vector3(-8,1.1f,-8); enemy.root.position=new Vector3(8,1.1f,10);
        FaceOpponent(player,enemy); FaceOpponent(enemy,player);
        player.boostHeld=true; Move(player,Vector3.right,.12f);
        player.boostHeld=false; player.velocity=Vector3.zero; player.jumpOriginY=player.root.position.y;
        player.jumpVelocity=jumpSpeed; Move(player,Vector3.zero,.06f);
        Special(enemy,player,(player.root.position-enemy.root.position).normalized);
        Attack(player,enemy,(enemy.root.position-player.root.position).normalized);
        UpdateCombat(.12f); Animate(player); Animate(enemy); UpdateCamera(10);
        canShoot=true; assistEngaged=true;
        yield return new WaitForEndOfFrame(); SavePreview("Preview.png");
        TestSetup(0,1);
        player.root.position=new Vector3(41,1.1f,50); enemy.root.position=new Vector3(30,1.1f,33);
        FaceOpponent(player,enemy); FaceOpponent(enemy,player); Animate(player); Animate(enemy); UpdateCamera(10);
        // Explicit orthographic fixture, independent of the previous free-look test.
        player.root.position=new Vector3(48,FighterGroundY,58);Animate(player);
        view.orthographic=true;view.transform.position=new Vector3(48,4,30);
        view.transform.LookAt(player.root.position+Vector3.up*.6f);
        Check(CameraOccludes(player),"pilot remains identifiable behind distant factory");
        yield return new WaitForEndOfFrame(); SavePreview("CoverPreview.png");
        SaveWeaponStudy();
        bool originalSmoke=smoke; smoke=false; TestSetup(0,1);
        Check(player.weaponSlot==1&&player.magazine==30,"all frames start with only the slot-one gun");
        player.root.position=new Vector3(100,1.1f,100); enemy.root.position=new Vector3(104,1.1f,100);
        FaceOpponent(player,enemy); FaceOpponent(enemy,player);
        player.weaponTimer=0; player.magazine=30;
        int shotsBefore=shots.Count;
        Attack(player,enemy,(enemy.root.position-player.root.position).normalized);
        Check(player.magazine==29&&shots.Count==shotsBefore+1&&Mathf.Abs(shots[shots.Count-1].velocity.magnitude-132*bulletSpeedScale)<.01f,"slot one fires the universal rapid gun and spends one round");
        player.magazine=1; player.reloadTime=0; player.weaponTimer=0;
        Attack(player,enemy,(enemy.root.position-player.root.position).normalized);
        Check(player.magazine==0&&player.reloadTime>0,"empty magazine starts automatic reload");
        TestStep(1.7f);
        Check(player.magazine==30&&player.reloadTime==0,"automatic reload refills the 30-round magazine");
        WispOvernightChecks(Check);
        smoke=originalSmoke;
        Debug.Log(passed?"VOXSKY_SMOKE_PASS "+count+" checks: ground, modular robots, camera, slot-one gun, mobility, practice AI, aim assist, audio":"VOXSKY_SMOKE_FAIL");
        yield return new WaitForSeconds(.3f); Application.Quit(passed?0:2);
    }
}
