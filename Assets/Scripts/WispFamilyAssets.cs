using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

// Evaluated Blender surfaces, cached across fighters and portrait rebuilds.
public static class WispFamilyAssets
{
    [Serializable] public sealed class Node { public string name,parent,motion;public float[] position;public int side,front; }
    [Serializable] public sealed class Surface { public string name;public float[] color,emission;public float metallic,smoothness; }
    [Serializable] public sealed class Group { public string node;public int material;public float[] positions,normals;public int[] triangles; }
    [Serializable] public sealed class Asset {public string id,category;public float waistHeight,waistBottom,neckTop;public float[] leftElbow,rightElbow,backMount;public Node[] nodes;public Surface[] materials;public Group[] groups;}
    public sealed class Instance {public Transform root;public Asset data;public readonly Dictionary<string,Transform> nodes=new Dictionary<string,Transform>();}
    sealed class Cached {public Asset data;public Mesh[] meshes;public Material[] materials;}
    static readonly Dictionary<string,Cached> cache=new Dictionary<string,Cached>();
    public static Vector3 V(float[] p)=>p!=null&&p.Length>=3?new Vector3(p[0],p[1],p[2]):Vector3.zero;
    public static bool Available=>Resources.Load<TextAsset>("WispFamily20261112/catalog")!=null;
    static Cached Load(string id)
    {
        if(cache.TryGetValue(id,out Cached found))return found;
        TextAsset text=Resources.Load<TextAsset>("WispFamily20261112/"+id);
        if(text==null)throw new InvalidOperationException("WISP family asset missing: "+id);
        var data=JsonUtility.FromJson<Asset>(text.text);var c=new Cached{data=data,meshes=new Mesh[data.groups.Length],materials=new Material[data.materials.Length]};
        for(int i=0;i<c.materials.Length;i++)
        {
            Surface s=data.materials[i];var m=new Material(Shader.Find("Standard")){name=id+" / "+s.name};
            m.color=new Color(s.color[0],s.color[1],s.color[2],s.color.Length>3?s.color[3]:1);
            m.SetFloat("_Metallic",s.metallic);m.SetFloat("_Glossiness",s.smoothness);
            Color glow=new Color(s.emission[0],s.emission[1],s.emission[2]);
            if(glow.maxColorComponent>0){m.EnableKeyword("_EMISSION");m.SetColor("_EmissionColor",glow);}
            m.enableInstancing=true;c.materials[i]=m;
        }
        for(int i=0;i<c.meshes.Length;i++)
        {
            Group g=data.groups[i];int count=g.positions.Length/3;var vertices=new Vector3[count];var normals=new Vector3[count];
            for(int v=0;v<count;v++){int j=v*3;vertices[v]=new Vector3(g.positions[j],g.positions[j+1],g.positions[j+2]);normals[v]=new Vector3(g.normals[j],g.normals[j+1],g.normals[j+2]);}
            var mesh=new Mesh{name=id+" / "+g.node,indexFormat=count>65535?IndexFormat.UInt32:IndexFormat.UInt16};mesh.vertices=vertices;mesh.normals=normals;mesh.triangles=g.triangles;mesh.RecalculateBounds();c.meshes[i]=mesh;
        }
        cache.Add(id,c);return c;
    }
    public static Asset Data(string id)=>Load(id).data;
    public static Instance Spawn(string id,Transform parent)
    {
        Cached c=Load(id);var instance=new Instance{root=new GameObject("WISP family / "+id).transform,data=c.data};instance.root.SetParent(parent,false);
        foreach(Node n in c.data.nodes)
        {
            Transform t=n.name=="Root"?instance.root:new GameObject(n.name).transform;
            if(t!=instance.root)t.SetParent(string.IsNullOrEmpty(n.parent)?instance.root:instance.nodes[n.parent],false);
            t.localPosition=V(n.position);instance.nodes.Add(n.name,t);
        }
        for(int i=0;i<c.data.groups.Length;i++)
        {
            Group g=c.data.groups[i];var go=new GameObject(g.node+" surface "+i);go.transform.SetParent(instance.nodes[g.node],false);
            go.AddComponent<MeshFilter>().sharedMesh=c.meshes[i];go.AddComponent<MeshRenderer>().sharedMaterial=c.materials[g.material];
        }
        return instance;
    }
}
