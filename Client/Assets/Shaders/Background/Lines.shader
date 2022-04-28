Shader "RMAZOR/Background/Lines"
{
    Properties {
		_Color1 ("Color 1", Color) = (0,0,0,1)
		_Color2 ("Color 2", Color) = (1,1,1,1)
		_Tiling ("Tiling", Range(1, 10)) = 1
		_Direction ("Direction", Range(-1, 2)) = 0
		_WrapScale ("Wrap Scale", Range(0, 1)) = 0
		_WrapTiling ("Wrap Tiling", Range(1, 10)) = 1
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

			#include "Common.cginc"

			fixed4 _Color1,_Color2;
			float  _Direction, _WrapScale, _WrapTiling, _Tiling;
			
			v2f vert (appdata v) {
				return vert_default(v);
			}

			fixed4 frag (v2f i) : SV_Target {
				if (all(_Color1 == _Color2))
					return _Color1;
				float2 pos = wrap_pos(i.uv, _Tiling, _Direction, _WrapScale, _WrapTiling);
				fixed lerp_coeff = 0.0;
#if SHADER_API_GLES3
				lerp_coeff = 0.0;
#else
				lerp_coeff = floor(frac(pos.x) + 0.5);
#endif
				return lerp(_Color1, _Color2, lerp_coeff);
			}
			ENDCG
		}
	}
}