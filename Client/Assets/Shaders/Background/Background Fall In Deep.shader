// https://www.shadertoy.com/view/XdVfzd
Shader "RMAZOR/Background/Fall In Deep"
{
    Properties
    {
        _Color1 ("Color 1", Color) = (0,0,0,1)
        _Color2 ("Color 2", Color) = (1,1,1,1)
        _Speed("Speed", Range(1,10)) = 1
        _Scale("Scale", Range(1,10)) = 1
        _Angle("Angle", Range(0,2)) = 0
        _Gc1("Gradient Coefficient 1", Range(0, 1)) = 0.5
        _Gc2("Gradient Coefficient 2", Range(0, 1)) = 0.5
        _Mc1("Multiply Coefficient 1", Range(-100, 100)) = 0
        _Mc2("Multiply Coefficient 2", Range(-100, 100)) = 0
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
            #define S smoothstep
            #define T (_Time.x * _Speed * 0.1)

            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ PIXELSNAP_ON
            #pragma target 2.0

            #include "UnityCG.cginc"
            #include "../Common.cginc"

            fixed4 _Color1, _Color2;
            float _Speed, _Scale, _Angle;
            fixed _Gc1, _Gc2, _Mc1, _Mc2;


            v2f vert(appdata v)
            {
                return vert_default(v);
            }

            #define MOD3 float3(.1031,.11369,.13787)

            float3 hash33(float3 p3)
            {
                p3 = frac(p3 * MOD3);
                p3 += dot(p3, p3.yxz + 19.19);
                return -1.0 + 2.0 * frac(float3((p3.x + p3.y) * p3.z, (p3.x + p3.z) * p3.y, (p3.y + p3.z) * p3.x));
            }

            float simplex_noise(float3 p)
            {
                const float K1 = 0.333333333;
                const float K2 = 0.166666667;

                float3 i = floor(p + (p.x + p.y + p.z) * K1);
                float3 d0 = p - (i - (i.x + i.y + i.z) * K2);

                float3 e = step(float3(0, 0, 0), d0 - d0.yzx);
                float3 i1 = e * (1.0 - e.zxy);
                float3 i2 = 1.0 - e.zxy * (1.0 - e);

                float3 d1 = d0 - (i1 - 1.0 * K2);
                float3 d2 = d0 - (i2 - 2.0 * K2);
                float3 d3 = d0 - (1.0 - 3.0 * K2);

                float4 h = max(0.6 - float4(dot(d0, d0), dot(d1, d1), dot(d2, d2), dot(d3, d3)), 0.0);
                float4 n = h * h * h * h * float4(dot(d0, hash33(i)), dot(d1, hash33(i + i1)), dot(d2, hash33(i + i2)),
                                                  dot(d3, hash33(i + 1.0)));
                float v = 31.316;
                return dot(float4(v, v, v, v), n);
            }

            fixed4 frag(v2f f_i) : SV_Target
            {
                float2 frag_coord = f_i.uv * _ScreenParams.xy;
                float2 uv = (frag_coord - _ScreenParams.xy * .5) / (_ScreenParams.y * _Scale);
                float2 uv1 = frag_coord.xy / _ScreenParams.xy;
                float cc = 0.;
                for (float i = 1.; i < 1.1; i += .05)
                {
                    float2 iuv = uv * i * 1.1;
                    float s = simplex_noise(float3(iuv * 10. * i, T * .25 + i * 5.));
                    s += T * .25 + i * 100.;
                    cc += frac(floor(s * 5.) / 5.) * .1;
                }
                float dist_center = pow(2.0 * length(uv1 - 0.5), 2.0);
                float clearness = 0.9 * smoothstep(0.1, 0.7, dist_center);
                float c = frac(cc) * 5;
                fixed4 frag_col = lerp(_Color1, _Color2, c);
                frag_col = lerp(_Color1, frag_col, clearness);
                return frag_col;
            }
            ENDCG
        }
    }
}