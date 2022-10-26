//https://www.shadertoy.com/view/WtcBzX
Shader "RMAZOR/Background/White Fire"
{
    Properties
    {
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
    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
        }
        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #define T (_Time.x * _Speed * 0.1)
            #define LINE_THICKNESS 0.01

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

            v2f vert(appdata v)
            {
                return vert_default(v);
            }

            float2 rand2(in float2 p)
            {
                return frac(float2(sin(p.x * 591.32 + p.y * 154.077), cos(p.x * 391.32 + p.y * 49.077)));
            }

            float voronoi(in float2 x)
            {
                float2 p = floor(x);
                float2 f = frac(x);
                float min_distance = 1.;
                for (int j = -1; j <= 1; j ++)
                {
                    for (int i = -1; i <= 1; i ++)
                    {
                        float2 b = float2(i, j);
                        float2 rand = .5 + .5 * sin(T * 3. + 12. * rand2(p + b));
                        float2 r = float2(b) - f + rand;
                        min_distance = min(min_distance, length(r));
                    }
                }
                return min_distance;
            }

            float rand(float2 co)
            {
                return frac(sin(dot(co.xy, float2(12.9898, 78.233))) * 43758.5453);
            }

            float hermite(float t)
            {
                return t * t * (3.0 - 2.0 * t);
            }

            float noise(float2 co, float frequency)
            {
                float2 v = float2(co.x * frequency, co.y * frequency);

                float ix1 = floor(v.x);
                float iy1 = floor(v.y);
                float ix2 = floor(v.x + 1.0);
                float iy2 = floor(v.y + 1.0);

                float fx = hermite(frac(v.x));
                float fy = hermite(frac(v.y));

                float fade1 = lerp(rand(float2(ix1, iy1)), rand(float2(ix2, iy1)), fx);
                float fade2 = lerp(rand(float2(ix1, iy2)), rand(float2(ix2, iy2)), fx);

                return lerp(fade1, fade2, fy);
            }

            float perlinNoise(float2 co, float freq, int steps, float persistence)
            {
                float value = 0.0;
                float ampl = 1.0;
                float sum = 0.0;
                for (int i = 0; i < steps; i++)
                {
                    sum += ampl;
                    value += noise(co, freq) * ampl;
                    freq *= 2.0;
                    ampl *= persistence;
                }
                return value / sum;
            }

            fixed4 frag(v2f f_i) : SV_Target
            {
                float2 uv = f_i.uv / _Scale;

                // Flame color.
                float4 firstColor = float4(float3(.8, .8, .8), 1.0);
                float4 secondColor = float4(float3(.9, .9, .9), 1.0);
                float4 thirdColor = float4(float3(1, 1, 1), 1.0);

                // Uniform scale the coordinates for sampling perlin noise.
                float2 pos = f_i.uv * _ScreenParams.xy / _ScreenParams.y;
                pos.y -= T;

                // Perlin noise.
                float noiseTexel = perlinNoise(pos, 10.0, 5, 0.5);

                // BackGround color.
                float4 color = float4(float3(.7, .7, .7), 1.0);

                // Outer flame.
                float firstStep = step(noiseTexel, 1.2 - uv.y);
                color = lerp(color, firstColor, firstStep);

                // Middel flame.
                float secondStep = step(noiseTexel, pow(1.1 - uv.y, 1.3));
                color = lerp(color, secondColor, secondStep);

                // Inner Flame
                float thirdStep = step(noiseTexel, pow(1.0 - uv.y, 1.8));
                color = lerp(color, thirdColor, thirdStep);

                fixed4 frag_col = lerp(_Color1, _Color2, (1 - color.r)*3);
                return frag_col;
            }
            ENDCG
        }
    }
}