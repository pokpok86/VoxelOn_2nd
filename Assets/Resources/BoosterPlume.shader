Shader "VoxelOn/BoosterPlume" {
 Properties { _Tint("Tint",Color)=(.4,.8,1,.3) }
 SubShader { Tags { "Queue"="Transparent" "RenderType"="Transparent" } Blend SrcAlpha OneMinusSrcAlpha ZWrite Off Cull Off
 Pass { CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "UnityCG.cginc"
 struct appdata { float4 vertex:POSITION; float4 color:COLOR; };
 struct v2f { float4 vertex:SV_POSITION; float4 color:COLOR; };
 fixed4 _Tint;
 v2f vert(appdata v){v2f o;o.vertex=UnityObjectToClipPos(v.vertex);o.color=v.color;return o;}
 fixed4 frag(v2f i):SV_Target{return i.color*_Tint;}
 ENDCG }
 } }
