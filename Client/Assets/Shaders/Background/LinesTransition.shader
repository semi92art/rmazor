Shader "RMAZOR/Background/LinesTransition"
{
    Properties {
		_Color1 ("Color 1", Color) = (0,0,0,1)
		_Color2 ("Color 2", Color) = (0,0,0,0)
    	_Tiling ("Tiling", Range(1, 10)) = 1
		_Direction ("Direction", Range(-5, 5)) = 0
    	_Indent("Indent", Range(0, 1)) = 0.5
		_WrapTiling ("Warp Tiling", Range(0, 3)) = 1
    	_WrapScale ("Warp Scale", Range(0, 0.1)) = 0
	}

	SubShader {
		Tags { 
			"Queue"="Transparent-1"
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

			#include "Common.cginc"

			fixed4 _Color1;
			fixed4 _Color2;
			int  _Tiling;
			float  _Direction;
			float  _Indent;
			float  _WrapTiling;
			float  _WrapScale;

			v2f vert (appdata v) {
				return vert_default(v);
			}

			fixed4 frag (v2f i) : SV_Target {
				if (all(_Color1 == _Color2))
					return _Color1;
				float2 pos = wrap_pos(i.uv, _Tiling, _Direction, _WrapScale, _WrapTiling);
				fixed lerp_coeff = floor(frac(pos.x) + 1.0 - _Indent);
				return lerp(_Color1, _Color2, lerp_coeff);
			}
			ENDCG
		}
	}
}