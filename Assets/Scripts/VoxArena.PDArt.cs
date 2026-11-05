using UnityEngine;
public sealed partial class VoxArena
{
    Transform SpawnPDPart(string name,Transform parent,Vector3 position,Vector3 scale)
    {
        GameObject prefab=Resources.Load<GameObject>("PDParts/"+name);
        if(prefab==null)throw new System.InvalidOperationException("Missing Blender-authored PD part: "+name);
        Transform mount=Module("Blender PD / "+name,parent,position);mount.localScale=scale;
        Instantiate(prefab,mount,false);return mount;
    }
    Transform SpawnNaturalRock(Transform parent,Vector3 position,Vector3 scale)
    {
        Transform near=SpawnPDPart("NaturalRock",parent,position,scale);
        GameObject lowPrefab=Resources.Load<GameObject>("PDParts/NaturalRockLOD");
        if(lowPrefab!=null)
        {
            Transform low=Instantiate(lowPrefab,near,false).transform;
            var fine=new System.Collections.Generic.List<Renderer>();foreach(Transform child in near)if(child!=low)fine.AddRange(child.GetComponentsInChildren<Renderer>());
            LODGroup lod=near.gameObject.AddComponent<LODGroup>();lod.SetLODs(new[]{new LOD(.08f,fine.ToArray()),new LOD(.001f,low.GetComponentsInChildren<Renderer>())});lod.RecalculateBounds();
        }
        return near;
    }
    void SetupWeaponVisuals(Fighter f)
    {
        if(f.fist!=null)f.fist.gameObject.SetActive(false);
        if(f.gun!=null)f.gun.gameObject.SetActive(false);
        f.modernWeaponArt=System.Array.IndexOf(System.Environment.GetCommandLineArgs(),"--legacy-weapon-art")<0;
        f.shoulderWeaponRest=new Quaternion[2];f.shoulderWeaponOffsets=new Vector3[2];
        f.weaponVisuals=new Transform[4];f.weaponGripPoints=new Transform[4];f.weaponMuzzlePoints=new Transform[4];
        Transform hand=f.rightArm.Find("Right_elbow/Right_hand");if(hand==null)hand=f.rightArm;
        string[] old={"WeaponRifle","WeaponCannon","WeaponRocket","WeaponRail"};
        Transform[] parents={f.weaponPart,hand,f.modernWeaponArt?f.leftArm:f.corePart,f.modernWeaponArt?f.rightArm:f.corePart};
        Vector3[] offsets={Vector3.zero,new Vector3(0,-.03f,.12f),new Vector3(-.55f,.65f,-.30f),new Vector3(.55f,.65f,-.30f)};
        for(int i=0;i<4;i++)
        {
            if(i>=2)
            {
                Transform shoulder=parents[i];float sign=i==2?-1:1;
                f.shoulderWeaponRest[i-2]=Quaternion.Inverse(shoulder.rotation)*f.root.rotation;
                f.shoulderWeaponOffsets[i-2]=Quaternion.Inverse(shoulder.rotation)*(f.root.right*(sign*.10f)+Vector3.up*.08f-f.root.forward*.48f);
            }
            if(f.weaponProfiles[i].isShield||f.weaponProfiles[i].isUnarmed)
            {
                f.weaponVisuals[i]=Module("Empty or shield inventory slot",parents[i],Vector3.zero);f.weaponGripPoints[i]=f.weaponProfiles[i].isShield?f.guardShield:f.weaponVisuals[i];f.weaponMuzzlePoints[i]=f.weaponGripPoints[i];continue;
            }
            string name=f.modernWeaponArt?WeaponArtResource(f.weaponArtIds!=null?f.weaponArtIds[i]:EquippedArtIds[i]).Substring(8):old[i];
            float scale=f.modernWeaponArt?(i<2?.95f:.75f):(i==0?.8f:i==1?.62f:.65f);
            if(f.modernWeaponArt&&i==0)scale/=Mathf.Max(.01f,f.weaponPart.localScale.x);
            Transform mount=SpawnPDPart(name,parents[i],offsets[i],Vector3.one*scale);f.weaponVisuals[i]=mount;
            if(f.modernWeaponArt){f.weaponGripPoints[i]=FindArtSocket(mount,"GRIP_MAIN");f.weaponMuzzlePoints[i]=FindArtSocket(mount,"MUZZLE");PrepareWeaponMaterials(mount);}
        }
        f.gun=f.weaponVisuals[0];ConnectArmGrips(f);
    }
    Vector3 WeaponMuzzle(Fighter f)
    {
        if(f.weaponVisuals==null)return f.root.position+f.facing*(f.Radius+.25f);
        int i=WeaponIndex(f);if(f.modernWeaponArt&&f.weaponMuzzlePoints[i]!=null)return f.weaponMuzzlePoints[i].position;float tip=i==0?1.05f:i==1?1.2f:i==2?.53f:1.48f;
        return f.weaponVisuals[i].TransformPoint(Vector3.forward*tip);
    }
    float SurfaceHeight(Vector3 p)=>arenaMap==ArenaMapKind.OpenPlains?PlainsSurface(p):GroundSurfaceY;
    Vector3 ReticleAimPoint(Ray ray)
    {
        float nearest=300;
        foreach(Bounds b in obstacles)if(b.IntersectRay(ray,out float hit)&&hit>0)nearest=Mathf.Min(nearest,hit);
        Vector3 oc=ray.origin-enemy.root.position;float along=Vector3.Dot(oc,ray.direction),c=oc.sqrMagnitude-enemy.Radius*enemy.Radius;
        float disc=along*along-c;
        if(disc>=0){float t=-along-Mathf.Sqrt(disc);if(t>0)nearest=Mathf.Min(nearest,t);}
        // Terrain intersection uses the same height as landing; weapons may aim above or below the pilot.
        for(float t=1;t<nearest;t+=1f){Vector3 p=ray.GetPoint(t);if(p.y<=SurfaceHeight(p)){nearest=t;break;}}
        return ray.GetPoint(nearest);
    }
}
