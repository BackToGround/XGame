Shader "Space Graphics Toolkit/SgtTerrainWater"
{
	Properties
	{
		_BumpTex("Bump Tex", 2D) = "normal" {}
		_Scale("Scale", Float) = 1.0
		_Color("Color", Color) = (1,1,1,1)
		_Glossiness("Glossiness", Range(0,1)) = 1.0
		_Metallic("Metallic", Range(0,1)) = 1.0
		_InvFade("Soft Factor", Float) = 1.0
	}
	SubShader
	{
		Tags{ "RenderType" = "Transparent" "Queue" = "Transparent" "ForceNoShadowCasting" = "True" "IgnoreProjector" = "True" }
		LOD 200
		Blend SrcAlpha OneMinusSrcAlpha, One One
		ZWrite Off

		CGPROGRAM
		#pragma surface Surf Standard fullforwardshadows vertex:vert alpha:blend

		//#pragma target 3.0

		sampler2D _MainTex;
		sampler2D _BumpTex;
		float4 _Color;
		float _Scale;

		struct Input
		{
			float3 localPos;
			float3 localDir;
			float4 screenPos;
			float eyeDepth;
		};

		half _Glossiness;
		half _Metallic;

		sampler2D_float _CameraDepthTexture;
		float4 _CameraDepthTexture_TexelSize;
		float           _InvFade;
		
		void vert(inout appdata_full v, out Input o)
		{
			UNITY_INITIALIZE_OUTPUT(Input, o);
			COMPUTE_EYEDEPTH(o.eyeDepth);
			o.localPos = v.vertex.xyz;
			o.localDir = v.normal.xyz;
		}

		void Surf(Input IN, inout SurfaceOutputStandard o)
		{
			float3 pos = IN.localPos * _Scale;

			o.Albedo     = _Color;
			o.Alpha      = _Color.a;
			o.Normal     = UnpackNormal(tex2D(_BumpTex, pos.yz));
			o.Metallic   = _Metallic;
			o.Smoothness = _Glossiness;

			float rawZ = SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(IN.screenPos));
			float sceneZ = LinearEyeDepth(rawZ);
			float partZ = IN.eyeDepth;

			float fade = 1.0;
			if (rawZ > 0.0) // Make sure the depth texture exists
				fade = saturate(_InvFade * (sceneZ - partZ));

			o.Alpha *= fade;
		}
		ENDCG
	}
	FallBack "Diffuse"
}