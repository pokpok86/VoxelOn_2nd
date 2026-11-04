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
        f.weaponVisuals=new Transform[4];
        f.weaponVisuals[0]=SpawnPDPart("WeaponRifle",f.weaponPart,Vector3.zero,Vector3.one*.8f);
        Transform hand=f.rightArm.Find("Right_elbow/Right_hand");if(hand==null)hand=f.rightArm;
        f.weaponVisuals[1]=SpawnPDPart("WeaponCannon",hand,new Vector3(0,-.03f,.12f),Vector3.one*.62f);
        f.weaponVisuals[2]=SpawnPDPart("WeaponRocket",f.corePart,new Vector3(-.55f,.65f,-.30f),Vector3.one*.65f);
        f.weaponVisuals[3]=SpawnPDPart("WeaponRail",f.corePart,new Vector3(.55f,.65f,-.30f),Vector3.one*.65f);
        f.gun=f.weaponVisuals[0];
    }
    Vector3 WeaponMuzzle(Fighter f)
    {
        if(f.weaponVisuals==null)return f.root.position+f.facing*(f.Radius+.25f);
        int i=WeaponIndex(f);float tip=i==0?1.05f:i==1?1.2f:i==2?.53f:1.48f;
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
