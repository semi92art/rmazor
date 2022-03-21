Shader "RMAZOR/Background/Liness"
{
    Properties {
		_Color1 ("Color 1", Color) = (0,0,0,1)
		_Color2 ("Color 2", Color) = (1,1,1,1)
		_Tiling ("Tiling", Range(1, 10)) = 1
		_Direction ("Direction", Range(-1, 2)) = 0
		_WarpScale ("Warp Scale", Range(0, 1)) = 0
		_WarpTiling ("Warp Tiling", Range(1, 10)) = 1
	}

	SubShader {
		Tags { 
			"Queue"="Transparent" 
			"IgnoreProjector"="True" 
			"RenderType"="Opaque" 
			"PreviewType"="Plane"
			"CanUseSpriteAtlas"="False"
		}
		Cull Off
		Lighting Off
		ZWrite Off
		Blend One OneMinusSrcAlpha
		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"
			#include "Common.cginc"

			fixed4 _Color1;
			fixed4 _Color2;
			int    _Tiling;
			float  _Direction;
			float  _WarpScale;
			float  _WarpTiling;

			inline fixed color_selector_anti_aliasing(float2 pos, float indent) {
				return floor(frac(pos.x + indent) + 0.5);
			}

			inline fixed color_selector(float2 pos) {
				fixed c1 =  floor(frac(pos.x + 0.0) + 0.5);
				fixed c2 =  floor(frac(pos.x + 0.005) + 0.5);
				fixed c3 =  floor(frac(pos.x - 0.005) + 0.5);
				return (c1 + c2 + c3) / 3.0;
			}

			v2f vert (appdata v) {
				return vert_default(v);
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