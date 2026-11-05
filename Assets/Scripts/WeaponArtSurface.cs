using UnityEngine;
public static class WeaponArtSurface
{
    public static void Configure(Material material)
    {
        Color color=material.color;string name=material.name;material.shader=Shader.Find("Standard");material.color=color;
        material.SetFloat("_Metallic",name.Contains("Steel")||name.Contains("Machined")?.35f:.08f);material.SetFloat("_Glossiness",.35f);
        if(name.Contains("Glow_")){material.EnableKeyword("_EMISSION");material.SetColor("_EmissionColor",new Color(color.r,color.g,color.b)*1.8f);}
        if(name.Contains("Glass_"))
        {
            color.a=.24f;material.color=color;material.SetFloat("_Mode",3);material.SetInt("_SrcBlend",(int)UnityEngine.Rendering.BlendMode.SrcAlpha);material.SetInt("_DstBlend",(int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);material.SetInt("_ZWrite",0);material.EnableKeyword("_ALPHABLEND_ON");material.renderQueue=3000;
        }
    }
}
