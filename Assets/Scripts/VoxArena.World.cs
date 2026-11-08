using System.Collections.Generic;
using UnityEngine;

public sealed partial class VoxArena
{
    // The fighter root is centered near the chest while grounded. Keep the
    // visible floor and obstacle collision bottoms aligned to the same plane.
    public enum ArenaMapKind { IndoorFactory, OutdoorCanyon, OpenPlains, CoastalBay=3 }
    [Header("Arena map (select before Play)")]
    public ArenaMapKind arenaMap=ArenaMapKind.IndoorFactory;
    const float ArenaHalfWidth=140f;
    const float GroundSurfaceY=-.91f;
    const float FighterSupportOffset=2.01f;
    const float FighterGroundY=GroundSurfaceY+FighterSupportOffset;
    sealed class Debris { public Transform visual; public Vector3 origin,size; public float phase,hp; public bool destroyed,gravityRegulator; public List<Transform> cracks=new List<Transform>(); }
    readonly List<Bounds> obstacles=new List<Bounds>();
    readonly List<Bounds> cameraSolids=new List<Bounds>();
    readonly List<Debris> debris=new List<Debris>();
    sealed class CoverFragment
    {
        public Transform visual;
        public Vector3 velocity;
        public float spin,age;
        public bool settled;
    }
    readonly List<CoverFragment> coverFragments=new List<CoverFragment>();
    const int MaxCoverFragments=320;
    Transform starRoot;
    // Terrain blocks share the obstacle model so navigation, shots and landing agree.
    readonly List<Bounds> terrainSteps=new List<Bounds>();
    readonly HashSet<Bounds> terrainStepLookup=new HashSet<Bounds>();
    static readonly int[] CanyonTerrainStepCounts={11,13,10,12,14,11,13,10,12};
    float spaceClock;
    [Header("Foundry gravity regulator")]
    [Range(.2f,1f)] public float foundryMinimumGravity=.55f;
    [Range(0,.5f)] public float foundryMobilityBonus=.2f;
    [Range(1,10)] public float foundryGravityRecoverySeconds=4f;
    float foundryGravityElapsed,foundryGravityDuration,foundryGravityRecovery;bool foundryGravityRegulatorSpent;
    float FoundryGravityEventStrength
    {
        get
        {
            if(foundryGravityDuration<=0)return 0;
            if(foundryGravityElapsed<foundryGravityDuration)return Mathf.SmoothStep(0,1,foundryGravityElapsed/foundryGravityDuration);
            return 1-Mathf.SmoothStep(0,1,foundryGravityRecovery/Mathf.Max(.1f,foundryGravityRecoverySeconds));
        }
    }
    float EnvironmentGravityScale(Fighter fighter)=>Mathf.Lerp(1,foundryMinimumGravity,FoundryGravityEventStrength)*WaterGravityScale(fighter);
    float EnvironmentMobilityScale=>1+foundryMobilityBonus*FoundryGravityEventStrength;
    float JumpLaunchSpeed(Fighter fighter,float impulse)=>jumpSpeed*impulse/Mathf.Sqrt(Mathf.Max(.5f,fighter.spec.jumpWeightFactor))*EnvironmentMobilityScale;
    float FallWeightFactor(Fighter fighter)
    {
        float factor=Mathf.Max(.5f,fighter.spec.jumpWeightFactor);float equipmentKg=0;
        if(fighter.weaponProfiles!=null)for(int i=0;i<fighter.weaponProfiles.Length;i++)
        {
            WeaponProfile profile=fighter.weaponProfiles[i];if(profile==null||profile.isUnarmed)continue;
            int rounds=fighter.weapons!=null&&i<fighter.weapons.Length&&fighter.weapons[i]!=null?Mathf.Max(0,fighter.weapons[i].ammo):Mathf.Max(0,profile.capacity);
            equipmentKg+=Mathf.Max(0,profile.weight)+Mathf.Max(0,profile.ammoWeight)*rounds;
        }
        // The frame factor represents the base 100 kg chassis class; equipped
        // weapon and loaded-ammunition kilograms add directly to that ratio.
        return Mathf.Clamp(factor+equipmentKg/100f,.5f,10f);
    }
    void UpdateGravityEvent(float dt)
    {
        if(foundryGravityDuration<=0)return;
        if(foundryGravityElapsed<foundryGravityDuration)foundryGravityElapsed=Mathf.Min(foundryGravityDuration,foundryGravityElapsed+dt);
        else
        {
            foundryGravityRecovery+=dt;
            if(foundryGravityRecovery>=foundryGravityRecoverySeconds){foundryGravityDuration=0;foundryGravityElapsed=foundryGravityRecovery=0;}
        }
    }
    void StartFoundryGravityEvent()
    {
        if(arenaMap!=ArenaMapKind.IndoorFactory||foundryGravityRegulatorSpent)return;
        foundryGravityRegulatorSpent=true;foundryGravityDuration=UnityEngine.Random.Range(10f,20f);foundryGravityElapsed=foundryGravityRecovery=0;
        Debug.Log("FOUNDRY_GRAVITY_EVENT duration="+foundryGravityDuration.ToString("F1")+"s minGravity="+foundryMinimumGravity.ToString("F2"));
    }

    void BuildWorld()
    {
        RenderSettings.ambientLight=new Color(.55f,.59f,.64f); RenderSettings.fog=false;
        QualitySettings.antiAliasing=4; QualitySettings.shadows=ShadowQuality.All; QualitySettings.shadowDistance=140;
        Light light=new GameObject("Distant sun").AddComponent<Light>();
        light.type=LightType.Directional; light.intensity=1.05f; light.shadows=LightShadows.Soft; light.shadowStrength=.65f;
        light.transform.rotation=Quaternion.Euler(40,-30,0);
        // Warm key and cool rim give the voxel armor readable depth without
        // adding dynamic shadows or touching physics / gameplay systems.
        light.color=new Color(1f,.94f,.85f);
        Light rim=new GameObject("Cool mech rim light").AddComponent<Light>();
        rim.type=LightType.Directional;
        rim.color=new Color(.42f,.69f,1f);
        rim.intensity=.34f;
        rim.shadows=LightShadows.None;
        rim.transform.rotation=Quaternion.Euler(28,145,0);
        RenderSettings.ambientMode=UnityEngine.Rendering.AmbientMode.Trilight;
        RenderSettings.ambientSkyColor=new Color(.39f,.45f,.54f);
        RenderSettings.ambientEquatorColor=new Color(.34f,.39f,.46f);
        RenderSettings.ambientGroundColor=new Color(.12f,.14f,.17f);
        view=new GameObject("Quarter view camera").AddComponent<Camera>(); view.tag="MainCamera";
        view.transform.position=new Vector3(46,58,-46); view.transform.LookAt(Vector3.zero);
        view.cullingMask=~(1<<8); view.orthographic=true; view.orthographicSize=24; view.farClipPlane=600;
        view.backgroundColor=new Color(.009f,.014f,.04f); view.clearFlags=CameraClearFlags.SolidColor;
        view.gameObject.AddComponent<AudioListener>();
        if(arenaMap==ArenaMapKind.OutdoorCanyon)
        {
            view.backgroundColor=new Color(.48f,.70f,.84f);
            light.intensity=1.15f; light.color=new Color(1,.91f,.75f);
            RenderSettings.ambientSkyColor=new Color(.50f,.57f,.65f);
            RenderSettings.ambientEquatorColor=new Color(.38f,.40f,.42f);
            RenderSettings.ambientGroundColor=new Color(.21f,.19f,.16f);
            RenderSettings.fog=true; RenderSettings.fogColor=new Color(.55f,.65f,.69f);
            RenderSettings.fogMode=FogMode.ExponentialSquared; RenderSettings.fogDensity=.0022f;
        }
        else if(arenaMap==ArenaMapKind.CoastalBay)
        {
            view.backgroundColor=new Color(.38f,.65f,.76f);
            light.intensity=1.08f;light.color=new Color(.84f,.94f,1f);
            RenderSettings.ambientSkyColor=new Color(.36f,.58f,.67f);
            RenderSettings.ambientEquatorColor=new Color(.25f,.43f,.49f);
            RenderSettings.ambientGroundColor=new Color(.09f,.19f,.23f);
            RenderSettings.fog=true;RenderSettings.fogColor=new Color(.38f,.63f,.70f);
            RenderSettings.fogMode=FogMode.ExponentialSquared;RenderSettings.fogDensity=.0018f;
        }
        else RenderSettings.fog=false;
        BuildGround();
        BuildVisualEnvironment();
        BuildArenaPerimeter();
        BuildSalvage();
        if(arenaMap==ArenaMapKind.IndoorFactory)
        {
        AddDebris(new Vector3(-16,1.1f,3),new Vector3(5,4.2f,5),0);
        AddDebris(new Vector3(20,1.1f,38),new Vector3(6,3.4f,5),1.4f,true);
        AddDebris(new Vector3(-5,1.1f,-19),new Vector3(4,1.8f,6),2.8f);
        AddDebris(new Vector3(6,1.1f,23),new Vector3(6,4.2f,4),4.1f);
        // Add a close-range cover ring around the central combat lanes while
        // keeping both diagonal spawn approaches open. These are destructible
        // Foundry props and use the same bounds for movement and projectiles.
        AddDebris(new Vector3(-30,1.1f,-7),new Vector3(6,3.0f,5),.7f);
        AddDebris(new Vector3(30,1.1f,7),new Vector3(5,2.4f,7),1.8f);
        AddDebris(new Vector3(-8,1.1f,31),new Vector3(7,3.6f,5),2.9f);
        AddDebris(new Vector3(8,1.1f,-31),new Vector3(5,2.8f,8),4.0f);
        AddDebris(new Vector3(-39,1.1f,17),new Vector3(8,2.2f,5),5.1f);
        AddDebris(new Vector3(39,1.1f,-17),new Vector3(5,4.0f,7),6.2f);
        AddDebris(new Vector3(-23,1.1f,39),new Vector3(6,2.6f,8),1.3f);
        AddDebris(new Vector3(23,1.1f,-39),new Vector3(8,3.2f,5),3.4f);
        AddDebris(new Vector3(-65,1.1f,-52),new Vector3(7,3.4f,6),5.5f);
        AddDebris(new Vector3(66,1.1f,50),new Vector3(6,2.0f,9),.2f);
        AddDebris(new Vector3(-88,1.1f,0),new Vector3(5,3.4f,5),5.4f);
        AddDebris(new Vector3(88,1.1f,-22),new Vector3(4,1.8f,6),6.7f);
        AddDebris(new Vector3(-20,1.1f,78),new Vector3(7,2.6f,4),1.1f);
        AddDebris(new Vector3(24,1.1f,-76),new Vector3(5,3.6f,7),2.2f);
        AddDebris(new Vector3(-80,1.1f,46),new Vector3(8,2.2f,5),3.3f);
        AddDebris(new Vector3(76,1.1f,-70),new Vector3(7,3.0f,5),4.4f);
        AddDebris(new Vector3(-98,1.1f,-42),new Vector3(5,4.0f,7),5.5f);
        AddDebris(new Vector3(98,1.1f,64),new Vector3(8,2.4f,5),6.6f);
        // Carry the cover out toward all four edges without placing it beyond
        // the hangar walls, spreading routes beyond the central combat lanes.
        AddDebris(new Vector3(-112,1.1f,102),new Vector3(8,3.4f,6),.5f);
        AddDebris(new Vector3(112,1.1f,92),new Vector3(6,2.2f,9),1.6f);
        AddDebris(new Vector3(118,1.1f,-54),new Vector3(7,4.0f,5),2.7f);
        AddDebris(new Vector3(-114,1.1f,-82),new Vector3(9,2.6f,7),3.8f);
        AddDebris(new Vector3(12,1.1f,115),new Vector3(7,3.6f,6),4.9f);
        AddDebris(new Vector3(-34,1.1f,-116),new Vector3(6,2.4f,8),6.0f);
        AddDebris(new Vector3(72,1.1f,115),new Vector3(8,3.0f,5),.9f);
        AddDebris(new Vector3(-75,1.1f,-114),new Vector3(7,4.2f,6),2.0f);
        AddDebris(new Vector3(120,1.1f,18),new Vector3(6,2.8f,8),3.1f);
        AddDebris(new Vector3(-120,1.1f,42),new Vector3(8,2.2f,6),4.2f);
        AddDebris(new Vector3(112,1.1f,112),new Vector3(9,3.4f,7),5.3f);
        AddDebris(new Vector3(-112,1.1f,-110),new Vector3(7,2.6f,9),6.4f);
        // Additional cover uses the mid and outer quadrants, keeping the
        // center lanes readable while preventing large empty map corners.
        AddDebris(new Vector3(40,1.1f,100),new Vector3(7,2.8f,6),.8f);
        AddDebris(new Vector3(-45,1.1f,100),new Vector3(8,3.2f,5),1.9f);
        AddDebris(new Vector3(40,1.1f,-110),new Vector3(6,2.4f,8),3.0f);
        AddDebris(new Vector3(-45,1.1f,-78),new Vector3(7,3.6f,6),4.1f);
        AddDebris(new Vector3(90,1.1f,0),new Vector3(8,2.6f,5),5.2f);
        AddDebris(new Vector3(-92,1.1f,-80),new Vector3(9,3.2f,7),6.3f);
        AddDebris(new Vector3(0,1.1f,90),new Vector3(6,2.2f,7),.4f);
        AddDebris(new Vector3(-110,1.1f,0),new Vector3(7,2.8f,6),1.5f);
        AddDebris(new Vector3(116,1.1f,-100),new Vector3(8,3.4f,6),2.6f);
        AddDebris(new Vector3(0,1.1f,-90),new Vector3(7,2.4f,5),3.7f);
        BuildElevationCourse();
        for(int side=-1;side<=1;side+=2) for(int i=0;i<5;i++)
        {
            float height=6+i%3*3+.3f;
            obstacles.Add(new Bounds(new Vector3(side*(48+i*14),GroundSurfaceY+height*.5f,45+i%2*16),new Vector3(10,height,13)));
        }
        }
        else if(arenaMap==ArenaMapKind.OutdoorCanyon)
        {
            // Keep the opening around the spawn area clear, then spread
            // destructible cover across the canyon so movement and shots have
            // meaningful obstruction throughout the arena.
            AddDebris(new Vector3(-82,1.1f,48),new Vector3(8,3.2f,6),.4f);
            AddDebris(new Vector3(110,1.1f,0),new Vector3(7,2.6f,9),1.3f);
            AddDebris(new Vector3(-112,1.1f,-76),new Vector3(9,4.0f,7),2.2f);
            AddDebris(new Vector3(116,1.1f,78),new Vector3(8,3.0f,8),3.1f);
            AddDebris(new Vector3(-52,1.1f,120),new Vector3(7,2.4f,10),4.0f);
            AddDebris(new Vector3(42,1.1f,-108),new Vector3(10,3.6f,7),4.9f);
            AddDebris(new Vector3(-25,1.1f,20),new Vector3(6,2.2f,6),5.8f);
            AddDebris(new Vector3(-8,1.1f,-74),new Vector3(7,3.4f,6),6.7f);
            // Stepped ridges and shelves are also registered as terrain
            // obstacles, so they block projectiles and resolve fighter motion.
            BuildCanyonTerrain();
        }
        else if(arenaMap==ArenaMapKind.CoastalBay)
        {
            // Cargo yards and port machinery stay on the land half; keep a broad
            // clear approach along the shore for land-to-water engagements.
            Vector3[] sites={new Vector3(-124,0,-112),new Vector3(-98,0,-112),new Vector3(-76,0,-112),new Vector3(-124,0,-62),new Vector3(-98,0,-62),new Vector3(-76,0,-62),new Vector3(-124,0,-8),new Vector3(-98,0,-8),new Vector3(-76,0,-8),new Vector3(-124,0,48),new Vector3(-98,0,48),new Vector3(-76,0,48),new Vector3(-124,0,108),new Vector3(-98,0,108),new Vector3(-76,0,108)};
            Vector3[] sizes={new Vector3(13,5,9),new Vector3(9,4,14),new Vector3(12,6,8),new Vector3(8,4,12),new Vector3(14,5,9),new Vector3(9,4,13),new Vector3(12,5,8),new Vector3(8,6,12),new Vector3(13,4,9),new Vector3(9,5,13),new Vector3(14,4,8),new Vector3(9,6,12),new Vector3(12,4,9),new Vector3(8,5,12),new Vector3(9,4,13)};
            for(int i=0;i<sites.Length;i++){sites[i].y=SurfaceHeight(sites[i])+sizes[i].y*.5f;AddDebris(sites[i],sizes[i],i*.73f);}
        }
        else BuildPlainsCover();
        player=MakeFighter("Pilot frame",Cyan); enemy=MakeFighter("Rival frame",Orange);
        BuildClimate();
        aimMarker=new GameObject("Aim reticle").transform;
        Shape(PrimitiveType.Cube,"Aim cross",aimMarker,Vector3.zero,new Vector3(.8f,.06f,.07f),Cyan,true);
        Shape(PrimitiveType.Cube,"Aim cross",aimMarker,Vector3.zero,new Vector3(.07f,.06f,.8f),Cyan,true);
    }
    void BuildArenaPerimeter()
    {
        if(arenaMap==ArenaMapKind.OpenPlains)return;
        // Geometry is readable from inside, but movement is also clamped so
        // high-jump and dash fighters cannot escape over a decorative boundary.
        Color wall=arenaMap==ArenaMapKind.IndoorFactory?
            new Color(.16f,.20f,.24f):arenaMap==ArenaMapKind.CoastalBay?new Color(.16f,.36f,.43f):new Color(.29f,.26f,.22f);
        float height=arenaMap==ArenaMapKind.IndoorFactory?65f:36f;
        bool authoredCanyon=arenaMap==ArenaMapKind.OutdoorCanyon &&
            PlaceEnvironment("CanyonBoundary",null,new Vector3(0,GroundSurfaceY,0),Vector3.one)!=null;
        if(arenaMap==ArenaMapKind.IndoorFactory)
        {
            Transform ceiling=Box("High hangar ceiling",null,new Vector3(0,GroundSurfaceY+69f,0),
                new Vector3(290,2,290),new Color(.11f,.15f,.19f));
            // This directional key represents interior lighting; the visual roof
            // must not occlude it and flatten all lighting to ambient only.
            ceiling.GetComponent<Renderer>().shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
            cameraSolids.Add(new Bounds(new Vector3(0,GroundSurfaceY+69f,0),new Vector3(290,2,290)));
        }
        for(int axis=0;axis<2;axis++)
        for(int side=-1;side<=1;side+=2)
        {
            Vector3 size=axis==0?new Vector3(5,height,290):new Vector3(290,height,5);
            Vector3 pos=axis==0?new Vector3(side*(ArenaHalfWidth+2.5f),GroundSurfaceY+height*.5f,0):
                new Vector3(0,GroundSurfaceY+height*.5f,side*(ArenaHalfWidth+2.5f));
            if(!authoredCanyon) Box(arenaMap==ArenaMapKind.IndoorFactory?"Hangar boundary wall":arenaMap==ArenaMapKind.CoastalBay?"Coastal bay boundary":"Canyon cliff boundary",
                null,pos,size,wall);
            cameraSolids.Add(new Bounds(pos,size));
            if(arenaMap==ArenaMapKind.OutdoorCanyon && !authoredCanyon)
            {
                // Layered blocky rock strata; the cliff is a perimeter, not a pit.
                for(int j=0;j<18;j++)
                {
                    float along=-130+j*15.3f;
                    float rockHeight=9f+Mathf.PerlinNoise(j*.31f,axis+side+2)*13f;
                    Vector3 rockPos=axis==0?
                        new Vector3(side*(ArenaHalfWidth-3f),GroundSurfaceY+rockHeight*.5f,along):
                        new Vector3(along,GroundSurfaceY+rockHeight*.5f,side*(ArenaHalfWidth-3f));
                    Vector3 rockSize=axis==0?new Vector3(9,rockHeight,16):new Vector3(16,rockHeight,9);
                    PlaceEnvironment("CanyonRock",null,new Vector3(rockPos.x,GroundSurfaceY,rockPos.z),rockSize);
                    cameraSolids.Add(new Bounds(rockPos,rockSize));
                }
            }
        }
    }
    Vector3 ArenaSpawnPosition(int teamId)
    {
        if(arenaMap==ArenaMapKind.CoastalBay)
        {
            Vector3 position=teamId==0?new Vector3(-50,0,0):new Vector3(45,0,12);
            position.y=SurfaceHeight(position)+FighterSupportOffset;
            return position;
        }
        return teamId==0?new Vector3(-15,FighterGroundY,-15):new Vector3(15,FighterGroundY,15);
    }
    void BuildCanyonTerrain()
    {
        // Low-poly slope meshes hide the terraced collision samples and break
        // the repeating stair silhouette into broad, irregular terrain forms.
        Color low=new Color(.34f,.30f,.25f),high=new Color(.43f,.37f,.29f);
        for(int ridge=0;ridge<CanyonTerrainStepCounts.Length;ridge++)
        {
            // The two basin cuts replace these overlapping stepped ridges with
            // broad continuous slopes that remain walkable underwater.
            if(ridge==7)continue;
            int count=CanyonTerrainStepCounts[ridge];
            float cx=ridge==4?38f:ridge==6?75f:ridge==8?120f:-84f+ridge*21f;
            float cz=ridge%2==0?25f:-54f;
            float totalRise=ridge==4?5.1f:1.65f+(ridge%3)*.26f;
            int colliderCount=ridge==4?56:count;
            float spacing=ridge==4?.475f:1.9f;
            BuildCanyonSlopeVisual(ridge,cx,cz,colliderCount*spacing,19f+ridge%3*3f,totalRise,low,high);
            float previousHeight=0;
            for(int step=0;step<colliderCount;step++)
            {
                float t=(step+1f)/colliderCount;
                float roughness=(Mathf.PerlinNoise(step*.29f+ridge*.31f,ridge*.83f)-.5f)*(ridge==4?.035f:.14f);
                float riseLimit=ridge==4?.09f:.34f;
                float h=Mathf.Max(previousHeight+.025f,totalRise*t+roughness);h=Mathf.Min(h,previousHeight+riseLimit);previousHeight=h;
                float x=cx+(step-(colliderCount-1)*.5f)*spacing;
                float z=CanyonRidgeZ(ridge,cz,t);
                AddTerrainCollider(new Vector3(x,GroundSurfaceY+h*.5f,z),
                    new Vector3(ridge==4?.1f:1.2f,h,19f+ridge%3*3f));
            }
        }
        // Scattered broad shelves provide sightline variation without dense cover.
        for(int i=0;i<5;i++)
        {
            float x=-50f+i*25f,z=(i%2==0?-57f:55f);
            float h=1.2f+i%3*.5f;
            AddTerrainBlock("Canyon shelf "+i,new Vector3(x,GroundSurfaceY+h*.5f,z),
                new Vector3(10,h,12),low);
        }
    }
    float CanyonRidgeZ(int ridge,float center,float t)
        =>center+Mathf.Sin(t*CanyonRidgeSegmentCount(ridge)*.57f+ridge*.91f)*1.1f+
          (Mathf.PerlinNoise(ridge*.71f,t*CanyonRidgeSegmentCount(ridge)*.38f)-.5f)*1.4f;
    int CanyonRidgeSegmentCount(int ridge)=>ridge==4?56:CanyonTerrainStepCounts[Mathf.Clamp(ridge,0,CanyonTerrainStepCounts.Length-1)];
    void BuildCanyonSlopeVisual(int ridge,float centerX,float centerZ,float length,float width,float rise,Color low,Color high)
    {
        const int across=8;
        int along=CanyonRidgeSegmentCount(ridge);
        var vertices=new List<Vector3>((along+1)*(across+1));
        for(int x=0;x<=along;x++)
        {
            float t=x/(float)along,centerZAtX=CanyonRidgeZ(ridge,centerZ,t);
            float xPos=centerX-length*.5f+t*length;
            for(int z=0;z<=across;z++)
            {
                float v=z/(float)across*2-1;
                float noise=(Mathf.PerlinNoise(x*.19f+ridge*2.1f,z*.47f+ridge*.7f)-.5f)*.14f*Mathf.Sin(t*Mathf.PI);
                float height=Mathf.Max(0,rise*t*(1-.16f*v*v)+noise);
                vertices.Add(new Vector3(xPos,GroundSurfaceY+height,centerZAtX+v*width*.5f));
            }
        }
        var materialTriangles=new[]{new List<int>(),new List<int>(),new List<int>()};
        for(int x=0;x<along;x++)for(int z=0;z<across;z++)
        {
            int a=x*(across+1)+z,b=(x+1)*(across+1)+z,c=a+1,d=b+1;
            int material=Mathf.Clamp(Mathf.FloorToInt(Mathf.PerlinNoise(x*.27f+ridge*.9f,z*.31f+ridge*1.3f)*3),0,2);
            List<int> tris=materialTriangles[material];tris.Add(a);tris.Add(c);tris.Add(b);tris.Add(b);tris.Add(c);tris.Add(d);
        }
        void AddSide(int topA,int topB,bool reverse)
        {
            int bottomA=vertices.Count;vertices.Add(new Vector3(vertices[topA].x,GroundSurfaceY,vertices[topA].z));
            int bottomB=vertices.Count;vertices.Add(new Vector3(vertices[topB].x,GroundSurfaceY,vertices[topB].z));
            List<int> tris=materialTriangles[1];
            if(reverse){tris.Add(topA);tris.Add(bottomA);tris.Add(topB);tris.Add(topB);tris.Add(bottomA);tris.Add(bottomB);}
            else{tris.Add(topA);tris.Add(topB);tris.Add(bottomA);tris.Add(topB);tris.Add(bottomB);tris.Add(bottomA);}
        }
        for(int x=0;x<along;x++)
        {
            AddSide(x*(across+1),(x+1)*(across+1),true);
            AddSide(x*(across+1)+across,(x+1)*(across+1)+across,false);
        }
        for(int z=0;z<across;z++)
        {
            AddSide(z,z+1,true);
            int end=along*(across+1);AddSide(end+z,end+z+1,false);
        }
        var mesh=new Mesh{name="Irregular canyon slope / ridge "+ridge};mesh.indexFormat=UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.SetVertices(vertices);mesh.subMeshCount=materialTriangles.Length;
        for(int i=0;i<materialTriangles.Length;i++)mesh.SetTriangles(materialTriangles[i],i);
        mesh.RecalculateNormals();mesh.RecalculateBounds();
        Transform root=Module("Canyon natural slope "+ridge,null,Vector3.zero);
        root.gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;
        MeshRenderer renderer=root.gameObject.AddComponent<MeshRenderer>();renderer.sharedMaterials=new[]{Mat(low),Mat(Color.Lerp(low,high,.52f)),Mat(high)};
    }
    void AddTerrainBlock(string name,Vector3 center,Vector3 size,Color color)
    {
        Transform terrain=Module(name,null,center);
        BuildVoxelCover(terrain,size,2);
        AddTerrainCollider(center,size);
    }
    void AddTerrainCollider(Vector3 center,Vector3 size)
    {
        Bounds bounds=new Bounds(center,size);
        obstacles.Add(bounds);
        terrainSteps.Add(bounds);
        terrainStepLookup.Add(bounds);
    }
    void BuildElevationCourse()
    {
        // Low, closely spaced terraces read as a gentle stepped ramp.
        // Taller isolated ledges demand a jump rather than automatic stepping.
        Color stone=new Color(.21f,.31f,.37f),edge=new Color(.35f,.45f,.50f);
        for(int i=0;i<7;i++)
        {
            float height=.36f*(i+1);
            AddTerrainBlock("West access ramp "+i,new Vector3(-24+i*2.5f,GroundSurfaceY+height*.5f,20),
                new Vector3(2.6f,height,8),stone);
        }
        // Match the final stair top so the deck does not form a taller hidden
        // collision wall across the last step.
        const float deckTop=GroundSurfaceY+.36f*7;
        float deckHeight=deckTop-GroundSurfaceY;
        AddTerrainBlock("West raised deck",new Vector3(-3,GroundSurfaceY+deckHeight*.5f,20),new Vector3(13,deckHeight,11),edge);
        for(int i=0;i<4;i++)
        {
            float height=1.7f*(i+1);
            AddTerrainBlock("East jump ledge "+i,new Vector3(20+i*5f,GroundSurfaceY+height*.5f,-14),
                new Vector3(4.5f,height,7),stone);
        }
    }
    Fighter MakeFighter(string name,Color accent)
    {
        Fighter f=new Fighter(); f.root=new GameObject(name).transform;
        f.shadow=Shape(PrimitiveType.Cylinder,"Contact shadow",null,new Vector3(0,GroundSurfaceY+.06f,0),new Vector3(2.7f,.012f,2.1f),new Color(.075f,.105f,.12f));
        f.jumpFlame=Shape(PrimitiveType.Capsule,"Jump exhaust flare",f.root,new Vector3(0,-.1f,-1.4f),
            new Vector3(.48f,.65f,.48f),new Color(.66f,.94f,1),true);
        f.jumpFlame.GetComponent<Renderer>().shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
        f.jumpFlame.GetComponent<Renderer>().receiveShadows=false;
        f.jumpFlame.gameObject.SetActive(false);
        f.thruster=new GameObject("Directional thruster").transform;
        for(int segment=0;segment<7;segment++)
        {
            float taper=1-segment/8f;
            Shape(PrimitiveType.Sphere,"Exhaust plasma",f.thruster,new Vector3(0,0,.25f+segment*.41f),
                new Vector3(.47f*taper,.35f*taper,.65f),Color.Lerp(new Color(.7f,.94f,1),new Color(.08f,.28f,.85f),segment/6f),true);
        }
        return f;
    }
    void AddDebris(Vector3 origin,Vector3 size,float phase,bool gravityRegulator=false)
    {
        if(arenaMap==ArenaMapKind.OutdoorCanyon||arenaMap==ArenaMapKind.CoastalBay)origin.y=SurfaceHeight(origin)+size.y*.5f;
        Transform root=new GameObject(gravityRegulator?"Foundry gravity regulator / destructible":"Ground cover "+debris.Count).transform;
        root.position=origin;
        BuildGroundCover(root,size,debris.Count);
        Debris piece=new Debris{visual=root,origin=origin,size=size,phase=phase,hp=120f+size.x*size.z*1.5f,gravityRegulator=gravityRegulator};
        if(gravityRegulator)BuildGravityRegulatorVisual(root,size);
        debris.Add(piece);
        obstacles.Add(DebrisBounds(origin,size));
    }
    void BuildGravityRegulatorVisual(Transform root,Vector3 size)
    {
        Color signal=new Color(.18f,.86f,1f);
        Shape(PrimitiveType.Cylinder,"Gravity regulator emitter ring",root,new Vector3(0,size.y*.36f,0),new Vector3(size.x*.54f,.10f,size.z*.54f),signal,true);
        Shape(PrimitiveType.Sphere,"Gravity regulator core",root,new Vector3(0,size.y*.24f,0),Vector3.one*.62f,signal,true);
        for(int side=-1;side<=1;side+=2)Shape(PrimitiveType.Cube,"Gravity regulator mast",root,new Vector3(side*size.x*.36f,size.y*.25f,0),new Vector3(.14f,size.y*.65f,.14f),new Color(.95f,.49f,.16f),true);
    }
    Bounds DebrisBounds(Vector3 center,Vector3 size)
    {
        float top=center.y+size.y*.5f;
        float bottom=SurfaceHeight(center);float height=Mathf.Max(.01f,top-bottom);
        return new Bounds(new Vector3(center.x,(top+bottom)*.5f,center.z),
            new Vector3(size.x,height,size.z));
    }
    void UpdateObstacles(float dt)
    {
        spaceClock+=dt;
        UpdateCoverFragments(dt);
        for(int i=0;i<debris.Count;i++)
        {
            Debris d=debris[i];
            if(d.destroyed) continue;
            Vector3 p=d.origin; // Ground cover stays fixed so players can learn reliable escape routes.
            d.visual.position=p;
            // Collision bounds and the visual translate together.
            obstacles[i]=DebrisBounds(p,d.size);
        }
        if(fighterRoster.Count>0)
        {
            foreach(Fighter fighter in fighterRoster) ResolveOverlap(fighter);
        }
        else if(player!=null) { ResolveOverlap(player); ResolveOverlap(enemy); }
    }
    void ScatterCoverFragments(Debris source,Vector3 impact)
    {
        if(source.visual==null) return;
        // Sample the actual child renderers: fragments inherit the cover's
        // original painted colours, not a generic explosion palette.
        Renderer[] parts=source.visual.GetComponentsInChildren<Renderer>();
        if(parts.Length==0) return;
        int count=Mathf.Clamp(Mathf.RoundToInt(source.size.x*source.size.z*.48f),16,34);
        for(int j=0;j<count;j++)
        {
            Renderer part=parts[Random.Range(0,parts.Length)];
            if(part==null) continue;
            Color color=part.sharedMaterial!=null && part.sharedMaterial.HasProperty("_Color")?
                part.sharedMaterial.color:new Color(.32f,.36f,.37f);
            Vector3 spawn=part.bounds.center+new Vector3(
                Random.Range(-.4f,.4f),Random.Range(-.3f,.3f),Random.Range(-.4f,.4f));
            float scale=j%5==0?Random.Range(.55f,.95f):Random.Range(.16f,.43f);
            Transform piece=AcquirePDPart("RockFragment",spawn,Vector3.one*scale,color);if(piece==null)continue;
            Collider collider=piece.GetComponent<Collider>();
            if(collider!=null) Destroy(collider);
            Vector3 direction=(spawn-impact).normalized+
                new Vector3(Random.Range(-1f,1f),Random.Range(.15f,1.3f),Random.Range(-1f,1f));
            direction.Normalize();
            coverFragments.Add(new CoverFragment{
                visual=piece,velocity=direction*Random.Range(4f,11f),
                spin=Random.Range(-260f,260f)
            });
        }
        // Retain a bounded set of settled debris, so long matches do not
        // accumulate thousands of GameObjects. All fragments are visual only.
        while(coverFragments.Count>MaxCoverFragments)
        {
            CoverFragment oldest=coverFragments[0];
            if(oldest.visual!=null) ReleasePDPart(oldest.visual);
            coverFragments.RemoveAt(0);
        }
    }
    void UpdateCoverFragments(float dt)
    {
        for(int i=coverFragments.Count-1;i>=0;i--)
        {
            CoverFragment f=coverFragments[i];f.age+=dt;if(f.visual==null){coverFragments.RemoveAt(i);continue;}
            if(f.age>7){ReleasePDPart(f.visual);coverFragments.RemoveAt(i);continue;}
            if(f.settled)continue;f.velocity+=Vector3.down*19*dt;Vector3 next=f.visual.position+f.velocity*dt;float floor=SurfaceHeight(next)+f.visual.localScale.y*.5f;
            if(next.y<=floor){next.y=floor;f.velocity=Vector3.zero;f.settled=true;}
            f.visual.position=next;f.visual.Rotate(Vector3.up,f.spin*dt,Space.World);
        }
    }
    void DamageDestructibleCover(Vector3 impact,float damage)
    {
        // Debris is registered first in obstacles, preserving index alignment.
        // Terrain steps, perimeter, and structural walls are indestructible.
        for(int i=0;i<debris.Count;i++)
        {
            Debris d=debris[i];
            if(d.destroyed) continue;
            Bounds b=obstacles[i];
            if((b.ClosestPoint(impact)-impact).sqrMagnitude>4f) continue;
            d.hp-=Mathf.Max(0,damage);
            if(d.hp>0)
            {
                if(d.cracks.Count<4)
                {
                    Vector3 normal=impact-d.origin;int axis=Mathf.Abs(normal.x)>Mathf.Abs(normal.z)?0:2;normal=axis==0?Vector3.right*Mathf.Sign(normal.x):Vector3.forward*Mathf.Sign(normal.z);
                    Transform crack=AcquirePDPart("Crack",b.ClosestPoint(impact)+normal*.025f,Vector3.one*Mathf.Min(d.size.x,d.size.z)*.7f,new Color(.025f,.03f,.035f));
                    if(crack!=null){crack.rotation=Quaternion.FromToRotation(Vector3.up,normal);d.cracks.Add(crack);}
                }
                return;
            }
            d.destroyed=true;foreach(Transform crack in d.cracks)ReleasePDPart(crack);d.cracks.Clear();
            if(d.gravityRegulator)StartFoundryGravityEvent();
            PlaySound(SoundKind.CoverBreak,impact,.95f);
            ScatterCoverFragments(d,impact);
            for(int cloud=0;cloud<8;cloud++)EmitPDParticle("SmokePuff",impact,Vector3.up+UnityEngine.Random.onUnitSphere,.3f,new Color(.36f,.32f,.26f,.45f),1.6f,true);
            EmitPDParticle("Ripple",impact,Vector3.zero,.8f,new Color(.65f,.56f,.40f,.4f),.65f,true);
            if(d.visual!=null) Destroy(d.visual.gameObject);
            // Keep list indices stable for UpdateObstacles.
            obstacles[i]=new Bounds(new Vector3(0,-10000f,0),Vector3.zero);
            return;
        }
    }
    void ResetCoverForRound()
    {
        foreach(CoverFragment fragment in coverFragments)ReleasePDPart(fragment.visual);coverFragments.Clear();
        foundryGravityElapsed=foundryGravityDuration=foundryGravityRecovery=0;foundryGravityRegulatorSpent=false;
        for(int i=0;i<debris.Count;i++)
        {
            Debris d=debris[i];foreach(Transform crack in d.cracks)ReleasePDPart(crack);d.cracks.Clear();
            if(d.destroyed){d.visual=new GameObject(d.gravityRegulator?"Foundry gravity regulator / destructible":"Ground cover "+i).transform;d.visual.position=d.origin;BuildGroundCover(d.visual,d.size,i);if(d.gravityRegulator)BuildGravityRegulatorVisual(d.visual,d.size);d.destroyed=false;}
            d.hp=120+d.size.x*d.size.z*1.5f;obstacles[i]=DebrisBounds(d.origin,d.size);
        }
    }
    bool Occupied(Vector3 p,float radius)
    {
        foreach(Bounds b in obstacles)
            if(terrainStepLookup.Contains(b)?InsideTerrainObstacle(b,p,radius):InsideObstacle(b,p,radius)) return true;
        return false;
    }
    bool OverlapsFootprint(Bounds obstacle,Vector3 point,float radius)
    {
        float dx=Mathf.Max(Mathf.Abs(point.x-obstacle.center.x)-obstacle.extents.x,0);
        float dz=Mathf.Max(Mathf.Abs(point.z-obstacle.center.z)-obstacle.extents.z,0);
        return dx*dx+dz*dz<=radius*radius;
    }
    bool InsideObstacle(Bounds obstacle,Vector3 point,float radius)
    {
        // Round the footprint corners to the fighter's circular radius; square
        // expansion made narrow gaps and corners unnecessarily sticky.
        return OverlapsFootprint(obstacle,point,radius) &&
            point.y>=obstacle.min.y && point.y<obstacle.max.y-.02f;
    }
    bool InsideTerrainObstacle(Bounds obstacle,Vector3 point,float radius)
    {
        // Terrain bounds are stored at the visible rock height, while the
        // fighter origin sits FighterSupportOffset above its feet.
        float feet=point.y-FighterSupportOffset;
        return OverlapsFootprint(obstacle,point,radius) &&
            feet>=obstacle.min.y-.05f && feet<obstacle.max.y-.02f;
    }
    void ResolveOverlap(Fighter f)
    {
        // Only eject a fighter when its collision center is actually inside a solid.
        // Preserve height: resetting Y here previously prevented rooftop traversal.
        for(int iteration=0;iteration<4;iteration++)
        foreach(Bounds b in obstacles)
        {
            Bounds e=b; e.Expand(new Vector3(f.Radius*2+.04f,0,f.Radius*2+.04f));
            Vector3 p=f.root.position;
            if(!InsideObstacle(b,p,f.Radius+.02f)) continue;
            // Course blocks are handled by movement/landing, never by spawn
            // overlap ejection. Ejecting near a step top pushed a climber away
            // before TryStepUp could advance onto the next terrace.
            if(terrainStepLookup.Contains(b)) continue;
            Vector3[] exits={
                new Vector3(e.min.x-.04f,p.y,p.z),new Vector3(e.max.x+.04f,p.y,p.z),
                new Vector3(p.x,p.y,e.min.z-.04f),new Vector3(p.x,p.y,e.max.z+.04f)};
            float best=float.PositiveInfinity; Vector3 chosen=p;
            foreach(Vector3 exit in exits)
                if(!Occupied(exit,f.Radius) && (exit-p).sqrMagnitude<best)
                { best=(exit-p).sqrMagnitude; chosen=exit; }
            f.root.position=chosen;
        }
    }
    float LandingHeight(Vector3 p,float radius,float previousY)
    {
        float ground=SurfaceHeight(p)+FighterSupportOffset;
        foreach(Bounds b in obstacles)
        {
            // A rooftop is a landing surface only if approached from above.
            if(OverlapsFootprint(b,p,radius) &&
                previousY>=b.max.y-.03f)
                ground=Mathf.Max(ground,b.max.y+FighterSupportOffset);
        }
        return ground;
    }
    bool ObstacleOnSegment(Vector3 from,Vector3 to,float radius,out float nearest)
    {
        Vector3 delta=to-from; float length=delta.magnitude; nearest=float.PositiveInfinity;
        if(length<.0001f) { if(Occupied(from,radius)) nearest=0; return nearest==0; }
        Ray ray=new Ray(from,delta/length);
        foreach(Bounds b in obstacles)
        {
            Bounds e=b; e.Expand(new Vector3(radius*2,0,radius*2));
            if(InsideObstacle(b,from,radius)) nearest=0;
            else
            {
                // Rays running over a roof should not treat the support surface as a wall.
                e.max=new Vector3(e.max.x,b.max.y-.02f,e.max.z);
                if(e.IntersectRay(ray,out float hit)&&hit<=length) nearest=Mathf.Min(nearest,hit);
            }
        }
        if(arenaMap==ArenaMapKind.OpenPlains)
        {
            int samples=Mathf.Max(1,Mathf.CeilToInt(length));
            for(int i=0;i<=samples;i++){float distance=length*i/samples;Vector3 point=from+delta*(i/(float)samples);if(point.y-radius<=SurfaceHeight(point)){nearest=Mathf.Min(nearest,distance);break;}}
        }
        return nearest<=length;
    }
    bool TryStepUp(Fighter f,Vector3 p,Vector3 horizontal,out Vector3 stepped)
    {
        stepped=p;
        if(horizontal.sqrMagnitude<.00001f || f.jumpVelocity>1f) return false;
        float maxStep=Mathf.Max(0,f.spec.stepHeight);
        float highestLanding=p.y;bool found=false;
        foreach(Bounds b in terrainSteps)
        {
            Vector3 ahead=p+horizontal;
            if(!OverlapsFootprint(b,ahead,f.Radius)) continue;
            float landingY=b.max.y+FighterSupportOffset;
            if(landingY>highestLanding){highestLanding=landingY;found=true;}
        }
        float rise=highestLanding-p.y;
        if(!found||rise<=.01f||rise>maxStep)return false;
        Vector3 candidate=new Vector3(p.x+horizontal.x,highestLanding+.01f,p.z+horizontal.z);
        if(Occupied(candidate,f.Radius))return false;
        stepped=candidate;return true;
    }
    void MoveWithObstacles(Fighter f,Vector3 displacement,bool poweredClimb=false)
    {
        ResolveOverlap(f);
        int count=Mathf.Max(1,Mathf.CeilToInt(displacement.magnitude/.25f));
        Vector3 step=displacement/count,p=f.root.position;
        for(int i=0;i<count;i++)
        {
            Vector3 horizontal=new Vector3(step.x,0,step.z);
            if(!Occupied(p+horizontal,f.Radius))
            {
                Vector3 nextHorizontal=p+horizontal;float rise=SurfaceHeight(nextHorizontal)+FighterSupportOffset-p.y;
                if(arenaMap!=ArenaMapKind.OpenPlains||rise<=f.spec.stepHeight+.01f){p=nextHorizontal;if(rise>0&&f.jumpVelocity<=0)p.y+=rise;}
            }
            else if(TryStepUp(f,p,horizontal,out Vector3 stepped)) p=stepped;
            else
            {
                Vector3 x=p+Vector3.right*step.x;
                if(!Occupied(x,f.Radius)) p=x;
                Vector3 z=p+Vector3.forward*step.z;
                if(!Occupied(z,f.Radius)) p=z;
            }
            float previousY=p.y;
            Vector3 next=p+Vector3.up*step.y;
            if(step.y>0)
            {
                float jumpTop=f.jumpOriginY+jumpMaxHeight*Mathf.Max(.1f,f.spec.jumpHeightScale)*EnvironmentMobilityScale;
                if(!poweredClimb && p.y<=jumpTop && next.y>jumpTop) { next.y=jumpTop; p=next; f.jumpVelocity=0; }
                else if(!Occupied(next,f.Radius)) p=next;
                else f.jumpVelocity=Mathf.Min(0,f.jumpVelocity);
            }
            else if(step.y<0)
            {
                float floor=LandingHeight(p,f.Radius,previousY);
                if(next.y<=floor)
                {
                    if(f.jumpVelocity < -7f)
                        PlaySound(f.spec.mobility==MobilityKind.Tank?SoundKind.HeavyLanding:SoundKind.Landing,
                            p,Mathf.Clamp01(Mathf.Abs(f.jumpVelocity)/22f));
                    if(f.jumpVelocity < -9f && floor>FighterGroundY+.3f)
                        DamageDestructibleCover(new Vector3(p.x,floor-FighterSupportOffset,p.z),
                            (Mathf.Abs(f.jumpVelocity)-9f)*17f);
                    p.y=floor;
                    f.jumpVelocity=0;
                }
                else if(!Occupied(next,f.Radius)) p=next;
            }
            p.y=Mathf.Max(SurfaceHeight(p)+FighterSupportOffset,p.y);
            p.x=Mathf.Clamp(p.x,-ArenaLimit+f.Radius,ArenaLimit-f.Radius);
            p.z=Mathf.Clamp(p.z,-ArenaLimit+f.Radius,ArenaLimit-f.Radius);
        }
        f.root.position=p;
    }
}
