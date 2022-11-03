//https://www.shadertoy.com/view/tljSWz
Shader "RMAZOR/Background/Squares Fall"
{
    Properties
    {
        _Color1 ("Color 1", Color) = (0,0,0,1)
        _Color2 ("Color 2", Color) = (1,1,1,1)
        _Speed("Speed", Range(1,10)) = 1
        _Scale("Scale", Range(1,10)) = 1
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
            #define T (_Time.x * _Speed)
            #define RAND1(p) frac(sin(p* 78.233)* 43758.5453)

            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ PIXELSNAP_ON
            #pragma target 2.0

            #include "UnityCG.cginc"
            #include "../Common.cginc"
            #include "Toon.cginc"

            fixed4 _Color1, _Color2;
            float _Speed, _Scale, _Angle;
            fixed _Gc1, _Gc2, _Mc1;
            int _Mc2;

            v2f vert(appdata v)
            {
                return vert_default(v);
            }

            fixed4 frag(v2f f_i) : SV_Target
            {
                float2 fragCoord = f_i.uv * _ScreenParams.xy;
                float2 ouv = (fragCoord - _ScreenParams.xy * .5) / _ScreenParams.y;

                float sf = .05 + abs(ouv.y);

                float m = 0.;
                for (float n = -1.; n <= 1.; n += 1.)
                {
                    float2 uv = ouv * float2(1., 1. + .025 * n) * (2. + sin(T * .25) * .2);
                    uv.y += T * .1;

                    uv = uv * 15.;
                    float2 gid = floor(uv);
                    float2 guv = frac(uv) - .5;

                    for (float y = -1.; y <= 1.; y += 1.)
                    {
                        for (float x = -1.; x <= 1.; x += 1.)
                        {
                            float2 iuv = guv + float2(x, y);
                            float2 iid = gid - float2(x, y);

                            float angle = RAND1(iid.x*25. + iid.y * 41.) * 10. +
                                (T * (RAND1(iid.x*10. + iid.y * 60.) + 1.5));

                            float ca = cos(angle);
                            float sa = sin(angle);
                            iuv = mul(iuv, float2x2(ca, -sa, sa, ca));

                            float size = RAND1(iid.x*50. + iid.y*25.) * .2 + .5;
                            float weight = size * .02;

                            float swp = size - weight;
                            float m1 = smoothstep(abs(iuv.x), abs(iuv.x) + sf, swp)
                                * smoothstep(abs(iuv.y), abs(iuv.y) + sf, swp);

                            swp = size + weight;
                            float m2 = smoothstep(abs(iuv.x), abs(iuv.x) + sf, swp)
                                * smoothstep(abs(iuv.y), abs(iuv.y) + sf, swp);

                            float rr = RAND1(iid.x*128. + iid.y*213.);
                            m1 *= rr > .075 ? 1.0 : (1. - rr * 5.);

                            m += clamp(m2 - m1, 0., 1.);
                        }
                    }
                }
                m*=2;
                m = clamp(m,0,1);
                return lerp(_Color1, _Color2, m);
            }
            ENDCG
        }
    }
}