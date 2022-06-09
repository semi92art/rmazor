Shader "RMAZOR/Background/Additional Background Mask" {
	//show values to edit in inspector
	Properties{
		[IntRange] _StencilRef ("Stencil Reference Value", Range(0,255)) = 0
	}

	SubShader{
		Tags{ "RenderType"="Transparent" "Queue"="Geometry-1"}

		Stencil{
			Ref [_StencilRef]
			Comp Always
			Pass Replace
		}

		Pass{
			Blend Zero One
			ZWrite Off

			CGPROGRAM
			#include "UnityCG.cginc"
			#include "../../Common.cginc"

			#pragma vertex vert
			#pragma fragment frag

			v2f vert(appdata v){
				return vert_default(v);
			}

			fixed4 frag(v2f _) : SV_TARGET {
				return 0;
			}

			ENDCG
		}
	}
}