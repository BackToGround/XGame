// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "LightingBox/Terrain 6-Layers"
{
	Properties
	{
		[HideInInspector] __dirty( "", Int ) = 1
		_TessValue( "Max Tessellation", Range( 1, 32 ) ) = 11
		_TessMin( "Tess Min Distance", Float ) = 100
		_TessMax( "Tess Max Distance", Float ) = 100
		_Control_2("Control_2", 2D) = "white" {}
		_Control_1("Control_1", 2D) = "white" {}
		_Layer_0("Layer_0", 2D) = "white" {}
		_Normal_0("Normal_0", 2D) = "bump" {}
		_Smoothness_0("Smoothness_0", Range( 0 , 10)) = 0.3
		_Normal_Power_0("Normal_Power_0", Range( 0 , 1)) = 1
		_Displacement_0("Displacement_0", Range( 0 , 3)) = 1
		_Layer_1("Layer_1", 2D) = "white" {}
		_Normal_1("Normal_1", 2D) = "bump" {}
		_Smoothness_1("Smoothness_1", Range( 0 , 10)) = 0.3
		_Normal_Power_1("Normal_Power_1", Range( 0 , 1)) = 1
		_Displacement_1("Displacement_1", Range( 0 , 3)) = 1
		_Layer_2("Layer_2", 2D) = "white" {}
		_Normal_2("Normal_2", 2D) = "bump" {}
		_Smoothness_2("Smoothness_2", Range( 0 , 10)) = 0.3
		_Normal_Power_2("Normal_Power_2", Range( 0 , 1)) = 1
		_Displacement_2("Displacement_2", Range( 0 , 3)) = 1
		_Layer_3("Layer_3", 2D) = "white" {}
		_Normal_3("Normal_3", 2D) = "bump" {}
		_Smoothness_3("Smoothness_3", Range( 0 , 10)) = 0.3
		_Normal_Power_3("Normal_Power_3", Range( 0 , 1)) = 1
		_Displacement_3("Displacement_3", Range( 0 , 3)) = 1
		_Layer_4("Layer_4", 2D) = "white" {}
		_Normal_4("Normal_4", 2D) = "bump" {}
		_Smoothness_4("Smoothness_4", Range( 0 , 10)) = 0.3
		_Normal_Power_4("Normal_Power_4", Range( 0 , 1)) = 1
		_Displacement_4("Displacement_4", Range( 0 , 3)) = 0
		_Layer_5("Layer_5", 2D) = "white" {}
		_Normal_5("Normal_5", 2D) = "bump" {}
		_Smoothness_5("Smoothness_5", Range( 0 , 10)) = 0.3
		_Normal_Power_5("Normal_Power_5", Range( 0 , 1)) = 1
		_Displacement_5("Displacement_5", Range( 0 , 3)) = 1
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" }
		Cull Back
		CGPROGRAM
		#include "UnityStandardUtils.cginc"
		#include "Tessellation.cginc"
		#pragma target 5.0
		#pragma only_renderers d3d11 d3d11_9x xboxone ps4 
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows novertexlights nolightmap  nodirlightmap noforwardadd vertex:vertexDataFunc tessellate:tessFunction 
		struct Input
		{
			float2 uv_texcoord;
		};

		struct appdata
		{
			float4 vertex : POSITION;
			float4 tangent : TANGENT;
			float3 normal : NORMAL;
			float4 texcoord : TEXCOORD0;
			float4 texcoord1 : TEXCOORD1;
			float4 texcoord2 : TEXCOORD2;
			float4 texcoord3 : TEXCOORD3;
			fixed4 color : COLOR;
			UNITY_VERTEX_INPUT_INSTANCE_ID
		};

		uniform sampler2D _Control_1;
		uniform float4 _Control_1_ST;
		uniform float _Normal_Power_0;
		uniform sampler2D _Normal_0;
		uniform float4 _Normal_0_ST;
		uniform float _Normal_Power_1;
		uniform sampler2D _Normal_1;
		uniform float4 _Normal_1_ST;
		uniform float _Normal_Power_2;
		uniform sampler2D _Normal_2;
		uniform float4 _Normal_2_ST;
		uniform float _Normal_Power_3;
		uniform sampler2D _Normal_3;
		uniform float4 _Normal_3_ST;
		uniform sampler2D _Control_2;
		uniform float4 _Control_2_ST;
		uniform float _Normal_Power_4;
		uniform sampler2D _Normal_4;
		uniform float4 _Normal_4_ST;
		uniform float _Normal_Power_5;
		uniform sampler2D _Normal_5;
		uniform float4 _Normal_5_ST;
		uniform sampler2D _Layer_0;
		uniform float4 _Layer_0_ST;
		uniform sampler2D _Layer_1;
		uniform float4 _Layer_1_ST;
		uniform sampler2D _Layer_2;
		uniform float4 _Layer_2_ST;
		uniform sampler2D _Layer_3;
		uniform float4 _Layer_3_ST;
		uniform sampler2D _Layer_4;
		uniform float4 _Layer_4_ST;
		uniform sampler2D _Layer_5;
		uniform float4 _Layer_5_ST;
		uniform float _Smoothness_0;
		uniform float _Smoothness_1;
		uniform float _Smoothness_2;
		uniform float _Smoothness_3;
		uniform float _Smoothness_4;
		uniform float _Smoothness_5;
		uniform float _Displacement_0;
		uniform float _Displacement_1;
		uniform float _Displacement_2;
		uniform float _Displacement_3;
		uniform float _Displacement_4;
		uniform float _Displacement_5;
		uniform float _TessValue;
		uniform float _TessMin;
		uniform float _TessMax;

		float4 tessFunction( appdata v0, appdata v1, appdata v2 )
		{
			return UnityDistanceBasedTess( v0.vertex, v1.vertex, v2.vertex, _TessMin, _TessMax, _TessValue );
		}

		void vertexDataFunc( inout appdata v )
		{
			float4 uv_Control_1 = float4(v.texcoord * _Control_1_ST.xy + _Control_1_ST.zw, 0 ,0);
			float4 tex2DNode1 = tex2Dlod( _Control_1, uv_Control_1 );
			float4 uv_Control_2 = float4(v.texcoord * _Control_2_ST.xy + _Control_2_ST.zw, 0 ,0);
			float4 tex2DNode2 = tex2Dlod( _Control_2, uv_Control_2 );
			float4 uv_Layer_0 = float4(v.texcoord * _Layer_0_ST.xy + _Layer_0_ST.zw, 0 ,0);
			float4 tex2DNode3 = tex2Dlod( _Layer_0, uv_Layer_0 );
			float4 uv_Layer_1 = float4(v.texcoord * _Layer_1_ST.xy + _Layer_1_ST.zw, 0 ,0);
			float4 tex2DNode4 = tex2Dlod( _Layer_1, uv_Layer_1 );
			float4 uv_Layer_2 = float4(v.texcoord * _Layer_2_ST.xy + _Layer_2_ST.zw, 0 ,0);
			float4 tex2DNode5 = tex2Dlod( _Layer_2, uv_Layer_2 );
			float4 uv_Layer_3 = float4(v.texcoord * _Layer_3_ST.xy + _Layer_3_ST.zw, 0 ,0);
			float4 tex2DNode6 = tex2Dlod( _Layer_3, uv_Layer_3 );
			float temp_output_64_0 = ( ( ( ( tex2DNode1.r * tex2DNode3.a ) + ( tex2DNode1.g * tex2DNode4.a ) ) + ( tex2DNode1.b * tex2DNode5.a ) ) + ( tex2DNode1.a * tex2DNode6.a ) );
			float3 ase_vertexNormal = v.normal.xyz;
			v.vertex.xyz += ( ( ( ( ( ( ( ( tex2DNode1.r * _Displacement_0 ) + ( tex2DNode1.g * _Displacement_1 ) ) + ( tex2DNode1.b * _Displacement_2 ) ) + ( tex2DNode1.a * _Displacement_3 ) ) + ( tex2DNode2.r * _Displacement_4 ) ) + ( tex2DNode2.g * _Displacement_5 ) ) * temp_output_64_0 ) * ase_vertexNormal );
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_Control_1 = i.uv_texcoord * _Control_1_ST.xy + _Control_1_ST.zw;
			float4 tex2DNode1 = tex2D( _Control_1, uv_Control_1 );
			float2 uv_Normal_0 = i.uv_texcoord * _Normal_0_ST.xy + _Normal_0_ST.zw;
			float2 uv_Normal_1 = i.uv_texcoord * _Normal_1_ST.xy + _Normal_1_ST.zw;
			float2 uv_Normal_2 = i.uv_texcoord * _Normal_2_ST.xy + _Normal_2_ST.zw;
			float2 uv_Normal_3 = i.uv_texcoord * _Normal_3_ST.xy + _Normal_3_ST.zw;
			float2 uv_Control_2 = i.uv_texcoord * _Control_2_ST.xy + _Control_2_ST.zw;
			float4 tex2DNode2 = tex2D( _Control_2, uv_Control_2 );
			float2 uv_Normal_4 = i.uv_texcoord * _Normal_4_ST.xy + _Normal_4_ST.zw;
			float2 uv_Normal_5 = i.uv_texcoord * _Normal_5_ST.xy + _Normal_5_ST.zw;
			o.Normal = ( ( ( ( ( ( tex2DNode1.r * UnpackScaleNormal( tex2D( _Normal_0, uv_Normal_0 ) ,_Normal_Power_0 ) ) + ( tex2DNode1.g * UnpackScaleNormal( tex2D( _Normal_1, uv_Normal_1 ) ,_Normal_Power_1 ) ) ) + ( tex2DNode1.b * UnpackScaleNormal( tex2D( _Normal_2, uv_Normal_2 ) ,_Normal_Power_2 ) ) ) + ( tex2DNode1.a * UnpackScaleNormal( tex2D( _Normal_3, uv_Normal_3 ) ,_Normal_Power_3 ) ) ) + ( tex2DNode2.r * UnpackScaleNormal( tex2D( _Normal_4, uv_Normal_4 ) ,_Normal_Power_4 ) ) ) + ( tex2DNode2.g * UnpackScaleNormal( tex2D( _Normal_5, uv_Normal_5 ) ,_Normal_Power_5 ) ) );
			float2 uv_Layer_0 = i.uv_texcoord * _Layer_0_ST.xy + _Layer_0_ST.zw;
			float4 tex2DNode3 = tex2D( _Layer_0, uv_Layer_0 );
			float2 uv_Layer_1 = i.uv_texcoord * _Layer_1_ST.xy + _Layer_1_ST.zw;
			float4 tex2DNode4 = tex2D( _Layer_1, uv_Layer_1 );
			float2 uv_Layer_2 = i.uv_texcoord * _Layer_2_ST.xy + _Layer_2_ST.zw;
			float4 tex2DNode5 = tex2D( _Layer_2, uv_Layer_2 );
			float2 uv_Layer_3 = i.uv_texcoord * _Layer_3_ST.xy + _Layer_3_ST.zw;
			float4 tex2DNode6 = tex2D( _Layer_3, uv_Layer_3 );
			float2 uv_Layer_4 = i.uv_texcoord * _Layer_4_ST.xy + _Layer_4_ST.zw;
			float4 tex2DNode97 = tex2D( _Layer_4, uv_Layer_4 );
			float2 uv_Layer_5 = i.uv_texcoord * _Layer_5_ST.xy + _Layer_5_ST.zw;
			float4 tex2DNode98 = tex2D( _Layer_5, uv_Layer_5 );
			o.Albedo = ( ( ( ( ( ( tex2DNode1.r * tex2DNode3 ) + ( tex2DNode1.g * tex2DNode4 ) ) + ( tex2DNode1.b * tex2DNode5 ) ) + ( tex2DNode1.a * tex2DNode6 ) ) + ( tex2DNode2.r * tex2DNode97 ) ) + ( tex2DNode2.g * tex2DNode98 ) ).rgb;
			o.Smoothness = ( ( ( ( ( ( tex2DNode1.r * ( tex2DNode3.b * _Smoothness_0 ) ) + ( tex2DNode1.g * ( tex2DNode4.b * _Smoothness_1 ) ) ) + ( tex2DNode1.b * ( tex2DNode5.b * _Smoothness_2 ) ) ) + ( tex2DNode1.a * ( tex2DNode6.b * _Smoothness_3 ) ) ) + ( tex2DNode2.r * ( tex2DNode97.b * _Smoothness_4 ) ) ) + ( tex2DNode2.g * ( tex2DNode98.b * _Smoothness_5 ) ) );
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=13101
7;29;1010;692;3586.564;531.4194;4.660022;True;False
Node;AmplifyShaderEditor.CommentaryNode;111;-2060.974,2628.189;Float;False;1337.953;846.051;Displacement Power;17;79;72;81;80;74;70;71;68;69;77;67;83;82;76;75;73;78;;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;68;-2001.559,2822.539;Float;False;Property;_Displacement_1;Displacement_1;17;0;1;0;3;0;1;FLOAT
Node;AmplifyShaderEditor.CommentaryNode;35;-1974.954,-1174.201;Float;False;1262.603;1692.423;Splats;13;11;13;10;12;9;6;8;7;5;4;3;97;98;;1,1,1,1;0;0
Node;AmplifyShaderEditor.SamplerNode;1;-3764.399,-428.0154;Float;True;Property;_Control_1;Control_1;7;0;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0.0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1.0;False;5;COLOR;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.CommentaryNode;56;-2433.727,-2867.692;Float;False;1717.215;1647.977;Comment;21;31;30;25;17;26;24;16;21;22;23;14;15;20;18;19;103;104;109;110;107;108;;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;55;-2062.149,567.6201;Float;False;1354.394;1076.475;Smoothness;21;46;43;45;42;44;50;54;49;41;40;47;48;53;51;52;89;90;91;92;93;94;;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;67;-2004.445,2696.422;Float;False;Property;_Displacement_0;Displacement_0;12;0;1;0;3;0;1;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;51;-1984.229,639.9022;Float;False;Property;_Smoothness_0;Smoothness_0;10;0;0.3;0;10;0;1;FLOAT
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;71;-1625.365,2678.189;Float;False;2;2;0;FLOAT;0.0;False;1;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;72;-1627.01,2803.211;Float;False;2;2;0;FLOAT;0.0;False;1;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;18;-2281.503,-2722.778;Float;False;Property;_Normal_Power_0;Normal_Power_0;11;0;1;0;1;0;1;FLOAT
Node;AmplifyShaderEditor.SamplerNode;3;-1865.438,-1124.201;Float;True;Property;_Layer_0;Layer_0;8;0;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0.0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1.0;False;5;COLOR;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;19;-2278.732,-2471.163;Float;False;Property;_Normal_Power_1;Normal_Power_1;16;0;1;0;1;0;1;FLOAT
Node;AmplifyShaderEditor.SamplerNode;4;-1867.275,-892.4848;Float;True;Property;_Layer_1;Layer_1;13;0;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0.0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1.0;False;5;COLOR;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;69;-2006.835,2938.556;Float;False;Property;_Displacement_2;Displacement_2;22;0;1;0;3;0;1;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;52;-1983.368,795.3799;Float;False;Property;_Smoothness_1;Smoothness_1;15;0;0.3;0;10;0;1;FLOAT
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;73;-1628.025,2923.299;Float;False;2;2;0;FLOAT;0.0;False;1;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.SimpleAddOpNode;75;-1409.867,2732.475;Float;False;2;2;0;FLOAT;0.0;False;1;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;70;-2010.974,3062.531;Float;False;Property;_Displacement_3;Displacement_3;27;0;1;0;3;0;1;FLOAT
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;47;-1578.376,622.6201;Float;False;2;2;0;FLOAT;0.0;False;1;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.CommentaryNode;65;-1980.625,1685.432;Float;False;793.3263;866.3174;Displacement;11;59;60;61;62;64;63;85;86;87;88;58;;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;48;-1572.75,772.9169;Float;False;2;2;0;FLOAT;0.0;False;1;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.SamplerNode;15;-1900.285,-2517.64;Float;True;Property;_Normal_1;Normal_1;14;0;None;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0.0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1.0;False;5;FLOAT3;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;20;-2288.522,-2209.214;Float;False;Property;_Normal_Power_2;Normal_Power_2;21;0;1;0;1;0;1;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;53;-1976.08,985.4167;Float;False;Property;_Smoothness_2;Smoothness_2;20;0;0.3;0;10;0;1;FLOAT
Node;AmplifyShaderEditor.SamplerNode;14;-1900.284,-2767.692;Float;True;Property;_Normal_0;Normal_0;9;0;None;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0.0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1.0;False;5;FLOAT3;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.SamplerNode;5;-1864.361,-640.4324;Float;True;Property;_Layer_2;Layer_2;18;0;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0.0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1.0;False;5;COLOR;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;21;-2301.624,-1922.743;Float;False;Property;_Normal_Power_3;Normal_Power_3;26;0;1;0;1;0;1;FLOAT
Node;AmplifyShaderEditor.SamplerNode;6;-1865.818,-376.8222;Float;True;Property;_Layer_3;Layer_3;23;0;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0.0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1.0;False;5;COLOR;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;59;-1923.914,1869.417;Float;False;2;2;0;FLOAT;0.0;False;1;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;23;-1443.088,-2484.25;Float;False;2;2;0;FLOAT;0,0,0;False;1;FLOAT3;0.0;False;1;FLOAT3
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;49;-1574.868,966.2974;Float;False;2;2;0;FLOAT;0.0;False;1;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.SamplerNode;2;-3715.019,39.03107;Float;True;Property;_Control_2;Control_2;6;0;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0.0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1.0;False;5;COLOR;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;7;-1470.983,-1042.552;Float;False;2;2;0;FLOAT;0.0,0,0,0;False;1;COLOR;0.0;False;1;COLOR
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;8;-1475.354,-825.4665;Float;False;2;2;0;FLOAT;0.0,0,0,0;False;1;COLOR;0.0;False;1;COLOR
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;74;-1630.594,3046.766;Float;False;2;2;0;FLOAT;0.0;False;1;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.SimpleAddOpNode;76;-1269.002,2852.586;Float;False;2;2;0;FLOAT;0.0;False;1;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;40;-1308.814,635.2728;Float;False;2;2;0;FLOAT;0.0;False;1;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;82;-2007.253,3210.221;Float;False;Property;_Displacement_4;Displacement_4;32;0;0;0;3;0;1;FLOAT
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;58;-1925.515,1745.378;Float;False;2;2;0;FLOAT;0.0;False;1;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;22;-1451.956,-2720.14;Float;False;2;2;0;FLOAT;0,0,0;False;1;FLOAT3;0;False;1;FLOAT3
Node;AmplifyShaderEditor.SamplerNode;16;-1900.285,-2253.385;Float;True;Property;_Normal_2;Normal_2;19;0;None;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0.0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1.0;False;5;FLOAT3;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;54;-1974.497,1150.086;Float;False;Property;_Smoothness_3;Smoothness_3;25;0;0.3;0;10;0;1;FLOAT
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;41;-1308.104,807.8271;Float;False;2;2;0;FLOAT;0.0;False;1;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.SimpleAddOpNode;77;-1131.658,3012.506;Float;False;2;2;0;FLOAT;0.0;False;1;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.SamplerNode;17;-1897.443,-1975.575;Float;True;Property;_Normal_3;Normal_3;24;0;None;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0.0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1.0;False;5;FLOAT3;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;24;-1443.087,-2175.646;Float;False;2;2;0;FLOAT;0,0,0;False;1;FLOAT3;0.0;False;1;FLOAT3
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;50;-1575.001,1128.353;Float;False;2;2;0;FLOAT;0.0;False;1;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.SamplerNode;97;-1854.008,-40.37652;Float;True;Property;_Layer_4;Layer_4;28;0;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0.0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1.0;False;5;COLOR;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.SimpleAddOpNode;44;-1094.52,698.1492;Float;False;2;2;0;FLOAT;0.0;False;1;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;9;-1475.355,-573.4142;Float;False;2;2;0;FLOAT;0.0,0,0,0;False;1;COLOR;0.0;False;1;COLOR
Node;AmplifyShaderEditor.RangedFloatNode;83;-2006.253,3358.048;Float;False;Property;_Displacement_5;Displacement_5;37;0;1;0;3;0;1;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;89;-1972.157,1314.41;Float;False;Property;_Smoothness_4;Smoothness_4;30;0;0.3;0;10;0;1;FLOAT
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;60;-1920.132,2021.431;Float;False;2;2;0;FLOAT;0.0;False;1;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;78;-1634.718,3193.172;Float;False;2;2;0;FLOAT;0.0;False;1;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.SimpleAddOpNode;11;-1208.735,-947.8511;Float;False;2;2;0;COLOR;0.0;False;1;COLOR;0.0,0,0,0;False;1;COLOR
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;42;-1307.571,990.9491;Float;False;2;2;0;FLOAT;0.0;False;1;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;109;-2301.372,-1654.07;Float;False;Property;_Normal_Power_4;Normal_Power_4;31;0;1;0;1;0;1;FLOAT
Node;AmplifyShaderEditor.SimpleAddOpNode;62;-1638.44,1779.252;Float;False;2;2;0;FLOAT;0.0;False;1;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.SimpleAddOpNode;26;-1224.933,-2627.913;Float;False;2;2;0;FLOAT3;0.0;False;1;FLOAT3;0,0,0;False;1;FLOAT3
Node;AmplifyShaderEditor.SimpleAddOpNode;45;-982.7109,862.3549;Float;False;2;2;0;FLOAT;0.0;False;1;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.SimpleAddOpNode;63;-1561.917,1969.948;Float;False;2;2;0;FLOAT;0.0;False;1;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.SimpleAddOpNode;80;-992.0855,3161.522;Float;False;2;2;0;FLOAT;0.0;False;1;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;90;-1569.953,1306.247;Float;False;2;2;0;FLOAT;0.0;False;1;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.SamplerNode;98;-1852.661,266.721;Float;True;Property;_Layer_5;Layer_5;33;0;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0.0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1.0;False;5;COLOR;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;79;-1632.434,3341.24;Float;False;2;2;0;FLOAT;0.0;False;1;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;92;-1977.813,1483.167;Float;False;Property;_Smoothness_5;Smoothness_5;35;0;0.3;0;10;0;1;FLOAT
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;43;-1303.93,1177.802;Float;False;2;2;0;FLOAT;0.0;False;1;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;10;-1472.44,-300.9646;Float;False;2;2;0;FLOAT;0.0,0,0,0;False;1;COLOR;0.0;False;1;COLOR
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;61;-1917.628,2170.316;Float;False;2;2;0;FLOAT;0.0;False;1;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.SamplerNode;103;-1896.651,-1709.16;Float;True;Property;_Normal_4;Normal_4;29;0;None;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0.0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1.0;False;5;FLOAT3;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.SimpleAddOpNode;30;-1085.889,-2291.001;Float;False;2;2;0;FLOAT3;0.0;False;1;FLOAT3;0.0,0,0;False;1;FLOAT3
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;25;-1437.766,-1920.249;Float;False;2;2;0;FLOAT;0,0,0;False;1;FLOAT3;0.0;False;1;FLOAT3
Node;AmplifyShaderEditor.RangedFloatNode;110;-2306.535,-1427.928;Float;False;Property;_Normal_Power_5;Normal_Power_5;36;0;1;0;1;0;1;FLOAT
Node;AmplifyShaderEditor.SimpleAddOpNode;12;-1028.073,-609.8387;Float;False;2;2;0;COLOR;0.0;False;1;COLOR;0.0,0,0,0;False;1;COLOR
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;91;-1310.995,1342.126;Float;False;2;2;0;FLOAT;0.0;False;1;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;93;-1565.815,1470.049;Float;False;2;2;0;FLOAT;0.0;False;1;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;105;-1456.077,-1684.587;Float;False;2;2;0;FLOAT;0.0,0,0;False;1;FLOAT3;0.0;False;1;FLOAT3
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;99;-1451.767,-21.14032;Float;False;2;2;0;FLOAT;0.0,0,0,0;False;1;COLOR;0.0;False;1;COLOR
Node;AmplifyShaderEditor.SimpleAddOpNode;46;-963.7377,1167.827;Float;False;2;2;0;FLOAT;0.0;False;1;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.SamplerNode;104;-1893.277,-1457.443;Float;True;Property;_Normal_5;Normal_5;34;0;None;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0.0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1.0;False;5;FLOAT3;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.SimpleAddOpNode;31;-992.1577,-1987.647;Float;False;2;2;0;FLOAT3;0.0;False;1;FLOAT3;0.0,0,0;False;1;FLOAT3
Node;AmplifyShaderEditor.SimpleAddOpNode;13;-866.3512,-340.3029;Float;False;2;2;0;COLOR;0.0;False;1;COLOR;0.0,0,0,0;False;1;COLOR
Node;AmplifyShaderEditor.SimpleAddOpNode;81;-877.0211,3289.943;Float;False;2;2;0;FLOAT;0.0;False;1;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.SimpleAddOpNode;64;-1496.508,2143.62;Float;False;2;2;0;FLOAT;0.0;False;1;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.SimpleAddOpNode;101;-1064.741,-80.4292;Float;False;2;2;0;COLOR;0.0;False;1;COLOR;0.0,0,0,0;False;1;COLOR
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;106;-1434.656,-1415.296;Float;False;2;2;0;FLOAT;0.0,0,0;False;1;FLOAT3;0.0;False;1;FLOAT3
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;66;-714.4514,1780.327;Float;False;2;2;0;FLOAT;0.0;False;1;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.SimpleAddOpNode;107;-911.1518,-1712.959;Float;False;2;2;0;FLOAT3;0.0;False;1;FLOAT3;0.0,0,0;False;1;FLOAT3
Node;AmplifyShaderEditor.SimpleAddOpNode;95;-930.953,1344.753;Float;False;2;2;0;FLOAT;0.0;False;1;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;94;-1315.77,1510.883;Float;False;2;2;0;FLOAT;0.0;False;1;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.NormalVertexDataNode;113;-277.4213,1890.597;Float;False;0;5;FLOAT3;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;100;-1433.651,275.3051;Float;False;2;2;0;FLOAT;0.0,0,0,0;False;1;COLOR;0.0;False;1;COLOR
Node;AmplifyShaderEditor.SimpleAddOpNode;87;-1409.527,2308.446;Float;False;2;2;0;FLOAT;0.0;False;1;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.SimpleAddOpNode;96;-890.4836,1503.393;Float;False;2;2;0;FLOAT;0.0;False;1;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;112;-348.9623,1630.188;Float;False;2;2;0;FLOAT;0.0,0,0;False;1;FLOAT3;0.0;False;1;FLOAT3
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;86;-1913.087,2443.627;Float;False;2;2;0;FLOAT;0.0;False;1;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;85;-1913.687,2311.196;Float;False;2;2;0;FLOAT;0.0;False;1;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.SimpleAddOpNode;108;-867.8946,-1473.175;Float;False;2;2;0;FLOAT3;0.0;False;1;FLOAT3;0.0,0,0,0;False;1;FLOAT3
Node;AmplifyShaderEditor.SimpleAddOpNode;102;-947.8094,158.3741;Float;False;2;2;0;COLOR;0.0;False;1;COLOR;0.0,0,0,0;False;1;COLOR
Node;AmplifyShaderEditor.SimpleAddOpNode;88;-1302.715,2443.628;Float;False;2;2;0;FLOAT;0.0;False;1;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;-75.55327,308.5786;Float;False;True;7;Float;ASEMaterialInspector;0;0;Standard;LightingBox/Terrain 6-Layers;False;False;False;False;False;True;True;False;True;False;False;True;False;False;False;False;False;Back;0;0;False;0;0;Opaque;0.5;True;True;0;False;Opaque;Geometry;All;False;True;False;False;False;False;True;False;True;True;False;False;False;True;True;True;True;False;0;255;255;0;0;0;0;True;0;11;100;100;False;0.5;True;0;Zero;Zero;0;Zero;Zero;Add;Add;0;False;0;0,0,0,0;VertexOffset;False;Cylindrical;False;Relative;0;;-1;-1;-1;0;0;0;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0.0;False;4;FLOAT;0.0;False;5;FLOAT;0.0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0.0;False;9;FLOAT;0.0;False;10;OBJECT;0.0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;71;0;1;1
WireConnection;71;1;67;0
WireConnection;72;0;1;2
WireConnection;72;1;68;0
WireConnection;73;0;1;3
WireConnection;73;1;69;0
WireConnection;75;0;71;0
WireConnection;75;1;72;0
WireConnection;47;0;3;3
WireConnection;47;1;51;0
WireConnection;48;0;4;3
WireConnection;48;1;52;0
WireConnection;15;5;19;0
WireConnection;14;5;18;0
WireConnection;59;0;1;2
WireConnection;59;1;4;4
WireConnection;23;0;1;2
WireConnection;23;1;15;0
WireConnection;49;0;5;3
WireConnection;49;1;53;0
WireConnection;7;0;1;1
WireConnection;7;1;3;0
WireConnection;8;0;1;2
WireConnection;8;1;4;0
WireConnection;74;0;1;4
WireConnection;74;1;70;0
WireConnection;76;0;75;0
WireConnection;76;1;73;0
WireConnection;40;0;1;1
WireConnection;40;1;47;0
WireConnection;58;0;1;1
WireConnection;58;1;3;4
WireConnection;22;0;1;1
WireConnection;22;1;14;0
WireConnection;16;5;20;0
WireConnection;41;0;1;2
WireConnection;41;1;48;0
WireConnection;77;0;76;0
WireConnection;77;1;74;0
WireConnection;17;5;21;0
WireConnection;24;0;1;3
WireConnection;24;1;16;0
WireConnection;50;0;6;3
WireConnection;50;1;54;0
WireConnection;44;0;40;0
WireConnection;44;1;41;0
WireConnection;9;0;1;3
WireConnection;9;1;5;0
WireConnection;60;0;1;3
WireConnection;60;1;5;4
WireConnection;78;0;2;1
WireConnection;78;1;82;0
WireConnection;11;0;7;0
WireConnection;11;1;8;0
WireConnection;42;0;1;3
WireConnection;42;1;49;0
WireConnection;62;0;58;0
WireConnection;62;1;59;0
WireConnection;26;0;22;0
WireConnection;26;1;23;0
WireConnection;45;0;44;0
WireConnection;45;1;42;0
WireConnection;63;0;62;0
WireConnection;63;1;60;0
WireConnection;80;0;77;0
WireConnection;80;1;78;0
WireConnection;90;0;97;3
WireConnection;90;1;89;0
WireConnection;79;0;2;2
WireConnection;79;1;83;0
WireConnection;43;0;1;4
WireConnection;43;1;50;0
WireConnection;10;0;1;4
WireConnection;10;1;6;0
WireConnection;61;0;1;4
WireConnection;61;1;6;4
WireConnection;103;5;109;0
WireConnection;30;0;26;0
WireConnection;30;1;24;0
WireConnection;25;0;1;4
WireConnection;25;1;17;0
WireConnection;12;0;11;0
WireConnection;12;1;9;0
WireConnection;91;0;2;1
WireConnection;91;1;90;0
WireConnection;93;0;98;3
WireConnection;93;1;92;0
WireConnection;105;0;2;1
WireConnection;105;1;103;0
WireConnection;99;0;2;1
WireConnection;99;1;97;0
WireConnection;46;0;45;0
WireConnection;46;1;43;0
WireConnection;104;5;110;0
WireConnection;31;0;30;0
WireConnection;31;1;25;0
WireConnection;13;0;12;0
WireConnection;13;1;10;0
WireConnection;81;0;80;0
WireConnection;81;1;79;0
WireConnection;64;0;63;0
WireConnection;64;1;61;0
WireConnection;101;0;13;0
WireConnection;101;1;99;0
WireConnection;106;0;2;2
WireConnection;106;1;104;0
WireConnection;66;0;81;0
WireConnection;66;1;64;0
WireConnection;107;0;31;0
WireConnection;107;1;105;0
WireConnection;95;0;46;0
WireConnection;95;1;91;0
WireConnection;94;0;2;2
WireConnection;94;1;93;0
WireConnection;100;0;2;2
WireConnection;100;1;98;0
WireConnection;87;0;64;0
WireConnection;87;1;85;0
WireConnection;96;0;95;0
WireConnection;96;1;94;0
WireConnection;112;0;66;0
WireConnection;112;1;113;0
WireConnection;86;0;2;2
WireConnection;86;1;98;4
WireConnection;85;0;2;1
WireConnection;85;1;97;4
WireConnection;108;0;107;0
WireConnection;108;1;106;0
WireConnection;102;0;101;0
WireConnection;102;1;100;0
WireConnection;88;0;87;0
WireConnection;88;1;86;0
WireConnection;0;0;102;0
WireConnection;0;1;108;0
WireConnection;0;4;96;0
WireConnection;0;11;112;0
ASEEND*/
//CHKSM=6639259A627BCB0819AD925CA1DDB1883E2DB6B3