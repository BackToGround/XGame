// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Houdini/Line" {

	Properties
	{
		_Color ("Color", Color) = (1,1,1)
	}

	SubShader {

		ZTest Always

		Pass {
			CGPROGRAM

				#pragma exclude_renderers flash
				#pragma vertex vert
				#pragma fragment frag

				float4 _Color;

				struct appdata {
					float4 pos : POSITION;
					float4 colour : COLOR;
				};

				struct v2f {
					float4 pos : SV_POSITION;
					float4 colour : COLOR;
				};

				v2f vert( appdata v ) {
					v2f o;
					o.pos = UnityObjectToClipPos( v.pos );
					o.colour = v.colour;
					return o;
				}

				half4 frag( v2f i ) : COLOR {
					// Temporary hack to hid curves when loading a scene file via code.
					// See bug: #75009
					clip( -1 );

					return i.colour * _Color;
				}

			ENDCG
		}
	}
}
