using UnityEngine;

public sealed partial class VoxArena
{
    Transform BuildProjectileVisual(Shot shot,Vector3 position)
    {
        Transform root=Module("Projectile",null,position);
        float r=shot.radius;
        Color team=TeamColor(shot.owner), shell=Color.Lerp(team,new Color(.16f,.21f,.25f),.5f);
        Color core=Color.Lerp(team,Color.white,.6f);
        // The colored silhouette stays within the gameplay radius transversely.
        // Trails are cosmetic and do not enlarge the collision sphere.
        if(shot.stopAndExplode)
        {
            Shape(PrimitiveType.Sphere,"Seeker core",root,Vector3.zero,Vector3.one*r*1.35f,core,true);
            ProjectileBand(root,r,team,45); ProjectileBand(root,r,team,-45);
        }
        else if(shot.tracking>0)
        {
            Shape(PrimitiveType.Cube,"Missile casing",root,Vector3.zero,new Vector3(r*.8f,r*.8f,r*1.8f),shell);
            Shape(PrimitiveType.Sphere,"Missile nose",root,new Vector3(0,0,r*.6f),Vector3.one*r*.8f,core,true);
            for(int side=-1;side<=1;side+=2)
                Shape(PrimitiveType.Cube,"Guidance fin",root,new Vector3(side*r*.6f,0,-r*.45f),new Vector3(r*.55f,r*.16f,r*.8f),team);
            Shape(PrimitiveType.Sphere,"Missile exhaust",root,new Vector3(0,0,-r*.9f),Vector3.one*r*.5f,core,true);
        }
        else switch(shot.owner.spec.primary)
        {
            case PrimaryKind.Plasma:
                Shape(PrimitiveType.Sphere,"Plasma shell",root,Vector3.zero,Vector3.one*r*1.85f,team,true);
                ProjectileBand(root,r,new Color(.85f,.95f,1),0); break;
            case PrimaryKind.Grenade:
                Shape(PrimitiveType.Cube,"Explosive jacket",root,Vector3.zero,Vector3.one*r*1.25f,shell);
                Shape(PrimitiveType.Cube,"Armed fuse",root,new Vector3(0,r*.65f,0),new Vector3(r*.9f,r*.12f,r*.4f),core,true);
                Shape(PrimitiveType.Sphere,"Shell nose",root,new Vector3(0,0,r*.55f),Vector3.one*r*.8f,team); break;
            case PrimaryKind.Rail:
                Shape(PrimitiveType.Cube,"Rail core",root,Vector3.zero,new Vector3(r*.65f,r*.65f,r*2),core,true);
                Shape(PrimitiveType.Cube,"Rail sheath",root,Vector3.zero,new Vector3(r*1.1f,r*1.1f,r*.65f),team,true); break;
            case PrimaryKind.Wave:
                Shape(PrimitiveType.Sphere,"Wave nucleus",root,Vector3.zero,Vector3.one*r,core,true);
                ProjectileBand(root,r,team,0); break;
            case PrimaryKind.Scatter:
                Transform shard=Shape(PrimitiveType.Cube,"Fragment",root,Vector3.zero,new Vector3(r,r,r*1.5f),team,true);
                shard.localRotation=Quaternion.Euler(0,0,45); break;
            case PrimaryKind.Gatling:
                Shape(PrimitiveType.Sphere,"Tracer tip",root,Vector3.zero,new Vector3(r*1.3f,r*1.3f,r*1.9f),core,true); break;
            default:
                Shape(PrimitiveType.Cube,"Bolt shell",root,Vector3.zero,new Vector3(r*1.5f,r*1.2f,r*1.8f),team,true);
                Shape(PrimitiveType.Cube,"Bolt core",root,new Vector3(0,0,r*.85f),new Vector3(r*.65f,r*.65f,r*.12f),core,true); break;
        }
        return root;
    }
    void ProjectileBand(Transform root,float radius,Color color,float tilt)
    {
        LineRenderer band=Line("Energy band",color,radius*.14f);
        band.transform.SetParent(root,false); band.useWorldSpace=false;
        Circle(band,Vector3.zero,radius*.88f);
        band.transform.localRotation=Quaternion.Euler(90,tilt,0);
    }
}
