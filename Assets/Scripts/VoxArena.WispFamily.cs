using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public sealed partial class VoxArena
{
    [Serializable] sealed class FamilyLoadout
    {
        public string key,head="HD01",body="BD01",legs,backpack="B-01_SKYWING";public bool enabled=true;
        public string[] auxiliary={"NONE","NONE"};public string[] weapons={"AUTO","AUTO","AUTO","AUTO"};
    }
    [Serializable] sealed class FamilyLoadoutFile {public int version=1;public List<FamilyLoadout> designs=new List<FamilyLoadout>();}
    sealed class FamilyRig
    {
        public Transform root,upper;public WispFamilyAssets.Instance body,head,legs,backpack;
        public WispFamilyAssets.Instance[] weapons=new WispFamilyAssets.Instance[4];public Transform[] muzzles=new Transform[4];
        public Vector3[] elbows=new Vector3[2];public string[] weaponIds=new string[4];public FamilyLoadout config;
        public float trackTravel;public Dictionary<string,Vector3> legRest=new Dictionary<string,Vector3>();
        public FamilySupportMesh[] supports;
    }
    sealed class FamilySupportMesh {public Transform transform;public Vector3[] vertices;}
    readonly Dictionary<Fighter,FamilyRig> familyRigs=new Dictionary<Fighter,FamilyRig>();
    FamilyLoadoutFile familyLoadouts;
    int familyArtCategory;Vector2 familyArtScroll;
    static readonly string[] FamilyHeads={"HD01","VIS_H01_RADOME","VIS_H02_LONGSIGHT","VIS_H03_SWIFTFIN","VIS_H04_BULWARK","VIS_H05_NEEDLE","VIS_H06_LANCET"};
    static readonly string[] FamilyBodies={"BD01","VIS_C01_STANDARD","VIS_C02_LIGHTFRAME","VIS_C03_BULWARK_CORE","VIS_C04_ARC_CORE"};
    static readonly string[] FamilyLegs={"L-01_STRIDER","L-02_BASTION_TRACK","L-03_TARANTULAN","L-05_SKYLIFT","VIS_L02_BASTION_TRACK","VIS_L03_TARANTULAN","VIS_L04_DRAFT_HOVER"};
    static readonly string[] FamilyBackpacks={"B-01_SKYWING","B-02_CROSSLANCE","B-03_OVERTHRUST","B-04_LONGTANK","B-05_RAMJET","VIS_B02_SKYWING","VIS_B03_CARGO_PACK","DRONE_ATTACK_V002","DRONE_INTERCEPT_V002","DRONE_JAMMER_V002"};
    static readonly string[] FamilyWeapons={"AUTO","W-01_LUMEN","W-02_BREAKER","W-03_RATTLE","W-04_EDGE"};
    static bool FamilyJointReviewRequested=>Array.IndexOf(Environment.GetCommandLineArgs(),"--wisp-joint-review")>=0;
    static bool FamilyReviewRequested=>FamilyJointReviewRequested||Array.IndexOf(Environment.GetCommandLineArgs(),"--wisp-family-review")>=0;
    // Shoulder weapons will return on authored backpack mounts. Preserve saved loadouts.
    static bool BackpackWeaponMountsPending=>Array.IndexOf(Environment.GetCommandLineArgs(),"--legacy-family-art")<0;
    void PreparePendingBackpackWeapons(Fighter f)
    {
        if(!BackpackWeaponMountsPending)return;
        for(int i=2;i<4;i++)
        {
            f.weaponProfiles[i]=new WeaponProfile("EMPTY",Loadout[i].mount,0,0,1,1,0,1){isUnarmed=true,shoulderBlocked=true};
            f.weaponArtIds[i]="UNARMED";
        }
        if(f.weaponSlot>2)f.weaponSlot=1;
    }
    string FamilyLoadoutPath=>Path.Combine(BalanceDirectory,"WISP-Visual-Loadouts.json");
    FamilyLoadout FamilyConfig(Fighter f)
    {
        if(familyLoadouts==null)
        {
            try{familyLoadouts=File.Exists(FamilyLoadoutPath)?JsonUtility.FromJson<FamilyLoadoutFile>(File.ReadAllText(FamilyLoadoutPath)):new FamilyLoadoutFile();}
            catch(Exception e){Debug.LogWarning("WISP_VISUAL_CONFIG "+e.Message);familyLoadouts=new FamilyLoadoutFile();}
            if(familyLoadouts==null)familyLoadouts=new FamilyLoadoutFile();if(familyLoadouts.designs==null)familyLoadouts.designs=new List<FamilyLoadout>();
        }
        bool design=f==player||f.root.name=="Portrait studio robot";
        string key=(design?((balanceEditorOpen?balanceDraft?.activeDesignId:null)??activeBalance?.activeDesignId??"DESIGN_01"):"AI")+"/"+f.spec.name;
        FamilyLoadout c=familyLoadouts.designs.Find(x=>x!=null&&x.key==key);
        if(c==null){c=new FamilyLoadout{key=key,enabled=f.spec.name=="WISP"||f.spec.name=="BASTION"||f.spec.name=="TARANTULA",head=FamilyHeads[0],body=FamilyBodies[0],legs=f.spec.mobility==MobilityKind.Tank?FamilyLegs[1]:f.spec.mobility==MobilityKind.Quadruped?FamilyLegs[2]:FamilyLegs[0]};familyLoadouts.designs.Add(c);}
        // New authored family art replaces the previous selectable looks for these three frames.
        // HOUND has no replacement family body yet, so retain its dedicated canine model.
        if(f.spec.name=="WISP"||f.spec.name=="BASTION"||f.spec.name=="TARANTULA")c.enabled=true;
        if(Array.IndexOf(FamilyHeads,c.head)<0)c.head=FamilyHeads[0];if(Array.IndexOf(FamilyBodies,c.body)<0)c.body=FamilyBodies[0];
        if(Array.IndexOf(FamilyLegs,c.legs)<0)c.legs=FamilyLegs[0];if(Array.IndexOf(FamilyBackpacks,c.backpack)<0)c.backpack=FamilyBackpacks[0];
        if(c.weapons==null||c.weapons.Length!=4)c.weapons=new[]{"AUTO","AUTO","AUTO","AUTO"};
        for(int i=0;i<4;i++)if(Array.IndexOf(FamilyWeapons,c.weapons[i])<0)c.weapons[i]="AUTO";
        if(c.auxiliary==null||c.auxiliary.Length!=2)c.auxiliary=new[]{"NONE","NONE"};
        for(int i=0;i<2;i++)if(Array.IndexOf(AuxiliaryIds,c.auxiliary[i])<0)c.auxiliary[i]="NONE";
        return c;
    }
    void SaveFamilyConfig()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(FamilyLoadoutPath));
        string temp=FamilyLoadoutPath+".tmp";File.WriteAllText(temp,JsonUtility.ToJson(familyLoadouts,true));
        if(File.Exists(FamilyLoadoutPath)){File.Copy(temp,FamilyLoadoutPath,true);File.Delete(temp);}else File.Move(temp,FamilyLoadoutPath);
        RefreshBalanceDraftPreview();
    }
    string FamilyWeapon(Fighter f,FamilyLoadout c,int slot)
    {
        if(slot>=2&&BackpackWeaponMountsPending)return null;
        if(f.weaponProfiles[slot].isUnarmed||f.weaponProfiles[slot].shoulderBlocked)return null;
        if(f.weaponProfiles[slot].isShield)return "W-05_AEGIS";
        string equipped=f.weaponArtIds?[slot];if(!string.IsNullOrEmpty(equipped)&&WeaponCatalog().models.Any(x=>x.id==equipped&&x.kind=="firearm"))return equipped;
        string picked=c.weapons[slot];if(picked!="AUTO")return picked;
        string id=(f.weaponArtIds?[slot]??"").ToUpperInvariant();
        if(id.Contains("SWORD")||id.Contains("BLADE")||id.Contains("SABER"))return "W-04_EDGE";
        if(id.Contains("GATLING")||id.Contains("LMG")||id.Contains("VULCAN"))return "W-03_RATTLE";
        if(id.Contains("_RL")||id.Contains("_GL")||id.Contains("ROCKET")||id.Contains("BAZOOKA")||id.Contains("CANNON"))return "W-02_BREAKER";
        return "W-01_LUMEN";
    }
    void InstallFamilyArt(Fighter f)
    {
        RemoveDroneRuntime(f);familyRigs.Remove(f);if(!WispFamilyAssets.Available||Array.IndexOf(Environment.GetCommandLineArgs(),"--legacy-family-art")>=0)return;
        FamilyLoadout c=FamilyConfig(f);if(!c.enabled)return;
        // Retain legacy animation/combat drivers, hide only their surfaces.
        foreach(Renderer renderer in f.body.GetComponentsInChildren<Renderer>(true))
        {
            Transform t=renderer.transform;
            if(renderer is ParticleSystemRenderer||f.thruster!=null&&t.IsChildOf(f.thruster)||f.jumpFlame!=null&&t.IsChildOf(f.jumpFlame)||f.shield!=null&&t.IsChildOf(f.shield)||f.boostVisual!=null&&t.IsChildOf(f.boostVisual))continue;
            renderer.enabled=false;
        }
        var r=new FamilyRig{root=Module("Approved WISP modular family",f.body,Vector3.zero),config=c};r.root.localScale=Vector3.one*4f;
        r.legs=WispFamilyAssets.Spawn(c.legs,r.root);float waist=r.legs.data.waistHeight;
        r.upper=Module("Upper assembly / fixed proportions",r.root,Vector3.up*waist);
        r.body=WispFamilyAssets.Spawn(c.body,r.upper);r.body.root.localPosition=Vector3.up*(-r.body.data.waistBottom);
        r.head=WispFamilyAssets.Spawn(c.head,r.upper);r.head.root.localPosition=new Vector3(0,-r.body.data.waistBottom+r.body.data.neckTop+.046f,.003f);
        r.backpack=SpawnDronePack(c.backpack,r.upper);r.backpack.root.localPosition=r.body.root.localPosition+WispFamilyAssets.V(r.body.data.backMount);
        Vector3[] rawElbows={WispFamilyAssets.V(r.body.data.leftElbow),WispFamilyAssets.V(r.body.data.rightElbow)};
        for(int side=0;side<2;side++)r.elbows[side]=rawElbows[side]-r.body.nodes[side==0?"LeftShoulder":"RightShoulder"].localPosition;
        foreach(var n in r.legs.nodes)r.legRest[n.Key]=n.Value.localPosition;
        var supportNodes=r.legs.data.nodes.Where(n=>n.motion=="ankle"||n.motion=="quadKnee"||n.motion=="quadAnkle"||n.motion=="tread").Select(n=>n.name).ToArray();
        r.supports=r.legs.root.GetComponentsInChildren<MeshFilter>().Where(m=>Array.IndexOf(supportNodes,m.transform.parent.name)>=0)
            .Select(m=>new FamilySupportMesh{transform=m.transform,vertices=m.sharedMesh.vertices.Distinct().ToArray()}).ToArray();
        for(int i=0;i<4;i++)
        {
            string id=FamilyWeapon(f,c,i);r.weaponIds[i]=id;if(id==null)continue;
            if(WeaponCatalog().models.Any(x=>x.id==id&&x.kind=="firearm"))
            {
                Transform holder=Module("Actual equipped gun / "+id,r.root,Vector3.zero);
                GameObject prefab=Resources.Load<GameObject>(WeaponArtResource(id));if(prefab==null)throw new InvalidOperationException("Missing gun "+id);
                Transform model=Instantiate(prefab,holder,false).transform;model.localScale*=.2375f;PrepareWeaponMaterials(model);
                Transform grip=FindArtSocket(model,"GRIP_MAIN"),muzzle=FindArtSocket(model,"MUZZLE");
                if(grip==null||muzzle==null)throw new InvalidOperationException("Missing gun socket "+id);
                model.position+=holder.position-grip.position;
                r.weapons[i]=new WispFamilyAssets.Instance{root=holder,data=new WispFamilyAssets.Asset{id=id}};r.muzzles[i]=muzzle;
                continue;
            }
            var art=WispFamilyAssets.Spawn(id,r.root);r.weapons[i]=art;art.root.localScale=Vector3.one*(i<2?.42f:.32f);
            if(i<2&&f.weaponProfiles[i].isBeamShield)f.shieldBeamVisuals[i]=CreateShieldBeamVisual(art.root);
            // The authored right-side blade/shield rolls reverse on the left.
            if((i==0||i==2)&&(id=="W-04_EDGE"||id=="W-05_AEGIS"))foreach(Transform child in art.root)child.localRotation=Quaternion.Euler(0,0,180)*child.localRotation;
            var points=art.data.groups.SelectMany(g=>Enumerable.Range(0,g.positions.Length/3).Select(v=>new Vector3(g.positions[v*3],g.positions[v*3+1],g.positions[v*3+2]))).ToArray();
            Vector3 min=points[0],max=points[0];foreach(Vector3 p in points){min=Vector3.Min(min,p);max=Vector3.Max(max,p);}
            r.muzzles[i]=Module("Actual authored muzzle",art.root,new Vector3((min.x+max.x)*.5f,(min.y+max.y)*.5f,max.z));
        }
        familyRigs.Add(f,r);SetupDroneRuntime(f,r);SetupAuxiliary(f,r);f.headPart=r.head.root;wispHeadBoundsReady=false;CacheWispHeadBounds(f);UpdateFamilyArt(f);
        Debug.Log("WISP_FAMILY_APPLIED "+f.spec.name+" legs="+c.legs+" back="+c.backpack);
    }
    void UpdateFamilyArt(Fighter f)
    {
        if(!familyRigs.TryGetValue(f,out FamilyRig r)||r.root==null||f.deadVisual)return;
        r.root.SetPositionAndRotation(f.root.position-Vector3.up*FighterSupportOffset,f.root.rotation);
        bool portrait=f.root.name=="Portrait studio robot";r.upper.localRotation=portrait?Quaternion.identity:f.body.localRotation;
        float stride=f.jumpHeight>.08f||f.jumpVelocity>1f?0:Mathf.Clamp01(Vector3.ProjectOnPlane(f.velocity,Vector3.up).magnitude/Mathf.Max(1,f.spec.speed));
        float gait=Mathf.Sin(roundTime*10)*stride;
        Vector3 travel=Quaternion.Inverse(f.root.rotation)*Vector3.ProjectOnPlane(f.velocity,Vector3.up);
        if(travel.sqrMagnitude>.001f)travel.Normalize();else travel=Vector3.zero;
        Vector3 aim=f==player&&!inMenu&&!spectatorMode&&Finite(aimDirection)&&aimDirection.sqrMagnitude>.01f?aimDirection.normalized:f.facing;
        Quaternion aimed=Quaternion.LookRotation(aim,Vector3.up);
        for(int side=0;side<2;side++)
        {
            float sign=side==0?-1:1;Transform arm=r.body.nodes[side==0?"LeftShoulder":"RightShoulder"];
            float guard=f.weaponProfiles[side].isShield?f.guardBlend:0;
            arm.localRotation=Quaternion.Euler(0,0,sign*30)*Quaternion.Euler(-gait*sign*8-guard*35,0,0);
            for(int slot=side;slot<4;slot+=2)
            {
                var art=r.weapons[slot];if(art==null)continue;
                art.root.gameObject.SetActive(!f.weaponProfiles[slot].isUnarmed&&!f.weaponProfiles[slot].shoulderBlocked);
                Quaternion carry=arm.rotation*Quaternion.Euler(50,0,0);
                if(f.weaponProfiles[slot].isShield)
                {
                    carry=f.root.rotation*Quaternion.Euler(90,0,0);
                    Quaternion guarded=f.root.rotation*Quaternion.Euler(0,sign*90,0)*Quaternion.Euler(90,0,0);
                    art.root.rotation=Quaternion.Slerp(carry,guarded,f.guardBlend);
                    if(slot<2)arm.rotation=Quaternion.FromToRotation(arm.right,art.root.right)*arm.rotation;
                    art.root.position=slot<2?arm.TransformPoint(r.elbows[side]):arm.position;
                    if(slot<2&&f.weaponProfiles[slot].isBeamShield&&f.shieldBeamVisuals[slot]!=null)
                    {
                        Transform field=f.shieldBeamVisuals[slot];field.gameObject.SetActive(f.guarding&&f.guardBlend>.02f&&!inMenu);
                        field.SetPositionAndRotation(art.root.position+f.root.forward*.42f-f.root.up*.3f,f.root.rotation);
                        field.localScale=new Vector3(.45f,Mathf.Lerp(.32f,.45f,f.guardBlend),.45f);
                    }
                }
                else
                {
                    if(!inMenu&&!portrait&&(WeaponIndex(f)==slot||slot>=2))
                    {
                        Vector3 hinge=Vector3.ProjectOnPlane(arm.right,aim);
                        if(hinge.sqrMagnitude<.001f)hinge=Vector3.Cross(Vector3.up,aim);
                        if(hinge.sqrMagnitude<.001f)hinge=Vector3.Cross(Vector3.forward,aim);
                        hinge.Normalize();
                        if(slot<2)arm.rotation=Quaternion.FromToRotation(arm.right,hinge)*arm.rotation;
                        art.root.rotation=Quaternion.LookRotation(aim,Vector3.Cross(aim,hinge));
                    }
                    else art.root.rotation=carry;
                    art.root.position=slot<2?arm.TransformPoint(r.elbows[side]):arm.position+f.root.up*.22f+f.root.right*(sign*.22f)-f.root.forward*.12f;
                }
            }
        }
        if(f.spec.mobility==MobilityKind.Tank)r.legs.root.rotation=Quaternion.LookRotation(f.chassisForward.sqrMagnitude>.01f?f.chassisForward:f.facing,Vector3.up);
        r.trackTravel+=Vector3.Dot(f.velocity,f.facing)*Time.deltaTime/Mathf.Max(.1f,4*f.spec.size);
        foreach(var n in r.legs.data.nodes)
        {
            Transform t=r.legs.nodes[n.name];float phase=roundTime*9+(n.side==n.front?0:Mathf.PI);
            if(n.motion=="hip")t.localRotation=Quaternion.Euler(gait*n.side*18*travel.z,0,gait*n.side*12*travel.x);
            else if(n.motion=="knee")t.localRotation=Quaternion.Euler(Mathf.Max(0,-gait*n.side)*25,0,0);
            else if(n.motion=="ankle")t.localRotation=Quaternion.Euler(-gait*n.side*18*travel.z-Mathf.Max(0,-gait*n.side)*25,0,-gait*n.side*12*travel.x);
            else if(n.motion=="quadHip")t.localRotation=Quaternion.Euler(Mathf.Sin(phase)*stride*12*travel.z,0,Mathf.Sin(phase)*stride*8*travel.x);
            else if(n.motion=="quadKnee")t.localRotation=Quaternion.Euler(Mathf.Max(0,Mathf.Sin(phase))*stride*19,0,0);
            else if(n.motion=="quadAnkle")t.localRotation=Quaternion.Euler(-Mathf.Max(0,Mathf.Sin(phase))*stride*12,0,0);
            else if(n.motion=="wheel")t.localRotation=Quaternion.Euler(r.trackTravel*650,0,0);
            else if(n.motion=="tread")AnimateFamilyTread(t,r.legRest[n.name],r.trackTravel);
        }
        // Seat the lowest actual support vertex at the controller's sole height.
        // Move the complete visual assembly together so hips/waist/elbows cannot separate.
        if(r.supports.Length>0)
        {
            float bottom=float.PositiveInfinity;
            foreach(var mesh in r.supports){Matrix4x4 matrix=mesh.transform.localToWorldMatrix;foreach(Vector3 p in mesh.vertices)bottom=Mathf.Min(bottom,matrix.MultiplyPoint3x4(p).y);}
            r.root.position+=Vector3.up*(f.root.position.y-FighterSupportOffset-bottom);
        }
    }
    static readonly Vector3[] FamilyTrackPath={new Vector3(0,.105f*.58f+.01f,.26f*.56f),new Vector3(0,.032f*.58f+.01f,.20f*.56f),new Vector3(0,.032f*.58f+.01f,-.20f*.56f),new Vector3(0,.105f*.58f+.01f,-.26f*.56f),new Vector3(0,.225f*.58f+.01f,-.26f*.56f),new Vector3(0,.298f*.58f+.01f,-.20f*.56f),new Vector3(0,.298f*.58f+.01f,.20f*.56f),new Vector3(0,.225f*.58f+.01f,.26f*.56f)};
    void AnimateFamilyTread(Transform t,Vector3 rest,float travel)
    {
        float nearest=float.PositiveInfinity,initial=0,total=0,restAngle=0;
        for(int i=0;i<8;i++)
        {
            Vector3 a=FamilyTrackPath[i],b=FamilyTrackPath[(i+1)%8],p=rest;p.x=0;Vector3 d=b-a;
            float f=Mathf.Clamp01(Vector3.Dot(p-a,d)/d.sqrMagnitude),distance=(p-a-d*f).sqrMagnitude;
            if(distance<nearest){nearest=distance;initial=total+d.magnitude*f;restAngle=Mathf.Atan2(d.y,d.z)*Mathf.Rad2Deg;}total+=d.magnitude;
        }
        float at=Mathf.Repeat(initial+travel,total);
        for(int i=0;i<8;i++){Vector3 a=FamilyTrackPath[i],b=FamilyTrackPath[(i+1)%8],d=b-a;if(at<=d.magnitude){Vector3 p=Vector3.Lerp(a,b,at/d.magnitude);p.x=rest.x;t.localPosition=p;t.localRotation=Quaternion.Euler(-(Mathf.Atan2(d.y,d.z)*Mathf.Rad2Deg-restAngle),0,0);break;}at-=d.magnitude;}
    }
    bool TryFamilyMuzzle(Fighter f,out Vector3 muzzle)
    {
        muzzle=Vector3.zero;if(!familyRigs.TryGetValue(f,out FamilyRig r)||r.root==null)return false;
        Transform t=r.muzzles[WeaponIndex(f)];if(t==null)return false;muzzle=t.position;return true;
    }
    readonly Dictionary<string,RenderTexture> familyPartPreviews=new Dictionary<string,RenderTexture>();
    RenderTexture FamilyPartPreview(string id)
    {
        if(id=="AUTO")id="W-01_LUMEN";
        if(familyPartPreviews.TryGetValue(id,out RenderTexture cached))return cached;
        Transform studio=new GameObject("Isolated part preview").transform;studio.position=new Vector3(5000,5000,5000);
        try
        {
            Transform part;
            if(AuxArtIndex(id)>=0){part=Instantiate(auxiliaryModelSources[AuxArtIndex(id)],studio,false).transform;PrepareWeaponMaterials(part);}
            else if(DronePackIndex(id)>=0){part=Instantiate(droneDockedPreviewSources[DronePackIndex(id)],studio,false).transform;part.localRotation=Quaternion.Euler(0,180,0)*part.localRotation;PrepareWeaponMaterials(part);}
            else if(WeaponCatalog().models.Any(x=>x.id==id&&x.kind=="firearm"))
            {
                part=Instantiate(Resources.Load<GameObject>(WeaponArtResource(id)),studio,false).transform;PrepareWeaponMaterials(part);
            }
            else part=WispFamilyAssets.Spawn(id,studio).root;
            foreach(Transform t in studio.GetComponentsInChildren<Transform>())t.gameObject.layer=30;
            Bounds bounds=AuxiliaryBounds(part,null);
            Camera camera=new GameObject("Part preview camera").AddComponent<Camera>();camera.transform.SetParent(studio,false);
            camera.enabled=false;camera.orthographic=true;camera.cullingMask=1<<30;
            camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.035f,.055f,.075f,1);
            camera.transform.position=bounds.center+new Vector3(2,1.4f,DronePackIndex(id)>=0?-3:3)*Mathf.Max(.2f,bounds.size.magnitude);
            camera.transform.LookAt(bounds.center);camera.nearClipPlane=.001f;camera.farClipPlane=100;
            Vector3 e=bounds.extents,u=camera.transform.up,v=camera.transform.right;
            float Extent(Vector3 axis)=>Mathf.Abs(axis.x)*e.x+Mathf.Abs(axis.y)*e.y+Mathf.Abs(axis.z)*e.z;
            camera.orthographicSize=Mathf.Max(.05f,Mathf.Max(Extent(u),Extent(v))*1.18f);
            Light light=new GameObject("Part preview light").AddComponent<Light>();light.transform.SetParent(studio,false);
            light.type=LightType.Directional;light.cullingMask=1<<30;light.intensity=1.3f;light.transform.rotation=Quaternion.Euler(35,-35,0);
            var rt=new RenderTexture(768,768,24);rt.Create();camera.targetTexture=rt;camera.Render();camera.targetTexture=null;
            familyPartPreviews[id]=rt;return rt;
        }
        finally{studio.gameObject.SetActive(false);Destroy(studio.gameObject);}
    }
    void SelectFamilyPart(int category,int direction)
    {
        if(category>=6){CycleAuxiliary(category-6,direction);return;}
        if(category>=4){SelectHandGun(category-4,direction);return;}
        FamilyLoadout c=FamilyConfig(player);string[] ids=FamilyCategoryIds(category);
        string current=FamilyCategoryValue(c,category);int index=Array.IndexOf(ids,current);
        string id=ids[(Mathf.Max(0,index)+direction+ids.Length)%ids.Length];
        if(category==0)c.head=id;else if(category==1)c.body=id;else if(category==2)c.legs=id;else if(category==3)c.backpack=id;else c.weapons[category-4]=id;
        SaveFamilyConfig();balanceMessage=FamilyPartLabel(id).Replace("신규 · ","")+" 적용 · 외형 저장 완료";
    }
    string[] FamilyCategoryIds(int category)=>category==0?FamilyHeads:category==1?FamilyBodies:category==2?FamilyLegs:category==3?FamilyBackpacks:FamilyWeapons;
    string FamilyCategoryValue(FamilyLoadout c,int category)=>category==0?c.head:category==1?c.body:category==2?c.legs:category==3?c.backpack:c.weapons[category-4];
    void DrawFamilyArtOptions()
    {
        FamilyLoadout c=FamilyConfig(player);
        string[] categories=new[]{"머리","몸통","하체","백팩","왼손","오른손"}.Concat(Enumerable.Range(0,BackpackAuxSlots(c.backpack)).Select(i=>"보조 "+(i+1))).ToArray();
        if(familyArtCategory>=categories.Length)familyArtCategory=3;float categoryWidth=552f/categories.Length;
        var categoryStyle=new GUIStyle(GUI.skin.button){fontSize=16,padding=new RectOffset(4,4,0,0),alignment=TextAnchor.MiddleCenter};
        for(int i=0;i<categories.Length;i++)
        {
            GUI.backgroundColor=familyArtCategory==i?Orange:new Color(.12f,.18f,.23f);
            if(GUI.Button(new Rect(694+i*categoryWidth,139,categoryWidth-4,36),categories[i],categoryStyle))familyArtCategory=i;
        }
        GUI.backgroundColor=Color.white;DrawBackpackMenu();
        if(familyArtCategory>=6){DrawAuxiliarySelector(familyArtCategory-6);return;}
        if(familyArtCategory>=4){DrawHandGunSelector(familyArtCategory-4);return;}
        string[] ids=FamilyCategoryIds(familyArtCategory);string current=FamilyCategoryValue(c,familyArtCategory);
        int index=Mathf.Max(0,Array.IndexOf(ids,current));
        Panel(new Rect(700,186,536,340),new Color(.035f,.055f,.075f));
        RenderTexture image=FamilyPartPreview(current);
        if(image!=null)GUI.DrawTexture(new Rect(758,190,420,330),image,ScaleMode.ScaleToFit,false);
        if(GUI.Button(new Rect(704,322,48,66),"◀"))SelectFamilyPart(familyArtCategory,-1);
        if(GUI.Button(new Rect(1182,322,48,66),"▶"))SelectFamilyPart(familyArtCategory,1);
        GUI.Label(new Rect(708,536,520,36),FamilyPartLabel(current).Replace("신규 · ",""),new GUIStyle(GUI.skin.label){fontSize=23,fontStyle=FontStyle.Bold,alignment=TextAnchor.MiddleCenter});
        GUI.Label(new Rect(708,576,520,25),(index+1)+" / "+ids.Length+"  ·  "+categories[familyArtCategory],new GUIStyle(small){alignment=TextAnchor.MiddleCenter});
        GUI.Label(new Rect(710,614,520,40),"좌우 화살표로 교체 · 왼쪽 기체에 즉시 적용",new GUIStyle(small){alignment=TextAnchor.MiddleCenter});
    }
    static string FamilyPartLabel(string id)
    {
        switch(id)
        {
            case "DRONE_ATTACK_V002":return "신규 · 공격 드론 백팩";
            case "DRONE_INTERCEPT_V002":return "신규 · 요격 드론 백팩";
            case "DRONE_JAMMER_V002":return "신규 · 교란 드론 백팩";
            case "HD01":return "공용 센서 헤드";
            case "BD01":return "공용 기본 코어";
            case "VIS_H01_RADOME":return "신규 · H-01 레이돔 헤드";
            case "VIS_H02_LONGSIGHT":return "신규 · H-02 롱사이트 헤드";
            case "VIS_H03_SWIFTFIN":return "신규 · H-03 스위프트핀 헤드";
            case "VIS_H04_BULWARK":return "신규 · H-04 불워크 헤드";
            case "VIS_H05_NEEDLE":return "신규 · H-05 니들 헤드";
            case "VIS_H06_LANCET":return "신규 · H-06 랜싯 헤드";
            case "VIS_C01_STANDARD":return "신규 · C-01 스탠더드 코어";
            case "VIS_C02_LIGHTFRAME":return "신규 · C-02 라이트프레임 코어";
            case "VIS_C03_BULWARK_CORE":return "신규 · C-03 불워크 코어";
            case "VIS_C04_ARC_CORE":return "신규 · C-04 아크 코어";
            case "VIS_B02_SKYWING":return "신규 · B-02 스카이윙 백팩";
            case "VIS_B03_CARGO_PACK":return "신규 · B-03 카고 백팩";
            case "VIS_L02_BASTION_TRACK":return "신규 · BASTION 전차 하체";
            case "VIS_L03_TARANTULAN":return "신규 · TARANTULAN 사족 하체";
            case "VIS_L04_DRAFT_HOVER":return "신규 · DRAFT 호버 하체";
            default:return id.Replace('_',' ');
        }
    }
}
