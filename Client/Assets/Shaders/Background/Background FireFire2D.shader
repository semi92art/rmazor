//https://www.shadertoy.com/view/ssScRD
Shader "RMAZOR/Background/FireFire2D" {
    Properties {
		_Color1 ("Color 1", Color) = (0,0,0,1)
		_Color2 ("Color 2", Color) = (1,1,1,1)
		_Speed("Speed", Range(1,10)) = 1
		_Scale("Scale", Range(1,10)) = 1
		_Gc1("Gradient Coefficient 1", Range(0, 1)) = 0.5
        _Gc2("Gradient Coefficient 2", Range(0, 1)) = 0.5
    	_Mc1("Multiply Coefficient 1", Range(0, 1)) = 0.5
    	[IntRange]_Mc2("Multiply Coefficient 2", Range(1, 50)) = 1
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
			#include "../Common.cginc"

			fixed4 _Color1, _Color2;
			float _Speed, _Scale;
			fixed _Gc1, _Gc2, _Mc1;
			int _Mc2;

			v2f vert (appdata v) {
				return vert_default(v);
			}

			fixed4 frag (v2f i) : SV_Target {
				float d = dist(0.5,i.uv * _Gc2)*_Gc1*1.5;
				fixed4 main_col = lerp(_Color1, _Color2, d);
				float2 u = i.uv * _ScreenParams.xy / _Scale;
			    float v = frac(u.x*.41);
			    fixed4 col = (frac((u.y*.02+u.x*.4)*v-_Time.x))*float4(1,.6,.4,1)/(1.+u.y*.02-v);
				return main_col + col * _Color2;
			}
			ENDCG
		}
	}
}