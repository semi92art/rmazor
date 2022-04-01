Shader "RMAZOR/Background/LinesTransition"
{
    Properties {
		_Color1 ("Color 1", Color) = (0,0,0,1)
		_Color2 ("Color 2", Color) = (0,0,0,0)
    	[IntRange] _Tiling ("Tiling", Range(1, 10)) = 1
		_Direction ("Direction", Range(-5, 5)) = 0
    	_Indent("Indent", Range(0, 1)) = 0.5
		_WrapTiling ("Wrap Tiling", Range(0, 3)) = 1
    	_WrapScale ("Wrap Scale", Range(0, 0.1)) = 0
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

			fixed4 _Color1,_Color2;
			float  _Direction, _WrapScale, _WrapTiling, _Tiling;
			float  _Indent;

			v2f vert (appdata v) {
				return vert_default(v);
			}

			bool inverse_indent(float2 pos)
			{
				if (pos.x + pos.y < 0.4)
					return false;
				if (pos.x + pos.y < 0.8)
					return true;
				if (pos.x + pos.y < 1.2)
					return false;
				if (pos.x + pos.y < 1.6)
					return true;
				return false;
			}

			fixed4 frag (v2f i) : SV_Target {
				if (all(_Color1 == _Color2))
					return _Color1;
				const fixed4 colEmpty = fixed4(0, 0, 0, 0); 
				float2 pos = wrap_pos(i.uv, _Tiling, _Direction, _WrapScale, _WrapTiling);
				fixed lerp_coeff_1 = frac(pos.x);
				lerp_coeff_1 = inverse_indent(i.uv) ? lerp_coeff_1 : 1.0 - lerp_coeff_1;
				fixed lerp_coeff_2 = floor(lerp_coeff_1 + _Indent);
				fixed4 col2 = lerp(_Color1, _Color2, lerp_coeff_1);
				fixed4 col3 = lerp(col2, colEmpty, lerp_coeff_2);
				return col3;
			}
			ENDCG
		}
	}
}