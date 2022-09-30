//https://www.shadertoy.com/view/sscyDB
Shader "RMAZOR/Background/Neon Stream" {
    Properties {
		_Color1 ("Color 1", Color) = (0,0,0,1)
		_Color2 ("Color 2", Color) = (1,1,1,1)
		_Speed("Speed", Range(1,10)) = 1
		_Gc1("Gradient Coefficient 1", Range(0, 1)) = 0.5
        _Gc2("Gradient Coefficient 2", Range(0, 1)) = 0.5
    	_Mc1("Multiply Coefficient 1", Range(0, 1)) = 0.5
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
			float _Speed;
			fixed _Gc1, _Gc2, _Mc1;

			v2f vert (appdata v) {
				return vert_default(v);
			}

			fixed4 add_stream(float2 uv, fixed4 color, int iterations, float2 shift, float speed)
			{
				float y_coeff = 0.5 + 0.5 * (1 - uv.y);
				uv += shift;
			    int n = iterations;
				fixed4 col = fixed4(0,0,0,0);
			    for (int j = 0; j < n; j++) {
			        float findex = float(j) / float(n);
			        float speed1 = speed * (1.0 + 1.0 - findex);
			        float2 uv2 = float2(uv.x, min(1., max(0., uv.y + 0.1 * sin(uv.x * 2. * PI + _Time.x * speed1 + findex * 10.1))));
			        float offset = 3. * findex * PI + 0.3 * _Time.x * (0.2 + findex * 0.3);
			        float f = 0.5 * (1. + sin(offset));
			        float d = abs(f - uv2.y);
			        float fcol = pow(1. - pow(d, 0.4), 3.5 + 4. * 0.5 * (1. + sin(_Time.x * (1. + findex) + 4. * findex * PI)));
			    	fcol *= y_coeff;
			        col += fixed4(fcol, fcol, fcol, 1);
			    }
				col *= color * _Mc1;
			    return col;
			}

			fixed4 frag (v2f i) : SV_Target {
				fixed4 col1 =  add_stream(i.uv, _Color1, 36, float2(0,0), 1);
				fixed4 col2 =  add_stream(i.uv, _Color1, 18, float2(-0.2,-0.2), 1.5);
				return _Color1 + col1 + col2;
			}
			ENDCG
		}
	}
}