Shader "VoxelOn/CameraHeadFade"
{
    Properties
    {
        _MainTex("Base atlas",2D)="white"{}
        _Color("Tint",Color)=(1,1,1,1)
        _EmissionMap("Emission atlas",2D)="black"{}
        _EmissionColor("Emission",Color)=(1,1,1,1)
        _Metallic("Metallic",Range(0,1))=.05
        _Glossiness("Smoothness",Range(0,1))=.14
        _CameraFade("Screen coverage",Range(0,1))=1
    }
    SubShader
    {
        Tags { "RenderType"="TransparentCutout" "Queue"="AlphaTest" }
        ZWrite On
        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows addshadow
        #pragma target 3.5
        sampler2D _MainTex,_EmissionMap;
        fixed4 _Color,_EmissionColor;
        half _Metallic,_Glossiness,_CameraFade;
        struct Input { float2 uv_MainTex; float4 screenPos; };
        void surf(Input IN,inout SurfaceOutputStandard o)
        {
            float2 pixel=floor(IN.screenPos.xy/max(IN.screenPos.w,.00001)*_ScreenParams.xy);
            int x=(int)pixel.x & 3;int y=(int)pixel.y & 3;
            const float4x4 bayer=float4x4(0,8,2,10,12,4,14,6,3,11,1,9,15,7,13,5);
            clip(_CameraFade-(bayer[y][x]+.5)/16);
            fixed4 base=tex2D(_MainTex,IN.uv_MainTex)*_Color;
            o.Albedo=base.rgb;o.Alpha=1;
            o.Emission=tex2D(_EmissionMap,IN.uv_MainTex).rgb*_EmissionColor.rgb;
            o.Metallic=_Metallic;o.Smoothness=_Glossiness;
        }
        ENDCG
    }
    Fallback "Standard"
}
