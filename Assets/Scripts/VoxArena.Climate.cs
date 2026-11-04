using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
public sealed partial class VoxArena
{
    public enum WeatherKind{Clear,Overcast,Rain,Thunderstorm}
    public enum RadarOutsidePolicy{PendingPDApproval,AlwaysDirection,AfterDetection}
    [Header("Weather test overrides: -1 = random each round")]
    public int forcedWeather=-1,forcedDay=-1;
    public RadarOutsidePolicy outsideRadarPolicy=RadarOutsidePolicy.AlwaysDirection;
    public Shader skyTextureShader,shallowWaterShader;
    WeatherKind roundWeather;bool evening,radarHasSeen;
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
    float RadarWeatherFactor()=>roundWeather==WeatherKind.Clear?1:roundWeather==WeatherKind.Overcast?.95f:roundWeather==WeatherKind.Rain?.85f:.72f;
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
        foreach(Vector3 p in new[]{new Vector3(0,0,0),new Vector3(arenaMap==ArenaMapKind.OpenPlains?25:70,0,0),new Vector3(arenaMap==ArenaMapKind.OpenPlains?-25:-90,0,-5)})
        {
            Vector3 center=new Vector3(p.x,SurfaceHeight(p)+.025f,p.z);ponds.Add(center);
            Transform pond=SpawnPDPart("ShallowPond_v002",null,center,new Vector3(7,1,5));SetPartMaterial(pond,waterMaterial);
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
        if(arenaMap==ArenaMapKind.IndoorFactory)return;
        bool rainy=weather>=WeatherKind.Rain;float cloud=weather==WeatherKind.Clear?1:weather==WeatherKind.Overcast?.86f:weather==WeatherKind.Rain?.76f:.70f;
        climateSunBase=(dark?.82f:1.15f)*cloud;climateSun.intensity=climateSunBase;climateSun.color=dark?new Color(1,.72f,.51f):new Color(1,.94f,.85f);
        climateSun.transform.rotation=Quaternion.Euler(dark?18:42,-30,0);
        RenderSettings.ambientSkyColor=Color.Lerp(new Color(.35f,.42f,.51f),new Color(.43f,.48f,.58f),dark?1:0)*cloud;
        RenderSettings.ambientEquatorColor=dark?new Color(.42f,.46f,.53f):new Color(.38f,.41f,.45f);
        RenderSettings.ambientGroundColor=dark?new Color(.20f,.22f,.26f):new Color(.21f,.19f,.16f);
        climateSun.shadowStrength=dark?.48f:.65f;
        if(climateRim!=null){climateRim.intensity=dark?.65f:.34f;climateRim.color=dark?new Color(.60f,.70f,.85f):new Color(.42f,.69f,1);climateRim.transform.rotation=Quaternion.Euler(dark?40:28,dark?45:145,0);}
        RenderSettings.fog=true;RenderSettings.fogMode=FogMode.ExponentialSquared;RenderSettings.fogDensity=weather>=WeatherKind.Rain?.002f:.0008f;
        RenderSettings.fogColor=dark?new Color(.29f,.31f,.37f):new Color(.58f,.67f,.72f);
        skyMaterial.color=dark?new Color(.49f,.49f,.61f):Color.white*cloud;
        for(int i=0;i<wetMaterials.Count;i++){wetMaterials[i].SetFloat("_Glossiness",rainy?.72f:originalGloss[i]);wetMaterials[i].color=originalWetColors[i]*(rainy?.82f:1);}
        foreach(Transform rain in rainDrops)rain.gameObject.SetActive(rainy);
    }
    void SelectRoundClimate()
    {
        radarHasSeen=false;if(arenaMap==ArenaMapKind.IndoorFactory)return;
        ApplyClimate((WeatherKind)(forcedWeather<0?UnityEngine.Random.Range(0,4):Mathf.Clamp(forcedWeather,0,3)),forcedDay<0?UnityEngine.Random.Range(0,2)==1:forcedDay==1);
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
        bool rainy=roundWeather>=WeatherKind.Rain;
        for(int i=0;i<rainDrops.Count&&rainy;i++)
        {
            float t=Time.time;
            Vector3 p=player.root.position+new Vector3(Mathf.Sin(i*5.73f)*20,Mathf.Repeat(i*.47f-t*22,22)-2,Mathf.Cos(i*2.91f)*20);
            rainDrops[i].position=p;rainDrops[i].rotation=Quaternion.Euler(0,0,-12);rainDrops[i].localScale=new Vector3(1,.65f,1);
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
                if(d.x*d.x/49+d.z*d.z/25<1&&f.root.position.y-FighterSupportOffset<=pond.y+.10f)
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
        Text(200,565,800,"ENV TEST / round fixed: "+roundWeather+" / "+(evening?"EVENING":"MORNING"),small);
        if(GUI.Button(new Rect(200,591,220,27),"WEATHER OVERRIDE")){forcedWeather=(forcedWeather+2)%5-1;SelectRoundClimate();}
        if(GUI.Button(new Rect(430,591,220,27),"DAY OVERRIDE")){forcedDay=(forcedDay+2)%3-1;SelectRoundClimate();}
        Text(670,591,395,"RADAR OUTSIDE: DIRECTION ONLY",small);
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
            if(roundWeather==WeatherKind.Thunderstorm&&Mathf.Repeat(Time.time,4)<.10f)Panel(new Rect(1038,229,110,2),new Color(.5f,.6f,.65f,.3f));
        }
        else if(outsideRadarPolicy==RadarOutsidePolicy.AlwaysDirection||outsideRadarPolicy==RadarOutsidePolicy.AfterDetection&&radarHasSeen)
        {
            Vector2 p=planar.normalized*61;Matrix4x4 prior=GUI.matrix;
            if(radarDirectionStyle==null)radarDirectionStyle=new GUIStyle(small){fontSize=22,fontStyle=FontStyle.Bold,alignment=TextAnchor.MiddleCenter};
            GUIUtility.RotateAroundPivot(Mathf.Atan2(p.y,p.x)*Mathf.Rad2Deg,new Vector2(1100+p.x,203+p.y));GUI.color=Orange;
            GUI.Label(new Rect(1088+p.x,191+p.y,24,24),">",radarDirectionStyle);GUI.color=Color.white;GUI.matrix=prior;
        }
        else Text(1037,246,138,"OUT OF RANGE",small);
        if(arenaMap!=ArenaMapKind.IndoorFactory)Text(1035,266,140,roundWeather.ToString().ToUpper(),small);
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
        check(Loadout[0].speed==132,"terrain and weather do not alter trial rifle projectile speed");
    }
}
