//https://www.shadertoy.com/view/Wdlyzj
Shader "RMAZOR/Background/Worm Hole"
{
    Properties
    {
        _MainTex("MainTex",2D) = "white"{}
        _SecondTex("SecondTex",2D) = "white"{}
        _Color1 ("Color 1", Color) = (0,0,0,1)
        _Color2 ("Color 2", Color) = (1,1,1,1)
        _Speed("Speed", Range(1,10)) = 1
        _Scale("Scale", Range(1,10)) = 1
        _Angle("Angle", Range(0,360)) = 0
        _Gc1("Gradient Coefficient 1", Range(0, 1)) = 0.5
        _Gc2("Gradient Coefficient 2", Range(0, 1)) = 0.5
        _TimeA("TimeA", Range(0,100)) = 0
        _Mc1("Multiply Coefficient 1", Range(0, 10)) = 1
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

            sampler2D _MainTex;
            sampler2D _SecondTex;

            fixed4 _Color1, _Color2;
            float _Speed, _Scale, _Angle, _TimeA;
            fixed _Gc1, _Gc2;
            float _Mc1;

            v2f vert(appdata v)
            {
                return vert_default(v);
            }

            #define TIME _Time.x * _Speed * 0.1 + _TimeA

            float2x2 r2d(float a)
            {
                float c = cos(a), s = sin(a);
                return float2x2(c, s, -s, c);
            }

            // http://mercury.sexy/hg_sdf/
            // hglib mirrorOctant
            void mo(inout float2 p, float2 d)
            {
                p.x = abs(p.x) - d.x;
                p.y = abs(p.y) - d.y;
                if (p.y > p.x) p = p.yx;
            }

            // hglib pMod1
            float re(float p, float d)
            {
                return fmod(p - d * .5, d) - d * .5;
            }

            // hglib pModPolar
            void amod(inout float2 p, float d)
            {
                float a = re(atan2(p.x, p.y), d); // beware, flipped y,x
                p = float2(cos(a), sin(a)) * length(p);
            }

            // signed cross
            float sc(float3 p, float d)
            {
                p = abs(p);
                p = max(p, p.yzx);
                return min(p.x, min(p.y, p.z)) - d;
            }

            float g = 0.;

            float de(float3 p)
            {
                float3 q = p;

                p.xy = mul(p.xy, r2d(-TIME * .1));
                p.xy = mul(p.xy, r2d(p.z * .02));
                p.z = re(p.z, 2.);

                amod(p.xy, 6.28 / 4.);
                mo(p.xz, float2(.7, 1.3));
                float sc2 = sc(p, 1.3);

                amod(p.xy, 6.28 / 2.);
                mo(p.xy, float2(1.2, .2));
                float d = sc(p, .4);

                d = max(d, -sc2);

                q.xy = mul(q.xy, r2d(-TIME));
                q.xy = mul(q.xy, r2d(q.z * .9));
                q.x = abs(q.x) - .3;
                q.y = abs(q.y) - .24;
                d = min(d, length(q.xy) - .01);

                q.x = abs(q.x) - 2.3;
                q.y = abs(q.y) - 3.;
                d = min(d, length(q.xy) - .03);

                g += .01 / (.02 + d * d);
                return d;
            }


            float random(float2 uv)
            {
                return frac(sin(dot(uv, float2(12.2544, 35.1571))) * 5418.548416);
            }


            fixed4 frag(v2f f_i) : SV_Target
            {
                float2 frag_coord = f_i.uv * _ScreenParams.xy;
                float2 uv = (frag_coord - .5 * _ScreenParams.xy) / _ScreenParams.y;
                uv *= 1.2;

                float3 ro = float3(0, 0, -4. + TIME * 4.);
                float3 rd = normalize(float3(uv, 1. - length(uv) * .2));

                float t = 0.;
                float3 p;
                float grain = random(uv);
                for (float i = 0.; i < 1.; i += .01)
                {
                    p = ro + rd * t;
                    float d = de(p);
                    d = max(abs(d), .02);
                    d *= 1. + grain * .03;
                    t += d * .4;
                }
                float3 c = _Color1.rgb * g;
                c = lerp(c, float3(.15, .1, .2), 1. - exp(-.01 * t * t));
                c *= .03 * _Mc1;
                return _Color1 + float4(c, 1);
            }
            ENDCG
        }
    }
}