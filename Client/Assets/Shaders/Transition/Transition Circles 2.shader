Shader "RMAZOR/Transition/Circles 2" {
    Properties {
        _Color1 ("Color", Color) = (0,0,0,1)
        _MainTex ("Texture", 2D) = "white" {}
        _TransitionValue("Transition Value", Range(0, 1)) = 0
    	_Scale("Scale", Range(1,10)) = 1
    	_Position("Scale", Float) = 0
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

			fixed4 _Color1;
			float _TransitionValue, _Scale, _Position;

			fixed4 frag (v2f i) : SV_Target {
				float2 uv = i.uv;
				uv.y /= screen_ratio();
				uv *= _Scale;
				float2 i_uv = floor(uv);
				float2 f_uv = frac(uv);
				float tv = (1. - _TransitionValue) * 3.9 - 1.6;
				float wave = max(0.,i_uv.y/_Scale - tv);
			    float2 center = f_uv*2.-1.;
			    float circle = length(center);
			    circle = 1. - step(wave,circle);
				return circle * _Color1;
			}
			ENDCG
		}
    }
}