// https://www.shadertoy.com/view/XsyXzw
Shader "RMAZOR/Background/Dancing Dots"
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

            #define PI 3.14159265359

            float3 hsv2rgb(in float3 hsv)
            {
                return hsv.z * (1.0 + 0.5 * hsv.y * (cos(2.0 * PI * (hsv.x + float3(0.0, 0.6667, 0.3333))) - 1.0));
            }

            float hash(float3 uv)
            {
                return frac(sin(dot(uv, float3(7.13, 157.09, 113.57))) * 48543.5453);
            }

            // better distance function thanks to Shane
            float map(float3 p)
            {
                float radius = .19;
                return length(frac(p) - .5) - .25 * radius;
            }

            // raymarching function
            float trace(float3 o, float3 r)
            {
                float t = 0.;

                for (int i = 0; i < 32; ++i)
                {
                    // Low iterations for blur.
                    float d = map(o + r * t);
                    t += d * .9; // Ray shortening to blur a bit more. 
                }

                return t;
            }

            fixed4 frag(v2f f_i) : SV_Target
            {
                float2 uv = (f_i.uv * 2. - 1.) / _Scale * 10;
                uv.x *= _ScreenParams.x / _ScreenParams.y;

                // ray
                float3 r = normalize(float3(uv, 2.));
                // origin
                float3 o = float3(-3, T, -1);

                // rotate origin and ray
                float a = -T;
                float2x2 rot = float2x2(cos(a), -sin(a), sin(a), cos(a));
                o.xz = mul(o.xz, rot);
                r.xy = mul(r.xy, rot);
                r.xz = mul(r.xz, rot);

                // march
                float f = trace(o, r);

                // calculate color from angle on xz plane
                // float3 p = o + f * r;
                // float angel = atan2(p.x, p.z) / PI / 2.;
                float q = 1. + f * f * .1;
                return lerp(_Color1, _Color2, 2.5/q);
            }
            ENDCG
        }
    }
}