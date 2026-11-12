using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed partial class VoxArena : MonoBehaviour
{
    public Shader hullShader;
    public Shader cameraFadeShader;
    [Header("Imported artist model references")]
    public GameObject wispModelSource,bastionModelSource,tarantulaModelSource;
    public GameObject wispSwordSource,wispShieldSource;
    [HideInInspector] public GameObject[] gloveReviewBaseline,referenceHandsBaseline;
    public Texture2D wispBaseColorTexture,wispEmissionTexture;
    Material wispRuntimeMaterial;
    bool IsArticulatedWisp=>wispModelSource!=null&&wispModelSource.transform.Find("WISP_Rig_v003/Root")!=null;
    public float boostRecovery = 40, recoveryDelay = .6f;
    [Header("Jump and vertical boost")]
    public float jumpSpeed = 10.6f, jumpGravity = 24f, jumpThrust = 34f, jumpBoostDrain = 24f;
    [Tooltip("Steady upward speed while powered vertical boost is held, before frame tuning.")]
    public float boostClimbSpeed=8f, boostClimbAcceleration=14f;
    [Tooltip("Landing hit-stun is disabled for current playtests; hard landings still use normal movement physics.")]
    public bool landingStaggerEnabled=false;
    [Tooltip("Maximum vertical rise of an unpowered jump from takeoff.")]
    public float jumpMaxHeight = 6.4f;
    [Tooltip("Extra gravity while descending so landings feel quicker.")]
    public float fallGravityMultiplier = 2.7f;
    public float gunHalfAngle = 65, shieldRecoveryDelay = 3, shieldFullRecoveryTime = 30;
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
        public Transform guardShield,guardElbow;
        public BoosterEmitter boostEmitter;public float shieldVisualScale=.8f;public Light boostLight;public Transform boostVisual;public float exhaustBlend,damageSmokeClock;public float[] partDamage;public int lastDamagedPart;public bool deadVisual;
        public ReferenceHandPose[] handPoses;public bool handPoseInitialized;
        public Transform[] shoulderConnectors;public Quaternion[] shoulderWeaponRest;public Vector3[] shoulderWeaponOffsets;
        public string[] weaponArtIds;public Transform[] weaponGripPoints,weaponMuzzlePoints,shieldBeamVisuals;public bool modernWeaponArt;
        public string headVisualId="HEAD_STANDARD",coreVisualId="CORE_STANDARD",leftArmVisualId="ARM_STANDARD",rightArmVisualId="ARM_STANDARD",legsVisualId="LEGS_STANDARD",boosterVisualId="BOOSTER_TWIN";
        public Transform[] weaponVisuals;public float aimSettled;
        public Fighter assistTrackedTarget;public ArmGrip[] gripRig;public QuadPose quadPose;
        public float aiWeaponReadyAt,aiCoverRetry;public bool aiCoverRouting;public Vector3 aiCoverWaypoint;
        public float importedRadius;public Vector3 importedChassisAnchor;public bool capsuleTreads;public float[] importedTrackTravel;public Quaternion[][] importedTreadRotations,importedWheelRest;public Transform[][] importedRoadWheels;public float[][] importedWheelTurns;
        public bool simpleShell,importedChassis; public Vector3[][] importedTreadRest; public Vector3[] importedHipRest;
        public float punchTimer;public int punchSide;
        public bool guarding;
        public float guardBlend;
        // Stable team identity; multiple fighters can share a team in future modes.
        public int teamId,matchKills,matchDeaths; public float respawnTimer,duelDamageDealt;
        public int frameIndex,selectableIndex=-1; public float hitFlash;
        public Transform[] quadrupedLegs,quadrupedKnees,quadrupedAnkles;
        public Transform[][] treadLinks;
        public float treadPhase,treadPerimeter;
        public float[] treadSidePhases;
        public Vector3 previousTreadHeading=Vector3.forward;
        public int weaponSlot=1;
        public WeaponState[] weapons=NewWeaponStates();public WeaponProfile[] weaponProfiles=NewWeaponProfiles();
        public WeaponState ActiveWeapon=>weapons[Mathf.Clamp(weaponSlot-1,0,3)];
        public int magazine{get=>ActiveWeapon.ammo;set=>ActiveWeapon.ammo=value;}
        public float reloadTime{get=>ActiveWeapon.reload;set=>ActiveWeapon.reload=value;}
        public float shotIdleTime{get=>ActiveWeapon.idle;set=>ActiveWeapon.idle=value;}
        public CraftSpec spec=CraftCatalog.Selectable[0];
        public Vector3 facing=Vector3.forward,velocity,pulseDirection,rushDirection;
        // Independent ground chassis heading for opt-in tank steering; aiming remains on facing.
        public Vector3 chassisForward=Vector3.forward;
        public float weaponTimer{get=>ActiveWeapon.cooldown;set=>ActiveWeapon.cooldown=value;}
        public float hp,boostEnergy,shieldEnergy,specialTimer,recoveryTime,shieldRecoveryTime,shieldFlash;
        public float pulseTime,lockTime,rushTime,aiClock,aiReaction,stuckTime,orbitTime;
        public Vector3 aiMove,lastPosition,smoothedTargetVelocity;
        public int orbitSign=1;
        public float jumpHeight,jumpVelocity,jumpOriginY,jumpHoldElapsed;
        public bool crouching;
        public float failedFireFlash;
        public float knockdownTime,invulnerableTime;
        public Vector3 dashDirection;
        public float dashRemaining,dashCooldownLeft;
        // Part modifiers: replace with equipped leg/booster stats when assembly is implemented.
        public float dashSpeedFactor=1f,dashDistanceFactor=1f,dashCostFactor=1f;
        public bool boostHeld,wasBoostHeld,boostLocked,boosting,rushHit,jumpHeld,jumpChargeCanceled;
        public string aiState="APPROACH";
        public float Radius=>Mathf.Max(importedRadius,spec.footprintRadius>0?spec.footprintRadius:CraftRadius*spec.size);
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
    bool spectatorMode,timedFragMode;
    bool multiAiPerformanceMode;
    float cameraShakeTime,cameraShakeStrength;
    bool targetLock;
    float occludedLockTimer;
    const float DefaultThirdPersonPitch=26f;
    float cameraYaw=0,cameraPitch=DefaultThirdPersonPitch,mouseSensitivity=2.4f;
    bool captureGameplayPreview,captureCustomizationEditor;
    int selectedCraft,enemyCraft,wins,losses;
    float roundTime;
    GUIStyle label,small,title;
    static readonly Color Cyan=new Color(.18f,.88f,.96f), Orange=new Color(1,.38f,.19f);
    bool Finished=>IsMatchFinished();
    // Shared roster/hostility queries are the first step toward multi-fighter matches.
    // Existing 1v1 modes still register exactly the pilot and rival.
    bool IsHostile(Fighter a,Fighter b)=>a!=null && b!=null && a!=b && a.teamId!=b.teamId;
    void RegisterFighter(Fighter f)
    {
        if(f!=null && !fighterRoster.Contains(f)) fighterRoster.Add(f);
    }
    void UnregisterFighter(Fighter f) { fighterRoster.Remove(f); }
    Fighter AddMatchParticipant(string name,int chassisIndex,int teamId)
    {
        int index=Mathf.Clamp(chassisIndex,0,Mathf.Max(0,CraftCatalog.UnlockedCount-1));
        Fighter fighter=MakeFighter(string.IsNullOrWhiteSpace(name)?"Match participant":name,teamId==0?Cyan:Orange);
        ApplySelectableCharacter(fighter,index);
        fighter.teamId=teamId==0?0:1;
        fighter.root.position=ArenaSpawnPosition(fighter.teamId);
        fighter.hp=fighter.spec.health;fighter.boostEnergy=fighter.spec.boostCapacity;fighter.shieldEnergy=fighter.spec.ShieldMax;
        fighter.weapons=NewWeaponStates(fighter.weaponProfiles);fighter.weaponSlot=1;
        fighter.magazine=fighter.weaponProfiles[0].capacity;fighter.lastPosition=fighter.root.position;
        fighter.root.gameObject.SetActive(true);
        RegisterFighter(fighter);
        return fighter;
    }
    void RemoveMatchParticipant(Fighter fighter)
    {
        if(fighter==null||fighter==player||fighter==enemy)return;
        for(int i=shots.Count-1;i>=0;i--)if(shots[i].owner==fighter)RemoveShot(i);
        for(int i=pending.Count-1;i>=0;i--)if(pending[i].owner==fighter)pending.RemoveAt(i);
        for(int i=beams.Count-1;i>=0;i--)
            if(beams[i].owner==fighter){Destroy(beams[i].visual.gameObject);beams.RemoveAt(i);}
        UnregisterFighter(fighter);
        if(fighter.root!=null){fighter.root.gameObject.SetActive(false);Destroy(fighter.root.gameObject);}
        if(fighter.shadow!=null){fighter.shadow.gameObject.SetActive(false);Destroy(fighter.shadow.gameObject);}
    }
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
        legacyProjectileReview=System.Array.IndexOf(System.Environment.GetCommandLineArgs(),"--legacy-projectiles")>=0;
        bool motionStudy=System.Array.IndexOf(System.Environment.GetCommandLineArgs(),"--chassis-motion")>=0;
        bool projectileGallery=System.Array.IndexOf(System.Environment.GetCommandLineArgs(),"--capture-projectiles")>=0;
        bool labCapture=System.Array.IndexOf(System.Environment.GetCommandLineArgs(),"--capture-live-weapons")>=0;
        bool radarCapture=System.Array.IndexOf(System.Environment.GetCommandLineArgs(),"--radar-capture")>=0;
        bool cornerSoak=Array.IndexOf(Environment.GetCommandLineArgs(),"--corner-soak")>=0;
        bool shellCapture=Array.IndexOf(Environment.GetCommandLineArgs(),"--capture-shell")>=0;
        bool effectCapture=Array.IndexOf(Environment.GetCommandLineArgs(),"--fx-capture")>=0;
        bool automaticBalanceStudy=Array.IndexOf(Environment.GetCommandLineArgs(),"--automatic-balance-study")>=0;
        bool balanceStudy=Array.IndexOf(Environment.GetCommandLineArgs(),"--balance-study")>=0;
        bool weaponDuelStudy=Array.IndexOf(Environment.GetCommandLineArgs(),"--weapon-duel-study")>=0;
        bool climateSmoke=Array.IndexOf(Environment.GetCommandLineArgs(),"--climate-smoke")>=0;
        bool prototypeSoak=Array.IndexOf(Environment.GetCommandLineArgs(),"--prototype-soak")>=0;
        bool wispSoak=Array.IndexOf(Environment.GetCommandLineArgs(),"--wisp-soak")>=0;
        bool performance=Array.IndexOf(Environment.GetCommandLineArgs(),"--performance-test")>=0;
        bool multiAIPerformance=Array.IndexOf(Environment.GetCommandLineArgs(),"--multi-ai-performance")>=0;
        multiAiPerformanceMode=multiAIPerformance;
        bool captureGallery=Array.IndexOf(Environment.GetCommandLineArgs(),"--capture-docs-gallery")>=0;
        bool captureVisualTargets=Array.IndexOf(Environment.GetCommandLineArgs(),"--capture-visual-targets")>=0;
        bool environmentSmoke=Array.IndexOf(Environment.GetCommandLineArgs(),"--environment-smoke")>=0;
        bool captureLineup=Array.IndexOf(Environment.GetCommandLineArgs(),"--capture-customization-lineup")>=0;
        captureCustomizationEditor=Array.IndexOf(Environment.GetCommandLineArgs(),"--capture-bastion-customization")>=0;
        captureGameplayPreview=FamilyReviewRequested||automaticBalanceStudy||captureCustomizationEditor||captureLineup||motionStudy||projectileGallery||labCapture||radarCapture||cornerSoak||shellCapture||effectCapture||balanceStudy||weaponDuelStudy||climateSmoke||prototypeSoak||Array.IndexOf(Environment.GetCommandLineArgs(),"--camera-review")>=0||wispSoak||Array.IndexOf(Environment.GetCommandLineArgs(),"--capture-gameplay-ui")>=0;
        smoke=FamilyReviewRequested||automaticBalanceStudy||captureLineup||motionStudy||projectileGallery||labCapture||radarCapture||cornerSoak||shellCapture||effectCapture||balanceStudy||weaponDuelStudy||climateSmoke||prototypeSoak||wispSoak||performance||multiAIPerformance||captureGallery||captureVisualTargets||environmentSmoke||Array.IndexOf(Environment.GetCommandLineArgs(),"--smoke-test")>=0;
        // Plains prototype is archived at PD request; ordinary play exposes bounded maps only.
        arenaMap=Array.IndexOf(Environment.GetCommandLineArgs(),"--coastal")>=0?ArenaMapKind.CoastalBay:
            Array.IndexOf(Environment.GetCommandLineArgs(),"--outdoor")>=0?ArenaMapKind.OutdoorCanyon:
            smoke?ArenaMapKind.IndoorFactory:ResolveStoredMap(PlayerPrefs.GetInt(PreferencePrefix+"Map",0));
        if(RunPreferenceProbe()) return;
        if(!smoke&&!captureGameplayPreview)
        {
            Screen.fullScreenMode=FullScreenMode.FullScreenWindow;
            int width=Display.main.systemWidth,height=Display.main.systemHeight;
            if(width>0&&height>0)Screen.SetResolution(width,height,FullScreenMode.FullScreenWindow);
        }
        BuildWorld();BuildPDVisualEffects(); BuildAudio(); LoadPreferences();
        selectedCraft=captureCustomizationEditor?Mathf.Min(1,CraftCatalog.UnlockedCount-1):Mathf.Clamp(selectedCraft,0,CraftCatalog.UnlockedCount-1);
        if(!smoke&&!captureGameplayPreview)InitializeAutomaticBalance();
        ApplySelectableCharacter(player,selectedCraft); ApplySelectableCharacter(enemy,ResolveRival());
        ResetRound(); BuildPortraits(); inMenu=true;
        if(captureCustomizationEditor){balanceFrameIndex=1;balanceTab=0;balanceMountIndex=0;OpenBalanceEditor();}
        if(automaticBalanceStudy)StartCoroutine(AutomaticBalanceStudy());
        else if(captureCustomizationEditor)StartCoroutine(CaptureBastionCustomization());
        else if(captureLineup)StartCoroutine(CaptureCustomizationLineup());
        else if(motionStudy)StartCoroutine(ChassisMotionStudy());
        else if(projectileGallery)StartCoroutine(CaptureProjectileGallery());
        else if(labCapture)StartCoroutine(CaptureLiveWeaponLab());
        else if(radarCapture)StartCoroutine(RadarCapture());
        else if(cornerSoak)StartCoroutine(CornerSoak());
        else if(shellCapture)StartCoroutine(CaptureShellComparison());
        else if(effectCapture)StartCoroutine(EffectCapture());
        else if(balanceStudy)StartCoroutine(BalanceStudy());
        else if(weaponDuelStudy)StartCoroutine(WeaponDuelStudy());
        else if(climateSmoke)StartCoroutine(ClimateSmoke());
        else if(prototypeSoak)StartCoroutine(PrototypeSoak());
        else if(wispSoak) StartCoroutine(WispSoak());
        else if(environmentSmoke) StartCoroutine(EnvironmentSmoke());
        else if(FamilyReviewRequested) StartCoroutine(FamilyArtReview());
        else if(captureGameplayPreview) StartCoroutine(CaptureGameplayUI());
        else if(multiAIPerformance) StartCoroutine(MultiAIPerformanceTest());
        else if(performance) StartCoroutine(PerformanceTest());
        else if(captureGallery) StartCoroutine(CaptureDocumentationGallery());
        else if(captureVisualTargets) StartCoroutine(CapturePrototypeVisuals());
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
        CraftSpec preset=CraftCatalog.All[Mathf.Clamp(index,0,CraftCatalog.All.Length-1)];
        ApplyCharacterPreset(f,preset,index,-1);
    }
    void ApplySelectableCharacter(Fighter f,int index)
    {
        int selected=Mathf.Clamp(index,0,CraftCatalog.Selectable.Length-1);
        CraftSpec preset=CraftCatalog.Selectable[selected];
        ApplyCharacterPreset(f,preset,preset.sourceFrameIndex,selected);
    }
    void ApplyCharacterPreset(Fighter f,CraftSpec preset,int visualFrameIndex,int selectableIndex)
    {
        if(f.deadVisual)RestorePDVisuals();
        f.spec=preset.RuntimeCopy();f.weaponProfiles=NewWeaponProfiles(); f.frameIndex=Mathf.Clamp(visualFrameIndex,0,CraftCatalog.All.Length-1);
        f.selectableIndex=selectableIndex; ResetAllocations(f);ApplyBalanceParts(f);f.weaponProfiles=BalanceWeaponProfiles(f);ApplyBalanceSkill(f);
        // Re-selecting a tank must not inherit the previous chassis heading.
        // Keep chassis yaw independent of the upper-body aiming direction after spawn.
        Vector3 initialChassis=Vector3.ProjectOnPlane(f.facing,Vector3.up);
        f.chassisForward=initialChassis.sqrMagnitude>.001f?initialChassis.normalized:Vector3.forward;
        BuildRobot(f); f.body.localScale=Vector3.one*f.spec.size;if(f.guardShield!=null)f.guardShield.gameObject.SetActive(UsesNativeGuardShield(f));
        bool artistWisp=(f.spec.name=="WISP"&&wispModelSource!=null||f.importedChassis)&&f.headPart!=null;
        foreach(Renderer r in f.body.GetComponentsInChildren<Renderer>())
            if(r.name.StartsWith("Armor") || r.name=="Visor")
            {
                bool importedModule=artistWisp&&(IsArticulatedWisp||
                    r.transform.IsChildOf(f.armsPart)||r.transform.IsChildOf(f.corePart)||
                    r.transform.IsChildOf(f.headPart)||r.transform.IsChildOf(f.legsPart));
                if(!importedModule) r.sharedMaterial=r.name=="Visor"?Glow(f.spec.color):Mat(f.spec.color);
            }
        ApplyBalanceDesignPaint(f);
        // Weapon silhouettes are built separately from arm modules.
        f.saber.gameObject.SetActive(false);
    }
    void BeginMatch()
    {
        selectedCraft=Mathf.Clamp(selectedCraft,0,CraftCatalog.UnlockedCount-1);
        enemyCraft=ResolveRival(); SavePreferences();
        ApplySelectableCharacter(player,selectedCraft); ApplySelectableCharacter(enemy,enemyCraft);
        ResetRound();StartMatchRules();SelectRoundClimate(); inMenu=false; view.orthographic=smoke||spectatorMode; view.fieldOfView=60;
        cameraPitch=spectatorMode?42:DefaultThirdPersonPitch;
        ResetMouseLookCapture();
        Cursor.lockState=spectatorMode?CursorLockMode.None:CursorLockMode.Locked; Cursor.visible=spectatorMode;
    }
    void ResetRound()
    {
        RestorePDVisuals();ResetCoverForRound();
        finishCleaned=false; targetLock=false;lockViewInitialized=wasInputTargetLock=false; occludedLockTimer=0; StopSounds(); aimMarker.gameObject.SetActive(true);
        ResetFeedback();
        ResetSalvage();
        ClearCombat();
        foreach(Spark s in sparks) ReleasePDPart(s.visual);
        sparks.Clear();
        player.teamId=0;
        enemy.teamId=1;
        RegisterFighter(player);
        RegisterFighter(enemy);
        foreach(Fighter f in fighterRoster)
        {
            f.deadVisual=false;f.respawnTimer=0;
            f.root.gameObject.SetActive(true);
            f.hp=f.spec.health; f.boostEnergy=f.spec.boostCapacity; f.shieldEnergy=f.spec.ShieldMax;
            f.weaponTimer=f.specialTimer=f.recoveryTime=f.shieldRecoveryTime=f.shieldFlash=f.hitFlash=0;
            f.weapons=NewWeaponStates(f.weaponProfiles);f.aiWeaponReadyAt=0;f.weaponSlot=1; f.magazine=f.weaponProfiles[0].capacity; f.reloadTime=0; f.shotIdleTime=0;
            f.pulseTime=f.lockTime=f.rushTime=f.aiClock=f.aiReaction=f.stuckTime=f.orbitTime=0;
            f.assistTrackedTarget=null;f.quadPose=null;
            f.velocity=f.aiMove=f.smoothedTargetVelocity=Vector3.zero;f.aiCoverRouting=false;f.aiCoverRetry=0;
            f.boostHeld=f.wasBoostHeld=f.boostLocked=f.boosting=f.rushHit=f.jumpHeld=false;
            f.jumpHeight=f.jumpVelocity=f.jumpOriginY=f.jumpHoldElapsed=0; f.jumpChargeCanceled=false; f.crouching=false; f.failedFireFlash=0;
            f.guarding=false; f.guardBlend=0;f.handPoseInitialized=false;f.punchTimer=0;f.punchSide=0;
            f.knockdownTime=f.invulnerableTime=0;
            f.dashRemaining=f.dashCooldownLeft=0;
            f.dashDirection=Vector3.zero;
            f.dashSpeedFactor=Mathf.Clamp(f.spec.speed/13f,.7f,1.25f);
            f.dashDistanceFactor=Mathf.Clamp(f.spec.speed/13f,.75f,1.2f)*Mathf.Max(.1f,f.spec.dashRangeScale);
            f.dashCostFactor=1f; // No mass-derived mobility modifiers in test baseline.
            f.orbitSign=1;
        }
        Vector3 playerSpawn=ArenaSpawnPosition(0),enemySpawn=ArenaSpawnPosition(1);
        player.root.position=playerSpawn;enemy.root.position=enemySpawn;
        Dictionary<int,int> teamSpawnCounts=new Dictionary<int,int>{{0,1},{1,1}};
        foreach(Fighter fighter in fighterRoster)
        {
            if(fighter==player||fighter==enemy)continue;
            int ordinal=teamSpawnCounts.TryGetValue(fighter.teamId,out int count)?count:0;
            Vector3 anchor=fighter.teamId==0?playerSpawn:enemySpawn;
            float angle=(ordinal-1)*2.3999632f+(fighter.teamId==0?0:Mathf.PI);
            float radius=8f+Mathf.Max(0,ordinal-1)*3f;
            fighter.root.position=anchor+new Vector3(Mathf.Cos(angle)*radius,0,Mathf.Sin(angle)*radius);
            teamSpawnCounts[fighter.teamId]=ordinal+1;
        }
        foreach(Fighter f in fighterRoster)
        {
            Fighter target=Opponent(f);
            if(target!=null)FaceOpponent(f,target);
        }
        // FaceOpponent changes aiming on round reset; initialize the independent
        // chassis heading only here, never on subsequent aim/lock updates.
        foreach(Fighter f in fighterRoster)
        {
            Vector3 heading=Vector3.ProjectOnPlane(f.facing,Vector3.up);
            f.chassisForward=heading.sqrMagnitude>.001f?heading.normalized:Vector3.forward;ResetCapsuleTreads(f);
        }
        cameraYaw=Mathf.Atan2(player.facing.x,player.facing.z)*Mathf.Rad2Deg; cameraPitch=spectatorMode?42:DefaultThirdPersonPitch; targetLock=false;
        foreach(Fighter f in fighterRoster)f.lastPosition=f.root.position;
        roundTime=0; paused=false; spaceClock=0; cameraShakeTime=cameraShakeStrength=0; UpdateObstacles(0);
        AnimateRoster();UpdateCamera(10);
    }
    void Update()
    {
        if(player==null || enemy==null) return;
        if(!Application.isFocused||Cursor.lockState!=CursorLockMode.Locked)ResetMouseLookCapture();
        UpdatePDVisualEffects(Time.deltaTime);UpdateClimate(Time.deltaTime);
        if(captureGameplayPreview) return;
        if(Input.GetKeyDown(KeyCode.F1)) { ToggleGuide(); return; }
        if(Input.GetKeyDown(KeyCode.F2) && !inMenu && !smoke) { testLabOpen=!testLabOpen; SetTestCursor(); }
        if(testLabOpen) { UpdateTestLabFrame(); return; }
        if(showGuide) { if(Input.GetKeyDown(KeyCode.Escape)) CloseGuide(); return; }
        // V is reserved for the later approved view selector; no mute hotkey.
        if(inMenu && balanceEditorOpen){if(Input.GetKeyDown(KeyCode.Escape))balanceEditorOpen=false;return;}
        if(inMenu && Input.GetKeyDown(KeyCode.F3)) { weaponGalleryOpen=!weaponGalleryOpen; if(weaponGalleryOpen) ShowWeaponPreview(weaponPreviewIndex); }
        if(inMenu && weaponGalleryOpen) { if(Input.GetKeyDown(KeyCode.Escape)) CloseWeaponGallery(); return; }
        if(inMenu) { if(Input.GetKeyDown(KeyCode.Escape)) showTuning=false; if(Input.GetKeyDown(KeyCode.Return)&&!showTuning) BeginMatch(); return; }
        if(Input.GetKeyDown(KeyCode.M)) { ReturnToHangar(); return; }
        if(Input.GetKeyDown(KeyCode.Escape)) SetPaused(!paused);
        if(Input.GetKeyDown(KeyCode.R))
        {
            if(Finished) BeginMatch();
            else if(!paused&&player.magazine<30&&player.reloadTime<=0) StartWeaponReload(player);
        }
        if(Finished) { FinishPresentation(); return; }
        if(paused || smoke) return;
        float dt=Mathf.Min(Time.deltaTime,.05f);
        roundTime+=dt; hitConfirmTime=Mathf.Max(0,hitConfirmTime-dt); UpdateGravityEvent(dt);UpdateObstacles(dt); ProcessTimedFrag(dt);
        if(!spectatorMode&&!player.deadVisual&&!playerRespawnPrompt) PlayerInput(dt);
        UpdateRosterAI(dt); FaceRosterTargets(dt);
        UpdateCombat(dt); UpdateSalvage(dt); UpdateSparks(dt); AnimateRoster();
        if(Finished) FinishPresentation();
    }
    void UpdateRosterAI(float dt)
    {
        if(testLabEnabled&&!smoke&&!aiMoveEnabled&&!aiAttackEnabled&&!aiJumpEnabled)return;
        foreach(Fighter actor in fighterRoster)
        {
            if(actor==player||actor==null||actor.deadVisual)continue;
            Fighter target=Opponent(actor);
            if(target!=null&&!target.deadVisual)EnemyAI(actor,target,dt);
        }
        if(spectatorMode&&!player.deadVisual)
        {
            Fighter target=Opponent(player);
            if(target!=null&&!target.deadVisual)EnemyAI(player,target,dt);
        }
    }
    void FaceRosterTargets(float dt)
    {
        foreach(Fighter actor in fighterRoster)
        {
            if(actor==null||actor.deadVisual||actor==player&&!targetLock)continue;
            Fighter target=actor==player&&IsHostile(player,enemy)&&!enemy.deadVisual?enemy:Opponent(actor);
            if(target!=null&&!target.deadVisual)FaceOpponent(actor,target,dt);
        }
    }
    void AnimateRoster()
    {
        foreach(Fighter actor in fighterRoster)if(actor!=null&&!actor.deadVisual&&actor.body!=null)Animate(actor);
    }
    void LateUpdate() { FlushAutomaticBalance();if(player!=null && enemy!=null) UpdateCamera(Time.deltaTime); }
    void UpdateCamera(float dt)
    {
        if(paused&&!inMenu&&!smoke&&!spectatorMode)return;
        if(multiAiPerformanceMode&&!inMenu)
        {
            Quaternion loadView=Quaternion.Euler(48,35,0);
            view.orthographic=true;view.orthographicSize=29f;
            view.transform.position=-(loadView*Vector3.forward)*82f;
            view.transform.LookAt(Vector3.zero);
            return;
        }
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
            if(starRoot!=null&&arenaMap!=ArenaMapKind.OpenPlains)starRoot.position=Vector3.zero;
            return;
        }
        if(!inMenu&&!paused&&!smoke)
        {
            UpdateThirdPersonCamera(dt);
            return;
        }
        view.orthographic=true;
        Vector3 center=(player.root.position+enemy.root.position)*.5f, separation=player.root.position-enemy.root.position;
        float zoom=Mathf.Max(12.5f,(Mathf.Abs(Vector3.Dot(separation,view.transform.up))*.5f+5)/.68f,
            (Mathf.Abs(Vector3.Dot(separation,view.transform.right))*.5f+7)/view.aspect);
        float blend=1-Mathf.Exp(-7*dt);
        view.orthographicSize=Mathf.Lerp(view.orthographicSize,zoom,blend);
        view.transform.position=Vector3.Lerp(view.transform.position,center+new Vector3(46,58,-46),blend);
        view.transform.LookAt(center);
        if(starRoot!=null&&arenaMap!=ArenaMapKind.OpenPlains)starRoot.position=Vector3.zero;
    }
    Vector3 ConstrainCamera(Vector3 focus,Vector3 candidate,float radius)
    {
        candidate.y=Mathf.Max(candidate.y,SurfaceHeight(candidate)+radius);
        Vector3 delta=candidate-focus; float length=delta.magnitude;
        if(length<.001f) return candidate;
        Ray ray=new Ray(focus,delta/length); float nearest=length+.08f;
        // Camera collision uses the full 3D volumes, including roofs and perimeter
        // geometry. Gameplay's rooftop ray exemptions do not apply to the camera.
        for(int group=0;group<2;group++)
        foreach(Bounds solid in group==0?obstacles:cameraSolids)
        {
            Bounds expanded=solid; expanded.Expand(radius*2f);
            if(expanded.Contains(focus)) nearest=.01f;
            else if(expanded.IntersectRay(ray,out float hit)&&hit<=length) nearest=Mathf.Min(nearest,hit);
        }
        Vector3 constrained=focus+delta/length*Mathf.Min(length,Mathf.Max(.01f,nearest-.08f));
        constrained.y=Mathf.Max(constrained.y,SurfaceHeight(constrained)+radius);return constrained;
    }
    bool CanStartLockOn(Fighter fighter)
    {
        return fighter!=null && fighter.spec!=null && fighter.weaponProfiles!=null && !HasEquippedGuard(fighter) && !EquippedProfile(fighter).isShield && !EquippedProfile(fighter).isUnarmed && HasWeaponTrait(EquippedProfile(fighter),"LOCK");
    }
    bool TryAcquireTargetLock(Fighter fighter,Fighter target)
    {
        float lockRange=WeatherLockRange(fighter);
        if(!CanStartLockOn(fighter) || target==null ||
            Vector3.Distance(fighter.root.position,target.root.position)>lockRange)
        {
            targetLock=false;
            return false;
        }
        if(!smoke&&fighter==player)
        {
            Vector3 eye=LockAimPoint(target);Vector3 p=view.WorldToViewportPoint(eye);
            if(p.z<=0||p.x<=0||p.x>=1||p.y<=0||p.y>=1||ObstacleOnSegment(fighter.root.position+Vector3.up*1.5f,eye,.15f,out _)){targetLock=false;return false;}
        }
        if(!targetLock)lockViewInitialized=false;
        targetLock=true;
        occludedLockTimer=fighter.spec.occludedLockSeconds;
        return true;
    }
    bool UpdateHeldManualLock(Fighter fighter,Fighter target,bool buttonHeld,bool lockAvailable)
    {
        if(!lockAvailable||!buttonHeld){targetLock=false;return false;}
        if(!targetLock)TryAcquireTargetLock(fighter,target);
        if(targetLock&&Vector3.Distance(fighter.root.position,target.root.position)>WeatherLockRange(fighter))targetLock=false;
        return targetLock;
    }
    bool CanFireSpecialWithCurrentLock(Fighter fighter)
    {
        return fighter!=null && fighter.spec!=null &&
            (!RequiresHomingLock(fighter.spec.special) || targetLock || !CanStartLockOn(fighter));
    }
    void PlayerInput(float dt)
    {
        bool wispHands=player.spec.name=="WISP";
        player.guarding=HasEquippedGuard(player)&&Input.GetMouseButton(1);
        for(int slot=0;slot<4;slot++)if(Input.GetKeyDown((KeyCode)((int)KeyCode.Alpha1+slot)))SelectWeaponOrdinal(player,slot);
        player.crouching=Input.GetKey(KeyCode.F);
        if(Input.GetKeyDown(KeyCode.F)) TryPickupSalvage();
        if(Input.GetKeyDown(KeyCode.G)) UseSalvage(aimDirection);
        Vector3 forward=Vector3.ProjectOnPlane(view.transform.forward,Vector3.up).normalized;
        Vector3 right=Vector3.ProjectOnPlane(view.transform.right,Vector3.up).normalized;
        float x=(Input.GetKey(KeyCode.D)?1:0)-(Input.GetKey(KeyCode.A)?1:0);
        float y=(Input.GetKey(KeyCode.W)?1:0)-(Input.GetKey(KeyCode.S)?1:0);
        Vector3 move=CameraRelativeMove(forward,right,x,y,wispHands);
        player.boostHeld=Input.GetKey(KeyCode.LeftShift)||Input.GetKey(KeyCode.RightShift);
        // Runner prototype: forward Shift stays a ground sprint; sideways/backward
        // Shift starts a short, low-hop distance dash. Legacy frames retain double-tap.
        bool boostPressed=Input.GetKeyDown(KeyCode.LeftShift)||Input.GetKeyDown(KeyCode.RightShift);
        if((player.spec.name=="WISP"||player.spec.usePrototypeMovement && player.spec.mobility==MobilityKind.Runner) &&
           boostPressed && move.sqrMagnitude>.01f && player.jumpHeight<=.02f)
        {
            float forwardIntent=Vector3.Dot(move.normalized,forward);
            if(forwardIntent<.5f && TryDash(player,move))
            {
                player.jumpOriginY=player.root.position.y;
                if(player.spec.name!="WISP")player.jumpVelocity=Mathf.Max(player.jumpVelocity,jumpSpeed*.23f);
            }
        }
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
        if(player.spec.chargedReleaseJump)
        {
            bool grounded=player.jumpHeight<=.02f && player.jumpVelocity<=0f;
            bool canCharge=grounded && player.knockdownTime<=0f && player.lockTime<=0f;
            // Shift cancels an ongoing charge, and cancellation remains latched
            // until Space is released so a held key cannot silently restart it.
            if(boostPressed && player.jumpHeld)
            {
                player.jumpHoldElapsed=0f;
                player.jumpChargeCanceled=true;
            }
            if(player.jumpHeld && canCharge && !player.jumpChargeCanceled)
                player.jumpHoldElapsed=Mathf.Min(player.spec.jumpChargeSeconds,player.jumpHoldElapsed+dt);
            else if(Input.GetKeyUp(KeyCode.Space))
            {
                if(canCharge && !player.jumpChargeCanceled && player.jumpHoldElapsed>0f)
                {
                    float charge=Mathf.Clamp01(player.jumpHoldElapsed/Mathf.Max(.01f,player.spec.jumpChargeSeconds));
                    float power=Mathf.Lerp(player.spec.jumpMinImpulseFactor,player.spec.jumpImpulseScale,charge);
                    player.jumpOriginY=player.root.position.y;
                    player.jumpVelocity=JumpLaunchSpeed(player,power);
                }
                player.jumpHoldElapsed=0f;
                player.jumpChargeCanceled=false;
            }
            else if(!canCharge) player.jumpHoldElapsed=0f;
        }
        else if(Input.GetKeyDown(KeyCode.Space)&&player.jumpHeight<=.02f && player.knockdownTime<=0f && player.lockTime<=0f)
        {
            player.jumpOriginY=player.root.position.y;
            player.jumpHoldElapsed=0f;
            player.jumpVelocity=JumpLaunchSpeed(player,player.spec.jumpImpulseScale);
        }
        bool lockOnAvailable=CanStartLockOn(player);
        UpdateHeldManualLock(player,enemy,Input.GetMouseButton(1),lockOnAvailable);
        if(wasInputTargetLock!=targetLock){ResetMouseLookCapture();wasInputTargetLock=targetLock;}
        if(!targetLock)
        {
            ApplyMouseLook(Input.GetAxis("Mouse X"),Input.GetAxis("Mouse Y"),
                Application.isFocused&&Cursor.lockState==CursorLockMode.Locked);
        }
        UpdatePlayerHeading(player,move,forward,targetLock,lockOnAvailable,dt);
        if(targetLock&&CanStartLockOn(player))move*=1-Mathf.Clamp01(EquippedProfile(player).lockPenalty);
        Move(player,move,dt);
        if(targetLock)
        {
            Vector3 viewport=view.WorldToViewportPoint(LockAimPoint(enemy));
            bool visible=viewport.z>0&&viewport.x>0&&viewport.x<1&&viewport.y>0&&viewport.y<1&&
                !ObstacleOnSegment(player.root.position+Vector3.up*1.5f,LockAimPoint(enemy),.15f,out _);
            if(visible) occludedLockTimer=player.spec.occludedLockSeconds;
            else
            {
                occludedLockTimer-=dt;
                if(Vector3.Distance(player.root.position,enemy.root.position)>player.spec.occludedLockRange || occludedLockTimer<=0)
                    targetLock=false;
            }
        }
        aimDirection=targetLock?(LockAimPoint(enemy)-WeaponMuzzle(player)).normalized:player.facing;
        Ray ray=view.ScreenPointToRay(new Vector3(Screen.width*.5f,Screen.height*.5f,0));
        Vector3 reticleTarget=ReticleAimPoint(ray);aimMarker.position=reticleTarget;
        if(!targetLock)aimDirection=(reticleTarget-WeaponMuzzle(player)).normalized;
        assistEngaged=!smoke&&CanAssist(player,enemy,aimDirection);
        UpdateAssistFeedback(dt); // Correction is applied only when a round actually fires.
        canShoot=InArc(player,aimDirection);
        foreach(Renderer r in aimMarker.GetComponentsInChildren<Renderer>()) r.sharedMaterial=Glow(canShoot?Cyan:Orange);
        if(Input.GetMouseButton(0))
        {
            if(player.reloadTime>0||player.magazine<=0)player.failedFireFlash=.2f;
            Attack(player,enemy,aimDirection);
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
    void UpdatePlayerHeading(Fighter f,Vector3 move,Vector3 cameraForward,bool locked,bool lockAvailable,float dt)
    {
        if(locked) FaceOpponent(f,enemy,dt);
        else FaceMovement(f,!f.spec.supportsLockOn?cameraForward:move,dt);
    }
    Vector3 CameraRelativeMove(Vector3 cameraForward,Vector3 cameraRight,float horizontal,float vertical,bool runner=false)
    {
        Vector3 forward=Vector3.ProjectOnPlane(cameraForward,Vector3.up).normalized;
        Vector3 right=Vector3.ProjectOnPlane(cameraRight,Vector3.up).normalized;
        return Vector3.ClampMagnitude(right*horizontal*(runner&&vertical>0?.3f:1f)+forward*vertical,1f);
    }
    bool TryDash(Fighter f,Vector3 direction)
    {
        // TARANTULA has no booster: reject double-tap/Shift dash at the shared entry point.
        if(!f.spec.allowHorizontalBoost||f.jumpHeight>.02f||f.jumpVelocity>0) return false;
        if(direction.sqrMagnitude<.01f || f.dashRemaining>0 || f.dashCooldownLeft>0 || f.boostLocked || f.lockTime>0) return false;
        // Runner ground dodges use cooldown/distance, not boost fuel.
        // Airborne dodges still consume the shared gauge.
        bool freeGroundDash=f.spec.name!="WISP"&&f.spec.mobility==MobilityKind.Runner && f.jumpHeight<=.02f && f.jumpVelocity<=0f;
        float cost=freeGroundDash?0f:dashFuelCost*f.dashCostFactor;
        if(f.boostEnergy<cost) return false;
        f.boostEnergy-=cost;
        if(cost>0f) f.recoveryTime=recoveryDelay;
        f.dashDirection=direction.normalized;
        f.dashRemaining=Mathf.Max(.01f,dashDistance*f.dashDistanceFactor);
        f.dashCooldownLeft=f.spec.name=="WISP"?.18f:dashCooldown;
        PlaySound(SoundKind.Boost,f.root.position,.6f);
        return true;
    }
    bool ShouldApplyLandingStagger(Fighter f,float takeoffHeight,float landingHeight,float fallSpeed)
        =>landingStaggerEnabled&&f.spec.name!="WISP"&&takeoffHeight>.05f&&landingHeight<=.001f&&fallSpeed>15f;
    float BoostDirectionFactor(Fighter f,Vector3 direction)
    {
        if(f.spec.name=="WISP") return 1f;
        // Prototype tank boost is direction-neutral; legacy frames keep target bias.
        if(f.spec.usePrototypeMovement && f.spec.mobility==MobilityKind.Tank) return 1f;
        Fighter target=Opponent(f);
        if(target==null) return 1f;
        Vector3 toward=(target.root.position-f.root.position).normalized;
        float dot=Vector3.Dot(direction.normalized,toward);
        return dot>=0?1+.18f*dot:1+.12f*dot;
    }
    void Move(Fighter f,Vector3 direction,float dt)
    {
        TickWeapons(f,dt);
        float maxJumpHeight=jumpMaxHeight*Mathf.Max(.1f,f.spec.jumpHeightScale);
        f.specialTimer=Mathf.Max(0,f.specialTimer-dt);
        f.dashCooldownLeft=Mathf.Max(0,f.dashCooldownLeft-dt);
        f.shieldFlash=Mathf.Max(0,f.shieldFlash-dt); f.hitFlash=Mathf.Max(0,f.hitFlash-dt);
        f.failedFireFlash=Mathf.Max(0,f.failedFireFlash-dt);
        f.knockdownTime=Mathf.Max(0,f.knockdownTime-dt);
        f.invulnerableTime=Mathf.Max(0,f.invulnerableTime-dt);
        float recovery=Mathf.Max(0,dt-f.shieldRecoveryTime);
        f.shieldRecoveryTime=Mathf.Max(0,f.shieldRecoveryTime-dt);
        f.shieldEnergy=Mathf.Min(f.spec.ShieldMax,f.shieldEnergy+f.spec.ShieldMax/Mathf.Max(1f,f.spec.shieldRecoverySeconds)*recovery);
        // Release both boost controls before rearming an exhausted shared gauge.
        if(!f.boostHeld && !f.jumpHeld) f.boostLocked=false;
        if(f.jumpHeight>.02f||f.jumpVelocity>0){f.boostHeld=false;f.pulseTime=0;f.dashRemaining=0;}
        bool wasBoosting=f.boosting;
        if(f.spec.allowHorizontalBoost && f.spec.boost==BoostKind.Pulse && !f.spec.usePrototypeMovement && f.boostHeld && !f.wasBoostHeld && f.lockTime<=0 &&
            f.boostEnergy>=f.spec.boostDrain*f.spec.pulseDuration && direction.sqrMagnitude>.01f)
        { f.pulseTime=f.spec.pulseDuration; f.pulseDirection=direction.normalized; }
        f.wasBoostHeld=f.boostHeld;
        bool dashing=f.dashRemaining>0;
        bool wants=f.spec.allowHorizontalBoost&&!dashing&&f.boostHeld&&direction.sqrMagnitude>.01f;
        // WISP prototype: Shift is a sprint only while moving forward relative
        // to the camera-aligned input. Side/reverse Shift uses the low-hop dash.
        if((f.spec.name=="WISP"||f.spec.usePrototypeMovement && f.spec.mobility==MobilityKind.Runner) && wants)
        {
            Vector3 horizontalFacing=Vector3.ProjectOnPlane(f.facing,Vector3.up).normalized;
            if(horizontalFacing.sqrMagnitude>.001f &&
               Vector3.Dot(direction.normalized,horizontalFacing)<.5f) wants=false;
        }
        f.boosting=wants&&!f.boostLocked&&f.lockTime<=0;
        Vector3 motion=Vector3.ClampMagnitude(direction,1);
        float speed=f.spec.usePrototypeMovement?f.spec.prototypeBaseSpeed:f.spec.speed;
        if(f.boosting)
        {
            float active=dt;
            bool forwardRun=f.spec.name=="WISP"&&Vector3.Dot(motion.normalized,f.facing)>.8f;
            float required=f.spec.boostDrain*active*(forwardRun?.5f:1f);
            float fraction=required>0?Mathf.Min(1,f.boostEnergy/required):1;
            speed=Mathf.Lerp(speed,(forwardRun?f.spec.prototypeBaseSpeed*1.9f:f.spec.boostSpeed)*BoostDirectionFactor(f,motion),fraction*active/dt);
            f.boostEnergy=Mathf.Max(0,f.boostEnergy-required); f.recoveryTime=recoveryDelay;
            if(f.boostEnergy<=0) { f.boostLocked=true; f.boosting=false; }
            if(!wasBoosting) PlaySound(SoundKind.Boost,f.root.position,.45f);
        }
        // Vertical thrust shares fuel with horizontal boost; larger/heavier frames cost more.
        if(f.jumpHeight<=.02f) f.jumpOriginY=f.root.position.y;
        bool airborne=f.jumpHeight>0 || f.jumpVelocity>0;
        bool verticalBoost=f.spec.allowVerticalBoost && f.spec.verticalBoostScale>0 && boostClimbSpeed>0 && airborne && f.jumpHeld && !f.boostLocked && f.boostEnergy>0;
        if(verticalBoost)
        {
            float weightFactor=Mathf.Max(.5f,f.spec.jumpWeightFactor); // Explicit per-frame tuning, independent of armor and visual scale.
            float drain=jumpBoostDrain*weightFactor*dt;
            float fraction=drain>0?Mathf.Min(1,f.boostEnergy/drain):1;
            float climbSpeed=Mathf.Max(0,boostClimbSpeed)*f.spec.verticalBoostScale/weightFactor*fraction*EnvironmentMobilityScale;
            f.jumpVelocity=Mathf.MoveTowards(f.jumpVelocity,climbSpeed,boostClimbAcceleration*dt);
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
        // An unpowered jump keeps its frame-specific arc. Powered boost instead
        // regulates a slow climb and spends the shared gauge, with no altitude ceiling.
        if(!verticalBoost)
        {
            float gravityScale=f.jumpVelocity<0?fallGravityMultiplier:1f;
            float weightGravity=Mathf.Sqrt(FallWeightFactor(f));
            f.jumpVelocity-=jumpGravity*gravityScale*f.spec.fallGravityFactor*weightGravity*EnvironmentGravityScale(f)*dt;
        }
        speed*=EnvironmentMobilityScale;
        Vector3 desired=motion*speed;
        if(dashing)
        {
            float dashStep=Mathf.Min(f.dashRemaining,Mathf.Max(.01f,dashSpeed*f.dashSpeedFactor)*EnvironmentMobilityScale*dt);
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
            if(f.spec.name=="WISP")
            {
                Vector3 side=Vector3.Cross(Vector3.up,facing);
                desired=f.boosting?motion.normalized*speed:
                    (facing*(forward>=0?forward*f.spec.forwardSpeedFactor:forward*f.spec.reverseSpeedFactor)+
                    side*lateral*(lateral<0?f.spec.leftSpeedFactor:f.spec.rightSpeedFactor))*speed;
            }
            else desired=motion.normalized*speed*ratio;
        }
        // Tank prototype favors forward travel; lateral/reverse movement remains possible.
        // No visual track or independent turret is implied by this movement-only tuning.
        if(f.spec.mobility==MobilityKind.Tank && !f.spec.usePrototypeMovement && desired.sqrMagnitude>.001f)
        {
            Vector3 heading=Vector3.ProjectOnPlane(f.facing,Vector3.up).normalized;
            float forward=Vector3.Dot(desired.normalized,heading);
            desired*=forward>.5f?1.3f:forward<-.5f?.55f:.7f;
        }
        if(!smoke&&!f.boosting&&f.dashRemaining<=0)desired*=normalTravelScale;
        desired*=ShieldMovementMultiplier(f);
        desired+=Vector3.left*CoastalWaveShoreSpeed(f);
        if(f.crouching) desired*=.4f;
        // Opt-in tank chassis: choose the shorter forward/reverse turn. Opposite input
        // reverses immediately, while oblique input rotates at a finite angular rate.
        // This is movement-only: the visual track/turret hierarchy is not yet separated.
        if(f.spec.usePrototypeMovement && f.spec.mobility==MobilityKind.Tank && !dashing
            && desired.sqrMagnitude>.001f)
        {
            Vector3 chassis=Vector3.ProjectOnPlane(f.chassisForward,Vector3.up);
            if(chassis.sqrMagnitude<.001f) chassis=Vector3.forward;
            chassis.Normalize();
            Vector3 request=desired.normalized;
            bool reverse=Vector3.Dot(chassis,request)<0f;
            Vector3 target=reverse?-request:request;
            float angle=Vector3.Angle(chassis,target);
            f.chassisForward=Vector3.RotateTowards(chassis,target,120f*Mathf.Deg2Rad*dt,0f).normalized;
            // Stationary pivot until aligned: prevents unrealistic sideways sliding.
            float alignment=Mathf.Clamp01(1f-angle/90f);
            desired=f.chassisForward*(reverse?-1f:1f)*desired.magnitude*alignment;
        }
        // Tank handling prototype: changing direction bleeds momentum, then ramps back.
        // Applies only to opt-in rigs; existing standard frames keep their response.
        if(f.spec.usePrototypeMovement && f.spec.turnAcceleration>0f && !dashing)
        {
            Vector3 previous=Vector3.ProjectOnPlane(f.velocity,Vector3.up);
            desired=Vector3.MoveTowards(previous,desired,speed*f.spec.turnAcceleration*dt);
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
        Vector3 before=f.root.position; MoveWithObstacles(f,desired*dt+Vector3.up*f.jumpVelocity*dt,verticalBoost);
        f.jumpHeight=Mathf.Max(0,f.root.position.y-LandingHeight(f.root.position,f.Radius,f.root.position.y));
        // High-speed landing briefly staggers the legs; unlike weapon knockdown it grants no invulnerability.
        if(ShouldApplyLandingStagger(f,preLandingHeight,f.jumpHeight,preLandingFallSpeed))
        {
            float severity=Mathf.Clamp01((preLandingFallSpeed-15f)/20f);
            float landingRecovery=Mathf.Max(.2f,f.spec.recoverySpeed);
            f.knockdownTime=Mathf.Max(f.knockdownTime,(.2f+.55f*severity)*f.spec.landingStaggerScale/landingRecovery);
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
    void Hit(Fighter target,Vector3 source,float damage,Vector3? impactPoint=null,Fighter attacker=null)
    {
        if(Finished || target.deadVisual || target.hp<=0 || target.invulnerableTime>0) return;
        damage*=1-GuardReduction(target,source);
        if(AbsorbSalvageShield(target,damage)) return;
        float before=target.hp+target.shieldEnergy;
        float incoming=damage*(1-target.spec.armor), absorbed=Mathf.Min(target.shieldEnergy,incoming);
        target.shieldEnergy-=absorbed; target.hp=Mathf.Max(0,target.hp-incoming+absorbed);
        float actualDamage=before-target.hp-target.shieldEnergy;
        if(attacker!=null) attacker.duelDamageDealt+=actualDamage;
        RecordDamage(target,actualDamage,absorbed>0);
        if(actualDamage>absorbed)NotifyPartDamage(target,impactPoint??target.root.position,actualDamage-absorbed);
        if(target==player && actualDamage>0)
        {
            cameraShakeTime=.3f;
            cameraShakeStrength=Mathf.Clamp(actualDamage/110f,.04f,.32f);
        }
        target.hitFlash=.14f; target.shieldRecoveryTime=shieldRecoveryDelay; target.shieldFlash=absorbed>0?.12f:0;
        Burst(target.root.position,absorbed>0?Cyan:Orange,6);
        Flash(target.root.position,absorbed>0?Cyan:new Color(1,.65f,.22f),.85f,.12f);
        PlaySound(absorbed>0?SoundKind.Shield:SoundKind.Hit,target.root.position,.65f);
        if(Finished&&!timedFragMode) { if(MatchWinnerTeam()==player.teamId) wins++; else losses++; } if(timedFragMode&&target.hp<=0) RegisterTimedFragDeath(attacker,target);
    }
    bool InArc(Fighter f,Vector3 direction)=>direction.sqrMagnitude>.01f&&Vector3.Angle(f.facing,direction)<=gunHalfAngle;
    void Burst(Vector3 p,Color color,int count)
    {
        for(int i=0;i<count;i++)
        {
            Transform visual=AcquirePDPart("SparkChunk",p,Vector3.one*.18f,color);if(visual==null)break;
            sparks.Add(new Spark{visual=visual,velocity=UnityEngine.Random.onUnitSphere*6,life=.35f});
        }
    }
    void UpdateSparks(float dt)
    {
        for(int i=sparks.Count-1;i>=0;i--)
        {
            Spark s=sparks[i]; s.life-=dt; s.visual.position+=s.velocity*dt;
            s.visual.localScale=Vector3.one*Mathf.Max(0,s.life*.6f);
            if(s.life<=0) { ReleasePDPart(s.visual); sparks.RemoveAt(i); }
        }
    }
    void Animate(Fighter f)
    {
        if(f.deadVisual)return;
        AnimateBoostVisual(f,Time.deltaTime);
        UpdateMechanicalAudio(f,Time.deltaTime);
        f.root.rotation=Quaternion.LookRotation(f.facing);
        float jumpChargePose=f.spec.chargedReleaseJump && f.jumpHeld && f.jumpHeight<=.02f ? Mathf.Clamp01(f.jumpHoldElapsed/Mathf.Max(.01f,f.spec.jumpChargeSeconds)):0f;
        bool importedWisp=f.spec.name=="WISP"&&wispModelSource!=null||f.importedChassis;
        float neutralBodyHeight=importedWisp?0f:.40f;
        float groundedBob=importedWisp&&f.jumpHeight<=.02f&&f.jumpVelocity<=0f?0f:Mathf.Sin(roundTime*2.4f)*.06f;
        f.body.localPosition=Vector3.up*(f.knockdownTime>0?-.38f:f.crouching?-.22f:neutralBodyHeight-jumpChargePose*.34f)+Vector3.up*groundedBob;
        if(f.shadow!=null) { f.shadow.position=new Vector3(f.root.position.x,SurfaceHeight(f.root.position)+.06f,f.root.position.z); f.shadow.localScale=new Vector3(2.7f*f.spec.size,.012f,2.1f*f.spec.size); }
        Vector3 localVelocity=f.root.InverseTransformDirection(f.velocity);
        f.body.localRotation=f.knockdownTime>0?Quaternion.Euler(68,0,0):Quaternion.Euler(Mathf.Clamp(localVelocity.z*.65f,-16,16)+jumpChargePose*28f,0,Mathf.Clamp(-localVelocity.x*.5f,-12,12));
        // BASTION's tracks follow the chassis yaw, while the root/core/weapon
        // continue to follow the independently aimed facing vector.
        // Use the body's world rotation so lean/knockdown cannot tilt the track deck.
        if(f.spec.mobility==MobilityKind.Tank && f.legsPart!=null)
        {
            Vector3 chassis=Vector3.ProjectOnPlane(f.chassisForward,Vector3.up);
            if(chassis.sqrMagnitude>.0001f)
                f.legsPart.localRotation=Quaternion.Inverse(f.body.rotation)*Quaternion.LookRotation(chassis.normalized,Vector3.up);
            if(f.capsuleTreads)f.legsPart.position=f.root.TransformPoint(f.importedChassisAnchor);
        }
        else if(f.legsPart!=null) f.legsPart.localRotation=Quaternion.identity;
        bool airborneLegPose=f.jumpHeight>.08f||f.jumpVelocity>1f;
        float stride=airborneLegPose?0f:Mathf.Clamp01(Vector3.ProjectOnPlane(f.velocity,Vector3.up).magnitude/Mathf.Max(1f,f.spec.speed));
        float gait=Mathf.Sin(roundTime*(7f+stride*9f))*stride;
        bool riggedWisp=importedWisp&&IsArticulatedWisp;
        f.leftLeg.localRotation=Quaternion.Euler(gait*25f,0,riggedWisp?0:-5);
        f.rightLeg.localRotation=Quaternion.Euler(-gait*25f,0,riggedWisp?0:5);
        AnimateLegJoints(f,stride,gait);
        if(riggedWisp&&f.jumpHeight<=.08f)
        {
            float strafe=Mathf.Clamp(localVelocity.x/Mathf.Max(1f,f.spec.prototypeBaseSpeed),-1f,1f);
            f.leftLeg.localRotation=Quaternion.Euler(gait*10f,0,Mathf.Sin(roundTime*16f)*strafe*23f);
            f.rightLeg.localRotation=Quaternion.Euler(-gait*10f,0,-Mathf.Sin(roundTime*16f)*strafe*23f);
        }
        if(f.importedChassis)AnimateImportedChassis(f,stride,Time.deltaTime);
        else AnimateTreads(f,Time.deltaTime);
        f.leftArm.localRotation=Quaternion.Euler(-gait*13f,0,0);
        f.rightArm.localRotation=Quaternion.Euler(gait*13f,0,0);
        if(f.guardShield!=null)f.guardShield.gameObject.SetActive(UsesNativeGuardShield(f));
        f.guardBlend=Mathf.MoveTowards(f.guardBlend,HasEquippedGuard(f)&&f.guarding&&!inMenu&&!Finished?1f:0f,Mathf.Min(Time.deltaTime,.05f)*8f);
        if(importedWisp&&UsesNativeGuardShield(f)&&f.guardShield!=null&&f.guardElbow!=null)
        {
            float guardTarget=HasEquippedGuard(f)&&f.guarding&&!inMenu&&!Finished?1f:0f;
            f.guardBlend=Mathf.MoveTowards(f.guardBlend,guardTarget,Mathf.Min(Time.deltaTime,.05f)*8f);
            f.rightArm.localRotation=Quaternion.Slerp(f.rightArm.localRotation,Quaternion.Euler(-55,-55,-5),f.guardBlend);
            f.guardElbow.localRotation=Quaternion.Euler(-30f*f.guardBlend,0,0);
            f.guardShield.localRotation=Quaternion.identity;
            f.guardShield.rotation=Quaternion.Slerp(f.guardShield.rotation,f.root.rotation,f.guardBlend);
        }
        AnimateArmGrips(f);PlaceGuardVisuals(f);UpdateFamilyArt(f);
        f.gun.gameObject.SetActive(f.rushTime<=0f);
        f.shield.gameObject.SetActive(f.shieldFlash>0&&!Finished&&!inMenu);
        bool jumpExhaust=(f.spec.name=="WISP"?f.jumpHeight>.08f&&f.jumpHeld&&f.boostEnergy>0&&!f.boostLocked:f.jumpVelocity>1f)&&!inMenu&&!Finished;
        bool moveExhaust=f.spec.name!="WISP"&&f.boosting&&Vector3.ProjectOnPlane(f.velocity,Vector3.up).sqrMagnitude>1&&!inMenu&&!Finished;
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
        if(balanceFont!=null)Destroy(balanceFont);DisposeWeaponGallery();DisposeClimateMaterials();if(fxSolid!=null)Destroy(fxSolid);if(fxSoft!=null)Destroy(fxSoft);
        if(guideFont!=null) Destroy(guideFont);
        if(wispRuntimeMaterial!=null) Destroy(wispRuntimeMaterial);
        foreach(Material fade in cameraFadeMaterials.Values)if(fade!=null)Destroy(fade);
        foreach(Material material in simpleShellMaterials.Values)if(material!=null)Destroy(material);
        foreach(Mesh mesh in environmentMeshes) if(mesh!=null) Destroy(mesh);
        if(framePortraits!=null) foreach(RenderTexture rt in framePortraits) if(rt!=null) { rt.Release(); Destroy(rt); }
        foreach(Material m in materials.Values) if(m!=null) Destroy(m);
        foreach(AudioClip clip in soundClips.Values) if(clip!=null) Destroy(clip);
    }
}














