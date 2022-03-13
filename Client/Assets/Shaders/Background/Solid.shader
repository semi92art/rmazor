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
			#include "UnityCG.cginc"
			
			struct appdata {
				float4 vertex : POSITION;
				float4 color  : COLOR;
				float2 uv     : TEXCOORD0;
			};

			struct v2f {
				float4 vertex : SV_POSITION;
				fixed4 color  : COLOR;
				float2 uv     : TEXCOORD0;
			};

			fixed4 _Color;

			v2f vert(appdata vec) {
				v2f OUT;
				OUT.vertex = UnityObjectToClipPos(vec.vertex);
				return OUT;
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