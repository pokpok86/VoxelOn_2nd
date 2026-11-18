using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public sealed partial class VoxArena
{
    public GameObject[] droneBackpackSources,droneModelSources,droneDockedPreviewSources;
    enum DronePhase { Docked,Deploying,Active,Returning,Charging }
    sealed class DroneRuntime
    {
        public Fighter owner;public int kind;public Transform visual,dock,frame;public Vector3 contact,localForward,localUp,restScale;public Quaternion basisCorrection;public DronePhase phase;
        public float timer,fireTimer,orbit,recharge,radius=.6f,blockedTime,fold=1;public int interceptions;
        public Transform fieldVisual;public Transform[] fins,muzzles;public int shotSequence;public Quaternion[] finRest;public readonly List<Vector3> route=new List<Vector3>();
    }
    readonly Dictionary<Fighter,DroneRuntime> droneRuntime=new Dictionary<Fighter,DroneRuntime>();
    static readonly string[] DronePackIds={"DRONE_ATTACK_V002","DRONE_INTERCEPT_V002","DRONE_JAMMER_V002"};
    static int DronePackIndex(string id)=>Array.IndexOf(DronePackIds,id);
    static int BackpackAuxSlots(string id)=>DronePackIndex(id)>=0?0:id=="B-02_CROSSLANCE"||id=="B-04_LONGTANK"?2:id=="B-01_SKYWING"||id=="VIS_B03_CARGO_PACK"?1:0;
    bool IsGroundDronePack(Fighter f)=>f!=null&&familyRigs.TryGetValue(f,out FamilyRig rig)&&rig.config.backpack==DronePackIds[0];
    Transform DroneSocket(Transform root,string name)=>root.GetComponentsInChildren<Transform>(true).FirstOrDefault(t=>t.name.EndsWith(name,StringComparison.Ordinal));
    WispFamilyAssets.Instance SpawnDronePack(string id,Transform parent)
    {
        int kind=DronePackIndex(id);if(kind<0)return WispFamilyAssets.Spawn(id,parent);
        if(droneBackpackSources==null||kind>=droneBackpackSources.Length||droneBackpackSources[kind]==null)throw new InvalidOperationException("Missing drone backpack source "+id);
        Transform holder=Module(id,parent,Vector3.zero),model=Instantiate(droneBackpackSources[kind],holder,false).transform;model.localRotation=Quaternion.Euler(0,180,0)*model.localRotation;PrepareWeaponMaterials(model);
        Bounds authored=AuxiliaryBounds(model,holder);model.localScale*=.40f/Mathf.Max(.001f,authored.size.x);
        Transform mount=DroneSocket(model,"BODY_MOUNT");if(mount==null)throw new InvalidOperationException("Missing backpack BODY_MOUNT "+id);
        model.position+=holder.position-mount.position;
        return new WispFamilyAssets.Instance{root=holder,data=new WispFamilyAssets.Asset{id=id,category="backpack"}};
    }
    void RemoveDroneRuntime(Fighter f)
    {
        if(!droneRuntime.TryGetValue(f,out DroneRuntime state))return;
        if(state.fieldVisual!=null){state.fieldVisual.gameObject.SetActive(false);Destroy(state.fieldVisual.gameObject);}
        if(state.visual!=null){state.visual.gameObject.SetActive(false);Destroy(state.visual.gameObject);}droneRuntime.Remove(f);
    }
    void SetupDroneRuntime(Fighter f,FamilyRig rig)
    {
        int kind=DronePackIndex(rig.config.backpack);if(kind<0)return;
        if(droneModelSources==null||kind>=droneModelSources.Length||droneModelSources[kind]==null)throw new InvalidOperationException("Missing detached drone source");
        Transform dock=DroneSocket(rig.backpack.root,"DRONE_DOCK");if(dock==null)throw new InvalidOperationException("Missing DRONE_DOCK");
        Transform model=Module("Drone motion root / "+kind,rig.backpack.root,Vector3.zero);
        Transform imported=Instantiate(droneModelSources[kind],model,false).transform;PrepareWeaponMaterials(imported);
        Bounds authoredDrone=AuxiliaryBounds(model,model);imported.localScale*=.34f/Mathf.Max(.001f,authoredDrone.size.x);
        Transform contact=DroneSocket(model,"DOCK_CONTACT");if(contact==null)throw new InvalidOperationException("Missing DOCK_CONTACT");
        var d=new DroneRuntime{owner=f,kind=kind,dock=dock,visual=model,contact=model.InverseTransformPoint(contact.position),phase=DronePhase.Docked};
        d.muzzles=kind==0?new[]{DroneSocket(model,"MUZZLE_L"),DroneSocket(model,"MUZZLE_R")}:Array.Empty<Transform>();if(kind==0&&d.muzzles.Any(t=>t==null))throw new InvalidOperationException("Missing actual drone cannon muzzles");
        d.frame=rig.backpack.root;d.restScale=model.localScale;Transform center=DroneSocket(model,"DRONE_CENTER"),top=DroneSocket(model,"Top identification");
        if(center==null||top==null)throw new InvalidOperationException("Missing drone orientation guides");
        d.localForward=model.InverseTransformDirection(center.position-contact.position).normalized;
        d.localUp=Vector3.ProjectOnPlane(model.InverseTransformDirection(top.position-center.position),d.localForward).normalized;
        d.basisCorrection=Quaternion.Inverse(Quaternion.LookRotation(d.localForward,d.localUp));
        d.fins=model.GetComponentsInChildren<Transform>(true).Where(t=>t.name.StartsWith("FIN_PIVOT_",StringComparison.Ordinal)).ToArray();d.finRest=d.fins.Select(t=>t.localRotation).ToArray();
        Bounds localGeometry=AuxiliaryBounds(model,model);float radius=.15f;
        for(int x=-1;x<=1;x+=2)for(int y=-1;y<=1;y+=2)for(int z=-1;z<=1;z+=2)radius=Mathf.Max(radius,model.TransformVector(localGeometry.center+Vector3.Scale(localGeometry.extents,new Vector3(x,y,z))).magnitude);
        d.radius=radius;
        droneRuntime[f]=d;SeatDrone(d);
    }
    void SeatDrone(DroneRuntime d)
    {
        if(d.visual==null||d.dock==null)return;
        if(d.visual.parent!=d.frame)d.visual.SetParent(d.frame,false);
        d.visual.localScale=d.restScale;d.visual.rotation=DroneDockRotation(d);d.visual.position=d.dock.position-d.visual.TransformVector(d.contact);ApplyDroneFold(d,1);
    }
    Quaternion DroneDockRotation(DroneRuntime d)=>d.frame.rotation*Quaternion.Euler(0,180,0)*d.basisCorrection;
    bool DeployDrone(Fighter f)
    {
        if(f==null||f.deadVisual||f.hp<=0||inMenu||Finished||!droneRuntime.TryGetValue(f,out DroneRuntime d)||d.phase!=DronePhase.Docked)return false;
        if(DroneFlightBlocked(d.visual.position,d.visual.position,d.radius,out _))return false;
        d.phase=DronePhase.Deploying;d.timer=0;d.fireTimer=0;d.blockedTime=0;d.interceptions=0;d.route.Clear();d.visual.SetParent(null,true);return true;
    }
    void ResetDrones()
    {
        foreach(DroneRuntime d in droneRuntime.Values){d.phase=DronePhase.Docked;d.timer=d.recharge=d.blockedTime=0;d.route.Clear();if(d.fieldVisual!=null)d.fieldVisual.gameObject.SetActive(false);if(d.owner.hp>0)SeatDrone(d);}
    }
    void ApplyDroneFold(DroneRuntime d,float amount)
    {
        d.fold=Mathf.Clamp01(amount);
        for(int i=0;i<d.fins.Length;i++)
        {
            Transform fin=d.fins[i];float side=fin.name.StartsWith("FIN_PIVOT_-1_",StringComparison.Ordinal)?-1:1;
            Vector3 axis=fin.parent.InverseTransformDirection(d.visual.TransformDirection(d.localForward));fin.localRotation=Quaternion.AngleAxis(side*85*d.fold,axis)*d.finRest[i];
        }
    }
    static readonly Vector3[] DroneTerrainOffsets={Vector3.right,Vector3.left,Vector3.forward,Vector3.back};
    float DroneTerrainFloor(Vector3 point,float radius)
    {
        float height=SurfaceHeight(point);
        foreach(Vector3 offset in DroneTerrainOffsets)height=Mathf.Max(height,SurfaceHeight(point+offset*radius));
        return height;
    }
    int DroneTerrainSamples(Vector3 from,Vector3 to)=>Mathf.Clamp(Mathf.CeilToInt(Vector3.Distance(from,to)*2),1,1024);
    bool DroneFlightBlocked(Vector3 from,Vector3 to,float radius,out Bounds obstacle)
    {
        obstacle=new Bounds();Vector3 delta=to-from;float length=delta.magnitude,nearest=float.PositiveInfinity;
        foreach(Bounds b in obstacles)
        {
            Bounds expanded=b;expanded.Expand(radius*2);
            if(expanded.Contains(from)){obstacle=b;return true;}
            if(length>.0001f&&expanded.IntersectRay(new Ray(from,delta/length),out float hit)&&hit<=length&&hit<nearest){nearest=hit;obstacle=b;}
        }
        if(nearest<=length)return true;
        int samples=DroneTerrainSamples(from,to);
        for(int i=0;i<=samples;i++)
        {
            Vector3 point=Vector3.Lerp(from,to,i/(float)samples);float floor=DroneTerrainFloor(point,radius);
            if(point.y-radius<floor){obstacle=new Bounds(new Vector3(point.x,floor-radius,point.z),Vector3.one*radius*2);return true;}
        }
        return false;
    }
    Vector3 DroneClearDestination(Vector3 desired,float radius)
    {
        desired.y=Mathf.Max(desired.y,DroneTerrainFloor(desired,radius)+radius+.25f);
        foreach(Bounds b in obstacles)if(desired.x>=b.min.x-radius&&desired.x<=b.max.x+radius&&desired.z>=b.min.z-radius&&desired.z<=b.max.z+radius&&desired.y<b.max.y+radius+.25f&&desired.y>b.min.y-radius)
            desired.y=b.max.y+radius+.25f;
        return desired;
    }
    bool MoveDroneSafely(DroneRuntime d,Vector3 destination,float speed,float dt,bool docking=false)
    {
        Vector3 current=d.visual.position;Vector3 goal=docking?destination:DroneClearDestination(destination,d.radius);
        if(d.route.Count>0&&Vector3.Distance(d.route[d.route.Count-1],goal)>8)d.route.Clear();
        while(d.route.Count>0&&Vector3.Distance(current,d.route[0])<.15f)d.route.RemoveAt(0);
        if(d.route.Count==0&&DroneFlightBlocked(current,goal,d.radius,out _))
        {
            float altitude=Mathf.Max(current.y,goal.y);Vector3 horizontal=goal-current;horizontal.y=0;
            foreach(Bounds b in obstacles)
            {
                Bounds footprint=b;footprint.center=new Vector3(b.center.x,0,b.center.z);footprint.size=new Vector3(b.size.x+d.radius*2,2,b.size.z+d.radius*2);
                Vector3 start=new Vector3(current.x,0,current.z);
                if(footprint.Contains(start)||horizontal.sqrMagnitude>.0001f&&footprint.IntersectRay(new Ray(start,horizontal.normalized),out float hit)&&hit<=horizontal.magnitude)altitude=Mathf.Max(altitude,b.max.y+d.radius+.35f);
            }
            int samples=DroneTerrainSamples(current,goal);
            for(int i=0;i<=samples;i++)altitude=Mathf.Max(altitude,DroneTerrainFloor(Vector3.Lerp(current,goal,i/(float)samples),d.radius)+d.radius+.35f);
            Vector3 up=new Vector3(current.x,altitude,current.z),across=new Vector3(goal.x,altitude,goal.z);
            if(!DroneFlightBlocked(current,up,d.radius,out _)&&!DroneFlightBlocked(up,across,d.radius,out _)&&!DroneFlightBlocked(across,goal,d.radius,out _))
            {d.route.Add(up);d.route.Add(across);d.route.Add(goal);}
        }
        Vector3 waypoint=d.route.Count>0?d.route[0]:goal,next=Vector3.MoveTowards(current,waypoint,Mathf.Max(0,speed*dt));
        if(DroneFlightBlocked(current,next,d.radius,out _)){d.blockedTime+=dt;return false;}
        d.visual.position=next;d.blockedTime=0;
        Vector3 facing=next-current;if(facing.sqrMagnitude>.00001f)d.visual.rotation=Quaternion.Slerp(d.visual.rotation,Quaternion.LookRotation(facing,Vector3.up)*d.basisCorrection,Mathf.Clamp01(dt*6));return true;
    }
    void UpdateDrones(float dt)
    {
        foreach(DroneRuntime d in droneRuntime.Values.ToArray())
        {
            try
            {
            Fighter f=d.owner;
            if(f==null||f.root==null||d.visual==null||d.dock==null){RemoveDroneRuntime(f);continue;}
            if(f.deadVisual||f.hp<=0){d.visual.gameObject.SetActive(false);d.phase=DronePhase.Docked;continue;}
            d.visual.gameObject.SetActive(true);
            if(inMenu||f.root.name=="Portrait studio robot"){d.phase=DronePhase.Docked;d.route.Clear();SeatDrone(d);continue;}
            d.timer+=dt;d.fireTimer-=dt;d.orbit+=dt*(d.kind==2?.8f:1.3f);
            if(d.phase==DronePhase.Docked){SeatDrone(d);continue;}
            if(d.phase==DronePhase.Charging){SeatDrone(d);d.recharge=Mathf.Max(0,d.recharge-dt);if(d.recharge<=0){d.phase=DronePhase.Docked;d.timer=0;}continue;}
            if(d.phase==DronePhase.Returning)
            {
                Vector3 seat=d.dock.position-DroneDockRotation(d)*Vector3.Scale(d.visual.lossyScale,d.contact);
                MoveDroneSafely(d,seat,18,dt,true);float distance=Vector3.Distance(d.visual.position,seat);ApplyDroneFold(d,Mathf.MoveTowards(d.fold,distance<2?1:0,dt*2));
                if(distance<.15f&&!DroneFlightBlocked(d.visual.position,seat,d.radius,out _)){d.phase=DronePhase.Charging;d.recharge=d.kind==0?8:6;d.timer=0;SeatDrone(d);}continue;
            }
            Fighter target=Opponent(f);Vector3 center=f.root.position;
            if(d.kind==0&&target!=null&&IsHostile(f,target)&&target.hp>0)center=target.root.position;
            Vector3 desired=center+new Vector3(Mathf.Cos(d.orbit)*(d.kind==2?9:4),d.kind==2?4:3,Mathf.Sin(d.orbit)*(d.kind==2?9:4));
            MoveDroneSafely(d,desired,d.phase==DronePhase.Deploying?18:12,dt);ApplyDroneFold(d,Mathf.MoveTowards(d.fold,0,dt*2));
            if(d.blockedTime>2){d.phase=DronePhase.Returning;d.timer=0;d.route.Clear();continue;}
            if(d.phase==DronePhase.Deploying){if(Vector3.Distance(d.visual.position,desired)<1||d.timer>=2){d.phase=DronePhase.Active;d.timer=0;}continue;}
            if(d.timer>12){d.phase=DronePhase.Returning;d.timer=0;d.route.Clear();continue;}
            if(d.kind==0&&target!=null&&target.hp>0&&IsHostile(f,target)&&d.fireTimer<=0)
            {
                Vector3 aim=(target.root.position-d.visual.position).normalized;d.visual.rotation=Quaternion.LookRotation(aim,Vector3.up)*d.basisCorrection;
                if(!ObstacleOnSegment(d.visual.position,target.root.position,.09f,out _))
                {Transform muzzle=d.muzzles[d.shotSequence++%d.muzzles.Length];Vector3 shotDirection=(target.root.position-muzzle.position).normalized;Fire(f,shotDirection,6,75,.09f,70,loadoutVisual:true,origin:muzzle.position,projectileArtSlot:0);d.fireTimer=.55f;}
            }
            else if(d.kind==1&&d.fireTimer<=0)
            {
                for(int i=shots.Count-1;i>=0;i--)
                {
                    Shot shot=shots[i];if(!shot.isMissile||!IsHostile(f,shot.owner)||Vector3.Distance(shot.visual.position,f.root.position)>18)continue;
                    if(ObstacleOnSegment(d.visual.position,shot.visual.position,.05f,out _))continue;
                    d.visual.rotation=Quaternion.LookRotation((shot.visual.position-d.visual.position).normalized,Vector3.up)*d.basisCorrection;SpawnAuthoredEffect(2,shot.visual.position,shot.velocity.normalized,.65f,.25f,f);Burst(shot.visual.position,Orange,8);RemoveShot(i);d.fireTimer=.35f;d.interceptions++;
                    if(d.interceptions>=6){d.phase=DronePhase.Returning;d.timer=0;d.route.Clear();}break;
                }
            }
            }
            finally{UpdateDroneField(d);}
        }
    }
    float DroneJammingFactor(Fighter victim)
    {
        foreach(DroneRuntime d in droneRuntime.Values)
            if(d.kind==2&&d.phase==DronePhase.Active&&d.owner.hp>0&&IsHostile(d.owner,victim)&&Vector3.Distance(d.visual.position,victim.root.position)<20)return .4f;
        return 1;
    }
    void DrawBackpackMenu()
    {
        FamilyLoadout c=FamilyConfig(player);int drone=DronePackIndex(c.backpack),slots=BackpackAuxSlots(c.backpack);
        GUI.Label(new Rect(710,650,520,28),drone>=0?"3번 키: "+(drone==0?"공격 드론 · 기체 지상 운용":drone==1?"요격 드론 · 복귀 충전":"교란 드론 · 광역 선회"):"보조 무기 슬롯: "+slots,new GUIStyle(small){alignment=TextAnchor.MiddleCenter});
    }
    void DrawDroneHud(int slot,Rect area)
    {
        if(slot!=2||!droneRuntime.TryGetValue(player,out DroneRuntime d))return;
        Panel(area,new Color(.045f,.12f,.16f));string state=d.phase==DronePhase.Docked?"준비":d.phase==DronePhase.Charging?"충전 "+d.recharge.ToString("0.0"):d.phase==DronePhase.Returning?"복귀":"출격 중";
        GUI.Label(area,"3 드론\n"+state,new GUIStyle(small){alignment=TextAnchor.MiddleCenter});
    }
    void CaptureDetachedDrone(DroneRuntime d,string name)
    {
        var layers=new Dictionary<Transform,int>();foreach(Transform t in d.visual.GetComponentsInChildren<Transform>(true)){layers[t]=t.gameObject.layer;t.gameObject.layer=9;}
        Camera camera=new GameObject("Detached drone evidence").AddComponent<Camera>();camera.enabled=false;camera.orthographic=true;camera.cullingMask=1<<9;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.035f,.05f,.07f);
        var rt=new RenderTexture(900,700,24);camera.targetTexture=rt;var image=new Texture2D(900,700,TextureFormat.RGB24,false);RenderTexture previous=RenderTexture.active;
        try
        {
            Bounds bounds=AuxiliaryBounds(d.visual,null);
            camera.orthographicSize=Mathf.Max(.7f,bounds.size.magnitude*.55f);camera.transform.position=bounds.center+Quaternion.LookRotation(d.visual.TransformDirection(d.localForward),d.visual.TransformDirection(d.localUp))*new Vector3(3,2,5)*Mathf.Max(1,bounds.size.magnitude);camera.transform.LookAt(bounds.center);camera.Render();
            RenderTexture.active=rt;image.ReadPixels(new Rect(0,0,900,700),0,0);image.Apply();string folder=System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(),"Docs","Validation","Overnight-20261119","Iteration9");System.IO.Directory.CreateDirectory(folder);System.IO.File.WriteAllBytes(System.IO.Path.Combine(folder,name+".png"),image.EncodeToPNG());
        }
        finally{RenderTexture.active=previous;foreach(var entry in layers)entry.Key.gameObject.layer=entry.Value;camera.targetTexture=null;rt.Release();Destroy(rt);Destroy(image);Destroy(camera.gameObject);}
    }
    static bool DronePlaygroundRequested=>Array.IndexOf(Environment.GetCommandLineArgs(),"--drone-playground")>=0;
    System.Collections.IEnumerator SetupDronePlayground()
    {
        yield return null;
        balanceDirectoryOverride=System.IO.Path.Combine(System.IO.Path.GetTempPath(),"VoxelOn-Drone-Playground-"+DateTime.Now.ToString("yyyyMMdd-HHmmss"));
        balanceDraft=activeBalance=null;balanceDirty=false;familyLoadouts=new FamilyLoadoutFile();InitializeAutomaticBalance();
        selectedCraft=0;TestSetupSelectable(0,1);smoke=false;paused=false;muted=true;inMenu=true;
        FamilyConfig(player).backpack=DronePackIds[0];BuildRobot(player);OpenBalanceEditor();balanceTab=6;familyArtCategory=3;SaveFamilyConfig();
        captureGameplayPreview=false;
        Debug.Log("DRONE_PLAYGROUND_READY isolatedBalance="+balanceDirectoryOverride);
    }
    void DroneTerrainChecks(DroneRuntime drone,Action<bool,string> check)
    {
        ArenaMapKind priorMap=arenaMap;Bounds[] priorObstacles=obstacles.ToArray();Vector3 priorPosition=drone.visual.position;float priorBlocked=drone.blockedTime;
        try
        {
            obstacles.Clear();
            foreach(ArenaMapKind map in new[]{ArenaMapKind.IndoorFactory,ArenaMapKind.OutdoorCanyon,ArenaMapKind.CoastalBay})
            {
                arenaMap=map;
                Vector3 from=map==ArenaMapKind.CoastalBay?new Vector3(15,0,0):map==ArenaMapKind.OutdoorCanyon?new Vector3(0,0,80):new Vector3(-20,0,0);
                Vector3 to=map==ArenaMapKind.CoastalBay?new Vector3(-100,0,0):map==ArenaMapKind.OutdoorCanyon?new Vector3(0,0,10):new Vector3(20,0,0);
                from.y=DroneTerrainFloor(from,drone.radius)+drone.radius+.4f;to.y=DroneTerrainFloor(to,drone.radius)+drone.radius+.4f;
                Vector3 buried=to;buried.y=SurfaceHeight(to)-.2f;
                check(DroneFlightBlocked(buried,buried,drone.radius,out _),"drone rejects below-ground position on "+map);
                check(DroneClearDestination(buried,drone.radius).y>SurfaceHeight(to)+drone.radius,"drone orbit destination rises above terrain on "+map);
                if(map!=ArenaMapKind.IndoorFactory)check(DroneFlightBlocked(from,to,drone.radius,out _),"direct flight through rising bank is blocked on "+map);
                drone.visual.position=from;drone.route.Clear();bool clear=true,continuous=true;
                for(int n=0;n<700&&Vector3.Distance(drone.visual.position,to)>.15f;n++)
                {
                    Vector3 previous=drone.visual.position;MoveDroneSafely(drone,to,12,.05f);
                    clear&=!DroneFlightBlocked(previous,drone.visual.position,drone.radius,out _);
                    continuous&=Vector3.Distance(previous,drone.visual.position)<=.601f;
                }
                check(clear&&continuous&&Vector3.Distance(drone.visual.position,to)<.2f,"drone reaches land across terrain without penetration or teleport on "+map);
                drone.route.Clear();for(int n=0;n<700&&Vector3.Distance(drone.visual.position,from)>.15f;n++)MoveDroneSafely(drone,from,12,.05f,true);
                check(Vector3.Distance(drone.visual.position,from)<.2f,"drone return navigation follows natural terrain on "+map);
                Vector3 probeStart=from+Vector3.up*30,probeEnd=to+Vector3.up*30;
                DroneFlightBlocked(probeStart,probeEnd,drone.radius,out _);
                long allocated=GC.GetAllocatedBytesForCurrentThread(),ticks=System.Diagnostics.Stopwatch.GetTimestamp();
                for(int probe=0;probe<1000;probe++)DroneFlightBlocked(probeStart,probeEnd,drone.radius,out _);
                double elapsedMs=(System.Diagnostics.Stopwatch.GetTimestamp()-ticks)*1000.0/System.Diagnostics.Stopwatch.Frequency;
                long allocatedBytes=GC.GetAllocatedBytesForCurrentThread()-allocated;
                Debug.Log("DRONE_TERRAIN_PROFILE map="+map+" probes=1000 ms="+elapsedMs.ToString("F3")+" managedBytes="+allocatedBytes);

            }
        }
        finally{arenaMap=priorMap;obstacles.Clear();obstacles.AddRange(priorObstacles);drone.visual.position=priorPosition;drone.route.Clear();drone.blockedTime=priorBlocked;}
    }
    void DroneChecks(Action<bool,string> check)
    {
        BalanceDocument saved=activeBalance;FamilyLoadoutFile oldConfig=familyLoadouts;bool oldSmoke=smoke,oldMenu=inMenu;
        try
        {
            activeBalance=BalanceData.Defaults();familyLoadouts=new FamilyLoadoutFile();
            for(int kind=0;kind<3;kind++)
            {
                TestSetupSelectable(0,1);FamilyLoadout config=FamilyConfig(player);config.backpack=DronePackIds[kind];BuildRobot(player);smoke=false;inMenu=false;
                check(droneDockedPreviewSources!=null&&droneDockedPreviewSources[kind]!=null,"drone selector has authored docked assembly rather than empty cradle "+kind);
                RenderTexture actualCard=FamilyPartPreview(config.backpack);check(actualCard!=null&&actualCard.IsCreated(),"actual docked drone backpack selector renders "+kind);
                SavePortraitEvidence(actualCard,"Drone-Backpack-Card-"+kind);
                DroneRuntime d=droneRuntime[player];check(d.phase==DronePhase.Docked&&d.visual.parent==d.frame,"drone starts docked in stable game-unit frame "+kind);
                check(d.fins.Length==4&&d.fold>.99f,"all four drone fins are physically folded while docked "+kind);
                float dockedWidth=AuxiliaryBounds(d.visual,d.visual).size.x;
                check(AuxiliaryBounds(d.visual,d.frame).size.magnitude<1&&AuxiliaryBounds(d.visual,d.frame).size.magnitude>.05f,"drone uses canonical meter dimensions independent of FBX import scale "+kind);
                check(AuxiliaryBounds(d.frame,d.frame).size.magnitude<1.2f,"drone backpack uses canonical meter dimensions "+kind);check(Vector3.Dot(d.visual.TransformDirection(d.localForward),-d.frame.forward)>.99f,"docked drone nose faces out from robot backpack "+kind);
                CaptureAuxiliaryAssembly(player,"Drone-"+kind+"-Docked-Assembly",iteration:"Iteration9");CaptureDetachedDrone(d,"Drone-"+kind+"-Folded");
                check(BackpackAuxSlots(config.backpack)==0,"drone backpack has no auxiliary weapon mounts "+kind);
                check(IsGroundDronePack(player)==(kind==0),"only attack pack imposes ground operation "+kind);
                check(DeployDrone(player)&&!DeployDrone(player),"slot3 deploy rejects duplicate launch "+kind);
                for(int n=0;n<30;n++)UpdateDrones(.1f);
                check(d.phase==DronePhase.Active&&d.visual.parent==null,"drone detaches and becomes active "+kind);
                check(d.fold<.01f,"drone fins unfold after deployment "+kind);
                check(AuxiliaryBounds(d.visual,d.visual).size.x>dockedWidth*1.02f,"actual unfolded wing geometry is wider than docked geometry "+kind);CaptureDetachedDrone(d,"Drone-"+kind+"-Deployed");
                if(kind==0)
                {
                    int before=shots.Count;d.fireTimer=0;UpdateDrones(.1f);
                    check(shots.Count>before&&shots.Last().owner==player,"attack drone fires real friendly-owned projectiles");
                    check(Vector3.Dot(d.visual.TransformDirection(d.localForward),shots.Last().velocity.normalized)>.99f,"authored drone nose aligns with its actual firing direction");
                    check(Vector3.Distance(shots.Last().visual.position,d.visual.position)<1,"drone rounds originate at detached drone rather than player");
                    check(d.muzzles.Any(t=>Vector3.Distance(t.position,shots.Last().visual.position)<.001f),"drone round starts at an actual authored cannon muzzle");
                }
                if(kind==1)
                {
                    ClearCombat();DeployDrone(player);for(int n=0;n<30;n++)UpdateDrones(.1f);d.fireTimer=0;
                    Shot hostile=Fire(enemy,Vector3.back,10,25,.1f,80,.8f,origin:player.root.position+Vector3.forward*6,missile:true);
                    Shot friendly=Fire(player,Vector3.forward,10,25,.1f,80,.8f,origin:player.root.position+Vector3.forward*7,missile:true);
                    Shot bullet=Fire(enemy,Vector3.back,10,25,.1f,80,origin:player.root.position+Vector3.forward*5);
                    Shot guidedRound=Fire(enemy,Vector3.back,10,25,.1f,80,.8f,origin:player.root.position+Vector3.forward*8);
                    Shot homingMine=Fire(enemy,Vector3.back,10,25,.1f,80,.8f,stop:true,origin:player.root.position+Vector3.forward*9);
                    check(!guidedRound.isMissile&&!homingMine.isMissile,"guidance alone does not classify a bullet or homing mine as a missile");
                    UpdateDrones(.1f);check(!shots.Contains(hostile)&&shots.Contains(friendly)&&shots.Contains(bullet),"interceptor removes hostile missile only, preserves friendly missile and bullet");
                    for(int n=0;n<5;n++)
                    {
                        Shot incoming=Fire(enemy,Vector3.back,10,25,.1f,80,origin:player.root.position+Vector3.forward*6,missile:true);
                        UpdateDrones(.4f);check(!shots.Contains(incoming),"interceptor removes explicitly classified unguided missile "+n);
                    }
                    check(shots.Contains(guidedRound)&&shots.Contains(homingMine),"interceptor preserves hostile guided non-missile rounds through a full charge");
                    check(d.interceptions==6&&d.phase==DronePhase.Returning,"six actual interceptions automatically exhaust charge and initiate return");
                    for(int n=0;n<100&&d.phase==DronePhase.Returning;n++)UpdateDrones(.1f);
                    check(d.phase==DronePhase.Charging&&d.recharge>0,"exhausted interceptor physically returns to dock for charging");
                    check(!DeployDrone(player),"exhausted interceptor cannot bypass charging with slot3");
                    UpdateDrones(6.1f);check(d.phase==DronePhase.Docked&&DeployDrone(player),"interceptor can deploy again after actual recharge");
                    for(int n=0;n<30;n++)UpdateDrones(.1f);
                }
                if(kind==2)
                {
                    enemy.root.position=player.root.position+Vector3.forward*80;
                    check(DroneJammingFactor(enemy)==1,"jammer has no effect outside its range");
                    enemy.root.position=player.root.position+Vector3.forward*5;
                    check(DroneJammingFactor(enemy)<1&&DroneJammingFactor(player)==1,"jammer degrades hostile targeting without friendly penalty");
                    d.phase=DronePhase.Returning;check(DroneJammingFactor(enemy)==1,"returning jammer immediately stops interference");d.phase=DronePhase.Active;
                }
                d.phase=DronePhase.Returning;d.timer=0;
                for(int n=0;n<100&&d.phase==DronePhase.Returning;n++)UpdateDrones(.1f);
                check(d.phase==DronePhase.Charging&&d.recharge>0&&d.visual.parent!=null,"drone returns to actual dock and recharges "+kind);
                check(!DeployDrone(player),"charging drone cannot redeploy "+kind);
                UpdateDrones(9);check(d.phase==DronePhase.Docked&&DeployDrone(player),"charged drone becomes deployable "+kind);
                ClearCombat();check(d.phase==DronePhase.Docked,"round combat reset recalls drone "+kind);
                DeployDrone(player);player.hp=0;UpdateDrones(.1f);check(!d.visual.gameObject.activeSelf,"owner death hides detached drone "+kind);player.hp=player.spec.health;
                config.backpack=FamilyBackpacks[0];BuildRobot(player);check(!droneRuntime.ContainsKey(player),"backpack replacement removes stale drone controller "+kind);
            }
            TestSetupSelectable(0,1);FamilyConfig(player).backpack=DronePackIds[0];BuildRobot(player);smoke=false;inMenu=false;DroneRuntime navigation=droneRuntime[player];
            Bounds launchBlock=new Bounds(navigation.visual.position,Vector3.one*2);obstacles.Add(launchBlock);
            check(!DeployDrone(player)&&navigation.phase==DronePhase.Docked,"obstructed drone dock rejects launch rather than teleporting through wall");obstacles.RemoveAt(obstacles.Count-1);
            check(DeployDrone(player),"cleared dock permits launch");Vector3 start=player.root.position+Vector3.up*3,goal=start+Vector3.forward*12;navigation.visual.position=start;navigation.route.Clear();
            Bounds wall=new Bounds(start+Vector3.forward*6,new Vector3(6,6,.6f));obstacles.Add(wall);bool collisionFree=true,noTeleport=true;float peak=start.y;
            try
            {
                for(int n=0;n<240&&Vector3.Distance(navigation.visual.position,goal)>.15f;n++)
                {Vector3 previous=navigation.visual.position;MoveDroneSafely(navigation,goal,12,.05f);collisionFree&=!DroneFlightBlocked(navigation.visual.position,navigation.visual.position,navigation.radius,out _);noTeleport&=Vector3.Distance(previous,navigation.visual.position)<=.601f;peak=Mathf.Max(peak,navigation.visual.position.y);}
                check(collisionFree&&noTeleport&&peak>wall.max.y,"drone avoids wall with continuous collision-free flight above roof");
                check(Vector3.Distance(navigation.visual.position,goal)<.2f,"drone reaches destination beyond intervening wall");
                navigation.route.Clear();for(int n=0;n<240&&Vector3.Distance(navigation.visual.position,start)>.15f;n++)MoveDroneSafely(navigation,start,12,.05f,true);
                check(Vector3.Distance(navigation.visual.position,start)<.2f,"return route also goes around blocking geometry");
            }
            finally{obstacles.RemoveAt(obstacles.Count-1);}
            DroneTerrainChecks(navigation,check);
            FamilyConfig(enemy).backpack=DronePackIds[0];BuildRobot(enemy);enemy.jumpHeight=enemy.jumpVelocity=0;enemy.jumpHoldElapsed=10;enemy.spec.chargedReleaseJump=true;EnemyAI(enemy,player,.1f);
            check(enemy.jumpVelocity<=0&&!enemy.jumpHeld,"attack drone backpack also prevents charged AI jumps");
        }
        finally{activeBalance=saved;familyLoadouts=oldConfig;smoke=oldSmoke;inMenu=oldMenu;ClearCombat();TestSetupSelectable(0,1);}
    }

}
