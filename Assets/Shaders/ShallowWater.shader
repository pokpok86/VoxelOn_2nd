Shader "VoxelOn/ShallowWater"
{
    Properties { _Color("Water tint",Color)=(0.22,0.40,0.44,0.28) _Glossiness("Smoothness",Range(0,1))=0.82 _Metallic("Metallic",Range(0,1))=0.08 }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        LOD 200
        Cull Off ZWrite Off
        CGPROGRAM
        #pragma surface surf Standard alpha:fade vertex:vert
        #pragma target 3.0
        struct Input { float4 color:COLOR; };
        fixed4 _Color;half _Glossiness,_Metallic;
        void vert(inout appdata_full v,out Input o){UNITY_INITIALIZE_OUTPUT(Input,o);o.color=v.color;}
        void surf(Input IN,inout SurfaceOutputStandard o)
        {
            o.Albedo=_Color.rgb*IN.color.rgb;o.Alpha=_Color.a*IN.color.a;o.Metallic=_Metallic;o.Smoothness=_Glossiness;
        }
        ENDCG
    }
    FallBack "Transparent/Diffuse"
}
