using UnityEngine;

public sealed partial class VoxArena
{
    void AddFrameIdentity(Fighter f,Color armor,Color hull,Color dark,Color trim)
    {
        // Distinct silhouettes live under the existing replaceable module sockets.
        switch(f.frameIndex)
        {
            case 0: // Balanced cross-fire: paired stabilizers and two side emitters.
                for(int side=-1;side<=1;side+=2)
                {
                    Transform fin=Plate("Cross-fire stabilizer",f.backPart,new Vector3(side*.86f,.03f,-.25f),new Vector3(.72f,.13f,.42f),armor);
                    fin.localRotation=Quaternion.Euler(0,side*20,side*12);
                    Plate("Side emitter",f.gun,new Vector3(side*.23f,.04f,.70f),new Vector3(.15f,.18f,.70f),trim);
                }
                break;
            case 1: // Siege chassis: armored shoulder collar and a broad plasma muzzle.
                foreach(Transform arm in new[]{f.leftArm,f.rightArm})
                {
                    Plate("Siege shoulder skirt",arm,new Vector3(0,-.23f,.10f),new Vector3(.78f,.54f,.88f),armor,-8);
                    for(int i=0;i<3;i++) Plate("Heat sink",arm,new Vector3((i-1)*.20f,.43f,-.15f),new Vector3(.09f,.25f,.65f),dark);
                }
                Plate("Plasma coil housing",f.gun,new Vector3(0,0,1.1f),new Vector3(.67f,.59f,.69f),hull);
                Shape(PrimitiveType.Cube,"Plasma aperture",f.gun,new Vector3(0,0,1.46f),new Vector3(.43f,.35f,.03f),armor,true);
                Plate("Siege brow",f.headPart,new Vector3(0,.17f,.27f),new Vector3(.75f,.14f,.22f),armor);
                break;
            case 2: // Light assault: swept helmet, rear blade sheath, wrist gatling.
                Plate("Assault crest",f.headPart,new Vector3(0,.29f,-.12f),new Vector3(.11f,.20f,.72f),armor,-18);
                Transform sheath=Plate("Blade sheath",f.backPart,new Vector3(-.30f,-.05f,-.43f),new Vector3(.16f,1.65f,.17f),dark);
                sheath.localRotation=Quaternion.Euler(0,0,-28);
                Shape(PrimitiveType.Cube,"Sheath edge",sheath,new Vector3(.51f,0,0),new Vector3(.05f,.8f,.7f),armor,true);
                f.leftArm.localRotation=Quaternion.Euler(-12,0,-10);
                break;
            case 3: // Missile carrier: visible ammunition racks and a compact radar mast.
                Transform radar=Module("Radar mount",f.backPart,new Vector3(0,1.03f,-.18f));
                Plate("Radar mast",radar,Vector3.zero,new Vector3(.12f,.72f,.12f),trim);
                Shape(PrimitiveType.Cylinder,"Radar dish",radar,new Vector3(0,.37f,0),new Vector3(.93f,.06f,.60f),hull);
                for(int side=-1;side<=1;side+=2)
                    for(int i=0;i<3;i++) Plate("Spare rocket",f.backPart,new Vector3(side*.73f,.50f,-.48f+i*.15f),new Vector3(.16f,.55f,.12f),armor);
                break;
            case 4: // Breacher: drum magazine, broad short muzzle, reinforced knees.
                Transform drum=Shape(PrimitiveType.Cylinder,"Shotgun drum",f.gun,new Vector3(0,-.32f,.23f),new Vector3(.57f,.20f,.57f),dark);
                drum.localRotation=Quaternion.Euler(0,0,90);
                Plate("Shotgun block",f.gun,new Vector3(0,0,1.03f),new Vector3(.57f,.26f,.18f),armor);
                foreach(Transform leg in new[]{f.leftLeg,f.rightLeg})
                    Plate("Breach kneepad",leg,new Vector3(0,-.57f,.30f),new Vector3(.56f,.37f,.19f),armor,-15);
                break;
            case 5: // Rail frame: split rails and a long-range sensor, asymmetric shoulder.
                for(int side=-1;side<=1;side+=2)
                {
                    Plate("Accelerator rail",f.gun,new Vector3(side*.16f,.11f,1.89f),new Vector3(.07f,.08f,1.85f),armor);
                    Shape(PrimitiveType.Cube,"Rail conduit",f.gun,new Vector3(side*.17f,.16f,1.88f),new Vector3(.025f,.025f,1.5f),armor,true);
                }
                Plate("Sensor shoulder",f.leftArm,new Vector3(-.13f,.25f,-.13f),new Vector3(.39f,.50f,.92f),dark,-15);
                Plate("Targeting antenna",f.headPart,new Vector3(-.32f,.40f,-.13f),new Vector3(.055f,.75f,.055f),hull);
                break;
            case 6: // Energy pursuit: three contained field emitters instead of conventional pods.
                for(int i=-1;i<=1;i++)
                {
                    Transform cell=Module("Field emitter",f.backPart,new Vector3(i*.65f,.66f+Mathf.Abs(i)*.10f,-.30f));
                    Plate("Emitter casing",cell,Vector3.zero,new Vector3(.23f,.83f,.30f),dark,-15);
                    Shape(PrimitiveType.Cube,"Emitter light",cell,new Vector3(0,.11f,.17f),new Vector3(.10f,.49f,.04f),armor,true);
                }
                Plate("Wave emitter",f.gun,new Vector3(0,0,1.1f),new Vector3(.54f,.54f,.14f),trim);
                break;
            case 7: // Area control: dense leg braces and canister storage.
                foreach(Transform leg in new[]{f.leftLeg,f.rightLeg})
                {
                    Plate("Heavy calf brace",leg,new Vector3(0,-.98f,-.15f),new Vector3(.69f,.52f,.61f),dark,-5);
                    Plate("Stability skid",leg,new Vector3(0,-1.34f,.12f),new Vector3(.71f,.15f,1.02f),trim);
                }
                for(int i=-1;i<=1;i++)
                    Shape(PrimitiveType.Cylinder,"Mine canister",f.backPart,new Vector3(i*.32f,-.36f,-.38f),new Vector3(.26f,.28f,.26f),armor);
                Plate("Blast muzzle",f.gun,new Vector3(0,0,1.75f),new Vector3(.63f,.53f,.28f),hull);
                break;
        }
    }
}
