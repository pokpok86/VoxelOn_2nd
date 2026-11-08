using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
public sealed partial class VoxArena
{
    public enum WeatherKind{Clear,Overcast,Rain,Thunderstorm,Sandstorm}
    public enum PrecipitationIntensity{Light,Moderate,Heavy}
    public enum RadarOutsidePolicy{PendingPDApproval,AlwaysDirection,AfterDetection}
    [Header("Weather test overrides: -1 = random each round")]
    public int forcedWeather=-1,forcedDay=-1,forcedTemperatureC=99,forcedHumidity=-1,forcedPrecipitation=-1;
    public RadarOutsidePolicy outsideRadarPolicy=RadarOutsidePolicy.AlwaysDirection;
    public Shader skyTextureShader,shallowWaterShader;
    WeatherKind roundWeather;bool evening,radarHasSeen;
    public float roundTemperatureC=18,roundHumidityPercent=45;PrecipitationIntensity precipitationIntensity;bool snowing;
    Light climateSun,climateRim;float climateSunBase;
    Transform skyDome;Material skyMaterial,waterMaterial,rippleMaterial,rainMaterial;
    readonly List<Vector3> ponds=new List<Vector3>();
    readonly List<Transform> rainDrops=new List<Transform>();
    readonly List<Material> wetMaterials=new List<Material>();
    readonly List<float> originalGloss=new List<float>();
    readonly List<Color> originalWetColors=new List<Color>();
    readonly List<Transform> ripples=new List<Transform>();
    readonly float[] rippleAge=new float[24];int nextRipple;float splashClock;
    readonly Vector3[] previousWaterPosition=new Vector3[2];
    readonly bool[] previouslyWet=new bool[2];
    readonly List<Transform> coastalWaveCrests=new List<Transform>();
    Transform coastalWaterSurface;
    const float CoastalBayStartX=-35f,CoastalLandRampStartX=-70f,CoastalBayDepth=18f;
    Color coastalSurfaceFogColor=new Color(.38f,.63f,.70f);float coastalSurfaceFogDensity=.0018f;bool coastalSubmerged;
    static readonly Vector2[] CanyonWaterCenters={new Vector2(0,80),new Vector2(85,-80)};
    const float CanyonWaterRadius=48f,CanyonWaterFloorRadius=8f,CanyonWaterDepth=9.5f;
    float OutdoorCanyonWaterDepth(Vector3 p)
    {
        if(arenaMap!=ArenaMapKind.OutdoorCanyon)return 0;
        float depth=0;
        foreach(Vector2 center in CanyonWaterCenters)
        {
            float distance=Vector2.Distance(new Vector2(p.x,p.z),center);
            if(distance>=CanyonWaterRadius)continue;
            float t=Mathf.InverseLerp(CanyonWaterFloorRadius,CanyonWaterRadius,distance);
            float bowl=distance<=CanyonWaterFloorRadius?CanyonWaterDepth:CanyonWaterDepth*(1-Mathf.SmoothStep(0,1,t));
            depth=Mathf.Max(depth,bowl);
        }
        return depth;
    }
    float OutdoorCanyonSurfaceHeight(Vector3 p)=>GroundSurfaceY-OutdoorCanyonWaterDepth(p);
    float CoastalBayDepthAt(Vector3 p)
    {
        if(arenaMap!=ArenaMapKind.CoastalBay||p.x<=CoastalLandRampStartX)return 0;
        float t=Mathf.SmoothStep(0,1,Mathf.InverseLerp(CoastalLandRampStartX,0,p.x));
        return CoastalBayDepth*t;
    }
    float CoastalBaySurfaceHeight(Vector3 p)=>GroundSurfaceY-CoastalBayDepthAt(p);
    float WaterlineHeight(Vector3 p)=>arenaMap==ArenaMapKind.OutdoorCanyon||arenaMap==ArenaMapKind.CoastalBay?GroundSurfaceY:SurfaceHeight(p);
    bool InsideWaterFootprint(Vector3 p)
    {
        if(arenaMap==ArenaMapKind.OutdoorCanyon)
        {
            foreach(Vector2 center in CanyonWaterCenters)if((new Vector2(p.x,p.z)-center).sqrMagnitude<CanyonWaterRadius*CanyonWaterRadius)return true;
            return false;
        }
        if(arenaMap==ArenaMapKind.CoastalBay)return p.x>=CoastalBayStartX&&Mathf.Abs(p.z)<=ArenaHalfWidth+5;
        foreach(Vector3 pond in ponds){Vector3 d=p-pond;if(d.x*d.x/49+d.z*d.z/25<1)return true;}
        return false;
    }
    float WaterGravityScale(Fighter f)
    {
        if(f==null||!InsideWaterFootprint(f.root.position))return 1f;
        float waterline=GroundSurfaceY;
        if(arenaMap!=ArenaMapKind.OutdoorCanyon)foreach(Vector3 pond in ponds)
        {
            Vector3 d=f.root.position-pond;if(d.x*d.x/49+d.z*d.z/25<1){waterline=pond.y;break;}
        }
        float bodyDepth=waterline+.4f-f.root.position.y;
        if(arenaMap==ArenaMapKind.CoastalBay&&bodyDepth<=0)return 1f;
        return Mathf.Lerp(1f,.5f,Mathf.InverseLerp(0,2f,bodyDepth));
    }
    float ProjectileWaterSpeedFactor(Vector3 position)=>arenaMap==ArenaMapKind.CoastalBay&&InsideWaterFootprint(position)&&position.y<GroundSurfaceY-.1f?.58f:1f;
    Vector3 ProjectileStep(Vector3 from,Vector3 velocity,float dt,float life)
    {
        float travelTime=Mathf.Min(dt,life);
        return velocity*(travelTime*ProjectileWaterSpeedFactor(from+velocity.normalized*travelTime*.5f));
    }
    float CoastalWaveShoreSpeed(Fighter f)
    {
        if(arenaMap!=ArenaMapKind.CoastalBay||f==null||!InsideWaterFootprint(f.root.position)||f.root.position.y>=GroundSurfaceY-.25f)return 0;
        return Mathf.Sin(roundTime*(2*Mathf.PI/11f)+f.root.position.z*.018f)*4.5f;
    }
    float TargetingWeatherFactor()=>arenaMap==ArenaMapKind.OutdoorCanyon&&roundWeather==WeatherKind.Sandstorm?.55f:1f;
    float WeatherLockRange(Fighter fighter)=>fighter!=null&&fighter.spec!=null?fighter.spec.lockRange*TargetingWeatherFactor():0;
    float FireIgnitionMultiplier=>Mathf.Lerp(1f,.2f,Mathf.Clamp01(roundHumidityPercent/100f));
    void BuildCanyonGroundMesh(Transform parent)
    {
        const int divisions=112;const float extent=140;
        var vertices=new Vector3[(divisions+1)*(divisions+1)];var uv=new Vector2[vertices.Length];var triangles=new int[divisions*divisions*6];
        for(int z=0;z<=divisions;z++)for(int x=0;x<=divisions;x++)
        {
            float wx=-extent+2*extent*x/divisions,wz=-extent+2*extent*z/divisions;int index=z*(divisions+1)+x;
            vertices[index]=new Vector3(wx,OutdoorCanyonSurfaceHeight(new Vector3(wx,0,wz)),wz);uv[index]=new Vector2(x/(float)divisions,z/(float)divisions);
        }
        int cursor=0;for(int z=0;z<divisions;z++)for(int x=0;x<divisions;x++)
        {
            int a=z*(divisions+1)+x,b=a+1,c=a+divisions+1,d=c+1;
            triangles[cursor++]=a;triangles[cursor++]=c;triangles[cursor++]=b;triangles[cursor++]=b;triangles[cursor++]=c;triangles[cursor++]=d;
        }
        var mesh=new Mesh{name="Canyon walkable deep-water basins"};mesh.vertices=vertices;mesh.uv=uv;mesh.triangles=triangles;mesh.RecalculateNormals();mesh.RecalculateBounds();
        var ground=new GameObject("Canyon basin ground / walkable slopes");ground.transform.SetParent(parent,false);ground.AddComponent<MeshFilter>().sharedMesh=mesh;
        var renderer=ground.AddComponent<MeshRenderer>();renderer.sharedMaterial=Mat(new Color(.33f,.29f,.24f));
    }
    void BuildCoastalGroundMesh(Transform parent)
    {
        const int divisions=120;const float extent=140;
        var vertices=new Vector3[(divisions+1)*(divisions+1)];var uv=new Vector2[vertices.Length];
        var land=new List<int>();var beach=new List<int>();var seabed=new List<int>();
        for(int z=0;z<=divisions;z++)for(int x=0;x<=divisions;x++)
        {
            float wx=-extent+2*extent*x/divisions,wz=-extent+2*extent*z/divisions;int index=z*(divisions+1)+x;
            vertices[index]=new Vector3(wx,CoastalBaySurfaceHeight(new Vector3(wx,0,wz)),wz);uv[index]=new Vector2(x/(float)divisions,z/(float)divisions);
        }
        for(int z=0;z<divisions;z++)for(int x=0;x<divisions;x++)
        {
            int a=z*(divisions+1)+x,b=a+1,c=a+divisions+1,d=c+1;
            List<int> target=-extent+2*extent*(x+.5f)/divisions<CoastalLandRampStartX?land:
                -extent+2*extent*(x+.5f)/divisions<0?beach:seabed;
            target.Add(a);target.Add(c);target.Add(b);target.Add(b);target.Add(c);target.Add(d);
        }
        var mesh=new Mesh{name="Coastal bay / shallow beach ramp and deep seabed"};mesh.vertices=vertices;mesh.uv=uv;
        mesh.subMeshCount=3;mesh.SetTriangles(land,0);mesh.SetTriangles(beach,1);mesh.SetTriangles(seabed,2);mesh.RecalculateNormals();mesh.RecalculateBounds();
        var ground=new GameObject("Earth coast / walkable beach and underwater floor");ground.transform.SetParent(parent,false);ground.AddComponent<MeshFilter>().sharedMesh=mesh;
        var renderer=ground.AddComponent<MeshRenderer>();renderer.sharedMaterials=new[]{Mat(new Color(.45f,.39f,.26f)),Mat(new Color(.48f,.43f,.31f)),Mat(new Color(.12f,.24f,.27f))};
    }
    float RadarWeatherFactor()=>roundWeather==WeatherKind.Clear?1:roundWeather==WeatherKind.Overcast?.95f:roundWeather==WeatherKind.Rain?.85f:roundWeather==WeatherKind.Thunderstorm?.72f:.46f;
    float rainIntensitySpeed=>precipitationIntensity==PrecipitationIntensity.Light?10f:precipitationIntensity==PrecipitationIntensity.Moderate?18f:28f;
    float PlainsHeight(float x,float z)
    {
        float t=Mathf.SmoothStep(0,1,Mathf.InverseLerp(40,100,new Vector2(x,z).magnitude));
        return t*(Mathf.Sin(x*.013f)*Mathf.Cos(z*.011f)*18+Mathf.Sin(z*.027f+x*.009f)*7);
    }
    struct TerrainFace {public Vector3 a,b,c;}
    readonly Dictionary<Vector2Int,List<TerrainFace>> plainsFaces=new Dictionary<Vector2Int,List<TerrainFace>>();
    void CachePlainsSurface()
    {
        plainsFaces.Clear();
        foreach(MeshFilter filter in starRoot.GetComponentsInChildren<MeshFilter>())
        {
            Vector3[] vertices=filter.sharedMesh.vertices;int[] indices=filter.sharedMesh.triangles;
            for(int i=0;i<indices.Length;i+=3)
            {
                TerrainFace face=new TerrainFace{a=filter.transform.TransformPoint(vertices[indices[i]]),b=filter.transform.TransformPoint(vertices[indices[i+1]]),c=filter.transform.TransformPoint(vertices[indices[i+2]])};
                Vector3 center=(face.a+face.b+face.c)/3;Vector2Int key=new Vector2Int(Mathf.FloorToInt(center.x/5),Mathf.FloorToInt(center.z/5));
                if(!plainsFaces.TryGetValue(key,out List<TerrainFace> list)){list=new List<TerrainFace>(2);plainsFaces[key]=list;}list.Add(face);
            }
        }
        Debug.Log("PLAINS_AUTHORED_SURFACE_CACHED cells="+plainsFaces.Count);
    }
    float PlainsSurface(Vector3 p)
    {
        Vector2Int key=new Vector2Int(Mathf.Clamp(Mathf.FloorToInt(p.x/5),-100,99),Mathf.Clamp(Mathf.FloorToInt(p.z/5),-100,99));
        if(plainsFaces.TryGetValue(key,out List<TerrainFace> faces))foreach(TerrainFace f in faces)
        {
            Vector3 b=f.b-f.a,c=f.c-f.a,d=p-f.a;float denominator=b.x*c.z-b.z*c.x;if(Mathf.Abs(denominator)<.00001f)continue;
            float u=(d.x*c.z-d.z*c.x)/denominator,v=(b.x*d.z-b.z*d.x)/denominator;
            if(u>=-.0001f&&v>=-.0001f&&u+v<=1.0001f)return f.a.y+b.y*u+c.y*v;
        }
        return GroundSurfaceY;
    }
    float ArenaLimit=>arenaMap==ArenaMapKind.OpenPlains?390:ArenaHalfWidth;
    Material TransparentMaterial(Color color,float gloss=0)
    {
        Material m=new Material(hullShader);m.color=color;m.SetFloat("_Mode",3);m.SetInt("_SrcBlend",(int)BlendMode.SrcAlpha);m.SetInt("_DstBlend",(int)BlendMode.OneMinusSrcAlpha);m.SetInt("_ZWrite",0);m.EnableKeyword("_ALPHABLEND_ON");m.renderQueue=3000;m.SetFloat("_Glossiness",gloss);return m;
    }
    void SetPartMaterial(Transform root,Material material)
    {
        foreach(Renderer r in root.GetComponentsInChildren<Renderer>()){r.sharedMaterial=material;r.shadowCastingMode=ShadowCastingMode.Off;}
    }
    void BuildClimate()
    {
        climateSun=GameObject.Find("Distant sun").GetComponent<Light>();
        if(arenaMap==ArenaMapKind.IndoorFactory)return;
        skyDome=SpawnPDPart("SkyDome",null,Vector3.zero,Vector3.one*900);
        skyMaterial=new Material(skyTextureShader);skyMaterial.mainTexture=Resources.Load<Texture2D>("PDParts/SkyGradient");SetPartMaterial(skyDome,skyMaterial);
        view.farClipPlane=1800;
        waterMaterial=new Material(shallowWaterShader){name="Shallow water / soft shoreline",color=new Color(.22f,.40f,.44f,.28f)};waterMaterial.SetFloat("_Glossiness",.82f);waterMaterial.SetFloat("_Metallic",.08f);
        climateRim=GameObject.Find("Cool mech rim light").GetComponent<Light>();
        rippleMaterial=TransparentMaterial(new Color(.65f,.9f,.94f,.55f));rainMaterial=TransparentMaterial(new Color(.68f,.81f,.88f,.28f));
        Vector3[] waterPoints=arenaMap==ArenaMapKind.CoastalBay?
                new[]{new Vector3(52.5f,GroundSurfaceY,0)}:arenaMap==ArenaMapKind.OutdoorCanyon?
            new[]{new Vector3(0,0,80),new Vector3(85,0,-80)}:
            new[]{new Vector3(0,0,0),new Vector3(arenaMap==ArenaMapKind.OpenPlains?25:70,0,0),new Vector3(arenaMap==ArenaMapKind.OpenPlains?-25:-90,0,-5)};
        foreach(Vector3 p in waterPoints)
        {
            Vector3 center=new Vector3(p.x,WaterlineHeight(p)+.025f,p.z);ponds.Add(center);
            if(arenaMap==ArenaMapKind.CoastalBay)
            {
                coastalWaterSurface=Shape(PrimitiveType.Plane,"Ocean surface / non-colliding",null,new Vector3(52.5f,GroundSurfaceY+.025f,0),new Vector3(17.5f,1,28.2f),Color.white);
                Transform sea=coastalWaterSurface;
                Collider seaCollider=sea.GetComponent<Collider>();if(seaCollider!=null)Destroy(seaCollider);SetPartMaterial(sea,waterMaterial);
                for(int i=0;i<14;i++)
                {
                    Transform crest=SpawnPDPart("Ripple",null,Vector3.zero,Vector3.one);SetPartMaterial(crest,rippleMaterial);crest.localScale=Vector3.one*(1.4f+(i%4)*.35f);coastalWaveCrests.Add(crest);
                }
            }
            else if(arenaMap==ArenaMapKind.OutdoorCanyon)
            {
                Transform pond=Shape(PrimitiveType.Cylinder,"Deep canyon water",null,center,new Vector3(CanyonWaterRadius*2,.07f,CanyonWaterRadius*2),Color.white);
                SetPartMaterial(pond,waterMaterial);
            }
            else {Transform pond=SpawnPDPart("ShallowPond_v002",null,center,new Vector3(7,1,5));SetPartMaterial(pond,waterMaterial);}
        }
        for(int i=0;i<24;i++){Transform r=SpawnPDPart("Ripple",null,Vector3.zero,Vector3.one);SetPartMaterial(r,rippleMaterial);r.gameObject.SetActive(false);ripples.Add(r);rippleAge[i]=10;}
        for(int i=0;i<96;i++){Transform r=SpawnPDPart("RainStreak",null,Vector3.zero,Vector3.one);SetPartMaterial(r,rainMaterial);r.gameObject.SetActive(false);rainDrops.Add(r);}
        // Clone only world materials once. Robot atlases are never altered by wetness.
        var cache=new Dictionary<Material,Material>();
        foreach(Renderer r in FindObjectsByType<Renderer>(FindObjectsSortMode.None))
        {
            if(r.transform.IsChildOf(skyDome)||r.sharedMaterial==waterMaterial||r.sharedMaterial==rippleMaterial||r.sharedMaterial==rainMaterial)continue;
            Material[] materials=r.sharedMaterials;
            for(int i=0;i<materials.Length;i++)
            {
                Material original=materials[i];if(original==null||!original.HasProperty("_Glossiness"))continue;
                if(!cache.TryGetValue(original,out Material clone)){clone=new Material(original);cache[original]=clone;wetMaterials.Add(clone);originalGloss.Add(clone.GetFloat("_Glossiness"));originalWetColors.Add(clone.color);}
                materials[i]=clone;
            }
            r.sharedMaterials=materials;
        }
        ReflectionProbe probe=new GameObject("Shallow water reflection probe").AddComponent<ReflectionProbe>();probe.transform.position=ponds[0]+Vector3.up*3;probe.size=new Vector3(200,80,200);probe.resolution=128;probe.mode=ReflectionProbeMode.Realtime;probe.refreshMode=ReflectionProbeRefreshMode.OnAwake;probe.cullingMask=~(1<<8);probe.intensity=.45f;
        ApplyClimate(WeatherKind.Clear,false);
    }
    void ApplyClimate(WeatherKind weather,bool dark)
    {
        roundWeather=weather;evening=dark;
        bool sandstorm=arenaMap==ArenaMapKind.OutdoorCanyon&&weather==WeatherKind.Sandstorm;
        float[] humidityRanges=sandstorm?new[]{5f,34f}:weather==WeatherKind.Clear?new[]{20f,58f}:weather==WeatherKind.Overcast?new[]{42f,76f}:weather==WeatherKind.Rain?new[]{68f,94f}:new[]{82f,100f};
        roundTemperatureC=forcedTemperatureC==99?UnityEngine.Random.Range(-10f,36f):Mathf.Clamp(forcedTemperatureC,-30,50);
        roundHumidityPercent=forcedHumidity<0?UnityEngine.Random.Range(humidityRanges[0],humidityRanges[1]):Mathf.Clamp(forcedHumidity,0,100);
        precipitationIntensity=(PrecipitationIntensity)(forcedPrecipitation<0?UnityEngine.Random.Range(0,3):Mathf.Clamp(forcedPrecipitation,0,2));
        snowing=(weather==WeatherKind.Rain||weather==WeatherKind.Thunderstorm)&&roundTemperatureC<=0;
        if(rainMaterial!=null)rainMaterial.color=sandstorm?new Color(.98f,.72f,.39f,.76f):snowing?new Color(.92f,.97f,1,.82f):new Color(.68f,.81f,.88f,.28f);
        if(arenaMap==ArenaMapKind.IndoorFactory)return;
        bool rainy=weather==WeatherKind.Rain||weather==WeatherKind.Thunderstorm;float cloud=sandstorm?.63f:weather==WeatherKind.Clear?1:weather==WeatherKind.Overcast?.86f:weather==WeatherKind.Rain?.76f:.70f;
        climateSunBase=(dark?.82f:1.15f)*(sandstorm?.86f:cloud);climateSun.intensity=climateSunBase;climateSun.color=sandstorm?new Color(1,.75f,.45f):dark?new Color(1,.72f,.51f):new Color(1,.94f,.85f);
        climateSun.transform.rotation=Quaternion.Euler(dark?18:42,-30,0);
        RenderSettings.ambientSkyColor=Color.Lerp(new Color(.35f,.42f,.51f),new Color(.43f,.48f,.58f),dark?1:0)*(sandstorm?.94f:cloud);
        RenderSettings.ambientEquatorColor=dark?new Color(.42f,.46f,.53f):new Color(.38f,.41f,.45f);
        RenderSettings.ambientGroundColor=dark?new Color(.20f,.22f,.26f):new Color(.21f,.19f,.16f);
        climateSun.shadowStrength=dark?.48f:.65f;
        if(climateRim!=null){climateRim.intensity=dark?.65f:.34f;climateRim.color=dark?new Color(.60f,.70f,.85f):new Color(.42f,.69f,1);climateRim.transform.rotation=Quaternion.Euler(dark?40:28,dark?45:145,0);}
        RenderSettings.fog=true;RenderSettings.fogMode=FogMode.ExponentialSquared;RenderSettings.fogDensity=sandstorm?.012f:rainy?.002f:.0008f;
        RenderSettings.fogColor=sandstorm?new Color(.68f,.50f,.32f):dark?new Color(.29f,.31f,.37f):new Color(.58f,.67f,.72f);
        if(sandstorm&&view!=null)view.backgroundColor=new Color(.48f,.32f,.18f);
        if(arenaMap==ArenaMapKind.CoastalBay){coastalSurfaceFogColor=dark?new Color(.30f,.46f,.53f):new Color(.38f,.63f,.70f);coastalSurfaceFogDensity=.0018f;RenderSettings.fogColor=coastalSurfaceFogColor;RenderSettings.fogDensity=coastalSurfaceFogDensity;}
        skyMaterial.color=sandstorm?new Color(.98f,.72f,.44f):dark?new Color(.49f,.49f,.61f):Color.white*cloud;
        for(int i=0;i<wetMaterials.Count;i++){wetMaterials[i].SetFloat("_Glossiness",rainy?.72f:originalGloss[i]);wetMaterials[i].color=originalWetColors[i]*(rainy?.82f:1);}
        int activeDrops=sandstorm?rainDrops.Count:!rainy?0:Mathf.RoundToInt(rainDrops.Count*(precipitationIntensity==PrecipitationIntensity.Light?.28f:precipitationIntensity==PrecipitationIntensity.Moderate?.62f:1f));
        for(int i=0;i<rainDrops.Count;i++)rainDrops[i].gameObject.SetActive(i<activeDrops);
    }
    void SelectRoundClimate()
    {
        radarHasSeen=false;if(arenaMap==ArenaMapKind.IndoorFactory)return;
        WeatherKind weather;
        if(arenaMap==ArenaMapKind.OutdoorCanyon)
        {
            if(forcedWeather<0){int choice=UnityEngine.Random.Range(0,3);weather=choice==0?WeatherKind.Clear:choice==1?WeatherKind.Overcast:WeatherKind.Sandstorm;}
            else {int choice=Mathf.Clamp(forcedWeather,0,4);weather=choice>=2&&choice<=3?WeatherKind.Sandstorm:(WeatherKind)choice;}
        }
        else weather=(WeatherKind)(forcedWeather<0?UnityEngine.Random.Range(0,4):Mathf.Clamp(forcedWeather,0,3));
        ApplyClimate(weather,forcedDay<0?UnityEngine.Random.Range(0,2)==1:forcedDay==1);
    }
    void CycleForcedWeather()
    {
        if(arenaMap==ArenaMapKind.OutdoorCanyon)forcedWeather=forcedWeather<0?0:forcedWeather==0?1:forcedWeather==1?4:-1;
        else forcedWeather=forcedWeather<0?0:forcedWeather>=3?-1:forcedWeather+1;
        SelectRoundClimate();
    }
    void EmitRipple(Vector3 at,float scale=1)
    {
        if(ripples.Count==0)return;int i=nextRipple++%ripples.Count;rippleAge[i]=0;
        ripples[i].position=at;ripples[i].localScale=Vector3.one*scale;ripples[i].gameObject.SetActive(true);
    }
    void UpdateClimate(float dt)
    {
        if(arenaMap==ArenaMapKind.IndoorFactory||skyDome==null||player==null||paused)return;
        skyDome.position=view.transform.position;
        if(arenaMap==ArenaMapKind.CoastalBay)
        {
            float depth=InsideWaterFootprint(player.root.position)?GroundSurfaceY+.4f-player.root.position.y:0;
            float underwater=Mathf.SmoothStep(0,1,Mathf.InverseLerp(.25f,4f,depth));
            Color deep=new Color(.025f,.17f,.25f);
            RenderSettings.fogColor=Color.Lerp(coastalSurfaceFogColor,deep,underwater);
            RenderSettings.fogDensity=Mathf.Lerp(coastalSurfaceFogDensity,.045f,underwater);
            view.backgroundColor=Color.Lerp(new Color(.38f,.65f,.76f),new Color(.008f,.055f,.09f),underwater);
            climateSun.intensity=climateSunBase*Mathf.Lerp(1,.48f,underwater);
            bool submerged=underwater>.5f;
            if(submerged!=coastalSubmerged)
            {
                int active=roundWeather<WeatherKind.Rain?0:Mathf.RoundToInt(rainDrops.Count*(precipitationIntensity==PrecipitationIntensity.Light?.28f:precipitationIntensity==PrecipitationIntensity.Moderate?.62f:1f));
                for(int i=0;i<rainDrops.Count;i++)rainDrops[i].gameObject.SetActive(!submerged&&i<active);
                skyDome.gameObject.SetActive(!submerged);
                coastalSubmerged=submerged;
            }
            if(coastalWaterSurface!=null)
            {
                float wave=Time.time*2*Mathf.PI/8f;
                coastalWaterSurface.position=new Vector3(52.5f,GroundSurfaceY+.025f+Mathf.Sin(wave)*.055f,0);
            }
            for(int i=0;i<coastalWaveCrests.Count;i++)
            {
                float progress=Mathf.Repeat(i/(float)coastalWaveCrests.Count+Time.time*.026f,1f);
                float z=(i%5-2)*49f+Mathf.Sin(Time.time*.7f+i*1.3f)*3.5f;
                Transform crest=coastalWaveCrests[i];crest.position=new Vector3(Mathf.Lerp(CoastalBayStartX,140,progress),GroundSurfaceY+.09f+Mathf.Sin(Time.time*.9f+i)*.025f,z);
                crest.localScale=Vector3.one*(1.2f+progress*1.5f);
            }
        }
        bool sandstorm=arenaMap==ArenaMapKind.OutdoorCanyon&&roundWeather==WeatherKind.Sandstorm;
        bool rainy=roundWeather==WeatherKind.Rain||roundWeather==WeatherKind.Thunderstorm;
        for(int i=0;i<rainDrops.Count&&(rainy||sandstorm);i++)
        {
            float t=Time.time;
            Vector3 p=sandstorm?
                player.root.position+new Vector3(Mathf.Repeat(i*.71f+t*11f,40)-20,Mathf.Repeat(i*.39f+t*1.2f,16)-7,Mathf.Sin(i*2.91f)*20):
                player.root.position+new Vector3(Mathf.Sin(i*5.73f)*20,Mathf.Repeat(i*.47f-t*(snowing?4:rainIntensitySpeed),22)-2,Mathf.Cos(i*2.91f)*20);
            if(snowing)p.x+=Mathf.Sin(t*.7f+i*1.71f)*1.7f;
            rainDrops[i].position=p;rainDrops[i].rotation=snowing?Quaternion.identity:sandstorm?Quaternion.Euler(0,0,88):Quaternion.Euler(0,0,-12);rainDrops[i].localScale=snowing?Vector3.one*(i%3==0?.17f:.11f):sandstorm?new Vector3(2.4f,.14f,.14f):new Vector3(1,.65f,1);
        }
        if(roundWeather==WeatherKind.Thunderstorm){float phase=Time.time%23;climateSun.intensity=climateSunBase+(phase<.12f?.22f*(1-phase/.12f):0);}
        splashClock+=dt;
        for(int i=0;i<ripples.Count;i++){rippleAge[i]+=dt;if(rippleAge[i]>1.2f){ripples[i].gameObject.SetActive(false);continue;}float scale=.2f+rippleAge[i]*2.2f;ripples[i].localScale=new Vector3(scale,1,scale);}
        Fighter[] actors={player,enemy};
        for(int i=0;i<2;i++)
        {
            Fighter f=actors[i];bool wet=false;
            foreach(Vector3 pond in ponds)
            {
                Vector3 d=f.root.position-pond;
                bool overWater=InsideWaterFootprint(f.root.position)&&(arenaMap==ArenaMapKind.OutdoorCanyon?
                    f.root.position.y-FighterSupportOffset<=GroundSurfaceY+.10f:f.root.position.y-FighterSupportOffset<=pond.y+.10f);
                if(overWater)
                {wet=true;if(!previouslyWet[i]||splashClock>.19f&&f.velocity.magnitude>1)
                    {
                        Vector3 at=new Vector3(f.root.position.x,pond.y+.005f,f.root.position.z);EmitRipple(at);
                        int count=!previouslyWet[i]?8:3;for(int drop=0;drop<count;drop++)EmitPDParticle("WaterDrop",at,new Vector3(Mathf.Sin(drop*2.4f)*2,2+(drop%3),Mathf.Cos(drop*2.4f)*2),.065f,new Color(.28f,.65f,.76f),.6f);
                    }}
            }
            previouslyWet[i]=wet;previousWaterPosition[i]=f.root.position;
        }
        if(splashClock>.2f){if(rainy&&ponds.Count>0){int index=Mathf.FloorToInt(Time.time*5)%ponds.Count;EmitRipple(ponds[index]+new Vector3(Mathf.Sin(Time.time*4)*4,.005f,Mathf.Cos(Time.time*3)*3),.4f);}splashClock=0;}
    }
    void DrawClimateControls()
    {
        if(arenaMap==ArenaMapKind.IndoorFactory)return;
        Text(200,565,800,$"ENV / {roundWeather} / {(evening?"EVENING":"MORNING")} / {roundTemperatureC:0}C / RH {roundHumidityPercent:0}%",small);
        if(GUI.Button(new Rect(200,591,172,27),"WEATHER: "+(forcedWeather<0?"AUTO":((WeatherKind)forcedWeather).ToString().ToUpper())))CycleForcedWeather();
        if(GUI.Button(new Rect(380,591,172,27),"DAY: "+(forcedDay<0?"AUTO":forcedDay==1?"EVENING":"MORNING"))){forcedDay=(forcedDay+2)%3-1;SelectRoundClimate();}
        if(arenaMap!=ArenaMapKind.OutdoorCanyon&&GUI.Button(new Rect(560,591,172,27),"RAIN: "+(forcedPrecipitation<0?"AUTO":((PrecipitationIntensity)forcedPrecipitation).ToString().ToUpper()))){forcedPrecipitation=forcedPrecipitation>=2?-1:forcedPrecipitation+1;ApplyClimate(roundWeather,evening);}
        if(GUI.Button(new Rect(740,591,172,27),"TEMP: "+(forcedTemperatureC==99?"AUTO":forcedTemperatureC<=0?"COLD":"MILD"))){forcedTemperatureC=forcedTemperatureC==99?-8:forcedTemperatureC<=0?12:99;ApplyClimate(roundWeather,evening);}
        if(GUI.Button(new Rect(920,591,172,27),"HUMIDITY: "+(forcedHumidity<0?"AUTO":forcedHumidity+"%"))){forcedHumidity=forcedHumidity<0?95:forcedHumidity>=95?30:-1;ApplyClimate(roundWeather,evening);}
    }
    float SensorRange(Fighter f)=>Mathf.Max(5,f.spec.radarRange*RadarWeatherFactor());
    bool HasSensorContact(Fighter observer,Fighter target)=>Vector3.Distance(observer.root.position,target.root.position)<=SensorRange(observer);
    GUIStyle radarDirectionStyle;
    void DrawSensorRadar()
    {
        Panel(new Rect(1025,110,150,174),new Color(.025f,.045f,.075f,.85f));Text(1035,111,140,"SENSOR / "+Mathf.RoundToInt(SensorRange(player))+"m",small);
        float range=SensorRange(player);Vector3 d=enemy.root.position-player.root.position;Vector2 planar=new Vector2(d.x,-d.z);float distance=d.magnitude;
        Panel(new Rect(1097,200,6,6),Cyan);
        if(HasSensorContact(player,enemy))
        {
            radarHasSeen=true;Vector2 p=planar/range*62;Panel(new Rect(1097+p.x,200+p.y,6,6),Orange);
            if(roundWeather==WeatherKind.Thunderstorm&&Mathf.Repeat(Time.time,4)<.10f||roundWeather==WeatherKind.Sandstorm&&Mathf.Repeat(Time.time,1.7f)<.24f)Panel(new Rect(1038,229,110,2),new Color(.68f,.52f,.34f,.48f));
        }
        else if(outsideRadarPolicy==RadarOutsidePolicy.AlwaysDirection||outsideRadarPolicy==RadarOutsidePolicy.AfterDetection&&radarHasSeen)
        {
            Vector2 p=planar.normalized*61;Matrix4x4 prior=GUI.matrix;
            if(radarDirectionStyle==null)radarDirectionStyle=new GUIStyle(small){fontSize=22,fontStyle=FontStyle.Bold,alignment=TextAnchor.MiddleCenter};
            GUIUtility.RotateAroundPivot(Mathf.Atan2(p.y,p.x)*Mathf.Rad2Deg,new Vector2(1100+p.x,203+p.y));GUI.color=Orange;
            GUI.Label(new Rect(1088+p.x,191+p.y,24,24),">",radarDirectionStyle);GUI.color=Color.white;GUI.matrix=prior;
        }
        else Text(1037,246,138,"OUT OF RANGE",small);
        if(arenaMap!=ArenaMapKind.IndoorFactory)
        {
            Text(1035,266,140,(snowing?"SNOW":roundWeather.ToString()).ToUpper()+(roundWeather==WeatherKind.Rain||roundWeather==WeatherKind.Thunderstorm?" / "+precipitationIntensity.ToString().ToUpper():""),small);
            Text(1035,282,140,$"{roundTemperatureC:0}C / RH {roundHumidityPercent:0}%",small);
        }
    }
    void BuildPlainsCover()
    {
        for(int i=0;i<40;i++)
        {
            float a=i*2.399963f,r=55+(i%9)*32;Vector3 p=new Vector3(Mathf.Cos(a)*r,0,Mathf.Sin(a)*r);float h=2+i%5;
            AddDebris(new Vector3(p.x,SurfaceHeight(p)+h*.5f,p.z),new Vector3(4+i%4,h,4+(i*3)%5),0);
        }
    }
    void BuildCanyonNaturalCover()
    {
        for(int i=0;i<12;i++)
        {
            float a=i*Mathf.PI/6,r=40+(i%3)*20,h=4+i%3*1.5f;
            AddDebris(new Vector3(Mathf.Sin(a)*r,GroundSurfaceY+h*.5f,Mathf.Cos(a)*r),new Vector3(7+i%3,h,8+i%2*2),0);
        }
    }
    void DisposeClimateMaterials()
    {
        foreach(Material m in wetMaterials)if(m!=null)Destroy(m);
        foreach(Material m in new[]{skyMaterial,waterMaterial,rippleMaterial,rainMaterial})if(m!=null)Destroy(m);
    }
    void ClimateChecks(Action<bool,string> check)
    {
        WeatherKind weather=roundWeather;roundWeather=WeatherKind.Clear;check(RadarWeatherFactor()==1,"clear radar full range");roundWeather=WeatherKind.Overcast;check(RadarWeatherFactor()==.95f,"overcast radar 95 percent");roundWeather=WeatherKind.Rain;check(RadarWeatherFactor()==.85f,"rain radar 85 percent");roundWeather=WeatherKind.Thunderstorm;check(RadarWeatherFactor()>=.65f&&RadarWeatherFactor()<=.8f,"storm radar bounded 65 to 80 percent");roundWeather=weather;
        check(outsideRadarPolicy==RadarOutsidePolicy.AlwaysDirection,"PD-approved out-of-range radar always provides direction only");
        check(Mathf.Abs(PlainsHeight(0,0))<.001f&&Mathf.Abs(PlainsHeight(200,0))>5,"plains has safe spawn and large authored elevation");
        check(Loadout[0].speed==264,"terrain and weather do not alter the doubled trial rifle projectile speed");
    }
}
