using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public sealed partial class VoxArena
{
    public GameObject[] auxiliaryModelSources,auxiliaryProjectileSources;public GameObject auxiliaryBracketSource;
    static readonly string[] AuxiliaryIds={"NONE","AUX_MISSILE_POD","AUX_PULSE_GUN"};
    sealed class AuxiliaryRuntime
    {
        public Fighter owner;public Transform pack;public Bounds packBounds;public Vector3[] restPositions=new Vector3[2],anchors=new Vector3[2],beamScales=new Vector3[2];public Transform[] brackets=new Transform[2];public string[] ids=new string[2];public Transform[] models=new Transform[2],muzzles=new Transform[2];
        public float[] cooldown=new float[2],reload=new float[2];public int[] ammo=new int[2];
    }
    readonly Dictionary<Fighter,AuxiliaryRuntime> auxiliaryRuntime=new Dictionary<Fighter,AuxiliaryRuntime>();
    static int AuxArtIndex(string id)=>id=="AUX_MISSILE_POD"?0:id=="AUX_PULSE_GUN"?1:-1;
    static string AuxName(string id)=>id=="AUX_MISSILE_POD"?"호밍 미사일 포드":id=="AUX_PULSE_GUN"?"펄스 빔건":"미장착";
    bool AuxSelected(Fighter f)=>f!=null&&f.activeAuxiliary>=0&&auxiliaryRuntime.TryGetValue(f,out AuxiliaryRuntime a)&&f.activeAuxiliary<2&&a.models[f.activeAuxiliary]!=null;
    void SetupAuxiliary(Fighter f,FamilyRig rig)
    {
        f.activeAuxiliary=-1;auxiliaryRuntime.Remove(f);int count=BackpackAuxSlots(rig.config.backpack);if(count==0)return;
        var a=new AuxiliaryRuntime{owner=f,pack=rig.backpack.root,packBounds=AuxiliaryBounds(rig.backpack.root,rig.backpack.root)};auxiliaryRuntime[f]=a;
        for(int i=0;i<count;i++)
        {
            string id=rig.config.auxiliary[i];a.ids[i]=id;int type=AuxArtIndex(id);if(type<0)continue;
            if(auxiliaryModelSources==null||auxiliaryModelSources[type]==null)throw new InvalidOperationException("Missing auxiliary model "+id);
            Transform mount=Module(id,rig.backpack.root,Vector3.zero);
            Transform model=Instantiate(auxiliaryModelSources[type],mount,false).transform;model.localScale*=.55f;PrepareWeaponMaterials(model);
            Transform socket=DroneSocket(model,"BACKPACK_MOUNT");if(socket==null)throw new InvalidOperationException("Missing auxiliary mount");model.position+=mount.position-socket.position;
            a.models[i]=mount;a.muzzles[i]=DroneSocket(model,"MUZZLE");a.ammo[i]=type==0?4:12;
            Bounds partBounds=AuxiliaryBounds(mount,rig.backpack.root);
            mount.localPosition+=new Vector3(0,a.packBounds.max.y-.05f-partBounds.center.y,a.packBounds.center.z-.025f-partBounds.center.z);
            ClearAuxiliaryHull(a,i);a.restPositions[i]=mount.localPosition;
            if(auxiliaryBracketSource==null)throw new InvalidOperationException("Missing Blender auxiliary bracket");
            a.anchors[i]=new Vector3(i==0?-.07f:.07f,Mathf.Clamp(mount.localPosition.y,a.packBounds.min.y,a.packBounds.max.y),a.packBounds.center.z);
            a.brackets[i]=Instantiate(auxiliaryBracketSource,rig.backpack.root,false).transform;PrepareWeaponMaterials(a.brackets[i]);a.beamScales[i]=DroneSocket(a.brackets[i],"ExtensionBeam").localScale;UpdateAuxiliaryBracket(a,i);

        }
    }
    Bounds AuxiliaryBounds(Transform model,Transform space)
    {
        bool first=true;Bounds bounds=new Bounds();
        foreach(MeshFilter filter in model.GetComponentsInChildren<MeshFilter>())
        {
            if(filter.sharedMesh==null)continue;Bounds b=filter.sharedMesh.bounds;
            for(int x=-1;x<=1;x+=2)for(int y=-1;y<=1;y+=2)for(int z=-1;z<=1;z+=2)
            {
                Vector3 worldPoint=filter.transform.TransformPoint(b.center+Vector3.Scale(b.extents,new Vector3(x,y,z)));Vector3 point=space!=null?space.InverseTransformPoint(worldPoint):worldPoint;
                if(first){bounds=new Bounds(point,Vector3.zero);first=false;}else bounds.Encapsulate(point);
            }
        }
        if(first)throw new InvalidOperationException("Auxiliary bounds have no authored geometry");return bounds;
    }
    void ClearAuxiliaryHull(AuxiliaryRuntime a,int i)
    {
        Bounds b=AuxiliaryBounds(a.models[i],a.pack);
        float offset=i==0?a.packBounds.min.x-.025f-b.max.x:a.packBounds.max.x+.025f-b.min.x;
        a.models[i].localPosition+=Vector3.right*offset;
    }
    void UpdateAuxiliaryBracket(AuxiliaryRuntime a,int i)
    {
        Transform bracket=a.brackets[i];if(bracket==null)return;Vector3 start=a.anchors[i],delta=a.models[i].localPosition-start;float length=delta.magnitude;
        bracket.localPosition=start;bracket.localRotation=Quaternion.FromToRotation(Vector3.right,delta.normalized);
        Transform beam=DroneSocket(bracket,"ExtensionBeam"),tip=DroneSocket(bracket,"TIP_PIVOT");
        if(beam==null||tip==null)throw new InvalidOperationException("Missing telescopic bracket nodes");
        Vector3 worldStart=a.pack.TransformPoint(start),worldEnd=a.models[i].position;
        Vector3 beamDirection=beam.parent.InverseTransformVector(worldEnd-worldStart);MeshFilter geometry=beam.GetComponent<MeshFilter>();
        if(geometry==null||geometry.sharedMesh==null)throw new InvalidOperationException("Bracket beam has no authored mesh");
        beam.position=(worldStart+worldEnd)*.5f;
        beam.localRotation=Quaternion.FromToRotation(Vector3.right,beamDirection.normalized);
        beam.localScale=new Vector3(beamDirection.magnitude/geometry.sharedMesh.bounds.size.x,a.beamScales[i].y,a.beamScales[i].z);
        tip.position=worldEnd;Transform contact=DroneSocket(bracket,"WEAPON_CONTACT");if(contact!=null)contact.position=worldEnd;

    }
    bool SelectAuxiliary(Fighter f,int index)
    {
        if(f==null||index<0||index>=2||!auxiliaryRuntime.TryGetValue(f,out AuxiliaryRuntime a)||a.models[index]==null)return false;
        f.activeAuxiliary=index;return true;
    }
    bool TryAuxMuzzle(Fighter f,out Vector3 p)
    {
        p=Vector3.zero;if(!AuxSelected(f))return false;AuxiliaryRuntime a=auxiliaryRuntime[f];p=a.muzzles[f.activeAuxiliary].position;return true;
    }
    bool AuxLockAvailable(Fighter f)=>AuxSelected(f)&&auxiliaryRuntime[f].ids[f.activeAuxiliary]=="AUX_MISSILE_POD";
    void AimAuxiliary(Fighter f,Vector3 direction)
    {
        if(!AuxSelected(f)||!Finite(direction)||direction.sqrMagnitude<.001f)return;
        AuxiliaryRuntime a=auxiliaryRuntime[f];int i=f.activeAuxiliary;a.models[i].localPosition=a.restPositions[i];a.models[i].rotation=Quaternion.LookRotation(direction,Vector3.up);ClearAuxiliaryHull(a,i);UpdateAuxiliaryBracket(a,i);
    }
    bool FireAuxiliary(Fighter f,Fighter target,Vector3 direction)
    {
        if(!AuxSelected(f)||Finished||f.deadVisual||f.hp<=0||f.lockTime>0||direction.sqrMagnitude<.001f)return false;
        AuxiliaryRuntime a=auxiliaryRuntime[f];int i=f.activeAuxiliary,type=AuxArtIndex(a.ids[i]);if(a.cooldown[i]>0||a.reload[i]>0||a.ammo[i]<=0)return false;
        AimAuxiliary(f,direction);Vector3 muzzle=a.muzzles[i].position;
        if(ObstacleOnSegment(f.root.position,muzzle,.08f,out _))return false;
        bool missile=type==0;float speed=missile?42:150,damage=missile?24:18,radius=missile?.12f:.07f,range=missile?110:90;
        bool seek=missile&&target!=null&&IsHostile(f,target)&&target.hp>0&&Vector3.Distance(muzzle,target.root.position)<range&&Vector3.Angle(direction,target.root.position-muzzle)<65&&!ObstacleOnSegment(muzzle,target.root.position,.08f,out _);
        Shot shot=Fire(f,direction,damage,speed,radius,range,seek?2.4f:0,missile?1.8f:0,origin:muzzle,projectileSource:auxiliaryProjectileSources[type],projectileVisualScale:missile?1f:2.5f,missile:missile);
        if(shot==null)return false;shot.isMissile=missile;shot.seekTarget=seek?target:null;shot.turnRate=1.8f;shot.weaponId=a.ids[i];AttachMissileExhaust(shot);
        a.ammo[i]--;a.cooldown[i]=missile?.7f:.25f;if(a.ammo[i]==0)a.reload[i]=missile?6:4;
        PlaySound(missile?SoundKind.Fire:SoundKind.Laser,muzzle,.4f);return true;
    }
    void UpdateAuxiliaries(float dt)
    {
        foreach(AuxiliaryRuntime a in auxiliaryRuntime.Values.ToArray())
        {
        if(a.owner.root==null||!familyRigs.ContainsKey(a.owner)){auxiliaryRuntime.Remove(a.owner);continue;}
        for(int i=0;i<2;i++)
        {
            a.cooldown[i]=Mathf.Max(0,a.cooldown[i]-dt);
            if(a.reload[i]>0){a.reload[i]=Mathf.Max(0,a.reload[i]-dt);if(a.reload[i]==0)a.ammo[i]=AuxArtIndex(a.ids[i])==0?4:12;}
        }
        }
    }
    void ResetAuxiliaries()
    {
        foreach(AuxiliaryRuntime a in auxiliaryRuntime.Values){a.owner.activeAuxiliary=-1;for(int i=0;i<2;i++){a.cooldown[i]=a.reload[i]=0;a.ammo[i]=AuxArtIndex(a.ids[i])==0?4:12;}}
    }
    void CycleAuxiliary(int slot,int direction)
    {
        FamilyLoadout c=FamilyConfig(player);if(slot<0||slot>=BackpackAuxSlots(c.backpack))return;
        int current=Array.IndexOf(AuxiliaryIds,c.auxiliary[slot]);c.auxiliary[slot]=AuxiliaryIds[(Mathf.Max(0,current)+direction+AuxiliaryIds.Length)%AuxiliaryIds.Length];
        SaveFamilyConfig();balanceMessage=(slot+1)+"번 보조 무기: "+AuxName(c.auxiliary[slot]);
    }
    void DrawAuxiliarySelector(int slot)
    {
        FamilyLoadout c=FamilyConfig(player);if(slot>=BackpackAuxSlots(c.backpack)){familyArtCategory=3;return;}
        string id=c.auxiliary[slot];Panel(new Rect(700,186,536,340),new Color(.035f,.055f,.075f));
        if(AuxArtIndex(id)>=0)GUI.DrawTexture(new Rect(758,190,420,330),FamilyPartPreview(id),ScaleMode.ScaleToFit,false);
        else GUI.Label(new Rect(758,300,420,70),"보조 무기 미장착",new GUIStyle(GUI.skin.label){fontSize=24,alignment=TextAnchor.MiddleCenter});
        if(GUI.Button(new Rect(704,322,48,66),"◀"))CycleAuxiliary(slot,-1);
        if(GUI.Button(new Rect(1182,322,48,66),"▶"))CycleAuxiliary(slot,1);
        GUI.Label(new Rect(708,536,520,36),AuxName(id),new GUIStyle(GUI.skin.label){fontSize=23,alignment=TextAnchor.MiddleCenter});
        GUI.Label(new Rect(708,576,520,25),(slot+3)+"번 키 선택 → 좌클릭 발사",new GUIStyle(small){alignment=TextAnchor.MiddleCenter});
    }
    void DrawAuxiliaryMountRow(int mount,Rect card)
    {
        FamilyLoadout c=FamilyConfig(player);int index=mount-2,capacity=BackpackAuxSlots(c.backpack);
        string text=DronePackIndex(c.backpack)>=0?(index==0?"3번 키: 드론 능력":"추가 보조 슬롯 없음"):index>=capacity?"이 백팩은 해당 슬롯을 지원하지 않습니다.":AuxName(c.auxiliary[index]);
        GUI.Label(new Rect(card.x+90,card.y+16,350,28),text,small);
        if(index<capacity&&GUI.Button(new Rect(card.x+90,card.y+47,340,28),"보조 무기 선택")){balanceTab=6;familyArtCategory=6+index;}
    }
    bool DrawAuxiliaryHud(int slot,Rect area)
    {
        int index=slot-2;if(index<0||index>=2||!auxiliaryRuntime.TryGetValue(player,out AuxiliaryRuntime a)||a.models[index]==null)return false;
        Panel(area,player.activeAuxiliary==index?new Color(.05f,.25f,.28f):new Color(.04f,.08f,.12f));
        GUI.Label(area,(slot+1)+" "+(AuxArtIndex(a.ids[index])==0?"미사일":"펄스")+"\n"+(a.reload[index]>0?"충전 "+a.reload[index].ToString("0.0"):a.ammo[index]+" 발"),new GUIStyle(small){alignment=TextAnchor.MiddleCenter});return true;
    }
    void CaptureAuxiliaryAssembly(Fighter f,string name,bool front=false,string iteration="Iteration3")
    {
        FamilyRig rig=familyRigs[f];var layers=new Dictionary<Transform,int>();foreach(Transform t in rig.root.GetComponentsInChildren<Transform>(true)){layers[t]=t.gameObject.layer;t.gameObject.layer=9;}
        Camera camera=new GameObject("Auxiliary assembly evidence").AddComponent<Camera>();camera.enabled=false;camera.orthographic=true;camera.cullingMask=1<<9;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.035f,.05f,.07f);
        var rt=new RenderTexture(800,900,24);camera.targetTexture=rt;var image=new Texture2D(800,900,TextureFormat.RGB24,false);RenderTexture previous=RenderTexture.active;
        try
        {
            Bounds bounds=AuxiliaryBounds(rig.root,null);
            camera.orthographicSize=Mathf.Max(bounds.size.y*.7f,bounds.size.x*.8f);camera.transform.position=bounds.center+new Vector3(front?4:-4,3,front?7:-7);camera.transform.LookAt(bounds.center);camera.Render();
            RenderTexture.active=rt;image.ReadPixels(new Rect(0,0,800,900),0,0);image.Apply();string folder=System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(),"Docs","Validation","Overnight-20261119",iteration);System.IO.Directory.CreateDirectory(folder);System.IO.File.WriteAllBytes(System.IO.Path.Combine(folder,name+".png"),image.EncodeToPNG());
        }
        finally{RenderTexture.active=previous;foreach(var entry in layers)entry.Key.gameObject.layer=entry.Value;camera.targetTexture=null;rt.Release();Destroy(rt);Destroy(image);Destroy(camera.gameObject);}
    }
    void AuxiliaryChecks(Action<bool,string> check)
    {
        BalanceDocument prior=activeBalance;FamilyLoadoutFile priorFamily=familyLoadouts;bool oldSmoke=smoke,oldMenu=inMenu;string priorDirectory=balanceDirectoryOverride;
        try
        {
            balanceDirectoryOverride=System.IO.Path.Combine(System.IO.Path.GetTempPath(),"VoxelOn-Auxiliary-Checks-"+DateTime.Now.ToString("yyyyMMdd-HHmmss"));
            activeBalance=BalanceData.Defaults();familyLoadouts=new FamilyLoadoutFile();TestSetupSelectable(0,1);smoke=false;inMenu=false;
            FamilyLoadout c=FamilyConfig(player);c.backpack="B-02_CROSSLANCE";c.auxiliary=new[]{"AUX_MISSILE_POD","AUX_PULSE_GUN"};BuildRobot(player);
            check(auxiliaryRuntime[player].models.All(x=>x!=null)&&BackpackAuxSlots(c.backpack)==2,"two-mount backpack creates both actual auxiliary models");
            SaveFamilyConfig();FamilyLoadoutFile stored=JsonUtility.FromJson<FamilyLoadoutFile>(System.IO.File.ReadAllText(FamilyLoadoutPath));check(stored.designs.First(x=>x.key==c.key).auxiliary.SequenceEqual(c.auxiliary),"auxiliary selections save independently of PD balance data");
            CaptureAuxiliaryAssembly(player,"Two-Auxiliary-Mounts");CaptureAuxiliaryAssembly(player,"Two-Auxiliary-Mounts-Front",true);
            for(int slot=0;slot<2;slot++)
            {
                AuxiliaryRuntime fitted=auxiliaryRuntime[player];Bounds mounted=AuxiliaryBounds(fitted.models[slot],fitted.pack);
                check(slot==0?mounted.max.x<fitted.packBounds.min.x:mounted.min.x>fitted.packBounds.max.x,"authored auxiliary model clears original backpack hull "+slot);
                check(fitted.brackets[slot]!=null,"auxiliary uses actual Blender telescopic connection "+slot);
                Transform contact=DroneSocket(fitted.brackets[slot],"WEAPON_CONTACT");check(contact!=null&&Vector3.Distance(contact.position,fitted.models[slot].position)<.01f,"extended bracket contact meets actual weapon mount "+slot);
                SelectAuxiliary(player,slot);AimAuxiliary(player,Quaternion.Euler(0,slot==0?-25:25,0)*Vector3.forward);mounted=AuxiliaryBounds(fitted.models[slot],fitted.pack);
                check(slot==0?mounted.max.x<fitted.packBounds.min.x:mounted.min.x>fitted.packBounds.max.x,"aimed auxiliary also clears original backpack hull "+slot);
            }
            player.activeAuxiliary=-1;
            SaveGunValidationImage(FamilyPartPreview("AUX_MISSILE_POD"),"AUX-Missile-Pod");SaveGunValidationImage(FamilyPartPreview("AUX_PULSE_GUN"),"AUX-Pulse-Gun");
            check(SelectAuxiliary(player,0)&&AuxLockAvailable(player),"slot3 selects missile pod with explicit lock support");ClearCombat();SelectAuxiliary(player,0);
            Vector3 original=player.facing;enemy.root.position=player.root.position+Vector3.forward*25+Vector3.right*5;
            check(FireAuxiliary(player,enemy,Vector3.forward)&&shots.Count==1&&shots[0].isMissile&&shots[0].tracking>0,"missile pod launches a missile that can home on a visible target");
            Shot missile=shots[0];float x=missile.velocity.x;UpdateCombat(.03f);check(missile.velocity.x>x,"homing missile turns toward target over time");
            int ammo=auxiliaryRuntime[player].ammo[0];check(!FireAuxiliary(player,enemy,Vector3.forward)&&auxiliaryRuntime[player].ammo[0]==ammo,"auxiliary cooldown prevents duplicate ammunition consumption");
            check(SelectAuxiliary(player,1)&&!AuxLockAvailable(player),"slot4 selects pulse gun without inheriting hand lock action");
            int beamCount=beams.Count,shotCount=shots.Count;check(FireAuxiliary(player,enemy,Vector3.forward)&&shots.Count==shotCount+1&&beams.Count==beamCount&&!shots.Last().isMissile,"pulse gun emits traveling non-missile bolt instead of continuous beam");
            Shot pulse=shots.Last();Vector3 position=pulse.visual.position;UpdateCombat(.02f);check(Vector3.Distance(pulse.visual.position,position)>1,"laser bolt actually travels after emission");
            ClearCombat();SelectAuxiliary(player,0);AuxiliaryRuntime a=auxiliaryRuntime[player];
            for(int n=0;n<4;n++){a.cooldown[0]=0;FireAuxiliary(player,enemy,Vector3.forward);}
            check(a.ammo[0]==0&&a.reload[0]>0&&!FireAuxiliary(player,enemy,Vector3.forward),"empty missile pod enters its own recharge and cannot fire");UpdateAuxiliaries(6.1f);check(a.ammo[0]==4&&a.reload[0]==0,"missile pod restores four rounds after recharge");
            c.backpack="B-03_OVERTHRUST";BuildRobot(player);check(!auxiliaryRuntime.ContainsKey(player)&&!SelectAuxiliary(player,0),"zero-mount backpack hides and rejects both auxiliary slots");
            c.backpack="B-01_SKYWING";BuildRobot(player);check(auxiliaryRuntime[player].models[0]!=null&&auxiliaryRuntime[player].models[1]==null&&!SelectAuxiliary(player,1),"one-mount backpack exposes first auxiliary only");
            c.backpack=DronePackIds[0];BuildRobot(player);check(droneRuntime.ContainsKey(player)&&!auxiliaryRuntime.ContainsKey(player)&&!SelectAuxiliary(player,0),"drone ability excludes normal auxiliaries on slot3");
            c.backpack="B-02_CROSSLANCE";BuildRobot(player);check(c.auxiliary[0]=="AUX_MISSILE_POD"&&c.auxiliary[1]=="AUX_PULSE_GUN","temporarily hidden auxiliary choices survive backpack switching");
            player.activeAuxiliary=1;ClearCombat();check(player.activeAuxiliary==-1,"round reset clears auxiliary selection");
        }
        finally{activeBalance=prior;familyLoadouts=priorFamily;balanceDirectoryOverride=priorDirectory;smoke=oldSmoke;inMenu=oldMenu;ClearCombat();TestSetupSelectable(0,1);}
    }

}
