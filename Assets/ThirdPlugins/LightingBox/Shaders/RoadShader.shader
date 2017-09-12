Shader "LightingBox/Road" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_AlphaMap ("AlphaMap", 2D) = "white" {}
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Specular ("Specular", Range(0,1)) = 0.0
		_Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
	}
	SubShader {
	Tags {"Queue"="AlphaTest" "IgnoreProjector"="True" "RenderType"="TransparentCutout"}

		LOD 200
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf StandardSpecular fullforwardshadows alphatest:_Cutoff

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;
		sampler2D _AlphaMap;
		struct Input {
			float2 uv_MainTex;
		};

		half _Glossiness;
		half _Specular;
		fixed4 _Color;

		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_CBUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_CBUFFER_END

		void surf (Input IN, inout SurfaceOutputStandardSpecular o) {
			// Albedo comes from a texture tinted by color
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			fixed4 aph = tex2D (_AlphaMap, IN.uv_MainTex);
			o.Albedo = c.rgb;
			// Metallic and smoothness come from slider variables
			o.Specular = _Specular;
			o.Smoothness = _Glossiness * c.a;
			o.Alpha = aph.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
