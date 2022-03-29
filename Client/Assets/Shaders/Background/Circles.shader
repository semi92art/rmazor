Shader "RMAZOR/Background/Circles" {
    Properties {
		_Color1 ("Color 1", Color) = (0,0,0,1)
		_Color2 ("Color 2", Color) = (1,1,1,1)
		_Radius ("Radius", Range(0, 0.1)) = 0
    	_CenterX ("Center X", Range(0, 1)) = 0
    	_CenterY ("Center Y", Range(0, 1)) = 0
    	[IntRange]_WavesCount ("Waves Count", Range(0, 20)) = 4
    	_Amplitude("Waves Amplitude", Range(0, 1)) = 0.5
    	[IntRange]_Tiling ("Tiling", Range(1, 10)) = 1
		_Direction ("Direction", Range(0, 1)) = 0
		_WrapScale ("Wrap Scale", Range(0, 1)) = 0
		_WrapTiling ("Wrap Tiling", Range(1, 10)) = 1
    	[IntRange] _StencilRef ("Stencil Reference Value", Range(0,255)) = 0
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
			#pragma multi_compile _ PIXELSNAP_ON
			#pragma target 2.0

			#include "UnityCG.cginc"
			#include "Common.cginc"

			fixed4 _Color1,_Color2;
			float  _Direction, _WrapScale, _WrapTiling, _Tiling;
			float _Radius, _CenterX, _CenterY, _Amplitude;
			int   _WavesCount;
			
			v2f vert (appdata v) {
				return vert_default(v);
			}
			
            fixed color_selector(float d1, float d2) {
                int k = 0;
                while (d1 > d2  * _Radius * k && k < 16)
                    k = k + 1;
                return k % 2 == 0 ? 0 : 1;
            }

			fixed4 frag (v2f i) : SV_Target {
				if (all(_Color1 == _Color2))
					return _Color1;
				float2 pos = wrap_pos(i.uv, _Tiling, _Direction, _WrapScale, _WrapTiling);
				float ratio = _ScreenParams.x / _ScreenParams.y;
				float x = (pos.x - _CenterX) * ratio;
				float y = pos.y - _CenterY;
				float d1 = sqrt(x * x + y * y);
				float ang = acos(x / d1);
				float c = 2 + 0.5 * _Amplitude * cos(ang * _WavesCount);
				float x2 = c * cos(ang);
				float y2 = c * sin(ang);
				float d2 = sqrt(x2 * x2 + y2 * y2);
				if (_Radius < EPS || d2 < EPS)
					return _Color1;
                fixed lerp_coeff = color_selector(d1, d2);
                return lerp(_Color1, _Color2, lerp_coeff);
			}
			ENDCG
		}
	}
}