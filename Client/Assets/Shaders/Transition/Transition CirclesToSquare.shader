Shader "RMAZOR/Transition/Circles To Square" {
    Properties {
        _Color1 ("Color", Color) = (0,0,0,1)
        _MainTex ("Texture", 2D) = "white" {}
        _TransitionValue("Transition Value", Range(0, 1)) = 0
    	[IntRange]_NumLines("Lines Count", Range(2, 10)) = 4
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
			float _NumLines;

			fixed4 frag (v2f i) : SV_Target {
				float2 pos = i.uv;
				pos.x *= screen_ratio();
			    pos *= 16.;
			    float t = fmod(_TransitionValue*.4, 2.) - 1.;
			    
			    float c = sin(pos.x) * cos(pos.y);  //Basis of effect
				fixed3 col = fixed3(0, 0, 0);

				c = smoothstep(0.,16./_ScreenParams.y, c - t );
			    return fixed4(c,c*.6,c*.3,1.);

			}
			ENDCG
		}
    }
}