using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed partial class VoxArena : MonoBehaviour
{
    public Shader hullShader;
    public float boostRecovery = 40, recoveryDelay = .6f;
    [Header("Jump and vertical boost")]
    public float jumpSpeed = 10.6f, jumpGravity = 24f, jumpThrust = 34f, jumpBoostDrain = 24f;
    [Tooltip("Maximum vertical rise from takeoff, including held Space thrust.")]
    public float jumpMaxHeight = 6.4f;
    [Tooltip("Extra gravity while descending so landings feel quicker.")]
    public float fallGravityMultiplier = 1.8f;
    public float gunHalfAngle = 65, shieldRecoveryDelay = 3, shieldFullRecoveryTime = 6;
    [Header("Weapon tuning (all frames)")]
    [Range(.5f,2)] public float bulletSpeedScale=1;
    [Range(.5f,2)] public float bulletSizeScale=1;
    [Range(.5f,2)] public float weaponCooldownScale=1;
    [Header("Double-tap dash (future part tuning)")]
    public float dashSpeed=34f, dashDistance=9f, dashFuelCost=30f, dashCooldown=.7f;
    [Range(.12f,.45f)] public float doubleTapWindow=.28f;
    readonly float[] lastDirectionTap={-10f,-10f,-10f,-10f};
    const float CraftRadius=1.15f;

    sealed class Fighter
    {
        public Transform root,body,gun,fist,shield,leftArm,rightArm,leftLeg,rightLeg,thruster,jumpFlame,saber;
        public Transform headPart,corePart,armsPart,legsPart,weaponPart,backPart,shadow;
        // Independent arm sockets; pelvis belongs to the leg module.
        public Transform pelvisPart;
        // Stable team identity; multiple fighters can share a team in future modes.
        public int teamId;
        public int frameIndex; public float hitFlash;
        public int weaponSlot=1,magazine=30; public float reloadTime;
        public CraftSpec spec=CraftCatalog.All[0];
        public Vector3 facing=Vector3.forward,velocity,pulseDirection,rushDirection;
        public float hp,boostEnergy,shieldEnergy,weaponTimer,specialTimer,recoveryTime,shieldRecoveryTime,shieldFlash;
        public float pulseTime,lockTime,rushTime,aiClock,aiReaction,stuckTime,orbitTime;
        public Vector3 aiMove,lastPosition,smoothedTargetVelocity;
        public int orbitSign=1;
        public float jumpHeight,jumpVelocity,jumpOriginY;
        public bool crouching;
        public float failedFireFlash;
        public float knockdownTime,invulnerableTime;
        public Vector3 dashDirection;
        public float dashRemaining,dashCooldownLeft;
        // Part modifiers: replace with equipped leg/booster stats when assembly is implemented.
        public float dashSpeedFactor=1f,dashDistanceFactor=1f,dashCostFactor=1f;
        public bool boostHeld,wasBoostHeld,boostLocked,boosting,rushHit,jumpHeld;
        public string aiState="APPROACH";
        public float Radius=>CraftRadius*spec.size;
    }
    sealed class Spark { public Transform visual; public Vector3 velocity; public float life; }
    Fighter player,enemy;
    // Active match participants; portrait-only preview robots are not registered.
    readonly List<Fighter> fighterRoster=new List<Fighter>();
    Camera view;
    Transform aimMarker;
    readonly List<Spark> sparks=new List<Spark>();
    readonly Dictionary<Color,Material> materials=new Dictionary<Color,Material>();
    Vector3 aimDirection;
    bool canShoot,paused,smoke,inMenu=true;
    bool spectatorMode;
    float cameraShakeTime,cameraShakeStrength;
    bool targetLock;
    float occludedLockTimer;
    float cameraYaw=0,cameraPitch=12,mouseSensitivity=2.4f;
    int selectedCraft,enemyCraft,wins,losses;
    float roundTime;
    GUIStyle label,small,title;
    static readonly Color Cyan=new Color(.18f,.88f,.96f), Orange=new Color(1,.38f,.19f);
    bool Finished=>player.hp<=0 || enemy.hp<=0;
    // Shared roster/hostility queries are the first step toward multi-fighter matches.
    // Existing 1v1 modes still register exactly the pilot and rival.
    bool IsHostile(Fighter a,Fighter b)=>a!=null && b!=null && a!=b && a.teamId!=b.teamId;
    void RegisterFighter(Fighter f)
    {
        if(f!=null && !fighterRoster.Contains(f)) fighterRoster.Add(f);
    }
    void UnregisterFighter(Fighter f) { fighterRoster.Remove(f); }
    Fighter Opponent(Fighter f)
    {
        if(f==null) return null;
        Fighter nearest=null;
        float bestDistance=float.PositiveInfinity;
        foreach(Fighter candidate in fighterRoster)
        {
            if(!IsHostile(f,candidate)) continue;
            float distance=(candidate.root.position-f.root.position).sqrMagnitude;
            if(distance<bestDistance) { nearest=candidate; bestDistance=distance; }
        }
        return nearest;
    }
    Color TeamColor(Fighter f)=>f.teamId==0?Cyan:Orange;

    void Start()
    {
        Application.targetFrameRate=120;
        bool performance=Array.IndexOf(Environment.GetCommandLineArgs(),"--performance-test")>=0;
        bool captureGallery=Array.IndexOf(Environment.GetCommandLineArgs(),"--capture-docs-gallery")>=0;
        smoke=performance||captureGallery||Array.IndexOf(Environment.GetCommandLineArgs(),"--smoke-test")>=0;
        if(RunPreferenceProbe()) return;
        BuildWorld(); BuildAudio(); LoadPreferences();
        ApplyCharacter(player,selectedCraft); ApplyCharacter(enemy,rivalSelection<0?1:rivalSelection);
        ResetRound(); BuildPortraits(); inMenu=true;
        if(performance) StartCoroutine(PerformanceTest());
        else if(captureGallery) StartCoroutine(CaptureDocumentationGallery());
        else if(smoke) StartCoroutine(SmokeTest());
    }
    Material Mat(Color color)
    {
        if(!materials.TryGetValue(color,out Material m))
        {
            m=new Material(hullShader!=null?hullShader:Shader.Find("Standard"));
            m.color=color; m.SetFloat("_Glossiness",.2f);
            materials.Add(color,m);
        }
        return m;
    }
    Material Glow(Color color)
    {
        Color key=new Color(color.r,color.g,color.b,.99f);
        if(!materials.TryGetValue(key,out Material m))
        {
            m=new Material(hullShader);
            m.color=color; m.EnableKeyword("_EMISSION"); m.SetColor("_EmissionColor",color*.75f);
            materials.Add(key,m);
        }
        return m;
    }
    Transform Shape(PrimitiveType type,string name,Transform parent,Vector3 pos,Vector3 size,Color color,bool glow=false)
    {
        var g=GameObject.CreatePrimitive(type); g.name=name; g.transform.SetParent(parent,false);
        g.transform.localPosition=pos; g.transform.localScale=size;
        g.GetComponent<Renderer>().sharedMaterial=glow?Glow(color):Mat(color);
        Destroy(g.GetComponent<Collider>()); return g.transform;
    }
    Transform Box(string name,Transform parent,Vector3 pos,Vector3 size,Color color)=>
        Shape(PrimitiveType.Cube,name,parent,pos,size,color);
    LineRenderer Line(string name,Color color,float width)
    {
        var l=new GameObject(name).AddComponent<LineRenderer>();
        l.sharedMaterial=Glow(color); l.startWidth=l.endWidth=width; l.positionCount=2;
        l.numCapVertices=3; return l;
    }
    void ApplyCharacter(Fighter f,int index)
    {
        f.spec=CraftCatalog.All[index].RuntimeCopy(); f.frameIndex=index; ResetAllocations(f); BuildRobot(f); f.body.localScale=Vector3.one*f.spec.size;
        foreach(Renderer r in f.body.GetComponentsInChildren<Renderer>())
            if(r.name.StartsWith("Armor") || r.name=="Visor") r.sharedMaterial=r.name=="Visor"?Glow(f.spec.color):Mat(f.spec.color);
        // Weapon silhouettes are built separately from arm modules.
        f.saber.gameObject.SetActive(false);
    }
    void BeginMatch()
    {
        enemyCraft=ResolveRival(); SavePreferences();
        ApplyCharacter(player,selectedCraft); ApplyCharacter(enemy,enemyCraft);
        ResetRound(); inMenu=false; view.orthographic=smoke||spectatorMode; view.fieldOfView=60;
        cameraPitch=spectatorMode?42:12;
        Cursor.lockState=spectatorMode?CursorLockMode.None:CursorLockMode.Locked; Cursor.visible=spectatorMode;
    }
    void ResetRound()
    {
        finishCleaned=false; targetLock=false; occludedLockTimer=0; StopSounds(); aimMarker.gameObject.SetActive(true);
        ResetFeedback();
        ResetSalvage();
        ClearCombat();
        foreach(Spark s in sparks) Destroy(s.visual.gameObject);
        sparks.Clear();
        player.teamId=0;
        enemy.teamId=1;
        fighterRoster.Clear();
        RegisterFighter(player);
        RegisterFighter(enemy);
        foreach(Fighter f in fighterRoster)
        {
            f.hp=f.spec.health; f.boostEnergy=f.spec.boostCapacity; f.shieldEnergy=f.spec.ShieldMax;
            f.weaponTimer=f.specialTimer=f.recoveryTime=f.shieldRecoveryTime=f.shieldFlash=f.hitFlash=0;
            f.weaponSlot=1; f.magazine=30; f.reloadTime=0;
            f.pulseTime=f.lockTime=f.rushTime=f.aiClock=f.aiReaction=f.stuckTime=f.orbitTime=0;
            f.velocity=f.aiMove=f.smoothedTargetVelocity=Vector3.zero;
            f.boostHeld=f.wasBoostHeld=f.boostLocked=f.boosting=f.rushHit=f.jumpHeld=false;
            f.jumpHeight=f.jumpVelocity=f.jumpOriginY=0; f.crouching=false; f.failedFireFlash=0;
            f.knockdownTime=f.invulnerableTime=0;
            f.dashRemaining=f.dashCooldownLeft=0;
            f.dashDirection=Vector3.zero;
            f.dashSpeedFactor=Mathf.Clamp(f.spec.speed/13f,.7f,1.25f);
            f.dashDistanceFactor=Mathf.Clamp(f.spec.speed/13f,.75f,1.2f);
            f.dashCostFactor=1f; // No mass-derived mobility modifiers in test baseline.
            f.orbitSign=1;
        }
        player.root.position=new Vector3(-15,1.1f,-15); enemy.root.position=new Vector3(15,1.1f,15);
        FaceOpponent(player,enemy); FaceOpponent(enemy,player);
        cameraYaw=Mathf.Atan2(player.facing.x,player.facing.z)*Mathf.Rad2Deg; targetLock=false;
        player.lastPosition=player.root.position; enemy.lastPosition=enemy.root.position;
        roundTime=0; paused=false; spaceClock=0; cameraShakeTime=cameraShakeStrength=0; UpdateObstacles(0);
        Animate(player); Animate(enemy); UpdateCamera(10);
    }
    void Update()
    {
        if(player==null || enemy==null) return;
        if(Input.GetKeyDown(KeyCode.F1)) { ToggleGuide(); return; }
        if(Input.GetKeyDown(KeyCode.F2) && !inMenu && !smoke) { testLabOpen=!testLabOpen; SetTestCursor(); }
        if(testLabOpen) { UpdateTestLabFrame(); return; }
        if(showGuide) { if(Input.GetKeyDown(KeyCode.Escape)) CloseGuide(); return; }
        if(Input.GetKeyDown(KeyCode.V)) ToggleMute();
        if(inMenu) { if(Input.GetKeyDown(KeyCode.Escape)) showTuning=false; if(Input.GetKeyDown(KeyCode.Return)&&!showTuning) BeginMatch(); return; }
        if(Input.GetKeyDown(KeyCode.M)) { ReturnToHangar(); return; }
        if(Input.GetKeyDown(KeyCode.Escape)) SetPaused(!paused);
        if(Input.GetKeyDown(KeyCode.R))
        {
            if(Finished) BeginMatch();
            else if(!paused&&player.magazine<30&&player.reloadTime<=0) player.reloadTime=1.65f;
        }
        if(Finished) { FinishPresentation(); return; }
        if(paused || smoke) return;
        float dt=Mathf.Min(Time.deltaTime,.05f);
        roundTime+=dt; hitConfirmTime=Mathf.Max(0,hitConfirmTime-dt); UpdateObstacles(dt);
        if(!spectatorMode) PlayerInput(dt);
        if(!testLabEnabled || aiMoveEnabled || aiAttackEnabled || aiJumpEnabled) EnemyAI(enemy,player,dt);
        if(spectatorMode) EnemyAI(player,enemy,dt);
        if(targetLock) FaceOpponent(player,enemy,dt);
        FaceOpponent(enemy,player,dt);
        UpdateCombat(dt); UpdateSalvage(dt); UpdateSparks(dt); Animate(player); Animate(enemy);
        if(Finished) FinishPresentation();
    }
    void LateUpdate() { if(player!=null && enemy!=null) UpdateCamera(Time.deltaTime); }
    void UpdateCamera(float dt)
    {
        if(spectatorMode&&!inMenu)
        {
            Vector3 spectatorCenter=(player.root.position+enemy.root.position)*.5f;
            if(Input.GetMouseButton(2))
            {
                cameraYaw+=Input.GetAxis("Mouse X")*3f;
                cameraPitch=Mathf.Clamp(cameraPitch-Input.GetAxis("Mouse Y")*2f,20,70);
            }
            else cameraYaw+=dt*8f;
            Quaternion orbit=Quaternion.Euler(cameraPitch,cameraYaw,0);
            view.orthographic=true; view.orthographicSize=26;
            view.transform.position=spectatorCenter-orbit*Vector3.forward*78;
            view.transform.LookAt(spectatorCenter);
            if(starRoot!=null) starRoot.position=new Vector3(Mathf.Floor(spectatorCenter.x/80)*80,0,Mathf.Floor(spectatorCenter.z/80)*80);
            return;
        }
        if(!inMenu&&!paused&&!smoke)
        {
            if(targetLock)
            {
                Vector3 toward=enemy.root.position-player.root.position;
                cameraYaw=Mathf.LerpAngle(cameraYaw,Mathf.Atan2(toward.x,toward.z)*Mathf.Rad2Deg,1-Mathf.Exp(-8*dt));
                cameraPitch=Mathf.Lerp(cameraPitch,12,1-Mathf.Exp(-8*dt));
            }
            Quaternion rotation=Quaternion.Euler(cameraPitch,cameraYaw,0);
            Vector3 focus=player.root.position+Vector3.up*2.2f;
            Vector3 desired=focus-rotation*Vector3.forward*8.5f;
            if(ObstacleOnSegment(focus,desired,.25f,out float obstruction))
                desired=focus+(desired-focus).normalized*Mathf.Max(2.2f,obstruction-.35f);
            float followBlend=1-Mathf.Exp(-12*dt);
            view.transform.position=Vector3.Lerp(view.transform.position,desired,followBlend);
            if(cameraShakeTime>0)
            {
                cameraShakeTime=Mathf.Max(0,cameraShakeTime-dt);
                view.transform.position+=new Vector3(Mathf.Sin(Time.time*83f),Mathf.Cos(Time.time*109f),0)*cameraShakeStrength*(cameraShakeTime/.3f);
            }
            view.transform.rotation=Quaternion.Slerp(view.transform.rotation,rotation,followBlend);
            return;
        }
        Vector3 center=(player.root.position+enemy.root.position)*.5f, separation=player.root.position-enemy.root.position;
        float zoom=Mathf.Max(12.5f,(Mathf.Abs(Vector3.Dot(separation,view.transform.up))*.5f+5)/.68f,
            (Mathf.Abs(Vector3.Dot(separation,view.transform.right))*.5f+7)/view.aspect);
        float blend=1-Mathf.Exp(-7*dt);
        view.orthographicSize=Mathf.Lerp(view.orthographicSize,zoom,blend);
        view.transform.position=Vector3.Lerp(view.transform.position,center+new Vector3(46,58,-46),blend);
        if(starRoot!=null) starRoot.position=new Vector3(Mathf.Floor(center.x/80)*80,0,Mathf.Floor(center.z/80)*80);
    }
    void PlayerInput(float dt)
    {
        if(Input.GetKeyDown(KeyCode.Alpha1)) player.weaponSlot=1;
        if(Input.GetKeyDown(KeyCode.Alpha2)) player.weaponSlot=2;
        if(Input.GetKeyDown(KeyCode.Alpha3)) player.weaponSlot=3;
        player.crouching=Input.GetKey(KeyCode.F);
        if(Input.GetKeyDown(KeyCode.F)) TryPickupSalvage();
        Vector3 forward=Vector3.ProjectOnPlane(view.transform.forward,Vector3.up).normalized;
        Vector3 right=Vector3.ProjectOnPlane(view.transform.right,Vector3.up).normalized;
        float x=(Input.GetKey(KeyCode.D)?1:0)-(Input.GetKey(KeyCode.A)?1:0);
        float y=(Input.GetKey(KeyCode.W)?1:0)-(Input.GetKey(KeyCode.S)?1:0);
        Vector3 move=Vector3.ClampMagnitude(right*x+forward*y,1);
        player.boostHeld=Input.GetKey(KeyCode.LeftShift)||Input.GetKey(KeyCode.RightShift);
        // A second press of the same movement key within the window starts a fixed-distance dash.
        KeyCode[] keys={KeyCode.W,KeyCode.A,KeyCode.S,KeyCode.D};
        Vector3[] axes={forward,-right,-forward,right};
        for(int i=0;i<keys.Length;i++)
        {
            if(!Input.GetKeyDown(keys[i])) continue;
            if(!player.spec.usePrototypeMovement && Time.time-lastDirectionTap[i]<=doubleTapWindow) TryDash(player,axes[i]);
            lastDirectionTap[i]=Time.time;
        }
        player.jumpHeld=Input.GetKey(KeyCode.Space);
        if(Input.GetKeyDown(KeyCode.Space)&&player.jumpHeight<=.02f && player.knockdownTime<=0f && player.lockTime<=0f)
        {
            player.jumpOriginY=player.root.position.y;
            player.jumpVelocity=jumpSpeed*player.spec.jumpImpulseScale/Mathf.Sqrt(Mathf.Max(.5f,player.spec.jumpWeightFactor));
        }
        if(Input.GetMouseButtonDown(1) && Vector3.Distance(player.root.position,enemy.root.position)<=player.spec.lockRange)
        {
            targetLock=true; occludedLockTimer=player.spec.occludedLockSeconds;
        }
        if(Input.GetMouseButtonUp(1) || Vector3.Distance(player.root.position,enemy.root.position)>player.spec.lockRange) targetLock=false;
        if(!targetLock)
        {
            cameraYaw+=Input.GetAxis("Mouse X")*mouseSensitivity;
            cameraPitch=Mathf.Clamp(cameraPitch-Input.GetAxis("Mouse Y")*mouseSensitivity,-5,45);
        }
        if(targetLock) FaceOpponent(player,enemy,dt);
        else FaceMovement(player,move,dt);
        Move(player,move,dt);
        if(targetLock)
        {
            Vector3 viewport=view.WorldToViewportPoint(enemy.root.position+Vector3.up*1.2f);
            bool visible=viewport.z>0&&viewport.x>0&&viewport.x<1&&viewport.y>0&&viewport.y<1&&
                !ObstacleOnSegment(player.root.position+Vector3.up*1.5f,enemy.root.position+Vector3.up*1.2f,.15f,out _);
            if(visible) occludedLockTimer=player.spec.occludedLockSeconds;
            else
            {
                occludedLockTimer-=dt;
                if(Vector3.Distance(player.root.position,enemy.root.position)>player.spec.occludedLockRange || occludedLockTimer<=0)
                    targetLock=false;
            }
        }
        aimDirection=targetLock?(enemy.root.position-player.root.position).normalized:player.facing;
        Ray ray=view.ScreenPointToRay(new Vector3(Screen.width*.5f,Screen.height*.5f,0));
        if(new Plane(Vector3.up,new Vector3(0,1.1f,0)).Raycast(ray,out float distance))
        {
            Vector3 target=ray.GetPoint(distance); if(!targetLock) aimDirection=Vector3.ProjectOnPlane(target-player.root.position,Vector3.up).normalized;
            aimMarker.position=target;
        }
        assistEngaged=!smoke&&CanAssist(player,enemy,aimDirection);
        if(!smoke) aimDirection=AssistAim(player,enemy,aimDirection);
        canShoot=InArc(player,aimDirection);
        foreach(Renderer r in aimMarker.GetComponentsInChildren<Renderer>()) r.sharedMaterial=Glow(canShoot?Cyan:Orange);
        if(Input.GetMouseButton(0))
        {
            if(player.weaponSlot==1)
            {
                if(player.magazine<=0 || player.reloadTime>0) player.failedFireFlash=.2f;
                Attack(player,enemy,aimDirection);
            }
            else if(player.weaponSlot==3)
            {
                if(Input.GetMouseButtonDown(0)) UseSalvage(aimDirection);
            }
            else if(player.weaponSlot==2)
            {
                if(player.specialTimer>0) player.failedFireFlash=.2f;
                else if(!RequiresHomingLock(player.spec.special) || targetLock) Special(player,enemy,aimDirection);
                else player.failedFireFlash=.2f;
            }
        }
    }
    bool RequiresHomingLock(SpecialKind kind)=>kind==SpecialKind.MissileFan || kind==SpecialKind.HomingMines;
    void FaceOpponent(Fighter f,Fighter target,float dt=0f)
    {
        Vector3 d=Vector3.ProjectOnPlane(target.root.position-f.root.position,Vector3.up);
        if(d.sqrMagnitude<=.001f) return; // Keep the previous heading at close pass-through.
        Vector3 desired=d.normalized;
        Vector3 current=Vector3.ProjectOnPlane(f.facing,Vector3.up);
        // Reset/setup calls deliberately snap; gameplay calls turn smoothly without changing velocity.
        if(dt<=0f || current.sqrMagnitude<=.001f) { f.facing=desired; return; }
        f.facing=Vector3.RotateTowards(current.normalized,desired,8f*dt,0f).normalized;
    }
    void FaceMovement(Fighter f,Vector3 move,float dt)
    {
        Vector3 desired=Vector3.ProjectOnPlane(move,Vector3.up);
        Vector3 current=Vector3.ProjectOnPlane(f.facing,Vector3.up);
        if(desired.sqrMagnitude<.001f || current.sqrMagnitude<.001f) return;
        f.facing=Vector3.RotateTowards(current.normalized,desired.normalized,8f*dt,0).normalized;
    }
    bool TryDash(Fighter f,Vector3 direction)
    {
        if(direction.sqrMagnitude<.01f || f.dashRemaining>0 || f.dashCooldownLeft>0 || f.boostLocked || f.lockTime>0) return false;
        // Runner ground dodges use cooldown/distance, not boost fuel.
        // Airborne dodges still consume the shared gauge.
        bool freeGroundDash=f.spec.mobility==MobilityKind.Runner && f.jumpHeight<=.02f && f.jumpVelocity<=0f;
        float cost=freeGroundDash?0f:dashFuelCost*f.dashCostFactor;
        if(f.boostEnergy<cost) return false;
        f.boostEnergy-=cost;
        if(cost>0f) f.recoveryTime=recoveryDelay;
        f.dashDirection=direction.normalized;
        f.dashRemaining=Mathf.Max(.01f,dashDistance*f.dashDistanceFactor);
        f.dashCooldownLeft=dashCooldown;
        PlaySound(SoundKind.Boost,f.root.position,.6f);
        return true;
    }
    float BoostDirectionFactor(Fighter f,Vector3 direction)
    {
        Fighter target=Opponent(f);
        if(target==null) return 1f;
        Vector3 toward=(target.root.position-f.root.position).normalized;
        float dot=Vector3.Dot(direction.normalized,toward);
        return dot>=0?1+.18f*dot:1+.12f*dot;
    }
    void Move(Fighter f,Vector3 direction,float dt)
    {
        if(f.reloadTime>0) { f.reloadTime=Mathf.Max(0,f.reloadTime-dt); if(f.reloadTime<=0) f.magazine=30; }
        f.weaponTimer=Mathf.Max(0,f.weaponTimer-dt); f.specialTimer=Mathf.Max(0,f.specialTimer-dt);
        f.dashCooldownLeft=Mathf.Max(0,f.dashCooldownLeft-dt);
        f.shieldFlash=Mathf.Max(0,f.shieldFlash-dt); f.hitFlash=Mathf.Max(0,f.hitFlash-dt);
        f.failedFireFlash=Mathf.Max(0,f.failedFireFlash-dt);
        f.knockdownTime=Mathf.Max(0,f.knockdownTime-dt);
        f.invulnerableTime=Mathf.Max(0,f.invulnerableTime-dt);
        float recovery=Mathf.Max(0,dt-f.shieldRecoveryTime);
        f.shieldRecoveryTime=Mathf.Max(0,f.shieldRecoveryTime-dt);
        f.shieldEnergy=Mathf.Min(f.spec.ShieldMax,f.shieldEnergy+f.spec.ShieldMax/shieldFullRecoveryTime*recovery);
        // Release both boost controls before rearming an exhausted shared gauge.
        if(!f.boostHeld && !f.jumpHeld) f.boostLocked=false;
        bool wasBoosting=f.boosting;
        if(f.spec.boost==BoostKind.Pulse && f.boostHeld && !f.wasBoostHeld && f.lockTime<=0 &&
            f.boostEnergy>=f.spec.boostDrain*f.spec.pulseDuration && direction.sqrMagnitude>.01f)
        { f.pulseTime=f.spec.pulseDuration; f.pulseDirection=direction.normalized; }
        f.wasBoostHeld=f.boostHeld;
        bool dashing=f.dashRemaining>0;
        bool wants=!dashing&&f.boostHeld&&direction.sqrMagnitude>.01f;
        f.boosting=wants&&!f.boostLocked&&f.lockTime<=0;
        Vector3 motion=Vector3.ClampMagnitude(direction,1);
        float speed=f.spec.speed;
        if(f.boosting)
        {
            float active=dt;
            float required=f.spec.boostDrain*active;
            float fraction=required>0?Mathf.Min(1,f.boostEnergy/required):1;
            speed=Mathf.Lerp(f.spec.speed,f.spec.boostSpeed*BoostDirectionFactor(f,motion),fraction*active/dt);
            f.boostEnergy=Mathf.Max(0,f.boostEnergy-required); f.recoveryTime=recoveryDelay;
            if(f.boostEnergy<=0) { f.boostLocked=true; f.boosting=false; }
            if(!wasBoosting) PlaySound(SoundKind.Boost,f.root.position,.45f);
        }
        // Vertical thrust shares fuel with horizontal boost; larger/heavier frames cost more.
        if(f.jumpHeight<=.02f && f.jumpVelocity>0) f.jumpOriginY=f.root.position.y;
        bool airborne=f.jumpHeight>0 || f.jumpVelocity>0;
        bool verticalBoost=f.spec.allowVerticalBoost && airborne && f.jumpHeight<jumpMaxHeight && f.jumpHeld && !f.boostLocked && f.boostEnergy>0;
        if(verticalBoost)
        {
            float weightFactor=Mathf.Max(.5f,f.spec.jumpWeightFactor); // Explicit per-frame tuning, independent of armor and visual scale.
            float drain=jumpBoostDrain*weightFactor*dt;
            float fraction=drain>0?Mathf.Min(1,f.boostEnergy/drain):1;
            f.jumpVelocity+=jumpThrust*f.spec.verticalBoostScale/weightFactor*fraction*dt;
            f.boostEnergy=Mathf.Max(0,f.boostEnergy-drain);
            f.recoveryTime=recoveryDelay;
            if(f.boostEnergy<=0) f.boostLocked=true;
        }
        if(!f.boosting && !verticalBoost && !dashing)
        {
            // Airborne fuel regeneration enables repeated climbs without landing.
            // Keep the recovery delay ticking in flight, but refill only on support.
            float t=Mathf.Max(0,dt-f.recoveryTime);
            f.recoveryTime=Mathf.Max(0,f.recoveryTime-dt);
            if(!airborne) f.boostEnergy=Mathf.Min(f.spec.boostCapacity,f.boostEnergy+boostRecovery*t);
        }
        f.pulseTime=Mathf.Max(0,f.pulseTime-dt);
        // Holding Space adds fuel-powered lift, capped at a readable obstacle-clearing height.
        if(f.jumpHeight>=jumpMaxHeight && f.jumpVelocity>0) f.jumpVelocity=0;
        float gravityScale=f.jumpVelocity<0?fallGravityMultiplier:1f;
        f.jumpVelocity-=jumpGravity*gravityScale*f.spec.fallGravityFactor*dt;
        Vector3 desired=motion*speed;
        if(dashing)
        {
            float dashStep=Mathf.Min(f.dashRemaining,Mathf.Max(.01f,dashSpeed*f.dashSpeedFactor)*dt);
            desired=f.dashDirection*(dashStep/Mathf.Max(dt,.0001f));
        }
        if(motion.sqrMagnitude>.001f && !dashing)
        {
            Vector3 facing=Vector3.ProjectOnPlane(f.facing,Vector3.up).normalized;
            float forward=Vector3.Dot(motion,facing);
            float lateral=Vector3.Dot(motion,Vector3.Cross(Vector3.up,facing));
            float leftFactor=f.boosting?Mathf.Max(.92f,f.spec.leftSpeedFactor):f.spec.leftSpeedFactor;
            float rightFactor=f.boosting?Mathf.Max(.92f,f.spec.rightSpeedFactor):f.spec.rightSpeedFactor;
            float ratio=Mathf.Max(0,forward)*f.spec.forwardSpeedFactor+
                Mathf.Max(0,-forward)*f.spec.reverseSpeedFactor+
                Mathf.Max(0,lateral)*rightFactor+
                Mathf.Max(0,-lateral)*leftFactor;
            desired=motion.normalized*speed*ratio;
        }
        // Tank prototype favors forward travel; lateral/reverse movement remains possible.
        // No visual track or independent turret is implied by this movement-only tuning.
        if(f.spec.mobility==MobilityKind.Tank && !f.spec.usePrototypeMovement && desired.sqrMagnitude>.001f)
        {
            Vector3 heading=Vector3.ProjectOnPlane(f.facing,Vector3.up).normalized;
            float forward=Vector3.Dot(desired.normalized,heading);
            desired*=forward>.5f?1.3f:forward<-.5f?.55f:.7f;
        }
        if(f.crouching) desired*=.4f;
        // Tank handling prototype: changing direction bleeds momentum, then ramps back.
        // Applies only to opt-in rigs; existing standard frames keep their response.
        if(f.spec.usePrototypeMovement && f.spec.turnAcceleration>0f && !dashing)
        {
            Vector3 previous=Vector3.ProjectOnPlane(f.velocity,Vector3.up);
            desired=Vector3.MoveTowards(previous,desired,f.spec.speed*f.spec.turnAcceleration*dt);
        }
        if(f.spec.boost==BoostKind.Glide && !dashing) desired=Vector3.Lerp(Vector3.ProjectOnPlane(f.velocity,Vector3.up),desired,1-Mathf.Exp(-10*dt));
        if(f.lockTime>0) { desired=Vector3.zero; f.lockTime=Mathf.Max(0,f.lockTime-dt); }
        if(f.knockdownTime>0) { desired=Vector3.zero; f.boosting=false; }
        if(f.rushTime>0)
        {
            desired=f.rushDirection*34; f.rushTime=Mathf.Max(0,f.rushTime-dt);
            f.boosting=true; f.saber.gameObject.SetActive(true);
        }
        float preLandingHeight=f.jumpHeight;
        float preLandingFallSpeed=-f.jumpVelocity;
        Vector3 before=f.root.position; MoveWithObstacles(f,desired*dt+Vector3.up*f.jumpVelocity*dt);
        float jumpCeiling=f.jumpOriginY+jumpMaxHeight;
        if(f.root.position.y>jumpCeiling) { f.root.position=new Vector3(f.root.position.x,jumpCeiling,f.root.position.z); f.jumpVelocity=0; }
        f.jumpHeight=Mathf.Max(0,f.root.position.y-LandingHeight(f.root.position,f.Radius,f.root.position.y));
        // High-speed landing briefly staggers the legs; unlike weapon knockdown it grants no invulnerability.
        if(preLandingHeight>.05f && f.jumpHeight<=.001f && preLandingFallSpeed>15f)
        {
            float severity=Mathf.Clamp01((preLandingFallSpeed-15f)/20f);
            float recovery=Mathf.Max(.2f,f.spec.recoverySpeed);
            f.knockdownTime=Mathf.Max(f.knockdownTime,(.2f+.55f*severity)*f.spec.landingStaggerScale/recovery);
            f.dashRemaining=0;
        }
        if(f.jumpHeight<=.001f && f.jumpVelocity<0) f.jumpVelocity=0;
        float actualDistance=Vector3.Distance(Vector3.ProjectOnPlane(f.root.position,Vector3.up),Vector3.ProjectOnPlane(before,Vector3.up));
        f.velocity=(f.root.position-before)/Mathf.Max(.0001f,dt);
        if(dashing)
        {
            f.dashRemaining=Mathf.Max(0,f.dashRemaining-actualDistance);
            // End dash on collision instead of pushing against a wall indefinitely.
            if(actualDistance<desired.magnitude*dt*.5f) f.dashRemaining=0;
        }
        if(f.rushTime>0 || f.saber.gameObject.activeSelf) CheckSaber(f,before,f.root.position);
        if(f.rushTime<=0) f.saber.gameObject.SetActive(false);
    }
    void Hit(Fighter target,Vector3 source,float damage)
    {
        if(Finished || target.invulnerableTime>0) return;
        if(AbsorbSalvageShield(target,damage)) return;
        float before=target.hp+target.shieldEnergy;
        float incoming=damage*(1-target.spec.armor), absorbed=Mathf.Min(target.shieldEnergy,incoming);
        target.shieldEnergy-=absorbed; target.hp=Mathf.Max(0,target.hp-incoming+absorbed);
        float actualDamage=before-target.hp-target.shieldEnergy;
        RecordDamage(target,actualDamage,absorbed>0);
        if(target==player && actualDamage>0)
        {
            cameraShakeTime=.3f;
            cameraShakeStrength=Mathf.Clamp(actualDamage/110f,.04f,.32f);
        }
        if(actualDamage>32f*target.spec.knockdownResistance && target.hp>0)
        {
            target.knockdownTime=.75f/Mathf.Max(.2f,target.spec.recoverySpeed);
            target.invulnerableTime=target.knockdownTime+.3f;
        }
        target.hitFlash=.14f; target.shieldRecoveryTime=shieldRecoveryDelay; target.shieldFlash=absorbed>0?.12f:0;
        Burst(target.root.position,absorbed>0?Cyan:Orange,6);
        Flash(target.root.position,absorbed>0?Cyan:new Color(1,.65f,.22f),.85f,.12f);
        PlaySound(absorbed>0?SoundKind.Shield:SoundKind.Hit,target.root.position,.65f);
        if(Finished) { if(enemy.hp<=0) wins++; else losses++; }
    }
    bool InArc(Fighter f,Vector3 direction)=>direction.sqrMagnitude>.01f&&Vector3.Angle(f.facing,direction)<=gunHalfAngle;
    void Burst(Vector3 p,Color color,int count)
    {
        for(int i=0;i<count;i++) sparks.Add(new Spark {visual=Box("Impact voxel",null,p,Vector3.one*.18f,color),
            velocity=UnityEngine.Random.onUnitSphere*6,life=.35f});
    }
    void UpdateSparks(float dt)
    {
        for(int i=sparks.Count-1;i>=0;i--)
        {
            Spark s=sparks[i]; s.life-=dt; s.visual.position+=s.velocity*dt;
            s.visual.localScale=Vector3.one*Mathf.Max(0,s.life*.6f);
            if(s.life<=0) { Destroy(s.visual.gameObject); sparks.RemoveAt(i); }
        }
    }
    void Animate(Fighter f)
    {
        f.root.rotation=Quaternion.LookRotation(f.facing);
        f.body.localPosition=Vector3.up*(f.knockdownTime>0?-.38f:f.crouching?-.22f:.40f)+Vector3.up*Mathf.Sin(roundTime*2.4f)*.06f;
        if(f.shadow!=null) { f.shadow.position=new Vector3(f.root.position.x,GroundSurfaceY+.06f,f.root.position.z); f.shadow.localScale=new Vector3(2.7f*f.spec.size,.012f,2.1f*f.spec.size); }
        Vector3 localVelocity=f.root.InverseTransformDirection(f.velocity);
        f.body.localRotation=f.knockdownTime>0?Quaternion.Euler(68,0,0):Quaternion.Euler(Mathf.Clamp(localVelocity.z*.65f,-16,16),0,Mathf.Clamp(-localVelocity.x*.5f,-12,12));
        float stride=Mathf.Clamp01(Vector3.ProjectOnPlane(f.velocity,Vector3.up).magnitude/Mathf.Max(1f,f.spec.speed));
        float gait=Mathf.Sin(roundTime*(7f+stride*9f))*stride;
        f.leftLeg.localRotation=Quaternion.Euler(gait*25f,0,-5);
        f.rightLeg.localRotation=Quaternion.Euler(-gait*25f,0,5);
        f.leftArm.localRotation=Quaternion.Euler(-gait*13f,0,0);
        f.rightArm.localRotation=Quaternion.Euler(gait*13f,0,0);
        f.gun.gameObject.SetActive(true);
        f.shield.gameObject.SetActive(f.shieldFlash>0&&!Finished&&!inMenu);
        bool jumpExhaust=f.jumpVelocity>1f&&!inMenu&&!Finished;
        bool moveExhaust=f.boosting&&Vector3.ProjectOnPlane(f.velocity,Vector3.up).sqrMagnitude>1&&!inMenu&&!Finished;
        if(f.jumpFlame!=null) f.jumpFlame.gameObject.SetActive(jumpExhaust);
        f.thruster.gameObject.SetActive(jumpExhaust||moveExhaust);
        if(f.thruster.gameObject.activeSelf)
        {
            Vector3 back=jumpExhaust&&!moveExhaust
                ? (Vector3.down-Vector3.ProjectOnPlane(f.facing,Vector3.up)*.55f).normalized
                : -f.velocity.normalized;
            f.thruster.position=jumpExhaust&&!moveExhaust
                ? f.root.position+back*(.35f*f.spec.size)
                : f.root.position+back*f.spec.size;
            f.thruster.rotation=Quaternion.LookRotation(back,jumpExhaust&&!moveExhaust?Vector3.forward:Vector3.up);
            float lift=Mathf.Clamp01(f.jumpVelocity/Mathf.Max(1,jumpSpeed));
            f.thruster.localScale=new Vector3(f.spec.size*(jumpExhaust?.95f:1),f.spec.size*(jumpExhaust?.95f:1),
                (jumpExhaust?1.25f+lift*.35f:1)+Mathf.Sin(roundTime*55)*.15f);
        }
    }
    void OnDestroy()
    {
        if(guideFont!=null) Destroy(guideFont);
        if(framePortraits!=null) foreach(RenderTexture rt in framePortraits) if(rt!=null) { rt.Release(); Destroy(rt); }
        foreach(Material m in materials.Values) if(m!=null) Destroy(m);
        foreach(AudioClip clip in soundClips.Values) if(clip!=null) Destroy(clip);
    }
}
