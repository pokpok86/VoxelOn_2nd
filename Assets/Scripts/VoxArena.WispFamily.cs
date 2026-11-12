using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public sealed partial class VoxArena
{
    [Serializable] sealed class FamilyLoadout
    {
        public string key,legs,backpack="B-01_SKYWING";public bool enabled=true;
        public string[] weapons={"AUTO","AUTO","AUTO","AUTO"};
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
    static readonly string[] FamilyLegs={"L-01_STRIDER","L-02_BASTION_TRACK","L-03_TARANTULAN","L-05_SKYLIFT"};
    static readonly string[] FamilyBackpacks={"B-01_SKYWING","B-02_CROSSLANCE","B-03_OVERTHRUST","B-04_LONGTANK","B-05_RAMJET"};
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
        string key=(design?(activeBalance?.activeDesignId??"DESIGN_01"):"AI")+"/"+f.spec.name;
        FamilyLoadout c=familyLoadouts.designs.Find(x=>x!=null&&x.key==key);
        if(c==null){c=new FamilyLoadout{key=key,enabled=f.spec.name=="WISP"||f.spec.name=="BASTION"||f.spec.name=="TARANTULA",legs=f.spec.mobility==MobilityKind.Tank?FamilyLegs[1]:f.spec.mobility==MobilityKind.Quadruped?FamilyLegs[2]:FamilyLegs[0]};familyLoadouts.designs.Add(c);}
        if(Array.IndexOf(FamilyLegs,c.legs)<0)c.legs=FamilyLegs[0];if(Array.IndexOf(FamilyBackpacks,c.backpack)<0)c.backpack=FamilyBackpacks[0];
        if(c.weapons==null||c.weapons.Length!=4)c.weapons=new[]{"AUTO","AUTO","AUTO","AUTO"};
        for(int i=0;i<4;i++)if(Array.IndexOf(FamilyWeapons,c.weapons[i])<0)c.weapons[i]="AUTO";
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
        string picked=c.weapons[slot];if(picked!="AUTO")return picked;
        string id=(f.weaponArtIds?[slot]??"").ToUpperInvariant();
        if(id.Contains("SWORD")||id.Contains("BLADE")||id.Contains("SABER"))return "W-04_EDGE";
        if(id.Contains("GATLING")||id.Contains("LMG")||id.Contains("VULCAN"))return "W-03_RATTLE";
        if(id.Contains("_RL")||id.Contains("_GL")||id.Contains("ROCKET")||id.Contains("BAZOOKA")||id.Contains("CANNON"))return "W-02_BREAKER";
        return "W-01_LUMEN";
    }
    void InstallFamilyArt(Fighter f)
    {
        familyRigs.Remove(f);if(!WispFamilyAssets.Available||Array.IndexOf(Environment.GetCommandLineArgs(),"--legacy-family-art")>=0)return;
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
        r.body=WispFamilyAssets.Spawn("BD01",r.upper);r.body.root.localPosition=Vector3.up*(-r.body.data.waistBottom);
        r.head=WispFamilyAssets.Spawn("HD01",r.upper);r.head.root.localPosition=new Vector3(0,-r.body.data.waistBottom+r.body.data.neckTop+.046f,.003f);
        r.backpack=WispFamilyAssets.Spawn(c.backpack,r.upper);r.backpack.root.localPosition=r.body.root.localPosition+WispFamilyAssets.V(r.body.data.backMount);
        Vector3[] rawElbows={WispFamilyAssets.V(r.body.data.leftElbow),WispFamilyAssets.V(r.body.data.rightElbow)};
        for(int side=0;side<2;side++)r.elbows[side]=rawElbows[side]-r.body.nodes[side==0?"LeftShoulder":"RightShoulder"].localPosition;
        foreach(var n in r.legs.nodes)r.legRest[n.Key]=n.Value.localPosition;
        var supportNodes=r.legs.data.nodes.Where(n=>n.motion=="ankle"||n.motion=="quadKnee"||n.motion=="tread").Select(n=>n.name).ToArray();
        r.supports=r.legs.root.GetComponentsInChildren<MeshFilter>().Where(m=>Array.IndexOf(supportNodes,m.transform.parent.name)>=0)
            .Select(m=>new FamilySupportMesh{transform=m.transform,vertices=m.sharedMesh.vertices.Distinct().ToArray()}).ToArray();
        for(int i=0;i<4;i++)
        {
            string id=FamilyWeapon(f,c,i);r.weaponIds[i]=id;if(id==null)continue;
            var art=WispFamilyAssets.Spawn(id,r.root);r.weapons[i]=art;art.root.localScale=Vector3.one*(i<2?.42f:.32f);
            if(i<2&&f.weaponProfiles[i].isBeamShield)f.shieldBeamVisuals[i]=CreateShieldBeamVisual(art.root);
            // The authored right-side blade/shield rolls reverse on the left.
            if((i==0||i==2)&&(id=="W-04_EDGE"||id=="W-05_AEGIS"))foreach(Transform child in art.root)child.localRotation=Quaternion.Euler(0,0,180)*child.localRotation;
            var points=art.data.groups.SelectMany(g=>Enumerable.Range(0,g.positions.Length/3).Select(v=>new Vector3(g.positions[v*3],g.positions[v*3+1],g.positions[v*3+2]))).ToArray();
            Vector3 min=points[0],max=points[0];foreach(Vector3 p in points){min=Vector3.Min(min,p);max=Vector3.Max(max,p);}
            r.muzzles[i]=Module("Actual authored muzzle",art.root,new Vector3((min.x+max.x)*.5f,(min.y+max.y)*.5f,max.z));
        }
        familyRigs.Add(f,r);f.headPart=r.head.root;wispHeadBoundsReady=false;CacheWispHeadBounds(f);UpdateFamilyArt(f);
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
    void DrawFamilyArtOptions()
    {
        FamilyLoadout c=FamilyConfig(player);
        GUI.Label(new Rect(370,135,850,30),"새 WISP 파츠 · 외형만 변경 / 이동 방식과 성능 수치는 유지",new GUIStyle(GUI.skin.label){fontSize=17});
        if(GUI.Button(new Rect(370,177,840,36),c.enabled?"신형 외형 사용 중 · 클릭하면 기존 외형":"기존 외형 사용 중 · 클릭하면 신형 외형")){c.enabled=!c.enabled;SaveFamilyConfig();return;}
        string Row(float y,string caption,string current,string[] ids)
        {
            GUI.Label(new Rect(370,y+7,150,30),caption);int i=Array.IndexOf(ids,current);if(i<0)i=0;
            if(GUI.Button(new Rect(525,y,45,42),"‹"))i=(i+ids.Length-1)%ids.Length;
            GUI.Label(new Rect(580,y+7,550,30),ids[i].Replace('_',' '));
            if(GUI.Button(new Rect(1165,y,45,42),"›"))i=(i+1)%ids.Length;return ids[i];
        }
        string back=Row(230,"백팩",c.backpack,FamilyBackpacks),legs=Row(284,"하체",c.legs,FamilyLegs);
        bool changed=back!=c.backpack||legs!=c.legs;c.backpack=back;c.legs=legs;
        for(int i=0;i<2;i++)
        {
            string current=c.weapons[i];string selected=Row(338+i*54,(i+1)+"번 무기 외형",current,FamilyWeapons);
            changed|=selected!=current;c.weapons[i]=selected;
        }
        GUI.Label(new Rect(370,466,840,50),"3·4번 어깨 무장: 미장착 · 백팩 장착부 준비 중",small);
        GUI.Label(new Rect(370,570,850,35),"AUTO는 현재 무기에 맞춘 외형 · 방패/빈 슬롯은 실제 장착 상태를 따름",small);
        if(changed){SaveFamilyConfig();balanceMessage="새 파츠 외형 적용 · 성능 수치 보존";}
    }
}
