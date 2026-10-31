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
        float shoulder=heavy?1.04f:lightFrame?.74f:.87f;
        Plate("Core inner frame",f.corePart,new Vector3(0,.45f,0),new Vector3(.82f,.95f,.62f),dark);
        Plate("Armor chest",f.corePart,new Vector3(0,.70f,.23f),new Vector3(heavy?1.45f:1.10f,.54f,.60f),armor,-12);
        Plate("Chest collar",f.corePart,new Vector3(0,1.03f,.03f),new Vector3(.85f,.18f,.55f),hull);
        Plate("Reactor glass",f.corePart,new Vector3(0,.62f,.58f),new Vector3(.27f,.22f,.06f),dark);
        Shape(PrimitiveType.Cube,"Reactor light",f.corePart,new Vector3(0,.62f,.62f),new Vector3(.14f,.07f,.025f),armor,true);
        for(int side=-1;side<=1;side+=2)
        {
            Plate("Chest trim",f.corePart,new Vector3(side*.45f,.78f,.55f),new Vector3(.18f,.08f,.06f),light);
            for(int vent=0;vent<3;vent++) Plate("Cooling vent",f.corePart,new Vector3(side*.36f,.34f+vent*.08f,.36f),new Vector3(.24f,.025f,.04f),dark);
            Plate("Armor hip skirt",f.pelvisPart,new Vector3(side*.45f,-.42f,.20f),new Vector3(.28f,.46f,.43f),armor,-16);
        }
        Plate("Waist coupling",f.corePart,new Vector3(0,-.09f,0),new Vector3(.5f,.35f,.43f),dark);
        Plate("Waist rotary bearing",f.pelvisPart,new Vector3(0,-.21f,0),new Vector3(.54f,.10f,.49f),hull);
        Plate("Pelvis",f.pelvisPart,new Vector3(0,-.39f,0),new Vector3(.70f,.29f,.55f),edge);
        // Heads intentionally use different profiles, while sharing the same socket origin.
        Plate("Helmet",f.headPart,Vector3.zero,new Vector3(heavy?.7f:.54f,.47f,.50f),hull);
        Plate("Helmet crown",f.headPart,new Vector3(0,.25f,-.04f),new Vector3(.34f,.13f,.41f),armor,-10);
        Plate("Face mask",f.headPart,new Vector3(0,-.12f,.26f),new Vector3(.28f,.22f,.10f),edge);
        Plate("Sensor housing",f.headPart,new Vector3(0,.02f,.27f),new Vector3(.49f,.15f,.08f),dark);
        Shape(PrimitiveType.Cube,"Visor",f.headPart,new Vector3(0,.03f,.32f),new Vector3(f.frameIndex==5?.16f:.40f,.065f,.025f),armor,true);
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
            Plate("Armor shoulder",arm,new Vector3(side*.12f,.08f,0),new Vector3(heavy?.88f:.58f,heavy?.62f:.45f,.72f),armor,-8);
            Plate("Shoulder cap",arm,new Vector3(side*.13f,.36f,0),new Vector3(heavy?.83f:.52f,.07f,.60f),hull);
            Plate("Upper arm",arm,new Vector3(0,-.37f,0),new Vector3(.30f,.42f,.33f),edge);
            Shape(PrimitiveType.Sphere,"Elbow joint",arm,new Vector3(0,-.58f,.08f),Vector3.one*.29f,dark);
            Plate("Armor forearm",arm,new Vector3(0,-.75f,.24f),new Vector3(.42f,.45f,.64f),hull,-12);
            for(int vent=0;vent<2;vent++)
                Plate("Forearm heat slit",arm,new Vector3(side*.224f,-.64f-vent*.13f,.25f),new Vector3(.025f,.045f,.30f),dark);
            Plate("Elbow servo cover",arm,new Vector3(0,-.56f,-.12f),new Vector3(.24f,.16f,.12f),armor);
            Plate("Wrist trim",arm,new Vector3(0,-.79f,.52f),new Vector3(.45f,.13f,.14f),armor);
            Plate("Manipulator",arm,new Vector3(0,-.83f,.64f),new Vector3(.28f,.28f,.28f),dark);
            if(side>0) f.fist=Plate("Punch gauntlet",arm,new Vector3(0,-.83f,.82f),new Vector3(.42f,.34f,.42f),armor);
            Transform leg=Module(side<0?"Left leg socket":"Right leg socket",f.legsPart,new Vector3(side*.30f,-.56f,0));
            Plate("Thigh",leg,new Vector3(0,-.26f,0),new Vector3(heavy?.53f:.37f,.52f,.43f),hull);
            Plate("Knee coupling",leg,new Vector3(0,-.59f,0),new Vector3(.31f,.22f,.34f),dark);
            Plate("Armor knee",leg,new Vector3(0,-.60f,.25f),new Vector3(.39f,.29f,.18f),armor,-18);
            Plate("Shin",leg,new Vector3(0,-.94f,-.02f),new Vector3(heavy?.57f:.38f,.64f,.46f),hull,-7);
            Plate("Knee piston",leg,new Vector3(0,-.75f,-.25f),new Vector3(.10f,.42f,.10f),edge);
            Plate("Shin side trim",leg,new Vector3(side*.24f,-.98f,.08f),new Vector3(.07f,.42f,.29f),armor);
            Plate("Calf booster",leg,new Vector3(0,-.85f,-.35f),new Vector3(.27f,.46f,.28f),dark,15);
            Shape(PrimitiveType.Cube,"Calf nozzle",leg,new Vector3(0,-1.1f,-.40f),new Vector3(.16f,.08f,.12f),armor,true);
            Plate("Foot",leg,new Vector3(0,-1.34f,.15f),new Vector3(heavy?.61f:.42f,.22f,.76f),dark);
            Plate("Toe armor",leg,new Vector3(0,-1.24f,.38f),new Vector3(heavy?.57f:.39f,.12f,.30f),armor,-10);
            Plate("Backpack booster",f.backPart,new Vector3(side*.38f,0,-.10f),new Vector3(.31f,.90f,.43f),edge);
            Shape(PrimitiveType.Cube,"Backpack nozzle",f.backPart,new Vector3(side*.38f,-.48f,-.14f),new Vector3(.23f,.09f,.27f),armor,true);
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
