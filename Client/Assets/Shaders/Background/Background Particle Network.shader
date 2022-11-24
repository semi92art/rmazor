//https://www.shadertoy.com/view/ssScRD
Shader "RMAZOR/Background/Particle Network" {
    Properties {
		_Color1 ("Color 1", Color) = (0,0,0,1)
		_Color2 ("Color 2", Color) = (1,1,1,1)
		_Speed("Speed", Range(1,10)) = 1
		_Scale("Scale", Range(1,10)) = 1
		_Gc1("Gradient Coefficient 1", Range(0, 1)) = 0.5
        _Gc2("Gradient Coefficient 2", Range(0, 1)) = 0.5
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

			#define NUMBER_OF_BUCKETS 64
			#define NUMBER_OF_PARTICLES 500
			#define BRIGHTNESS_PARTICLE -100000.0
			#define BRIGHTNESS_WIRE -500.0
			#define CUTOFF 0.01
			#define SPEED 0.04
			#define TIME_OFFSET 999.0
			#define ANGLE 1.2
			
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile _ PIXELSNAP_ON
			#pragma target 2.0

			#include "UnityCG.cginc"
			#include "../Common.cginc"

			fixed4 _Color1, _Color2;
			float _Speed, _Scale;
			fixed _Gc1, _Gc2;

			v2f vert (appdata v) {
				return vert_default(v);
			}

			float ring[NUMBER_OF_BUCKETS];

			float2x2 rotate_matrix(float angle)
			{
				return float2x2(cos(angle), -sin(angle), sin(angle), cos(angle));
			}

			fixed4 frag (v2f i) : SV_Target {
				float temp_array[NUMBER_OF_BUCKETS] = ring;
				int half_of_buckets = (int)NUMBER_OF_BUCKETS * 0.5;
				float2 uv = i.uv;
				uv.x *= screen_ratio();
			    float2 pole = 10 * uv / _Scale;
			    float2 particle = float2(SPEED * (TIME_OFFSET + _Time.x * _Speed), 0);
			    float brightness = 0.0;
			    int j;
			    for (j = 0; j < NUMBER_OF_PARTICLES; j++)
			    {
			    	particle = mul(particle,rotate_matrix(ANGLE));
			        float2 partial_wire = frac(particle - pole) - 0.5;
			        float radius = dot(partial_wire, partial_wire);
			        
			        if (radius < CUTOFF)
			        {
			        	float bucket_angle_ratio = half_of_buckets / 3.14159265359;
			            float radial_brightness = exp(radius * BRIGHTNESS_WIRE);
			            float bucket = bucket_angle_ratio * (atan2(partial_wire.y, partial_wire.x) + 7.0);
			            float leaking_brightness = radial_brightness * frac(bucket);
			            int b = int(bucket);
			            temp_array[b % (float)NUMBER_OF_BUCKETS] += radial_brightness - leaking_brightness;
			            temp_array[(b+1) % NUMBER_OF_BUCKETS] += leaking_brightness;
			            brightness += exp(radius * BRIGHTNESS_PARTICLE);
			        }
			    }
			    ring = temp_array;
			    for (j = 0; j < half_of_buckets; j++)
			    {
			        brightness += ring[j] * ring[j + half_of_buckets];
			    }
				float d = dist(0.5,i.uv * _Gc2)*_Gc1*1.5;
                fixed4 main_col = lerp(_Color1, _Color2, d);
                fixed4 main_col1 = lerp(_Color2, _Color1, d);
                main_col.rgb *= main_col.a;
				
			    return main_col + fixed4(main_col1.rgb * brightness * (1 - uv.y), 1.0);
			}
			ENDCG
		}
	}
}