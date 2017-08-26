// UNITY_SHADER_NO_UPGRADE
Shader "HyperealVR/Mirror"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_AlternativeTex("Texture", 2D) = "black" {}
		_UVAspectRatio ("UV aspect ratio", Vector) = (0.0, 1.0, 0.0, 1.0)
		_UVClipFlip ("UV clip flip", Vector) = (0.0, 0.5, 0.0, 1.0)
	}
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _UVAspectRatio;
			float4 _UVClipFlip;
			v2f vert (appdata v)
			{
				v2f o;
#if UNITY_VERSION >= 540
				o.vertex = UnityObjectToClipPos(v.vertex);
#else
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
#endif
				o.uv = v.uv;

				o.uv.x = (v.uv.x - _UVAspectRatio.x) / (_UVAspectRatio.y - _UVAspectRatio.x);
				o.uv.y = (v.uv.y - _UVAspectRatio.z) / (_UVAspectRatio.w - _UVAspectRatio.z);

				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				float2 uv = i.uv;

				uv.x = uv.x * _UVClipFlip.y + _UVClipFlip.x;		//scale horizontal
				uv.y = lerp(uv.y, 1.0 - uv.y, _UVClipFlip.w);		//flip vertical

				fixed4 col = tex2D(_MainTex, uv);
				if (uv.x < _UVClipFlip.x || uv.x > _UVClipFlip.x + _UVClipFlip.y || 
					uv.y < 0.0 || uv.y > 1.0)
					col = fixed4(0, 0, 0, 1);
				return col;
			}
			ENDCG
		}

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			sampler2D _AlternativeTex;
			float4 _UVAspectRatio;
			float4 _UVClipFlip;
			v2f vert(appdata v)
			{
				v2f o;
#if UNITY_VERSION >= 540
				o.vertex = UnityObjectToClipPos(v.vertex);
#else
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
#endif
				o.uv = v.uv;

				o.uv.x = (v.uv.x - _UVAspectRatio.x) / (_UVAspectRatio.y - _UVAspectRatio.x);
				o.uv.y = (v.uv.y - _UVAspectRatio.z) / (_UVAspectRatio.w - _UVAspectRatio.z);

				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				float2 uv = i.uv;

				fixed4 col = fixed4(0, 0, 0, 1);
				uv.x *= 2.0;
				if (uv.x < 1.0)
				{
					col = tex2D(_MainTex, uv);
				}
				else
				{
					uv.x -= 1.0;
					col = tex2D(_AlternativeTex, uv);
				}

				if (uv.x < _UVClipFlip.x || uv.x > _UVClipFlip.x + _UVClipFlip.y ||
					uv.y < 0.0 || uv.y > 1.0)
					col = fixed4(0, 0, 0, 1);
				return col;
			}
			ENDCG
		}
	}
}
