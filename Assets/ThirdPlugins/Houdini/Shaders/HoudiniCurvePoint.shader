// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Houdini/CurvePoint" {
	
	Properties
	{
		_PointSize ("PointSize", Float) = 10.0
		_Color ("Color", Color) = (1,1,1,1)
	}

	SubShader {

		ZTest Always
		ZWrite Off

		Pass {
			CGPROGRAM

				#pragma exclude_renderers flash
				#pragma vertex vert
				#pragma fragment frag

				float _PointSize;
				float4 _Color;

				struct appdata {
					float4 pos : POSITION;
				};

				struct v2f {
					float4 pos : SV_POSITION;
					float size : PSIZE;
				};

				v2f vert( appdata v ) {
					v2f o;
					o.pos = UnityObjectToClipPos( v.pos );
					o.size = _PointSize;
					return o;
				}

				half4 frag( v2f i ) : COLOR {
					return _Color;
				}

			ENDCG
		}

		Pass {
			CGPROGRAM

				#pragma exclude_renderers flash
				#pragma vertex vert
				#pragma fragment frag

				float _PointSize;
				float4 _Color;

				struct appdata {
					float4 pos : POSITION;
					float4 colour : COLOR;
				};

				struct v2f {
					float4 pos : SV_POSITION;
					float4 colour : COLOR;
					float size : PSIZE;
				};

				v2f vert( appdata v ) {
					v2f o;
					o.pos = UnityObjectToClipPos( v.pos );
					o.colour = v.colour;
					o.size = _PointSize;
					return o;
				}

				half4 frag( v2f i ) : COLOR {
					return i.colour * _Color;
				}

			ENDCG
		}
	}
}
