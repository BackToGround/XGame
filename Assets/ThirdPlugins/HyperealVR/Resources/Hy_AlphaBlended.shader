// UNITY_SHADER_NO_UPGRADE
Shader "HyperealVR/AlphaBlended"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		// No culling or depth
		Tags{ "QUEUE" = "Transparent" "RenderType" = "Transparent" }
		Cull Back ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;

			v2f vert (appdata_base v)
			{
				v2f o;
#if UNITY_VERSION >= 540
				o.vertex = UnityObjectToClipPos(v.vertex);
#else
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
#endif
				o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
				return o;
			}

			float4 frag (v2f i) : COLOR
			{
				float4 col = tex2D(_MainTex, i.uv);
				return col;
			}
			ENDCG
		}
	}
}
