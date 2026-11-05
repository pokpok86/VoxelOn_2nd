using System.Collections.Generic;
using UnityEngine;
public sealed partial class VoxArena
{
    readonly Dictionary<Material,Material> simpleShellMaterials=new Dictionary<Material,Material>();
    bool SimpleShellModel(GameObject model)=>model!=null&&(model.name=="WISP_v007"||model.name=="WISP_v008"||model.name=="WISP_v009"||model.name=="WISP_v010"||model.name=="WISP_v014"||model.name=="WISP_v013"||model.name=="WISP_v012"||model.name=="WISP_v011"||model.name=="BASTION_v002"||model.name=="BASTION_v003"||model.name=="BASTION_v004"||model.name=="BASTION_v005"||model.name=="BASTION_v009"||model.name=="BASTION_v010"||model.name=="BASTION_v008"||model.name=="BASTION_v007"||model.name=="BASTION_v006"||model.name=="TARANTULA_v002"||model.name=="TARANTULA_v003"||model.name=="TARANTULA_v004"||model.name=="TARANTULA_v008"||model.name=="TARANTULA_v007"||model.name=="TARANTULA_v006"||model.name=="TARANTULA_v005");
    void ApplySimpleShellMaterials(Transform root)
    {
        foreach(Renderer renderer in root.GetComponentsInChildren<Renderer>(true))
        {
            Material[] list=renderer.sharedMaterials;
            for(int i=0;i<list.Length;i++)
            {
                Material source=list[i];if(source==null)continue;
                if(!simpleShellMaterials.TryGetValue(source,out Material material))
                {
                    material=new Material(hullShader){name=source.name+" / plain shell"};
                    Color color=source.HasProperty("_Color")?source.color:new Color(.84f,.85f,.80f);
                    if(source.name.Contains("Ivory"))color=new Color(.84f,.85f,.80f);
                    else if(source.name.Contains("Orange"))color=new Color(.98f,.33f,.035f);
                    else if(source.name.Contains("Dark"))color=new Color(.045f,.055f,.052f);
                    else if(source.name.Contains("Trim"))color=new Color(.20f,.24f,.22f);
                    else if(source.name.Contains("Eyes"))color=new Color(.035f,.66f,.73f);
                    material.mainTexture=null;material.color=color;material.SetFloat("_Metallic",.08f);material.SetFloat("_Glossiness",.20f);
                    if(source.name.Contains("Eyes")){material.EnableKeyword("_EMISSION");material.SetColor("_EmissionColor",color*1.3f);}
                    simpleShellMaterials[source]=material;
                }
                list[i]=material;
            }
            renderer.sharedMaterials=list;
        }
    }
    bool HoseFollowsKnee(Fighter f)
    {
        foreach(SkinnedMeshRenderer skin in f.body.GetComponentsInChildren<SkinnedMeshRenderer>())
        {
            if(!skin.name.Contains("Rear service assembly"))continue;Debug.Log("HOSE_SKIN "+skin.name);bool blended=false;int endpoint=-1;
            BoneWeight[] weights=skin.sharedMesh.boneWeights;
            for(int i=0;i<weights.Length;i++)
            {
                BoneWeight w=weights[i];if(w.weight1>.01f&&(skin.bones[w.boneIndex0].name=="Left_knee"||skin.bones[w.boneIndex1].name=="Left_knee"))blended=true;
                if(w.weight0>.99f&&skin.bones[w.boneIndex0].name=="Left_knee")endpoint=i;
            }
            Debug.Log("HOSE_WEIGHTS blended="+blended+" endpoint="+endpoint);if(!blended||endpoint<0)return false;Transform knee=f.leftLeg.Find("Left_knee");Quaternion rest=knee.localRotation;
            Mesh before=new Mesh(),after=new Mesh();skin.BakeMesh(before);knee.localRotation=Quaternion.Euler(25,0,0)*rest;skin.BakeMesh(after);knee.localRotation=rest;
            float displacement=0;for(int i=0;i<weights.Length;i++)if(weights[i].weight0>.99f&&skin.bones[weights[i].boneIndex0].name=="Left_knee")displacement=Mathf.Max(displacement,Vector3.Distance(before.vertices[i],after.vertices[i]));Debug.Log("HOSE_KNEE_DISPLACEMENT "+displacement);bool moved=displacement>.02f;Destroy(before);Destroy(after);return moved;
        }
        return false;
    }
    bool SimplePaletteReady(Fighter f)
    {
        bool ivory=false,orange=false,eyes=false;int vertices=0;
        foreach(SkinnedMeshRenderer skin in f.body.GetComponentsInChildren<SkinnedMeshRenderer>())
        {
            vertices+=skin.sharedMesh.vertexCount;
            foreach(Material m in skin.sharedMaterials)
            {
                if(m.mainTexture!=null){Debug.Log("SIMPLE_SHELL_UNEXPECTED_TEXTURE "+m.name+" texture="+m.mainTexture.name);return false;}
                if(m.name.Contains("Ivory"))ivory=true;
                if(m.name.Contains("Orange"))orange=true;
                if(m.name.Contains("Eyes")&&m.IsKeywordEnabled("_EMISSION"))eyes=true;
            }
        }
        Debug.Log("SIMPLE_SHELL_PALETTE vertices="+vertices+" ivory="+ivory+" orange="+orange+" emissiveEyes="+eyes);
        return ivory&&orange&&eyes&&vertices<15000;
    }
}
