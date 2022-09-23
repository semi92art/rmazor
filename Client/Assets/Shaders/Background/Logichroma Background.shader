//https://www.shadertoy.com/view/wlVBRR
Shader "RMAZOR/Background/Logichroma"
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

            v2f vert(appdata v)
            {
                return vert_default(v);
            }

            float func(float t)
            {
                float u = T * 2.;
                return (sin(t * 57. + u) + sin(t * 59. + u * 1.1) + t * 1.313) * .8;
            }

            float func(float2 t)
            {
                return func(t.x + 8.5463 * t.y);
            }

            float4 pal(float4 a, float4 b, float4 c, float4 d, float t)
            {
                return a + b * cos(2. * acos(-1.) * (c * t + d));
            }

            float4 logichroma(float t)
            {
                return pal(
                    float4(.5,.5,.5,.5),
                    float4(.5,.5,.5,.5),
                    float4(0,0,0,0),
                    float4(0,0,0,0),
                    t
                );
            }

            fixed4 frag(v2f f_i) : SV_Target
            {
                float2 uv = (f_i.uv - .5) / _Scale;
                fixed4 frag_col = fixed4(0,0,0,0);
                float i_max = 20.0;
                for (int i = 0; i < i_max; ++i)
                {
                    float f = frac(func(float2(
                        uv.x * .5,
                        floor(uv.y * 20. + .5)
                    )) + T * .5);

                    float c = step(.7, f) * step(.3, frac(uv.y * 20. + .15 + .5));
                    float4 pattern = float4(c,c,c,c);
                    float4 color = logichroma(1. - float(i) / i_max);
                    float alpha = 1. - float(i) / i_max;

                    frag_col += pattern * color * alpha;

                    uv *= 0.998;
                }
                frag_col *= .07;
                return lerp(_Color1,_Color2,frag_col.r);
            }
            ENDCG
        }
    }
}