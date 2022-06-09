Shader "RMAZOR/Background/Solid" {
	Properties {
		_Color ("Tint", Color) = (1,1,1,1)
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
			#include "../Common.cginc"
		
			fixed4 _Color;

			v2f vert(appdata v) {
				return vert_default(v);
			}

			fixed4 frag(v2f _) : SV_Target {
				fixed4 c = _Color;
				c.rgb *= c.a;
				return c;
			}
		ENDCG
		}
	}
}