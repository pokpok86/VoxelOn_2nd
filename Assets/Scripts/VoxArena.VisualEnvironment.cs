using UnityEngine;

// Numeric, reproducible visual-only scenery. All elements are outside the
// central combat lane; no physics colliders or navigation bounds are added.
public sealed partial class VoxArena
{
    static readonly Vector3[] skylineSites={
        new Vector3(-55,0,-74),new Vector3(-25,0,-82),
        new Vector3(14,0,-88),new Vector3(51,0,-79),
        new Vector3(-79,0,-28),new Vector3(81,0,-23),
        new Vector3(-76,0,54),new Vector3(80,0,58)
    };
    static readonly float[] skylineHeights={22,33,27,41,25,35,30,24};
    static readonly float[] skylineWidths={9,12,10,13,11,10,12,9};

    void VisualBox(string name,Transform parent,Vector3 pos,Vector3 size,Color color,bool glow=false)
    {
        Transform part=Box(name,parent,pos,size,color);
        // Box is the shared game primitive; remove collider if present so the
        // decoration cannot alter movement, bullets, pickups or AI navigation.
        Collider collider=part.GetComponent<Collider>();
        if(collider!=null) Destroy(collider);
        if(glow)
        {
            Renderer renderer=part.GetComponent<Renderer>();
            if(renderer!=null) renderer.shadowCastingMode=
                UnityEngine.Rendering.ShadowCastingMode.Off;
        }
    }
    void VisualReef(string name,Transform parent,Vector3 pos,Vector3 size,Color color,float yaw)
    {
        Transform rock=Shape(PrimitiveType.Sphere,name,parent,pos,size,color);
        rock.rotation=Quaternion.Euler(0,yaw,0);
        Collider collider=rock.GetComponent<Collider>();if(collider!=null)Destroy(collider);
        Renderer renderer=rock.GetComponent<Renderer>();if(renderer!=null)renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
    }
    void BuildVisualEnvironment()
    {
        if(arenaMap==ArenaMapKind.OpenPlains)return;
        if(arenaMap==ArenaMapKind.OutdoorCanyon) { BuildOutdoorVista(); return; }
        if(arenaMap==ArenaMapKind.CoastalBay) { BuildCoastalVista(); return; }
        if(PlaceEnvironment("FoundryBackdrop",null,new Vector3(0,GroundSurfaceY,0),Vector3.one)!=null) return;
        Transform root=new GameObject("Visual environment - no collision").transform;
        Color concrete=new Color(.27f,.32f,.38f);
        Color panel=new Color(.39f,.43f,.47f);
        Color trim=new Color(.15f,.21f,.27f);
        Color warning=new Color(.95f,.60f,.19f);
        Color lamp=new Color(.27f,.73f,1f);
        Color darkVent=new Color(.09f,.14f,.18f);

        // Distant industrial skyline, each tower assembled from numeric data.
        for(int i=0;i<skylineSites.Length;i++)
        {
            Vector3 site=skylineSites[i];
            float height=skylineHeights[i],width=skylineWidths[i];
            Transform tower=new GameObject("Skyline tower "+i).transform;
            tower.SetParent(root,false);tower.localPosition=site;
            VisualBox("Main tower",tower,new Vector3(0,height*.5f+1.1f,0),
                new Vector3(width,height,width*.78f),concrete);
            VisualBox("Crown",tower,new Vector3(0,height+1.8f,0),
                new Vector3(width+1.8f,1.4f,width*.78f+1.8f),trim);
            VisualBox("Roof antenna",tower,new Vector3(0,height+4.3f,0),
                new Vector3(.3f,4.2f,.3f),panel);
            // Paired roof ventilation units add an industrial silhouette without
            // placing new obstacles or changing any combat collision bounds.
            for(int ventSide=-1;ventSide<=1;ventSide+=2)
            {
                float ventX=ventSide*width*.28f;
                VisualBox("Roof ventilation housing",tower,
                    new Vector3(ventX,height+3.15f,0),
                    new Vector3(width*.22f,1.4f,width*.24f),trim);
                VisualBox("Roof ventilation cap",tower,
                    new Vector3(ventX,height+3.92f,0),
                    new Vector3(width*.27f,.18f,width*.29f),panel);
                VisualBox("Roof ventilation grille",tower,
                    new Vector3(ventX,height+3.14f,width*.125f),
                    new Vector3(width*.15f,.54f,.06f),darkVent);
            }
            // Sparse roof navigation beacons make the skyline legible at night.
            // Mesh-only accents: no light components, colliders or gameplay changes.
            for(int beaconSide=-1;beaconSide<=1;beaconSide+=2)
            {
                VisualBox("Roof beacon pedestal",tower,
                    new Vector3(beaconSide*width*.40f,height+2.65f,0),
                    new Vector3(.30f,.32f,.30f),trim);
                VisualBox("Roof navigation beacon",tower,
                    new Vector3(beaconSide*width*.40f,height+2.88f,0),
                    new Vector3(.20f,.16f,.20f),warning,true);
            }
            // Layered exterior plates give distant towers readable industrial
            // depth. They are non-colliding, numeric, and use the tower's local
            // coordinates so changing the site data moves every detail.
            for(int side=-1;side<=1;side+=2)
            {
                VisualBox("Facade support rail",tower,
                    new Vector3(side*width*.37f,height*.48f+1.1f,width*.39f+.10f),
                    new Vector3(.28f,height*.80f,.24f),panel);
                VisualBox("Side service duct",tower,
                    new Vector3(side*(width*.5f+.24f),height*.42f+1.1f,0),
                    new Vector3(.48f,height*.62f,width*.22f),trim);
            }
            for(int band=0;band<4;band++)
            {
                float y=3.5f+band*(height-5f)/4f;
                VisualBox("Facade maintenance panel",tower,
                    new Vector3(0,y,width*.39f+.13f),
                    new Vector3(width*.38f,.75f,.15f),band%2==0?panel:trim);
            }
            for(int level=0;level<3;level++)
            {
                float y=5+level*(height-7)/3f;
                VisualBox("Recessed floor band",tower,new Vector3(0,y,width*.39f+.07f),
                    new Vector3(width+.2f,.38f,.16f),trim);
                for(int side=-1;side<=1;side+=2)
                    VisualBox("Window beacon",tower,
                        new Vector3(side*width*.27f,y+1.5f,width*.39f+.14f),
                        new Vector3(.65f,.42f,.18f),level==1?warning:lamp,true);
            }
        }
        // Low-profile floor markings are visual only and leave the arena open.
        for(int i=-5;i<=5;i++)
        {
            float x=i*7f;
            VisualBox("West lane stripe "+i,root,new Vector3(x,1.105f,-10),
                new Vector3(3.6f,.018f,.20f),warning);
            VisualBox("East lane stripe "+i,root,new Vector3(x,1.105f,10),
                new Vector3(3.6f,.018f,.20f),warning);
        }
        // Perimeter pylons: strong readable silhouettes without blocking play.
        for(int side=-1;side<=1;side+=2)
            for(int i=-3;i<=3;i++)
            {
                float x=side*66f,z=i*19f;
                Transform pylon=new GameObject("Perimeter pylon").transform;
                pylon.SetParent(root,false);pylon.localPosition=new Vector3(x,0,z);
                VisualBox("Pylon",pylon,new Vector3(0,5.1f,0),
                    new Vector3(1.5f,8f,1.5f),trim);
                VisualBox("Signal",pylon,new Vector3(0,9.25f,.82f),
                    new Vector3(.75f,.6f,.12f),warning,true);
                VisualBox("Pylon collar",pylon,new Vector3(0,8.8f,0),
                    new Vector3(2.3f,.45f,2.3f),panel);
            }
    }
    void BuildCoastalVista()
    {
        Transform root=new GameObject("Earth coastal harbor / visual only").transform;
        Color concrete=new Color(.20f,.31f,.34f),steel=new Color(.16f,.26f,.31f),rust=new Color(.61f,.34f,.19f),lamp=new Color(.34f,.79f,.85f);
        for(int i=0;i<4;i++)
        {
            float x=-92+i*26,z=(i%2==0?-126:126),height=25+i%2*6;
            VisualBox("Harbor crane tower",root,new Vector3(x,height*.5f,z),new Vector3(2.4f,height,2.4f),steel);
            VisualBox("Harbor crane crossarm",root,new Vector3(x+8,height,z),new Vector3(19,1.6f,2.2f),rust);
            VisualBox("Crane hanging cable",root,new Vector3(x+14,height*.72f,z),new Vector3(.22f,height*.55f,.22f),steel);
            VisualBox("Crane hook",root,new Vector3(x+14,height*.43f,z),new Vector3(1.1f,.7f,.65f),lamp,true);
        }
        for(int i=0;i<7;i++)
        {
            float x=-133+i*17;
            VisualBox("Coastal warehouse silhouette",root,new Vector3(x,7.5f,-137),new Vector3(13,15,8),concrete);
            VisualBox("Warehouse roof",root,new Vector3(x,15.2f,-137),new Vector3(14,1,9),steel);
        }
        for(int i=0;i<16;i++)
        {
            float x=20+(i%4)*27,z=-108+(i/4)*70,y=CoastalBaySurfaceHeight(new Vector3(x,0,z));
            Color reef=i%2==0?new Color(.10f,.31f,.33f):new Color(.20f,.35f,.31f);
            VisualReef("Undersea reef boulder",root,new Vector3(x,y+1.05f,z),new Vector3(4+(i%3),2.1f+(i%4),3+(i*3%4)),reef,i*17);
            VisualReef("Reef outcrop",root,new Vector3(x+2.2f,y+.45f,z+1.4f),new Vector3(2.5f,1.3f,2.1f),Color.Lerp(reef,new Color(.38f,.49f,.33f),.45f),i*29);
        }
    }
}
