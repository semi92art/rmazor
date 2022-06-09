Shader "RMAZOR/Transition/Circles" {
    Properties {
        _Color1 ("Color 1", Color) = (0,0,0,1)
        _EdgesColor("Edges Color", Color) = (1,1,1,1)
        _MainTex ("Texture", 2D) = "white" {}
        _TransitionValue("Transition Value", Range(0, 1)) = 0
    	_C1("C1", Range(-1, 1)) = -0.4
    	_C2("C2", Range(0,  20)) = 11.0
    	_C3("C3", Range(0,  10)) = 3.0
    	_C4("C4", Range(0,  20)) = 10.0
    	_C5("C5", Range(10, 50)) = 35.0
    	_C6("C6", Range(0,  3)) = 1.3
    	_C7("C7", Range(0,  4)) = 1.1
    }
    SubShader {
		Tags { 
			"Queue"             = "Transparent-1"
			"IgnoreProjector"   = "True" 
			"RenderType"        = "Opaque" 
			"PreviewType"       = "Plane"
			"CanUseSpriteAtlas" = "False"
		}
		Cull Off
		Lighting Off
		ZWrite Off
		Blend One OneMinusSrcAlpha
		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "../Common.cginc"

            v2f vert (appdata v) {
				return vert_default(v);
			}

			fixed4 _Color1, _EdgesColor;
			float _TransitionValue;
			float _C1, _C2, _C3, _C4, _C5, _C6, _C7;

			fixed4 frag (v2f i) : SV_Target {
				float2 p = i.uv;
				p.x = (p.x - .5) * screen_ratio();
				p.y -= .5;
				float d = length(p) - _C1;
			    float c = 1. - _TransitionValue;
			    float3 frag_color = float_3(1.);
				frag_color *= _C2 * cos(_C5 * d - d * pow(d, .1)) * _C3 + _C2 * d * cos(abs(c * _C6) + _C7) * _C4; 
				float a_raw = frag_color.r;
				float a = a_raw < .95 ? 0. : 1.;
				frag_color.rgb = _Color1.rgb;
				frag_color.rgb *= a;
				if (a_raw > .001 && a_raw < .95) {
					frag_color.rgb = _EdgesColor.rgb;
					a = 1;
				}
				return fixed4(frag_color, a);
			}
			ENDCG
		}
    }
}