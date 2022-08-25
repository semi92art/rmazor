//https://www.shadertoy.com/view/ssScRD
Shader "RMAZOR/Background/Connected Dots" {
    Properties {
		_Color1 ("Color 1", Color) = (0,0,0,1)
		_Color2 ("Color 2", Color) = (1,1,1,1)
		_Speed("Speed", Range(1,10)) = 1
		_Scale("Scale", Range(1,10)) = 1
		_Angle("Angle", Range(0,360)) = 0
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

			#define S smoothstep
			#define T (_Time.x * _Speed)
			#define STRIPES_AMOUNT 435.2
			
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile _ PIXELSNAP_ON
			#pragma target 2.0

			#include "UnityCG.cginc"
			#include "../Common.cginc"

			fixed4 _Color1, _Color2;
			float _Speed, _Scale, _Angle;
			fixed _Gc1, _Gc2, _Mc1;
			int _Mc2;

			v2f vert (appdata v) {
				return vert_default(v);
			}

			#define S smoothstep
			#define T (_Speed * _Time.x)

			#define STRIPES_AMOUNT 435.2

			float distance_line(float2 p, float2 a, float2 b)
			{
 				float2 pa = p-a;
			    float2 ba = b-a;
			    float t = clamp(dot(pa, ba)/dot(ba, ba), 0.0, 1.0);
			    
			    return length(pa - ba*t);
			}

			float Line(float2 p, float2 a, float2 b)
			{
				float d = distance_line(p, a, b);
			    float m = S(.01, .0, d - .001);
			    
			    // if the length of the line segment is bigger han 1.2 = invisible
			    // .8 above fades in
			    m *= S(1.2, .8, length(a - b));
			    return m;
			}

			float rnd(float2 p)
			{
			    p = frac(p * float2(284.4, 931.5));
			    p += dot(p, p + 24.5);
			    
			    return frac(p.x * p.y);
			}

			float2 rnd_point(float2 p)
			{
			    // X coordinate
				float n = rnd(p);
			    // X & Y
			    return float2(n, rnd(p + n));
			}

			float2 get_pos(float2 id, float2 offset)
			{
			    // Get a noise x, y value
			    float2 n = rnd_point(id + offset) * T;
			    return offset + sin(n) * .4;
			}

			float dots(float2 uv)
			{
   				float2 grid = frac(uv) - .5;
			    float2 id = floor(uv);
			    
			    float m = 0.0;
				float2 p[9];
			    
			    int i = 0;
			    for(float y= -1.0; y <= 1.0; y++)
			    {
			        for(float x = -1.0; x<= 1.0; x++)
			        {
        				p[i++] = get_pos(id, float2(x,y)); 
			        }
			    }
			    
			    // Think of it as a matrix
			    // 0 1 2
			    // 3 4 5
			    // 6 7 8
			    for(int i =0; i<9; i++)
			    {
    				m += Line(grid, p[4], p[i]);
			        
			        float2 j = (p[i] - grid) * 20.0;
			        float sparkle = 1./dot(j, j);
			        m += sparkle;
			    }
			    // Then we connect the 4 missing connections from this group
			    m += Line(grid, p[1], p[3]);
			    m += Line(grid, p[1], p[5]);
			    m += Line(grid, p[5], p[7]);
			    m += Line(grid, p[7], p[3]); 
				
			    return m;
			}

			void rotate(inout float2 p, float a) {
			    p = cos(a) * p + sin(a) * float2(p.y, -p.x);
			}

			fixed4 frag(v2f f_i) : SV_Target
			{
				float2 frag_coord = f_i.uv * _ScreenParams.xy;
				float2 uv = (10.0 / _Scale)
				* (frag_coord - fmod(frag_coord, float2(3,4)) - .5*_ScreenParams.xy) / _ScreenParams.y;
				uv *= .5;
				rotate(uv, _Angle * PI / 180.0);
				float m = .0;    
			    for (float i = .0; i < 1.; i += 1./3.)
			    {
			        float z = frac(i + T * .01);
			        float size = lerp(10.0, 0.5, z);
			        float fade = S(0., .2, z) * S(1., .6, z);
			        m += dots(uv * size + i * 50.) * fade;
			    }
			    float3 dots_col_rgb = _Color1 * m;
			    float3 base = sin(T * float3(.45, .123, .542)) * .4 + .6;
			    dots_col_rgb *= base;
			    // dots_col_rgb *= step(0.9, frac((frag_coord.x + float3(0.499, 1.499, 2.499)) * 0.3333));
				float d = dist(0.5,f_i.uv * _Gc2)*_Gc1*1.5;
                fixed4 main_col = lerp(_Color1, _Color2, d);
			    return main_col + fixed4(dots_col_rgb,1.0);
			}
			ENDCG
		}
	}
}