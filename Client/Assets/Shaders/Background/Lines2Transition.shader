Shader "RMAZOR/Background/Lines 2 Transition"
{
    Properties {
		_BackgroundColor ("Background Color", Color) = (0,0,0,1)
    	[IntRange] _LinesCount("Lines Count", Range(2, 30)) = 5
    	_Delay("Delay", Range(0, 0.2)) = 0.05
    	[MaterialToggle] _TwoSide("Two Size", Float) = 0
    	_TransitionValue("Transition Value", Range(0, 1)) = 0
    	_Direction("Direction", Range(0, 1)) = 0
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

			fixed4 _BackgroundColor;
			float _LinesCount, _Delay, _Direction, _TransitionValue;
			bool _TwoSide;

			v2f vert (appdata v) {
				return vert_default(v);
			}

			float2 rotate(float2 uv) {
				float2 pos = uv;
		        pos.x = lerp(uv.x, uv.y, _Direction);
		        pos.y = lerp(uv.y, 1 - uv.x, _Direction);
				return pos;
			}

			int line_number(float2 uv) {
				float2 pos = rotate(uv);
				return floor(pos.x * _LinesCount) + 1; 
			}

			bool fill(float2 uv, int line_number) {
				float2 pos = rotate(uv);
				float val = pos.y;
				float a = 1.0 + _LinesCount * _Delay;
				float b = -_LinesCount * _Delay;
				float threshold = line_number * _Delay + ((1.0 - _TransitionValue) * a + b);
				if (!_TwoSide)
					return val > threshold;
				if (odd(line_number))
					return val > threshold;
				return val < 1.0 - threshold;
			}

			fixed4 frag (v2f i) : SV_Target {
				const fixed4 col_empty = fixed4(0, 0, 0, 0); 
				int num = line_number(i.uv);
				bool do_fill = fill(i.uv, num);
				fixed4 color = do_fill ? _BackgroundColor : col_empty;
				return color;
			}
			ENDCG
		}
	}
}