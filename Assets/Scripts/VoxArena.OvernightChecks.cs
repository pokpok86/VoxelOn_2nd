using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public sealed partial class VoxArena
{
    void WispOvernightChecks(Action<bool,string> check)
    {
        bool savedSmoke=smoke; smoke=true;
        float[] speeds=new float[4];Vector3[] directions={Vector3.forward,Vector3.back,Vector3.left,Vector3.right};
        for(int i=0;i<4;i++) { TestSetupSelectable(0,0);player.facing=Vector3.forward;Move(player,directions[i],.05f);speeds[i]=Vector3.ProjectOnPlane(player.velocity,Vector3.up).magnitude; }
        check(Mathf.Abs(speeds[0]-3*speeds[1])<.02f&&Mathf.Abs(speeds[0]-speeds[2])<.02f&&Mathf.Abs(speeds[0]-speeds[3])<.02f,"WISP actual normal travel is forward/back/left/right 3:1:3:3");
        Vector3 oblique=CameraRelativeMove(Vector3.forward,Vector3.right,1,1,true);
        check(Vector3.Angle(oblique,Vector3.forward)>10&&Vector3.Angle(oblique,Vector3.forward)<20,"forward plus side input runs with a slight turn, not a 45 degree strafe");
        for(int i=0;i<4;i++)
        {
            TestSetupSelectable(0,0);player.facing=Vector3.forward;player.boostHeld=true;Vector3 start=player.root.position;
            for(int t=0;t<14;t++)Move(player,directions[i],.025f);
            float distance=Vector3.Distance(start,player.root.position);
            check(player.boosting&&player.jumpHeight<.01f&&distance>speeds[i]*.35f+1f,"held Shift provides continuous grounded movement direction "+i);
        }
        TestSetupSelectable(0,0);player.facing=Vector3.forward;player.root.rotation=Quaternion.identity;
        player.velocity=Vector3.right*15;roundTime=.1f;Animate(player);
        check(Mathf.Abs(Mathf.DeltaAngle(0,player.leftLeg.localEulerAngles.z))>10&&Mathf.Abs(Mathf.DeltaAngle(0,player.rightLeg.localEulerAngles.z))>10,"WISP lateral travel uses articulated sideways steps");
        player.boosting=true;player.jumpHeight=0;player.jumpHeld=false;Animate(player);
        check(!player.thruster.gameObject.activeSelf,"ground running and sliding do not display aerial booster flames");
        player.jumpHeight=2;player.jumpVelocity=4;player.jumpHeld=true;Animate(player);
        check(player.thruster.gameObject.activeSelf&&player.jumpFlame.gameObject.activeSelf,"airborne held Space displays vertical booster flames");
        player.jumpHeld=false;Animate(player);check(!player.thruster.gameObject.activeSelf,"releasing airborne Space immediately removes booster flames");
        TestSetupSelectable(0,0);player.jumpOriginY=player.root.position.y;player.jumpVelocity=jumpSpeed*player.spec.jumpImpulseScale/Mathf.Sqrt(player.spec.jumpWeightFactor);
        float peak=0,airTime=0;
        for(int tick=0;tick<160;tick++){Move(player,Vector3.zero,.0125f);peak=Mathf.Max(peak,player.jumpHeight);airTime+=.0125f;if(tick>10&&player.jumpHeight<=.001f)break;}
        check(peak>3.1f&&airTime<1.4f,"WISP tap jump rises higher and completes its faster arc (peak="+peak.ToString("F2")+" time="+airTime.ToString("F2")+")");
        TestSetupSelectable(0,0);smoke=false;
        Attack(player,enemy,Vector3.forward);int rounds=player.magazine;Attack(player,enemy,Vector3.forward);
        check(rounds==29&&player.magazine==29&&Mathf.Abs(player.weaponTimer-PrimaryFireInterval)<.001f,"rifle trial frequency is 35 percent lower than the 0.18s baseline");
        for(int tick=0;tick<80;tick++)Move(player,Vector3.zero,.025f);
        check(player.magazine==29&&player.reloadTime<=0,"partial magazine stays available during the shooting pause");
        for(int tick=0;tick<14;tick++)Move(player,Vector3.zero,.025f);
        check(player.magazine==29&&player.reloadTime>0,"partial magazine auto reload begins after 2.2 idle seconds");
        for(int tick=0;tick<104;tick++)Move(player,Vector3.zero,.025f);
        check(player.magazine==30&&player.reloadTime==0,"idle reload completes without requiring an empty magazine");
        CameraMotionChecks(check);
        smoke=savedSmoke;TestSetup();
    }
    void SaveEnvironmentView(string file,Vector3 from,Vector3 at)
    {
        string folder=Path.Combine(Directory.GetCurrentDirectory(),"Docs","Screenshots","Overnight-20261104");Directory.CreateDirectory(folder);
        RenderTexture target=new RenderTexture(1280,720,24);Texture2D image=new Texture2D(1280,720,TextureFormat.RGB24,false);
        RenderTexture previous=RenderTexture.active;view.targetTexture=target;view.orthographic=false;view.fieldOfView=55;
        view.transform.position=from;view.transform.LookAt(at);view.Render();RenderTexture.active=target;
        image.ReadPixels(new Rect(0,0,1280,720),0,0);image.Apply();File.WriteAllBytes(Path.Combine(folder,file),image.EncodeToPNG());
        RenderTexture.active=previous;view.targetTexture=null;target.Release();Destroy(target);Destroy(image);
    }
    void SaveWispMovementStudy(string map)
    {
        Vector3 anchor=new Vector3(100,FighterGroundY,100);
        string[] poses={"stand","strafe-left","strafe-right","run","jump-held","jump-released"};
        foreach(string pose in poses)
        {
            TestSetupSelectable(0,0);player.root.position=anchor;player.facing=Vector3.forward;player.root.rotation=Quaternion.identity;
            enemy.root.gameObject.SetActive(false);enemy.shadow.gameObject.SetActive(false);
            Vector3 input=pose=="strafe-left"?Vector3.left:pose=="strafe-right"?Vector3.right:pose=="run"?Vector3.forward:Vector3.zero;
            player.boostHeld=pose=="run";
            if(pose.StartsWith("jump"))
            {
                player.jumpOriginY=anchor.y;player.jumpVelocity=jumpSpeed*player.spec.jumpImpulseScale/Mathf.Sqrt(player.spec.jumpWeightFactor);
                player.jumpHeld=pose=="jump-held";
            }
            for(int tick=0;tick<12;tick++)Move(player,input,1f/60);
            roundTime=.1f;Animate(player);
            // Fixed floor-level target keeps actual jump height visible; no camera follow.
            SaveEnvironmentView(map+"-motion-"+pose+".png",anchor+new Vector3(7,4,10),anchor+Vector3.up*.7f);
        }
        TestSetupSelectable(0,0);ResetRound();inMenu=false;Animate(player);Animate(enemy);
    }
    bool MeasureWispStandingEnvelope(string map)
    {
        TestSetupSelectable(0,0);player.root.position=new Vector3(0,FighterGroundY,0);
        player.root.rotation=Quaternion.identity;player.facing=Vector3.forward;player.velocity=Vector3.zero;Animate(player);
        Bounds all=new Bounds();bool first=true;int vertices=0,coreVertices=0;float bodyHalfX=0,bodyHalfZ=0,maxSkinError=0;
        Mesh baked=new Mesh();
        foreach(SkinnedMeshRenderer skin in player.body.GetComponentsInChildren<SkinnedMeshRenderer>())
        {
            // true compensates renderer scale; TransformPoint applies it once.
            // Independently compare every baked point with bone/bind skinning.
            skin.BakeMesh(baked,true);Vector3[] points=baked.vertices;BoneWeight[] weights=skin.sharedMesh.boneWeights;
            Vector3[] rest=skin.sharedMesh.vertices;Matrix4x4[] bind=skin.sharedMesh.bindposes;
            for(int v=0;v<points.Length;v++)
            {
                Vector3 local=player.root.InverseTransformPoint(skin.transform.TransformPoint(points[v]));
                if(first){all=new Bounds(local,Vector3.zero);first=false;}else all.Encapsulate(local);
                vertices++;
                int index=weights[v].boneIndex0;Transform bone=skin.bones[index];
                Vector3 direct=player.root.InverseTransformPoint((bone.localToWorldMatrix*bind[index]).MultiplyPoint3x4(rest[v]));
                maxSkinError=Mathf.Max(maxSkinError,Vector3.Distance(local,direct));
                if(bone==player.corePart || bone.IsChildOf(player.corePart))
                {coreVertices++;bodyHalfX=Mathf.Max(bodyHalfX,Mathf.Abs(local.x));bodyHalfZ=Mathf.Max(bodyHalfZ,Mathf.Abs(local.z));}
            }
        }
        Destroy(baked);
        Debug.Log("WISP_ENVELOPE "+map+" vertices="+vertices+" coreVertices="+coreVertices+
            " height="+all.size.y.ToString("F3")+" visualHalfX="+Mathf.Max(Mathf.Abs(all.min.x),Mathf.Abs(all.max.x)).ToString("F3")+
            " coreHalfX="+bodyHalfX.ToString("F3")+" coreHalfZ="+bodyHalfZ.ToString("F3")+" collisionRadius="+player.Radius.ToString("F3")+" bakeVsBindMaxError="+maxSkinError.ToString("F6"));
        // Record genuine visual overlap without widening hitboxes or declaring
        // decoration clearance equivalent to a combat-physics requirement.
        player.root.position=new Vector3(ArenaHalfWidth-player.Radius,FighterGroundY,0);Animate(player);
        enemy.root.gameObject.SetActive(false);enemy.shadow.gameObject.SetActive(false);
        SaveEnvironmentView(map+"-clearance-wall.png",player.root.position+new Vector3(-7,3,-9),player.root.position+Vector3.up*.7f);
        TestSetupSelectable(0,0);ResetRound();inMenu=false;
        return vertices>1000&&coreVertices>0&&(player.simpleShell?Mathf.Abs(all.size.y-3.5105f)<.03f:all.size.y>3.7f&&all.size.y<4.3f)&&maxSkinError<.005f;
    }
    void SaveFoundryLightingStudy()
    {
        Vector3 from=new Vector3(42,28,-45),at=new Vector3(0,3,10);
        player.root.gameObject.SetActive(false);enemy.root.gameObject.SetActive(false);
        SaveEnvironmentView("Foundry-lighting-01-baseline.png",from,at);
        var changed=new List<Renderer>();var modes=new List<UnityEngine.Rendering.ShadowCastingMode>();
        foreach(Renderer r in FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            if(r.name=="Hangar boundary wall"||r.name=="High hangar ceiling")
            {changed.Add(r);modes.Add(r.shadowCastingMode);r.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;}
        SaveEnvironmentView("Foundry-lighting-02-no-legacy-shadows.png",from,at);
        GameObject art=GameObject.Find("Blender FBX / FoundryBackdrop");
        Renderer[] surfaces=art.GetComponentsInChildren<Renderer>();
        foreach(Renderer r in surfaces)r.receiveShadows=false;
        SaveEnvironmentView("Foundry-lighting-03-no-backdrop-receive.png",from,at);
        foreach(Renderer r in surfaces)r.receiveShadows=true;
        for(int i=0;i<changed.Count;i++)changed[i].shadowCastingMode=modes[i];
        player.root.gameObject.SetActive(true);enemy.root.gameObject.SetActive(true);
        Debug.Log("FOUNDRY_LIGHTING_STUDY_PASS 3 real renders; shadow settings restored");
    }
    IEnumerator EnvironmentSmoke()
    {
        yield return new WaitForEndOfFrame();
        bool passed=true;int count=0;
        void Check(bool ok,string name){count++;passed&=ok;Debug.Log("ENV_CHECK "+name+" "+(ok?"PASS":"FAIL"));}
        foreach(string asset in new[]{"FoundryCargo","FoundryGenerator","FoundryBarricade","CanyonRock","OutdoorGround","CanyonVista","FoundryBackdrop","CanyonBoundary"})
            Check(Resources.Load<GameObject>("Environment/"+asset)!=null,"Blender FBX resource "+asset);
        Check(environmentFallbacks==0,"all environment graphics load from Blender FBX assets");
        TestSetupSelectable(0,0);ResetRound();inMenu=false;
        Check(!Occupied(player.root.position,player.Radius)&&!Occupied(enemy.root.position,enemy.Radius),"both WISP spawns are outside map solids");
        Check(cameraSolids.Count>=4,"camera sees map perimeter volumes");
        if(arenaMap==ArenaMapKind.OutdoorCanyon)
        {
            GameObject boundary=GameObject.Find("Blender FBX / CanyonBoundary");
            Renderer cliff=boundary!=null?boundary.GetComponentInChildren<Renderer>():null;
            Check(cliff!=null&&Mathf.Abs(cliff.bounds.min.y-GroundSurfaceY)<.02f&&
                Mathf.Abs(cliff.bounds.min.x+145)<.02f&&Mathf.Abs(cliff.bounds.max.x-145)<.02f&&
                Mathf.Abs(cliff.bounds.min.z+145)<.02f&&Mathf.Abs(cliff.bounds.max.z-145)<.02f&&
                cliff.bounds.max.y<GroundSurfaceY+36,"Blender boundary aligns to arena footprint and safe camera height");
            Check(GameObject.Find("Canyon cliff boundary")==null,"authored cliff replaces the old plain wall without duplicate surfaces");
            for(int side=-1;side<=1;side+=2)
            {
                Vector3 focus=new Vector3(side*138,4,0),outside=new Vector3(side*150,4,0);
                Check(Mathf.Abs(ConstrainCamera(focus,outside,.30f).x)<140,"camera remains inside authored canyon X boundary "+side);
                focus=new Vector3(0,4,side*138);outside=new Vector3(0,4,side*150);
                Check(Mathf.Abs(ConstrainCamera(focus,outside,.30f).z)<140,"camera remains inside authored canyon Z boundary "+side);
            }
            Check(terrainSteps.Count>=50&&RenderSettings.fog,"outdoor canyon has climbable strata and daylight atmosphere");
            Check(GameObject.Find("Blender FBX / CanyonVista")!=null&&GameObject.Find("Blender FBX / OutdoorGround")!=null,"outdoor floor and vista use authored FBXs");
            Renderer floor=GameObject.Find("Blender FBX / OutdoorGround").GetComponentInChildren<Renderer>();
            Check(floor!=null&&floor.bounds.size.y<.03f&&Mathf.Abs(floor.bounds.max.y-GroundSurfaceY)<.02f,
                "outdoor FBX ground is horizontal and exactly at the playable floor");
            Renderer rock=GameObject.Find("Canyon tall voxel cover 0").GetComponentInChildren<Renderer>();
            Check(rock!=null&&Mathf.Abs(rock.bounds.min.y-GroundSurfaceY)<.02f&&Mathf.Abs(rock.bounds.size.y-4f)<.02f,
                "outdoor Blender rock dimensions match their analytical collision");
        }
        else
        {
            Check(debris.Count>=20&&GameObject.Find("Blender FBX / FoundryBackdrop")!=null,"indoor foundry has authored cover and background");
            foreach(Debris d in debris)
            {
                Bounds shown=new Bounds();bool found=false;
                foreach(Renderer r in d.visual.GetComponentsInChildren<Renderer>()) {if(!found){shown=r.bounds;found=true;}else shown.Encapsulate(r.bounds);}
                Bounds solid=DebrisBounds(d.origin,d.size);
                Check(found&&Mathf.Abs(shown.min.y-solid.min.y)<.02f&&Mathf.Abs(shown.max.y-solid.max.y)<.02f,"FBX cover floor/roof matches collision "+d.visual.name);
            }
        }
        string map=arenaMap==ArenaMapKind.OutdoorCanyon?"Canyon":"Foundry";
        Check(MeasureWispStandingEnvelope(map),"genuine baked WISP envelope has expected standing scale and torso weights");
        SaveWispMovementStudy(map);
        if(arenaMap==ArenaMapKind.IndoorFactory&&Array.IndexOf(Environment.GetCommandLineArgs(),"--lighting-study")>=0) SaveFoundryLightingStudy();
        player.root.gameObject.SetActive(false);enemy.root.gameObject.SetActive(false);player.shadow.gameObject.SetActive(false);enemy.shadow.gameObject.SetActive(false);
        SaveEnvironmentView(map+"-overview.png",new Vector3(42,28,-45),new Vector3(0,3,10));
        Bounds close=arenaMap==ArenaMapKind.OutdoorCanyon?terrainSteps[terrainSteps.Count-1]:obstacles[0];
        SaveEnvironmentView(map+"-cover.png",close.center+new Vector3(9,6,-11),close.center);
        player.root.gameObject.SetActive(true);enemy.root.gameObject.SetActive(true);player.shadow.gameObject.SetActive(true);enemy.shadow.gameObject.SetActive(true);
        var times=new List<float>();double begin=Time.realtimeSinceStartupAsDouble,last=begin;
        while(Time.realtimeSinceStartupAsDouble-begin<6)
        {
            yield return null;double now=Time.realtimeSinceStartupAsDouble;float frame=(float)(now-last);last=now;
            if(now-begin>1)times.Add(frame*1000);
            float dt=Mathf.Clamp(frame,.001f,.05f);roundTime+=dt;EnemyAI(player,enemy,dt);EnemyAI(enemy,player,dt);UpdateCombat(dt);Animate(player);Animate(enemy);player.hp=player.spec.health;enemy.hp=enemy.spec.health;
        }
        times.Sort();float total=0;foreach(float ms in times)total+=ms;
        Check(times.Count>60,"real rendered environment frame sample collected");
        if(times.Count>0)Debug.Log("ENV_PERF "+map+" samples="+times.Count+" avgMs="+(total/times.Count).ToString("F2")+" p95Ms="+times[(int)(times.Count*.95f)].ToString("F2"));
        Debug.Log(passed?"VOXELON_ENV_PASS "+map+" "+count+" checks":"VOXELON_ENV_FAIL "+map);
        Application.Quit(passed?0:2);
    }
}
