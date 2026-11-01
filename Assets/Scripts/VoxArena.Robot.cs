using UnityEngine;

// All geometry below is local to a detachable module. Gameplay stats stay in CraftSpec
// until the assembly system is introduced; visual changes never silently change balance.
public sealed partial class VoxArena
{
    Transform Module(string name,Transform parent,Vector3 position)
    {
        Transform t=new GameObject(name).transform; t.SetParent(parent,false); t.localPosition=position; return t;
    }
    Transform Plate(string name,Transform parent,Vector3 p,Vector3 size,Color color,float tilt=0)
    {
        Transform t=Box(name,parent,p,size,color); t.localRotation=Quaternion.Euler(tilt,0,0); return t;
    }
    // Eight-sided silhouette with a recessed top and bottom edge.
    // Mesh is built in normalized local space; Box handles existing materials.
    // Visual mesh only: collision and gameplay use the original arena logic.
    Transform BeveledPlate(string name,Transform parent,Vector3 p,Vector3 size,
        Color color,float tilt=0,float bevel=.16f,float taper=.88f)
    {
        Transform t=Plate(name,parent,p,size,color,tilt);
        MeshFilter filter=t.GetComponent<MeshFilter>();
        if(filter==null) return t;
        Mesh mesh=new Mesh();
        float[] heights={-.5f,-.5f+bevel,.5f-bevel,.5f};
        float[] widths={taper,1f,1f,taper};
        Vector3[] vertices=new Vector3[16];
        for(int ring=0;ring<4;ring++)
        {
            float half=widths[ring]*.5f;
            vertices[ring*4+0]=new Vector3(-half,heights[ring],-half);
            vertices[ring*4+1]=new Vector3( half,heights[ring],-half);
            vertices[ring*4+2]=new Vector3( half,heights[ring], half);
            vertices[ring*4+3]=new Vector3(-half,heights[ring], half);
        }
        int[] triangles=new int[84];
        int n=0;
        // Winding is outward-facing when viewed from outside the plate.
        for(int ring=0;ring<3;ring++)
            for(int side=0;side<4;side++)
            {
                int a=ring*4+side,b=ring*4+(side+1)%4;
                int c=(ring+1)*4+side,d=(ring+1)*4+(side+1)%4;
                triangles[n++]=a;triangles[n++]=c;triangles[n++]=b;
                triangles[n++]=b;triangles[n++]=c;triangles[n++]=d;
            }
        triangles[n++]=0;triangles[n++]=1;triangles[n++]=2;
        triangles[n++]=0;triangles[n++]=2;triangles[n++]=3;
        triangles[n++]=12;triangles[n++]=14;triangles[n++]=13;
        triangles[n++]=12;triangles[n++]=15;triangles[n++]=14;
        mesh.vertices=vertices;
        mesh.triangles=triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        filter.sharedMesh=mesh;
        return t;
    }
    void BuildRobot(Fighter f)
    {
        if(f.body!=null) { f.body.gameObject.SetActive(false); Destroy(f.body.gameObject); }
        f.body=Module("Modular robot",f.root,Vector3.zero);
        f.corePart=Module("Core module",f.body,Vector3.zero);
        f.headPart=Module("Head module",f.body,new Vector3(0,1.34f,.02f));
        f.armsPart=Module("Arm mounting frame (separate left and right sockets)",f.body,Vector3.zero);
        f.legsPart=Module("Legs and pelvis module",f.body,Vector3.zero);
        f.pelvisPart=Module("Pelvis socket",f.legsPart,Vector3.zero);
        f.backPart=Module("Backpack module",f.corePart,new Vector3(0,.42f,-.52f));
        Color armor=f.spec.color, hull=new Color(.55f,.62f,.68f), dark=new Color(.065f,.09f,.12f);
        Color edge=new Color(.28f,.35f,.40f), light=new Color(.85f,.9f,.91f);
        bool heavy=f.frameIndex==1||f.frameIndex==7;
        bool lightFrame=f.frameIndex==2||f.frameIndex==5;
        bool kestrelSd=f.frameIndex==0;
        float shoulder=kestrelSd?1.00f:heavy?1.04f:lightFrame?.74f:.87f;
        Plate("Core inner frame",f.corePart,new Vector3(0,.45f,0),new Vector3(kestrelSd?.94f:.82f,.95f,kestrelSd?.70f:.62f),dark);
        BeveledPlate("Armor chest",f.corePart,new Vector3(0,.70f,.23f),
            new Vector3(heavy?1.45f:kestrelSd?1.36f:1.10f,kestrelSd?.64f:.54f,kestrelSd?.68f:.60f),armor,-12);
        Plate("Chest collar",f.corePart,new Vector3(0,1.03f,.03f),new Vector3(.85f,.18f,.55f),hull);
        Plate("Reactor glass",f.corePart,new Vector3(0,.62f,.58f),new Vector3(.27f,.22f,.06f),dark);
        Shape(PrimitiveType.Cube,"Reactor light",f.corePart,new Vector3(0,.62f,.62f),new Vector3(.14f,.07f,.025f),armor,true);
        for(int side=-1;side<=1;side+=2)
        {
            Plate("Chest trim",f.corePart,new Vector3(side*.45f,.78f,.55f),new Vector3(.18f,.08f,.06f),light);
            for(int vent=0;vent<3;vent++) Plate("Cooling vent",f.corePart,new Vector3(side*.36f,.34f+vent*.08f,.36f),new Vector3(.24f,.025f,.04f),dark);
            BeveledPlate("Armor hip skirt",f.pelvisPart,new Vector3(side*(kestrelSd?.50f:.45f),-.42f,.20f),
                new Vector3(kestrelSd?.34f:.28f,kestrelSd?.50f:.46f,kestrelSd?.47f:.43f),armor,-16);
        }
        Plate("Waist coupling",f.corePart,new Vector3(0,-.09f,0),new Vector3(.5f,.35f,.43f),dark);
        Plate("Waist rotary bearing",f.pelvisPart,new Vector3(0,-.21f,0),new Vector3(.54f,.10f,.49f),hull);
        Plate("Pelvis",f.pelvisPart,new Vector3(0,-.39f,0),new Vector3(kestrelSd?.82f:.70f,kestrelSd?.32f:.29f,kestrelSd?.61f:.55f),edge);
        // Heads intentionally use different profiles, while sharing the same socket origin.
        BeveledPlate("Helmet",f.headPart,Vector3.zero,
            new Vector3(kestrelSd?.76f:heavy?.7f:.54f,kestrelSd?.64f:.47f,kestrelSd?.61f:.50f),hull);
        Plate("Helmet crown",f.headPart,new Vector3(0,kestrelSd?.32f:.25f,-.04f),
            new Vector3(kestrelSd?.48f:.34f,kestrelSd?.16f:.13f,kestrelSd?.49f:.41f),armor,-10);
        Plate("Face mask",f.headPart,new Vector3(0,-.12f,.29f),new Vector3(kestrelSd?.39f:.28f,.22f,.12f),edge);
        Plate("Sensor housing",f.headPart,new Vector3(0,.02f,.30f),new Vector3(kestrelSd?.62f:.49f,kestrelSd?.17f:.15f,.08f),dark);
        Shape(PrimitiveType.Cube,"Visor",f.headPart,new Vector3(0,.03f,.35f),new Vector3(f.frameIndex==5?.16f:kestrelSd?.52f:.40f,kestrelSd?.075f:.065f,.025f),armor,true);
        if(f.frameIndex==5) Plate("Range finder",f.headPart,new Vector3(.31f,.1f,.20f),new Vector3(.17f,.19f,.50f),dark);
        if(!heavy)
            for(int side=-1;side<=1;side+=2)
            {
                Transform fin=Plate("Sensor fin",f.headPart,new Vector3(side*.28f,.27f,0),new Vector3(.06f,.38f,.09f),armor);
                fin.localRotation=Quaternion.Euler(0,0,side*-25);
            }
        for(int side=-1;side<=1;side+=2)
        {
            Transform arm=Module(side<0?"Left arm socket":"Right arm socket",f.armsPart,new Vector3(side*shoulder,.74f,0));
            Plate("Shoulder hinge",arm,Vector3.zero,new Vector3(.28f,.36f,.42f),dark);
            BeveledPlate("Armor shoulder",arm,new Vector3(side*(kestrelSd?.16f:.12f),.08f,0),
                new Vector3(heavy?.88f:kestrelSd?.78f:.58f,heavy?.62f:kestrelSd?.58f:.45f,kestrelSd?.78f:.72f),armor,-8);
            Plate("Shoulder cap",arm,new Vector3(side*(kestrelSd?.16f:.13f),.36f,0),
                new Vector3(heavy?.83f:kestrelSd?.73f:.52f,.07f,kestrelSd?.68f:.60f),hull);
            Plate("Upper arm",arm,new Vector3(0,-.37f,0),new Vector3(.30f,.42f,.33f),edge);
            Shape(PrimitiveType.Sphere,"Elbow joint",arm,new Vector3(0,-.58f,.08f),Vector3.one*.29f,dark);
            BeveledPlate("Armor forearm",arm,new Vector3(0,-.75f,.24f),new Vector3(.42f,.45f,.64f),hull,-12);
            for(int vent=0;vent<2;vent++)
                Plate("Forearm heat slit",arm,new Vector3(side*.224f,-.64f-vent*.13f,.25f),new Vector3(.025f,.045f,.30f),dark);
            Plate("Elbow servo cover",arm,new Vector3(0,-.56f,-.12f),new Vector3(.24f,.16f,.12f),armor);
            Plate("Wrist trim",arm,new Vector3(0,-.79f,.52f),new Vector3(.45f,.13f,.14f),armor);
            Plate("Manipulator",arm,new Vector3(0,-.83f,.64f),new Vector3(.28f,.28f,.28f),dark);
            if(side>0) f.fist=Plate("Punch gauntlet",arm,new Vector3(0,-.83f,.82f),new Vector3(.42f,.34f,.42f),armor);
            Transform leg=Module(side<0?"Left leg socket":"Right leg socket",f.legsPart,new Vector3(side*.30f,-.56f,0));
            BeveledPlate("Thigh",leg,new Vector3(0,-.26f,0),new Vector3(heavy?.53f:.37f,.52f,.43f),hull);
            Plate("Knee coupling",leg,new Vector3(0,-.59f,0),new Vector3(.31f,.22f,.34f),dark);
            Plate("Armor knee",leg,new Vector3(0,-.60f,.25f),new Vector3(.39f,.29f,.18f),armor,-18);
            BeveledPlate("Shin",leg,new Vector3(0,-.94f,-.02f),new Vector3(heavy?.57f:.38f,.64f,.46f),hull,-7);
            Plate("Knee piston",leg,new Vector3(0,-.75f,-.25f),new Vector3(.10f,.42f,.10f),edge);
            Plate("Shin side trim",leg,new Vector3(side*.24f,-.98f,.08f),new Vector3(.07f,.42f,.29f),armor);
            // Frame 01: a recessed shin intake and two floating armor lips.
            // Attached to the existing leg socket; animation and hitboxes are untouched.
            if(f.frameIndex==0)
            {
                Plate("Shin intake recess",leg,new Vector3(0,-1.00f,.228f),
                    new Vector3(.22f,.28f,.025f),dark);
                for(int slit=0;slit<3;slit++)
                    Plate("Shin intake grille "+slit,leg,
                        new Vector3(0,-.90f-slit*.075f,.248f),
                        new Vector3(.17f,.022f,.018f),edge);
                BeveledPlate("Shin upper guard",leg,new Vector3(0,-.70f,.235f),
                    new Vector3(.34f,.12f,.13f),armor,-8);
                Plate("Shin lower guard",leg,new Vector3(0,-1.20f,.22f),
                    new Vector3(.32f,.075f,.12f),hull);
            }
            Plate("Calf booster",leg,new Vector3(0,-.85f,-.35f),new Vector3(.27f,.46f,.28f),dark,15);
            Shape(PrimitiveType.Cube,"Calf nozzle",leg,new Vector3(0,-1.1f,-.40f),new Vector3(.16f,.08f,.12f),armor,true);
            Plate("Foot",leg,new Vector3(0,-1.34f,kestrelSd?.18f:.15f),
                new Vector3(heavy?.61f:kestrelSd?.64f:.42f,kestrelSd?.25f:.22f,kestrelSd?.92f:.76f),dark);
            BeveledPlate("Toe armor",leg,new Vector3(0,kestrelSd?-1.21f:-1.24f,kestrelSd?.43f:.38f),
                new Vector3(heavy?.57f:kestrelSd?.59f:.39f,kestrelSd?.14f:.12f,kestrelSd?.40f:.30f),armor,-10);
            Plate("Backpack booster",f.backPart,new Vector3(side*.38f,0,-.10f),new Vector3(.31f,.90f,.43f),edge);
            Shape(PrimitiveType.Cube,"Backpack nozzle",f.backPart,new Vector3(side*.38f,-.48f,-.14f),new Vector3(.23f,.09f,.27f),armor,true);
            // Frame 01 only: recessed armored thruster cassette and service detailing.
            // These decorative pieces remain parented to the existing backpack socket.
            if(f.frameIndex==0)
            {
                Plate("Thruster outer rail",f.backPart,new Vector3(side*.58f,.04f,-.13f),
                    new Vector3(.075f,.83f,.42f),dark);
                Plate("Thruster upper service cap",f.backPart,new Vector3(side*.38f,.49f,-.13f),
                    new Vector3(.40f,.10f,.49f),hull);
                Plate("Thruster lower guard",f.backPart,new Vector3(side*.38f,-.38f,-.38f),
                    new Vector3(.39f,.12f,.11f),dark);
                for(int rib=0;rib<3;rib++)
                    Plate("Thruster heat rib "+rib,f.backPart,
                        new Vector3(side*.38f,.22f-rib*.19f,-.365f),
                        new Vector3(.30f,.055f,.09f),hull);
                Plate("Thruster status lamp",f.backPart,new Vector3(side*.38f,.37f,-.375f),
                    new Vector3(.14f,.07f,.04f),armor);
            }
            if(f.frameIndex==0||f.frameIndex==6)
            {
                Transform wing=Plate("Stabilizer",f.backPart,new Vector3(side*.69f,.21f,-.2f),new Vector3(.17f,.9f,.30f),armor,-20);
                wing.localRotation=Quaternion.Euler(-20,0,side*-30);
            }
            if(f.frameIndex==3||f.frameIndex==7)
            {
                Transform pod=Plate("Launcher pod",f.backPart,new Vector3(side*.71f,.70f,.07f),new Vector3(.49f,.55f,.85f),dark,-12);
                for(int n=0;n<4;n++) Shape(PrimitiveType.Cube,"Missile cap",pod,new Vector3(n%2*.44f-.22f,n/2*.40f-.20f,.51f),new Vector3(.27f,.25f,.05f),armor);
            }
            if(side<0) { f.leftArm=arm;f.leftLeg=leg; } else {f.rightArm=arm;f.rightLeg=leg;}
        }
        f.weaponPart=Module("Right weapon socket",f.rightArm,new Vector3(0,-.75f,.70f));
        f.gun=Module("Primary weapon module",f.weaponPart,Vector3.zero);
        bool longGun=f.frameIndex==5; float length=longGun?2.6f:heavy?1.55f:1.05f;
        Plate("Weapon receiver",f.gun,new Vector3(0,0,.26f),new Vector3(heavy?.58f:.34f,.34f,.80f),dark);
        Plate("Weapon casing",f.gun,new Vector3(0,.20f,.30f),new Vector3(.30f,.09f,.63f),armor);
        if(f.frameIndex==2||f.frameIndex==4)
        {
            for(int n=0;n<(f.frameIndex==2?3:2);n++)
                Plate("Multi barrel",f.gun,new Vector3((n-1)*.13f,0,.9f),new Vector3(.09f,.12f,.76f),edge);
        }
        else
        {
            Plate("Barrel",f.gun,new Vector3(0,0,length*.64f),new Vector3(longGun?.15f:.27f,.20f,length),edge);
            Plate("Muzzle",f.gun,new Vector3(0,0,length*1.12f),new Vector3(.35f,.30f,.18f),dark);
            Shape(PrimitiveType.Cube,"Bore glow",f.gun,new Vector3(0,0,length*1.12f+.10f),new Vector3(.19f,.13f,.03f),armor,true);
        }
        Transform leftWeapon=Module("Left weapon socket",f.leftArm,new Vector3(0,-.7f,.7f));
        Plate("Secondary emitter",leftWeapon,Vector3.zero,new Vector3(.26f,.30f,.54f),dark);
        f.saber=Shape(PrimitiveType.Cube,"Energy saber",leftWeapon,new Vector3(0,0,1.4f),new Vector3(.13f,.13f,2.8f),armor,true);
        f.saber.gameObject.SetActive(false);
        f.shield=Module("Shield flash",f.body,new Vector3(0,.2f,0));
        for(int axis=0;axis<3;axis++)
        {
            LineRenderer field=Line("Shield contour",armor,.055f); field.transform.SetParent(f.shield,false);
            field.useWorldSpace=false; Circle(field,Vector3.zero,1.55f);
            field.transform.localRotation=axis==0?Quaternion.identity:Quaternion.Euler(axis==1?90:0,0,axis==2?90:0);
        }
        f.shield.gameObject.SetActive(false);
        // Visual-only model-kit pass: raised seams, colored vents, and readable
        // silhouette accents. No colliders or combat parameters are changed.
        Color seam=new Color(.12f,.17f,.21f);
        Color lamp=Color.Lerp(armor,Color.white,.55f);
        Plate("Chest armor split",f.corePart,new Vector3(0,.79f,.548f),new Vector3(.045f,.32f,.045f),seam,-12);
        Plate("Lower chest seam",f.corePart,new Vector3(0,.47f,.49f),new Vector3(.71f,.035f,.05f),seam);
        Plate("Core status indicator",f.corePart,new Vector3(0,.64f,.665f),new Vector3(.10f,.05f,.025f),lamp);
        for(int side=-1;side<=1;side+=2)
        {
            Plate("Chest shoulder gasket",f.corePart,new Vector3(side*.65f,.81f,.03f),
                new Vector3(.12f,.37f,.46f),dark,side*8);
            Plate("Hip armor accent",f.pelvisPart,new Vector3(side*.46f,-.35f,.43f),
                new Vector3(.19f,.045f,.035f),light);
            Transform arm=side<0?f.leftArm:f.rightArm;
            Transform leg=side<0?f.leftLeg:f.rightLeg;
            Plate("Shoulder inset seam",arm,new Vector3(side*.13f,.12f,.367f),
                new Vector3(heavy?.60f:.39f,.035f,.025f),seam);
            Plate("Shoulder unit marking",arm,new Vector3(side*.12f,.23f,.375f),
                new Vector3(.16f,.055f,.025f),lamp);
            Plate("Forearm armor seam",arm,new Vector3(0,-.72f,.565f),
                new Vector3(.29f,.035f,.025f),seam);
            Plate("Knee running light",leg,new Vector3(0,-.59f,.356f),
                new Vector3(.19f,.045f,.025f),lamp);
            Plate("Shin center ridge",leg,new Vector3(0,-.96f,.223f),
                new Vector3(.055f,.40f,.035f),armor,-7);
            Plate("Toe split line",leg,new Vector3(0,-1.175f,.54f),
                new Vector3(.03f,.035f,.20f),seam);
            Plate("Backpack caution stripe",f.backPart,new Vector3(side*.38f,.27f,-.325f),
                new Vector3(.19f,.06f,.035f),lamp);
        }
        Plate("Helmet eyebrow",f.headPart,new Vector3(0,.13f,.32f),
            new Vector3(heavy?.62f:.48f,.055f,.09f),armor,-8);
        Plate("Chin guard",f.headPart,new Vector3(0,-.245f,.305f),
            new Vector3(.21f,.065f,.095f),dark);
        // Prototype the 01 standard unit's higher-resolution voxel skin.
        // Keep the existing modular skeleton, joints and gameplay hit volumes.
        if(f.frameIndex==0)
        {
            Color ceramic=new Color(.81f,.82f,.78f);
            Color steel=new Color(.19f,.25f,.34f);
            Color highlight=new Color(.94f,.89f,.76f);
            // Staggered small plates form a layered visor brow and chest shell.
            for(int x=-3;x<=3;x++)
            {
                float px=x*.155f;
                float height=.09f+(3-Mathf.Abs(x))*.022f;
                Plate("01 stepped helmet brow",f.headPart,
                    new Vector3(px,.20f+height*.25f,.29f),
                    new Vector3(.146f,height,.17f),x%2==0?ceramic:highlight);
            }
            for(int x=-3;x<=3;x++)
            {
                float px=x*.155f;
                float depth=.60f-Mathf.Abs(x)*.027f;
                Plate("01 segmented breastplate",f.corePart,
                    new Vector3(px,.88f,depth),
                    new Vector3(.145f,.13f,.105f),x%3==0?steel:ceramic,-12);
            }
            // 01 torso shoulder-root fairings: segmented floating armor over
            // the fixed core, leaving the moving arm sockets unobstructed.
            // These are purely visual; collision and CraftSpec are unchanged.
            for(int side=-1;side<=1;side+=2)
            {
                BeveledPlate("01 upper chest shoulder fairing",f.corePart,
                    new Vector3(side*.43f,.89f,.31f),
                    new Vector3(.28f,.19f,.35f),ceramic,-14,.12f,.86f);
                Plate("01 fairing dark undercut",f.corePart,
                    new Vector3(side*.45f,.76f,.48f),
                    new Vector3(.23f,.055f,.045f),steel,-14);
                for(int rib=0;rib<2;rib++)
                    Plate("01 collar vent rib",f.corePart,
                        new Vector3(side*(.27f+rib*.11f),1.045f,.23f),
                        new Vector3(.055f,.035f,.19f),rib==0?steel:highlight);
            }
            for(int side=-1;side<=1;side+=2)
            {
                Transform arm=side<0?f.leftArm:f.rightArm;
                Transform leg=side<0?f.leftLeg:f.rightLeg;
                for(int row=0;row<3;row++)
                {
                    Plate("01 shoulder armor tile",arm,
                        new Vector3(side*.13f,.27f-row*.14f,.38f),
                        new Vector3(.42f-row*.035f,.12f,.095f),
                        row==1?steel:ceramic);
                    Plate("01 shin layered armor",leg,
                        new Vector3(0,-.76f-row*.19f,.235f),
                        new Vector3(.34f-row*.025f,.17f,.10f),
                        row==1?steel:ceramic,-7);
                }
                Plate("01 backpack exhaust guard",f.backPart,
                    new Vector3(side*.38f,-.31f,-.35f),
                    new Vector3(.29f,.20f,.13f),steel);
                // Frame 01 rear heat exchanger: shallow recessed fins, kept
                // behind the core to avoid changing the forward silhouette.
                for(int fin=0;fin<4;fin++)
                {
                    Plate("01 rear radiator fin",f.backPart,
                        new Vector3(side*.38f,.12f-fin*.105f,-.355f),
                        new Vector3(.23f,.038f,.045f),fin==0?highlight:steel);
                }
                // Mechanical shoulder identification stripes follow arm animation.
                for(int stripe=0;stripe<2;stripe++)
                {
                    Plate("01 shoulder hazard stripe",arm,
                        new Vector3(side*.13f,.33f-stripe*.065f,.445f),
                        new Vector3(.19f,.028f,.025f),stripe==0?highlight:steel);
                }
                // Rear-facing stacked nozzle trim: follows the backpack module,
                // and is decorative only (existing boost logic is untouched).
                for(int ring=0;ring<3;ring++)
                {
                    float width=.34f-ring*.055f;
                    Plate("01 booster nozzle collar",f.backPart,
                        new Vector3(side*.38f,-.50f-ring*.065f,-.16f),
                        new Vector3(width,.045f,width),ring==1?highlight:steel);
                }
                Plate("01 booster exhaust core",f.backPart,
                    new Vector3(side*.38f,-.68f,-.16f),
                    new Vector3(.12f,.025f,.12f),new Color(.28f,.77f,.95f));
                // Side-mounted knee actuator and mechanical inset stay attached
                // to the leg module so movement does not leave parts behind.
                Plate("01 knee side actuator",leg,
                    new Vector3(side*.235f,-.59f,-.045f),
                    new Vector3(.085f,.19f,.20f),steel);
                Plate("01 knee actuator pin",leg,
                    new Vector3(side*.29f,-.59f,-.045f),
                    new Vector3(.035f,.09f,.09f),highlight);
                // 01 ankle articulation: shallow guards, exposed hinge blocks
                // and a toe-side maintenance seam. Parent to the existing leg
                // module, not the root, so animation and collision stay intact.
                Plate("01 ankle flexible gaiter",leg,
                    new Vector3(0,-1.185f,-.015f),
                    new Vector3(.29f,.085f,.36f),steel);
                for(int ankleSide=-1;ankleSide<=1;ankleSide+=2)
                {
                    Plate("01 ankle pivot bracket",leg,
                        new Vector3(ankleSide*.235f,-1.225f,.045f),
                        new Vector3(.075f,.19f,.24f),ceramic);
                    Plate("01 ankle hinge pin",leg,
                        new Vector3(ankleSide*.28f,-1.225f,.065f),
                        new Vector3(.035f,.075f,.085f),highlight);
                    Plate("01 toe outer guard",leg,
                        new Vector3(ankleSide*.215f,-1.275f,.425f),
                        new Vector3(.09f,.10f,.25f),ceramic,-10);
                }
                Plate("01 toe maintenance seam",leg,
                    new Vector3(0,-1.169f,.525f),
                    new Vector3(.25f,.018f,.035f),steel,-10);
                Plate("01 shoulder hinge cover",arm,
                    new Vector3(side*.37f,-.02f,-.02f),
                    new Vector3(.085f,.24f,.29f),steel);
            }
        }
        AddFrameIdentity(f,armor,hull,dark,edge);
    }
    RenderTexture[] framePortraits;
    void BuildPortraits()
    {
        framePortraits=new RenderTexture[8];
        Fighter model=MakeFighter("Portrait studio robot",Cyan); model.root.position=new Vector3(0,60,0);
        Camera camera=new GameObject("Portrait studio camera").AddComponent<Camera>();
        camera.enabled=false; camera.orthographic=true; camera.orthographicSize=2.9f; camera.clearFlags=CameraClearFlags.SolidColor;
        camera.backgroundColor=new Color(.065f,.095f,.14f,1); camera.cullingMask=1<<8;
        camera.transform.position=model.root.position+new Vector3(5,3.5f,7); camera.transform.LookAt(model.root.position+Vector3.up*.1f);
        for(int i=0;i<8;i++)
        {
            ApplyCharacter(model,i); model.body.localPosition=Vector3.up*.4f;
            model.gun.gameObject.SetActive(true); model.fist.gameObject.SetActive(false);
            foreach(Transform t in model.body.GetComponentsInChildren<Transform>()) t.gameObject.layer=8;
            framePortraits[i]=new RenderTexture(192,224,16); camera.targetTexture=framePortraits[i]; camera.Render();
        }
        camera.targetTexture=null; Destroy(camera.gameObject); Destroy(model.root.gameObject); Destroy(model.thruster.gameObject); Destroy(model.shadow.gameObject);
    }
}
