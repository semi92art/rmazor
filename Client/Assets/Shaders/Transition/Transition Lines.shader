Shader "RMAZOR/Transition/Lines" {
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
				float2 tiled_uv = float2(i.uv.x, frac(i.uv.y * _NumLines / 2.));
				fixed4 col = _Color1;
				if (tiled_uv.y < 0.5) {
					if(tiled_uv.x > _TransitionValue)
						col.a = .0;
				} else {
					if (tiled_uv.x < 1. - _TransitionValue) 
						col.a = .0;
				}
				col.rgb *= col.a;
				return col;
			}
			ENDCG
		}
    }
}