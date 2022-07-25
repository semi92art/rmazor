//https://www.shadertoy.com/view/WsdSzr
Shader "RMAZOR/Transition/Circles To Square" {
    Properties {
        _Color1 ("Color", Color) = (0,0,0,1)
        _MainTex ("Texture", 2D) = "white" {}
        _TransitionValue("Transition Value", Range(0, 1)) = 0
    	_Scale("Scale", Range(0.1, 10)) = 1
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
			float _TransitionValue, _Scale;

			fixed4 frag (v2f i) : SV_Target {
				float2 pos = i.uv;
				pos.x *= screen_ratio();
			    pos *= 64. / _Scale;
				float tv = 1.0 - _TransitionValue;
				tv = tv * 2.049 - .05;
			    float t = fmod(tv, 2.) - 1.;
			    float c = sin(pos.x) * cos(pos.y);
				fixed4 col = fixed4(0,0,0,0);
				c = smoothstep(0.,64./_ScreenParams.y, c - t);
				fixed4 frag_col = lerp(col,_Color1,c);
				frag_col.rgb *= (1.0 - c);
			    return frag_col;
			}
			ENDCG
		}
    }
}