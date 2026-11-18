using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public sealed partial class VoxArena
{
    public GameObject[] combatEffectSources;
    sealed class AuthoredCombatEffect {public Fighter owner;public Transform root;public float age,life;public int kind;}
    readonly List<AuthoredCombatEffect> authoredCombatEffects=new List<AuthoredCombatEffect>();
    Transform CreateAuthoredEffect(int kind,Vector3 position,Quaternion rotation,float size)
    {
        if(combatEffectSources==null||kind<0||kind>=combatEffectSources.Length||combatEffectSources[kind]==null)return null;
        Transform root=Module("Blender combat FX / "+kind,null,position);Transform model=Instantiate(combatEffectSources[kind],root,false).transform;PrepareWeaponMaterials(model);
        Bounds bounds=AuxiliaryBounds(root,root);model.localScale*=size/Mathf.Max(.001f,Mathf.Max(bounds.size.x,bounds.size.y,bounds.size.z));root.rotation=rotation;return root;
    }
    void SpawnAuthoredEffect(int kind,Vector3 position,Vector3 direction,float size,float life,Fighter owner)
    {
        if(authoredCombatEffects.Count>=64)return;
        Quaternion rotation=direction.sqrMagnitude>.001f?Quaternion.LookRotation(direction,Mathf.Abs(Vector3.Dot(direction.normalized,Vector3.up))>.95f?Vector3.forward:Vector3.up):Quaternion.identity;
        Transform root=CreateAuthoredEffect(kind,position,rotation,size);if(root==null)return;
        authoredCombatEffects.Add(new AuthoredCombatEffect{root=root,life=life,kind=kind,owner=owner});
    }
    void TickAuthoredEffects(float dt)
    {
        for(int i=authoredCombatEffects.Count-1;i>=0;i--)
        {
            AuthoredCombatEffect e=authoredCombatEffects[i];e.age+=dt;
            if(e.root==null||e.age>=e.life){if(e.root!=null){e.root.gameObject.SetActive(false);Destroy(e.root.gameObject);}authoredCombatEffects.RemoveAt(i);continue;}
            float t=e.age/e.life;e.root.localScale=Vector3.one*(e.kind==0?Mathf.Lerp(1,.15f,t):Mathf.Sin(Mathf.PI*t)*.9f+.1f);
        }
    }
    void RemoveAuthoredEffects(Fighter owner=null)
    {
        for(int i=authoredCombatEffects.Count-1;i>=0;i--)if(owner==null||authoredCombatEffects[i].owner==owner)
        {Transform root=authoredCombatEffects[i].root;if(root!=null){root.gameObject.SetActive(false);Destroy(root.gameObject);}authoredCombatEffects.RemoveAt(i);}
    }
    void AttachMissileExhaust(Shot shot)
    {
        if(shot==null||!shot.isMissile||shot.exhaust!=null)return;Transform socket=DroneSocket(shot.visual,"EXHAUST");if(socket==null)return;
        Transform flame=CreateAuthoredEffect(4,socket.position,shot.visual.rotation,.28f);if(flame==null)return;
        flame.SetParent(shot.visual,true);shot.exhaust=flame;shot.exhaustScale=flame.localScale;
    }
    void UpdateDroneField(DroneRuntime d)
    {
        bool show=d.kind==2&&d.phase==DronePhase.Active&&!inMenu&&!Finished&&d.owner!=null&&d.owner.hp>0&&d.owner.root!=null&&d.visual!=null&&d.visual.gameObject.activeInHierarchy;
        if(show&&d.fieldVisual==null)d.fieldVisual=CreateAuthoredEffect(3,d.owner.root.position,Quaternion.identity,18);
        if(d.fieldVisual==null)return;d.fieldVisual.gameObject.SetActive(show);if(!show)return;
        d.fieldVisual.position=d.owner.root.position-Vector3.up*FighterSupportOffset+Vector3.up*.06f;
        d.fieldVisual.rotation=Quaternion.Euler(0,d.timer*20,0);d.fieldVisual.localScale=Vector3.one*(1+.025f*Mathf.Sin(d.timer*5));
    }
    void AuthoredFxChecks(Action<bool,string> check)
    {
        TestSetupSelectable(0,1);bool oldSmoke=smoke,oldMenu=inMenu;smoke=false;inMenu=false;
        try
        {
            for(int kind=0;kind<3;kind++)
            {
                SpawnAuthoredEffect(kind,player.root.position,Vector3.forward,.6f,.3f,player);
                AuthoredCombatEffect effect=authoredCombatEffects.Last();check(effect.root.GetComponentsInChildren<MeshFilter>().Length>0,"combat effect uses actual Blender meshes "+kind);
                CaptureCombatObject(effect.root,"Effect-"+kind);
            }
            TickAuthoredEffects(.4f);check(authoredCombatEffects.Count==0,"expired combat mesh effects release their scene objects");
            SpawnAuthoredEffect(2,player.root.position,Vector3.forward,.6f,.3f,player);ClearCombat();check(authoredCombatEffects.Count==0,"round reset clears authored effects");
        }
        finally{RemoveAuthoredEffects();smoke=oldSmoke;inMenu=oldMenu;}
    }
    void DroneCombatIntegrationChecks(Action<bool,string> check)
    {
        BalanceDocument oldBalance=activeBalance;FamilyLoadoutFile oldFamily=familyLoadouts;bool oldSmoke=smoke,oldMenu=inMenu,oldSpectator=spectatorMode;Fighter extra=null;
        try
        {
            activeBalance=BalanceData.Defaults();familyLoadouts=new FamilyLoadoutFile();TestSetupSelectable(0,1);smoke=false;inMenu=false;spectatorMode=true;
            FamilyLoadout config=FamilyConfig(player);config.backpack="B-02_CROSSLANCE";config.auxiliary=new[]{"AUX_MISSILE_POD","AUX_PULSE_GUN"};BuildRobot(player);
            FamilyConfig(enemy).backpack=DronePackIds[2];BuildRobot(enemy);DroneRuntime jammer=droneRuntime[enemy];jammer.visual.position=player.root.position+Vector3.forward*8;
            enemy.root.position=player.root.position+new Vector3(6,0,25);SelectAuxiliary(player,0);ClearCombat();SelectAuxiliary(player,0);
            FireAuxiliary(player,enemy,Vector3.forward);Shot clear=shots.Last();Vector3 heading=clear.velocity;UpdateShots(.05f);float clearTurn=Vector3.Angle(heading,clear.velocity);RemoveShot(shots.IndexOf(clear));auxiliaryRuntime[player].cooldown[0]=0;
            jammer.phase=DronePhase.Active;jammer.visual.position=player.root.position+Vector3.forward*8;UpdateDroneField(jammer);
            check(jammer.fieldVisual!=null&&jammer.fieldVisual.gameObject.activeSelf,"active jammer displays Blender-authored interference field");CaptureCombatObject(jammer.fieldVisual,"Jammer-Field");
            FireAuxiliary(player,enemy,Vector3.forward);Shot jammed=shots.Last();heading=jammed.velocity;UpdateShots(.05f);float jammedTurn=Vector3.Angle(heading,jammed.velocity);
            check(clearTurn>0&&jammedTurn>0&&jammedTurn<clearTurn*.6f,"hostile jamming reduces actual auxiliary missile steering over equal elapsed time");
            check(jammed.exhaust!=null,"authored missile has attached Blender exhaust");Bounds missileBounds=AuxiliaryBounds(jammed.visual,jammed.visual);check(Mathf.Max(missileBounds.size.x,missileBounds.size.y)<=jammed.radius*2.2f,"missile fins and exhaust stay inside transverse hit-radius envelope");CaptureCombatObject(jammed.visual,"Homing-Missile-In-Flight");
            Vector3 requested=Quaternion.AngleAxis(1.2f,Vector3.up)*(enemy.root.position-WeaponMuzzle(player)).normalized;
            jammer.phase=DronePhase.Docked;player.assistTrackedTarget=null;bool clearAssist=CanWeakAssist(player,enemy,requested);jammer.phase=DronePhase.Active;player.assistTrackedTarget=null;bool jamAssist=CanWeakAssist(player,enemy,requested);
            check(clearAssist&&!jamAssist,"jammer reduces actual weak-assist acquisition cone");
            float yaw=cameraYaw,pitch=cameraPitch;UpdateDroneField(jammer);check(cameraYaw==yaw&&cameraPitch==pitch,"interference visual never pulls the camera");
            jammer.phase=DronePhase.Returning;UpdateDroneField(jammer);check(!jammer.fieldVisual.gameObject.activeSelf,"returning jammer hides interference visual immediately");
            FamilyConfig(enemy).backpack=DronePackIds[1];BuildRobot(enemy);DroneRuntime interceptor=droneRuntime[enemy];interceptor.phase=DronePhase.Active;interceptor.visual.SetParent(null,true);interceptor.visual.position=enemy.root.position+Vector3.up*3;jammed.visual.position=enemy.root.position-Vector3.forward*8;
            UpdateDrones(.01f);check(!shots.Contains(jammed)&&authoredCombatEffects.Any(e=>e.kind==2&&e.owner==enemy),"interceptor destroys an actual auxiliary homing missile and creates authored impact");
            SelectAuxiliary(player,1);FireAuxiliary(player,enemy,Vector3.forward);Shot pulse=shots.Last();CaptureCombatObject(pulse.visual,"Traveling-Pulse-In-Flight");check(!pulse.isMissile,"traveling laser remains outside missile interception class");Bounds pulseBounds=AuxiliaryBounds(pulse.visual,pulse.visual);check(Mathf.Max(pulseBounds.size.x,pulseBounds.size.y)<=pulse.radius*2.2f,"visible pulse thickness stays within its physical hit-radius envelope");
            extra=AddMatchParticipant("Drone lifecycle review",2,1);extra.root.position=player.root.position+Vector3.right*6;FamilyConfig(extra).backpack=DronePackIds[0];BuildRobot(extra);DeployDrone(extra);Transform detached=droneRuntime[extra].visual;SpawnAuthoredEffect(2,extra.root.position,Vector3.forward,.4f,.3f,extra);
            RemoveMatchParticipant(extra);check(!droneRuntime.ContainsKey(extra)&&!familyRigs.ContainsKey(extra)&&!authoredCombatEffects.Any(e=>e.owner==extra)&&(detached==null||!detached.gameObject.activeInHierarchy),"participant removal releases detached drone, rig cache and owned effects");extra=null;
            extra=AddMatchParticipant("Auxiliary lifecycle review",2,1);extra.root.position=player.root.position+Vector3.right*7;FamilyConfig(extra).backpack="B-02_CROSSLANCE";FamilyConfig(extra).auxiliary=new[]{"AUX_MISSILE_POD","NONE"};BuildRobot(extra);
            Shot seeking=Fire(player,Vector3.forward,1,30,.1f,70,1,origin:player.root.position+Vector3.up*2,projectileSource:auxiliaryProjectileSources[0]);seeking.seekTarget=extra;
            RemoveMatchParticipant(extra);check(!auxiliaryRuntime.ContainsKey(extra)&&seeking.seekTarget==null&&seeking.tracking==0,"removed auxiliary participant cannot leave stale controller or missile target reference");extra=null;
        }
        finally{if(extra!=null&&fighterRoster.Contains(extra))RemoveMatchParticipant(extra);activeBalance=oldBalance;familyLoadouts=oldFamily;smoke=oldSmoke;inMenu=oldMenu;spectatorMode=oldSpectator;ClearCombat();TestSetupSelectable(0,1);}
    }
    void CaptureCombatObject(Transform root,string name)
    {
        var layers=new Dictionary<Transform,int>();foreach(Transform t in root.GetComponentsInChildren<Transform>(true)){layers[t]=t.gameObject.layer;t.gameObject.layer=9;}
        Camera camera=new GameObject("Combat mesh evidence").AddComponent<Camera>();camera.enabled=false;camera.orthographic=true;camera.cullingMask=1<<9;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.035f,.05f,.07f);
        var rt=new RenderTexture(900,700,24);camera.targetTexture=rt;var image=new Texture2D(900,700,TextureFormat.RGB24,false);RenderTexture previous=RenderTexture.active;
        try
        {
            Bounds bounds=AuxiliaryBounds(root,null);camera.orthographicSize=Mathf.Max(.18f,bounds.size.magnitude*.5f);camera.transform.position=bounds.center+root.rotation*new Vector3(3,2,5)*Mathf.Max(1,bounds.size.magnitude);camera.transform.LookAt(bounds.center);camera.Render();
            RenderTexture.active=rt;image.ReadPixels(new Rect(0,0,900,700),0,0);image.Apply();string folder=System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(),"Docs","Validation","Overnight-20261119","Iteration12");System.IO.Directory.CreateDirectory(folder);System.IO.File.WriteAllBytes(System.IO.Path.Combine(folder,name+".png"),image.EncodeToPNG());
        }
        finally{RenderTexture.active=previous;foreach(var entry in layers)entry.Key.gameObject.layer=entry.Value;camera.targetTexture=null;rt.Release();Destroy(rt);Destroy(image);Destroy(camera.gameObject);}
    }
}
