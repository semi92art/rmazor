Shader "RMAZOR/Background/Single Circle" {
    Properties {
		_Color1 ("Color 1", Color) = (0,0,0,1)
		_Color2 ("Color 2", Color) = (1,1,1,1)
		_Radius ("Radius", Range(0, 1)) = 0
	}
	SubShader {
		Tags { 
			"Queue"="Transparent" 
			"RenderType"="Transparent" 
			"PreviewType"="Plane"
		}

		Cull Off
		Lighting Off
		ZWrite Off
		Blend One OneMinusSrcAlpha
		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile _ PIXELSNAP_ON
			#pragma target 2.0

			#include "UnityCG.cginc"
			#include "Common.cginc"

			fixed4 _Color1, _Color2;
			float _Radius;

			v2f vert (appdata v) {
				return vert_default(v);
			}

			inline fixed color_selector(float d1, float d2) {
                int k = 0;
                while (d1 > d2 * _Radius)
                    k = k + 1;
                return d1 > d2 * _Radius ? 0 : 1;
            }

			inline fixed4 circle(float2 uv, float2 pos, float rad, float3 color) {
				float d = length(pos - uv) - rad;
				float t = d >rad ? 1.0 : 0.0;
				return fixed4(color, t);
			}

			fixed4 frag (v2f i) : SV_Target {
				if (_Radius < EPS)
					return _Color1;
				float2 uv = i.uv;
				float ratio = _ScreenParams.x / _ScreenParams.y;
				float x = (uv.x - 0.5) * ratio;
				float y = uv.y - 0.5;
				float d1 = sqrt(x * x + y * y);
				fixed4 lerp_coeff = color_selector(d1, _Radius);
				fixed4 col = lerp(_Color1, _Color2, lerp_coeff); 
				col.rgb *= col.a;
				return col;
			}
			ENDCG
		}
	}
}