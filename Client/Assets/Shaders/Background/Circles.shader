Shader "RMAZOR/Background/Circles" {
    Properties {
		_Color1 ("Color 1", Color) = (0,0,0,1)
		_Color2 ("Color 2", Color) = (1,1,1,1)
		_Radius ("Radius", Range(0, 0.1)) = 0
    	_CenterX ("Center X", Range(0, 1)) = 0
    	_CenterY ("Center Y", Range(0, 1)) = 0
    	_WavesCount ("Waves Count", Range(0, 20)) = 4
    	_Amplitude("Waves Amplitude", Range(0, 1)) = 0.5
    	_Tiling ("Tiling", Range(1, 500)) = 10
		_Direction ("Direction", Range(0, 1)) = 0
		_WarpScale ("Warp Scale", Range(0, 1)) = 0
		_WarpTiling ("Warp Tiling", Range(1, 10)) = 1
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
			#pragma target 2.0

			#include "UnityCG.cginc"
			#include "BackgroundCommon.cginc"

			fixed4 _Color1, _Color2;
			float _Radius, _CenterX, _CenterY, _Amplitude;
			int _WavesCount;
			int    _Tiling;
			float  _Direction;
			float  _WarpScale;
			float  _WarpTiling;

			struct appdata {
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};
			
			struct v2f {
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			v2f vert (appdata v) {
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}
            inline fixed color_selector_anti_aliasing(float d1, float d2, float indent) {
                int k = 0;
                while (d1 > (d2 + indent) * _Radius * k && k < 100)
                    k = k + 1;
                return k % 2 == 0 ? 0 : 1;
            }
            
            inline fixed color_selector(float d1, float d2) {
                fixed v1 = color_selector_anti_aliasing(d1, d2, 0);
                fixed v2 = color_selector_anti_aliasing(d1, d2, 0.001);
                return (v1 + v2) / 2;
            }

			fixed4 frag (v2f i) : SV_Target {
				float2 pos = wrap_pos(i.uv, _Tiling, _Direction, _WarpScale, _WarpTiling);
				const float epsilon = 0.00001;
				float ratio = _ScreenParams.x / _ScreenParams.y;
				float x = (pos.x - _CenterX) * ratio;
				float y = pos.y - _CenterY;
				float d1 = sqrt(x * x + y * y);
				float ang = acos(x / d1);
				float c = 2 + 0.5 * _Amplitude * cos(ang * _WavesCount);
				float x2 = c * cos(ang);
				float y2 = c * sin(ang);
				float d2 = sqrt(x2 * x2 + y2 * y2);
				if (_Radius < epsilon)
					return _Color1;
				if (d2 < epsilon)
					return _Color1;
                fixed lerp_coeff = color_selector(d1, d2);
                fixed4 col = lerp(_Color1, _Color2, lerp_coeff);
				col.rgb *= col.a;
				return col;
			}
			ENDCG
		}
	}
}