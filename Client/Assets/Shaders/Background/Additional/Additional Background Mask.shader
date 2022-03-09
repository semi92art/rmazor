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

			#pragma vertex vert
			#pragma fragment frag

			struct appdata{
				float4 vertex : POSITION;
			};

			struct v2f{
				float4 position : SV_POSITION;
			};

			v2f vert(appdata v){
				v2f o;
				o.position = UnityObjectToClipPos(v.vertex);
				return o;
			}

			fixed4 frag(v2f i) : SV_TARGET{
				return 0;
			}

			ENDCG
		}
	}
}