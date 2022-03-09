Shader "RMAZOR/Background/Liness"
{
    Properties {
		_Color1 ("Color 1", Color) = (0,0,0,1)
		_Color2 ("Color 2", Color) = (1,1,1,1)
		_Tiling ("Tiling", Range(1, 500)) = 10
		_Direction ("Direction", Range(0, 1)) = 0
		_WarpScale ("Warp Scale", Range(0, 1)) = 0
		_WarpTiling ("Warp Tiling", Range(1, 10)) = 1
	}

	SubShader {
		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"
			#include "BackgroundCommon.cginc"

			fixed4 _Color1;
			fixed4 _Color2;
			int    _Tiling;
			float  _Direction;
			float  _WarpScale;
			float  _WarpTiling;

			struct appdata {
				float4 vertex : POSITION;
				float2 uv     : TEXCOORD0;
			};

			struct v2f {
				float2 uv     : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			inline fixed color_selector_anti_aliasing(float2 pos, float indent) {
				return floor(frac(pos.x + indent) + 0.5);
			}
			
			inline fixed color_selector(float2 pos) {
				fixed v1 = color_selector_anti_aliasing(pos, 0);
				fixed v2 = color_selector_anti_aliasing(pos, 0.005);
				return (v1 + v2) / 2;
			}

			v2f vert (appdata v) {
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}

			fixed4 frag (v2f i) : SV_Target {
				float2 pos = wrap_pos(i.uv, _Tiling, _Direction, _WarpScale, _WarpTiling);
				fixed lerp_coeff = color_selector(pos);
				return lerp(_Color1, _Color2, lerp_coeff);
			}
			ENDCG
		}
	}
}