//https://www.shadertoy.com/view/Wdlyzj
Shader "RMAZOR/Background/Swirl for Planet"
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
        _TimeA("TimeA", Range(0,100)) = 0
        _Mc1("Multiply Coefficient 1", Range(0, 3)) = 1
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
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ PIXELSNAP_ON
            #pragma target 2.0

            #include "UnityCG.cginc"
            #include "../Common.cginc"

            fixed4 _Color1, _Color2;
            float _Speed, _Scale, _Angle, _TimeA;
            fixed _Gc1, _Gc2;
            float _Mc1;

            v2f vert(appdata v)
            {
                return vert_default(v);
            }

            #define TAU 6.28318530718

            #define TILING_FACTOR 4
            #define MAX_ITER 4


            float waterHighlight(float2 p, float time, float foaminess)
            {
                float2 i = float2(p);
                float c = 0.0;
                float foaminess_factor = lerp(1.0, 6.0, foaminess);
                float inten = .005 * foaminess_factor;

                for (int n = 0; n < MAX_ITER; n++)
                {
                    float t = time * (1.0 - (3.5 / float(n + 1)));
                    i = p + float2(cos(t - i.x) + sin(t + i.y), sin(t - i.y) + cos(t + i.x));
                    c += 1.0 / length(float2(p.x / (sin(i.x + t)), p.y / (cos(i.y + t))));
                }
                c = 0.2 + c / (inten * float(MAX_ITER));
                c = 1.17 - pow(c, 1.4);
                c = pow(abs(c), 8.0);
                return c / sqrt(foaminess_factor);
            }

            fixed4 frag(v2f f_i) : SV_Target
            {
                float2 fragCoord = f_i.uv * _ScreenParams.xy;
                fragCoord.y *= 0.9;
                float time = _Time.y * 0.1 + 23.0;
                float2 uv = fragCoord.xy / _ScreenParams.xy;
                float2 uv_square = float2(uv.x * _ScreenParams.x / _ScreenParams.y, uv.y);
                float dist_center = pow(2.0 * length(uv - 0.5), 2.0);

                float foaminess = smoothstep(0.4, 1.8, dist_center);
                float clearness = 0.1 + 0.9 * smoothstep(0.1, 0.5, dist_center);

                float2 p = fmod(uv_square * TAU * TILING_FACTOR, TAU) - 250.0;

                float c = waterHighlight(p, time, foaminess);
                c *= _Mc1;

                float3 water_color = _Color1;
                float3 color = _Color1.rgb * c;
                color = clamp(color + water_color, 0.0, 1.0);

                color = lerp(water_color, color, clearness);

                return float4(color, 1.0);
            }
            ENDCG
        }
    }
}