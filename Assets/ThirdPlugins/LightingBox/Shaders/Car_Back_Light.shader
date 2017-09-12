//--------------------------------------------------------------
//
//                    Car Parking Kit
//          Writed by AliyerEdon in summer 2016
//           Contact me : aliyeredon@gmail.com
//    edited in summer 2017   for Outdoor Lighting Box
//
//--------------------------------------------------------------

// This shader is for car back light intensity

Shader "LightingBox/Car BackLight" {
Properties {
_Color("Color",Color) = (1,1,1,1)
	_MainTex ("Base (RGB) Gloss (A)", 2D) = "white" {}
	_Intensity("Light Intensity",Float) = 1
}
SubShader {
	Tags { "RenderType"="Opaque" }
	LOD 200
	
CGPROGRAM
#pragma surface surf Lambert

sampler2D _MainTex;

fixed4 _Color;
half _Intensity;
struct Input {
	float2 uv_MainTex;
	float2 uv_Illum;
};

void surf (Input IN, inout SurfaceOutput o) {
	fixed4 tex = tex2D(_MainTex, IN.uv_MainTex);
	fixed4 c = tex;
	o.Albedo = c.rgb*_Intensity * _Color;
	o.Emission = c.rgb * _Color * _Intensity;
}
ENDCG
} 
FallBack "Self-Illumin/VertexLit"
}
