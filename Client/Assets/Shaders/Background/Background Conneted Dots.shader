// https://www.shadertoy.com/view/XdVfzd
Shader "RMAZOR/Background/Connected Dots"
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
            #define T (_Time.x * _Speed * 0.1)

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

            float DistLine(float2 p, float2 a, float2 b)
            {
                float2 pa = p - a;
                float2 ba = b - a;
                float t = clamp(dot(pa, ba) / dot(ba, ba), 0., 1.);
                return length(pa - ba * t);
            }

            float N21(float2 p)
            {
                p = frac(p * float2(233.34, 851.73));
                p += dot(p, p + 23.45);
                return frac(p.x * p.y);
            }

            float2 N22(float2 p)
            {
                float n = N21(p);
                return float2(n, N21(p + n));
            }

            float2 GetPos(float2 id, float2 offs)
            {
                float2 n = N22(id + offs) * T;

                return offs + sin(n) * .4;
            }

            float Line(float2 p, float2 a, float2 b)
            {
                float d = DistLine(p, a, b); // distance to the line segment
                float m = S(.03, .01, d); // cut out the line
                float d2 = length(a - b);
                m *= S(1.2, .8, d2) * .5 + S(.05, .03, abs(d2 - .75)); // make them not all visible
                return m;
            }

            float Layer(float2 uv)
            {
                float m = 0.;
                float2 gv = frac(uv) - .5;
                float2 id = floor(uv);

                float2 p[9];
                int i = 0;


                for (float y = -1.; y <= 1.; y++)
                {
                    for (float x = -1.; x <= 1.; x++)
                    {
                        p[i++] = GetPos(id, float2(x, y));
                    }
                }

                float t = T * 20.;
                for (int i = 0; i < 9; i++)
                {
                    m += Line(gv, p[4], p[i]);

                    float2 j = (p[i] - gv) * 15.;
                    float sparkle = 1. / dot(j, j);

                    m += sparkle * (sin(t + frac(p[i].x) * 10.) * .5 + .5);
                }
                m += Line(gv, p[1], p[3]);
                m += Line(gv, p[1], p[5]);
                m += Line(gv, p[5], p[7]);
                m += Line(gv, p[7], p[3]);

                return m;
            }

            fixed4 frag(v2f f_i) : SV_Target
            {
                float2 fragCoord = f_i.uv * _ScreenParams.xy;
                float2 uv = (fragCoord - .5 * _ScreenParams.xy) / _ScreenParams.y;
                float m = 0.;
                float t = T * .1;

                float s = sin(t);
                float c = cos(t);
                float2x2 rot = float2x2(c, -s, s, c);

                uv = mul(uv, rot);

                for (float i = 0.; i < 1.; i += 1. / 4.)
                {
                    float z = frac(i + t);
                    float size = lerp(10., .5, z);
                    float fade = S(0., .5, z) * S(1., .8, z);
                    m += Layer(uv * size + i * 20) * fade;
                }
                float3 col = m * _Color2.rgb * _Mc1;

                return _Color1 + float4(col, 1.0);
            }
            ENDCG
        }
    }
}