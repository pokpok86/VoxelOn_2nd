using System.Collections.Generic;
using UnityEngine;

// Blender-authored source: ArtSource/Environment/VoxelOn_Environment_v001.blend.
// Runtime only places FBX assets; collision remains the analytical arena bounds.
public sealed partial class VoxArena
{
    readonly List<Mesh> environmentMeshes=new List<Mesh>();
    readonly Dictionary<string,GameObject> environmentPrefabs=new Dictionary<string,GameObject>();
    int environmentFallbacks;
    Transform PlaceEnvironment(string name,Transform parent,Vector3 position,Vector3 scale)
    {
        if(!environmentPrefabs.TryGetValue(name,out GameObject prefab))
        {
            prefab=Resources.Load<GameObject>("Environment/"+name); environmentPrefabs[name]=prefab;
        }
        if(prefab==null) { environmentFallbacks++; Debug.LogError("Missing Blender environment FBX: "+name); return null; }
        // Preserve FBX axis/unit conversion on its own transform. Apply gameplay
        // placement and dimensions on a Unity-aligned parent, never overwrite
        // the imported model's rotation or its authored scale.
        Transform placement=Module("Blender FBX / "+name,parent,position);placement.localScale=scale;
        Instantiate(prefab,placement,false);
        return placement;
    }
    void BuildVoxelCover(Transform root,Vector3 size,int kind)
    {
        float bottom=SurfaceHeight(root.position)-root.position.y,top=size.y*.5f;
        if(arenaMap!=ArenaMapKind.IndoorFactory){SpawnNaturalRock(root,new Vector3(0,bottom,0),new Vector3(size.x,top-bottom,size.z));return;}
        string asset=arenaMap==ArenaMapKind.OutdoorCanyon?"CanyonRock":kind%3==0?"FoundryCargo":kind%3==1?"FoundryGenerator":"FoundryBarricade";
        PlaceEnvironment(asset,root,new Vector3(0,bottom,0),new Vector3(size.x,top-bottom,size.z));
    }
    void BuildOutdoorFloorDetails(Transform root)
    {
        PlaceEnvironment("OutdoorGround",root,new Vector3(0,GroundSurfaceY,0),Vector3.one);
    }
    void BuildOutdoorVista()
    {
        PlaceEnvironment("CanyonVista",null,new Vector3(0,GroundSurfaceY,0),Vector3.one);
    }
}
