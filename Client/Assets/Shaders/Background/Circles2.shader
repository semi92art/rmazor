Shader "RMAZOR/Background/Circles 2" {
    Properties {
		_Color1 ("Color 1", Color) = (0,0,0,1)
		_Color2 ("Color 2", Color) = (1,1,1,1)
		_Radius ("Radius", Range(0.05, 0.5)) = 0.2
    	_StepX  ("Step X", Range(0.05, 0.5)) = 0.2
    	_StepY  ("Step Y", Range(0.05, 0.5)) = 0.2
    	[Toggle]  _AlternateX("Alternate X", Float) = 0
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
			#pragma target 2.0

			#include "UnityCG.cginc"
			#include "BackgroundCommon.cginc"

            struct appdata {
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f {
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

            fixed4 _Color1, _Color2;
            float  _StepX, _StepY, _Radius;
            float _AlternateX, _AlternateY;
            int    _Tiling;
			float  _Direction, _WarpScale, _WarpTiling;

            v2f vert(appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            inline float screen_ratio() {
	            return _ScreenParams.x / _ScreenParams.y;
            }

            inline float4 circle(float2 uv, float2 center, float radius, float3 color) {
				radius *= screen_ratio();
            	float delta_x = uv.x - center.x;
            	float delta_y = (uv.y - center.y) / screen_ratio();
                float d = delta_x * delta_x + delta_y * delta_y - radius * radius;
            	float a = d > radius ? 0 : 1;
                return float4(color, a);
            }

            inline float4 mix_color(float4 color, float2 pos, float2 center, bool choose_color_1)
            {
                float3 c = choose_color_1 ? _Color1 : _Color2;
                float4 l = circle(pos, center, _Radius * 0.1, c);
            	float lerp_coeff = l.a > EPS ? 1 : 0;
                return lerp(color, l, lerp_coeff);
            }

            inline bool alternate_x() {
	            return _AlternateX > EPS;
            }

            fixed4 frag(v2f i) : SV_Target {
            	float2 pos = wrap_pos(i.uv, _Tiling, _Direction, _WarpScale, _WarpTiling);
                float3 col12 = lerp(_Color1, _Color2, 0.5);
                float4 final_col = float4(col12, 1.0);
            	if (_StepX < 0.05 || _StepY < 0.05 || _Radius < EPS)
            		return final_col;
                bool choose_col_1 = false;
            	bool alt_x = alternate_x();

                for (float y = 0.0; y < 1 + 2 * _StepY + EPS; y += _StepY) {
                    for (float x = 0.0; x < 1 + 2 * _StepX + EPS; x += _StepX) {
                    	float x1 = alt_x ? 1 - x : x;
                        choose_col_1 = !choose_col_1;
                    	float2 center = float2(x1, y);
                    	final_col = mix_color(final_col, pos, center, choose_col_1);
                    }
                }
                return final_col;
            }
            ENDCG
        }
    }
}