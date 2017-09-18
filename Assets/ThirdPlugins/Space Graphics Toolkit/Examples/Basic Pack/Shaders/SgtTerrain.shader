Shader "Space Graphics Toolkit/SgtTerrain"
{
	Properties
	{
		_Color("Color", Color) = (1,1,1,1)
		_Glossiness("Glossiness", Range(0,1)) = 1.0
		_Metallic("Metallic", Range(0,1)) = 1.0
		[Toggle(SGT_A)] _FragmentBlending("Fragment Blending", Float) = 0

		_BaseTex("Base Tex", 2D) = "white" {}
		[NoScaleOffset]_BaseBump("Base Bump", 2D) = "normal" {}
		_BaseBumpScale("Base Bump Scale", Float) = 1.0

		_LandMin("Land Min", Float) = 10.0
		_LandMax("Land Max", Float) = 11.0
		[NoScaleOffset]_LandTex("Land Tex", 2D) = "white" {}
		[NoScaleOffset]_LandBump("Land Bump", 2D) = "normal" {}
		_LandBumpScale("Land Bump Scale", Float) = 1.0

		_SnowMin("Snow Min", Float) = 11.0
		_SnowMax("Snow Max", Float) = 12.0
		[NoScaleOffset]_SnowTex("Snow Tex", 2D) = "white" {}
		[NoScaleOffset]_SnowBump("Snow Bump", 2D) = "normal" {}
		_SnowBumpScale("Snow Bump Scale", Float) = 1.0
	}
	SubShader
	{
		Tags{ "RenderType" = "Geometry" "Queue" = "Geometry" }
		LOD 200

		CGPROGRAM
		#pragma surface Surf Standard fullforwardshadows vertex:Vert nolightmap
		// Fragment Blending
		#pragma multi_compile __ SGT_A

		//#pragma target 3.0

		half4 _Color;
		half  _Glossiness;
		half  _Metallic;

		sampler2D _BaseTex;
		sampler2D _BaseBump;
		half      _BaseBumpScale;

		half      _LandMin;
		half      _LandMax;
		sampler2D _LandTex;
		sampler2D _LandBump;
		half      _LandBumpScale;

		half      _SnowMin;
		half      _SnowMax;
		sampler2D _SnowTex;
		sampler2D _SnowBump;
		half      _SnowBumpScale;

		struct appdata
		{
			float4 vertex   : POSITION;
			float3 normal   : NORMAL;
			float4 color    : COLOR;
			float4 tangent  : TANGENT;
			float2 texcoord : TEXCOORD0;
		};

		struct Input
		{
			float2 uv_BaseTex;
			float2 texcoord : TEXCOORD0;
			float4 color    : COLOR;
		};

		half4 ScaleNormal(half4 packednormal, half bumpScale)
		{
#if defined(UNITY_NO_DXT5nm)
			packednormal.xy = ((packednormal.xy * 2 - 1) * bumpScale + 1) / 2;
#else
			packednormal.wy = ((packednormal.wy * 2 - 1) * bumpScale + 1) / 2;
#endif
			return packednormal;
		}

		float4 CalculateBlending(float height)
		{
			float4 o;

			o.x = smoothstep(0.0f, 1.0f, saturate((height - _LandMin) / (_LandMax - _LandMin)));
			o.y = smoothstep(0.0f, 1.0f, saturate((height - _SnowMin) / (_SnowMax - _SnowMin)));
			o.z = 0.0f;
			o.w = 0.0f;

			return o;
		}

		void Vert(inout appdata v)
		{
			float height = length(v.vertex);
#if SGT_A // Fragment Blending
			v.color = height;
#else
			v.color = CalculateBlending(height);
#endif
		}

		void Surf(Input IN, inout SurfaceOutputStandard o)
		{
			float2 uv = IN.uv_BaseTex;
#if SGT_A // Fragment Blending
			float4 blend = CalculateBlending(IN.color.r);
#else
			float4 blend = IN.color;
#endif
			// Read base textures
			half3 albedo = tex2D(_BaseTex , uv).rgb;
			half4 normal = tex2D(_BaseBump, uv); normal = ScaleNormal(normal, _BaseBumpScale);

			// Blend in land textures
			half3 landTex  = tex2D(_LandTex , uv).rgb;
			half4 landBump = tex2D(_LandBump, uv); landBump = ScaleNormal(landBump, _LandBumpScale);

			albedo = lerp(albedo, landTex, blend.r);
			normal = lerp(normal, landBump, blend.r);

			// Blend in snow textures
			half3 snowTex  = tex2D(_SnowTex , uv).rgb;
			half4 snowBump = tex2D(_SnowBump, uv); snowBump = ScaleNormal(snowBump, _SnowBumpScale);

			albedo = lerp(albedo, snowTex, blend.g);
			normal = lerp(normal, snowBump, blend.g);

			o.Albedo     = albedo * _Color;
			o.Normal     = UnpackNormal(normal);
			o.Metallic   = _Metallic;
			o.Smoothness = _Glossiness;
		}
		ENDCG
	}
	FallBack "Diffuse"
}