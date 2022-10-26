//https://www.shadertoy.com/view/wdjGzy
Shader "RMAZOR/Background/Oily"
{
    Properties
    {
        _Color1 ("Color 1", Color) = (0,0,0,1)
        _Color2 ("Color 2", Color) = (1,1,1,1)
        _Speed("Speed", Range(1,10)) = 1
        _Scale("Scale", Range(1,10)) = 1
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
            #define T (_Time.x * _Speed * 0.1)
            #define RES(f_i) _ScreenParams.xy * f_i.uv

            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ PIXELSNAP_ON
            #pragma target 2.0

            #include "UnityCG.cginc"
            #include "../Common.cginc"

            fixed4 _Color1, _Color2;
            float _Speed, _Scale;
            fixed _Gc1, _Gc2, _Mc1;
            int _Mc2;

            v2f vert(appdata v)
            {
                return vert_default(v);
            }

            float map(float x, float a1, float a2, float b1, float b2)
            {
                return b1 + (b2 - b1) * (x - a1) / (a2 - a1);
            }

            float3 hsv2rgb(in float3 c)
            {
                float3 rgb = clamp(abs(fmod(c.x * 6.0 + float3(0.0, 4.0, 2.0), 6.0) - 3.0) - 1.0, 0.0, 1.0);
                rgb = rgb * rgb * (3.0 - 2.0 * rgb);
                return c.z * lerp(float3(1,1,1), rgb, c.y);
            }

            float3 random3(float3 c)
            {
                float j = 4096.0 * sin(dot(c, float3(17.0, 59.4, 15.0)));
                float3 r;
                r.z = frac(512.0 * j);
                j *= .125;
                r.x = frac(512.0 * j);
                j *= .125;
                r.y = frac(512.0 * j);
                return r - 0.5;
            }

            const float F3 = 0.3333333;
            const float G3 = 0.1666667;

            float snoise(float3 p)
            {
                float3 s = floor(p + dot(p, float3(F3, F3, F3)));
                float3 x = p - s + dot(s, float3(G3, G3, G3));

                float3 e = step(float3(0, 0, 0), x - x.yzx);
                float3 i1 = e * (1.0 - e.zxy);
                float3 i2 = 1.0 - e.zxy * (1.0 - e);

                float3 x1 = x - i1 + G3;
                float3 x2 = x - i2 + 2.0 * G3;
                float3 x3 = x - 1.0 + 3.0 * G3;

                float4 w, d;

                w.x = dot(x, x);
                w.y = dot(x1, x1);
                w.z = dot(x2, x2);
                w.w = dot(x3, x3);

                w = max(0.6 - w, 0.0);

                d.x = dot(random3(s), x);
                d.y = dot(random3(s + i1), x1);
                d.z = dot(random3(s + i2), x2);
                d.w = dot(random3(s + 1.0), x3);

                w *= w;
                w *= w;
                d *= w;

                return dot(d, float4(52, 52, 52, 52));
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv;
                uv.x *= screen_ratio();
                float scl = 2.85;
                float d = distance(float2(uv.x, uv.y), float2(.5, .5));
                uv += T * .25 + (1.8 * snoise(float3(uv.x * scl, uv.y * scl, T * 0.012)));
                float n = snoise(float3(uv.x * scl, uv.y * scl, T * .015));
                n = S(0.146, 0.702, n - d * .1);
                return fixed4(fixed3(n, n, n), 1.0);
            }
            ENDCG
        }
    }
}