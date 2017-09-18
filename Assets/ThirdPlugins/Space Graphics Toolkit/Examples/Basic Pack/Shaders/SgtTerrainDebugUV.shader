// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Space Graphics Toolkit/SgtTerrainDebugUV"
{
	SubShader
	{
		Tags
		{
			"Queue"           = "Transparent"
			"RenderType"      = "Transparent"
			"IgnoreProjector" = "True"
		}
		Pass
		{
			CGPROGRAM
				#pragma vertex Vert
				#pragma fragment Frag
				
				struct a2v
				{
					float4 vertex    : POSITION;
					float2 texcoord0 : TEXCOORD0;
					float2 texcoord1 : TEXCOORD1;
				};
				
				struct v2f
				{
					float4 vertex    : SV_POSITION;
					float2 texcoord0 : TEXCOORD0;
					float2 texcoord1 : TEXCOORD1;
				};
				
				struct f2g
				{
					float4 color : COLOR;
				};
				
				void Vert(a2v i, out v2f o)
				{
					o.vertex    = UnityObjectToClipPos(i.vertex);
					o.texcoord0 = i.texcoord0;
					o.texcoord1 = i.texcoord1;
				}
				
				void Frag(v2f i, out f2g o)
				{
					i.texcoord0 = frac(i.texcoord0);
					i.texcoord1 = frac(i.texcoord1);
					
					o.color.r = max(i.texcoord0.x, i.texcoord0.y);
					o.color.g = max(i.texcoord1.x, i.texcoord1.y);
					o.color.b = 0.0f;
					o.color.a = 1.0f;
				}
			ENDCG
		} // Pass
	} // SubShader
} // Shader