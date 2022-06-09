Shader "RMAZOR/Spear Mask"{
	Properties {
		_Color ("Tint", Color) = (1,1,1,1)
		[MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
		[IntRange] _StencilRef ("Stencil Reference Value", Range(0,255)) = 0
	}

	SubShader {
		Tags { 
			"Queue"="Transparent" 
			"IgnoreProjector"="True" 
			"RenderType"="Transparent" 
			"PreviewType"="Plane"
			"CanUseSpriteAtlas"="True"
		}

		Cull Off
		Lighting Off
		ZWrite Off
		Blend One One
		BlendOp RevSub
		
		Stencil {
			Ref [_StencilRef]
			Comp Equal
		}

		Pass {
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile _ PIXELSNAP_ON
			#include "Common.cginc"
		
		
			fixed4 _Color;

			v2f vert(appdata v) {
				return vert_default(v);
			}

			fixed4 frag(v2f i) : SV_Target {
				fixed4 c = i.color;
				c.rgb *= c.a;
				return c;
			}
		ENDCG
		}
	}
}