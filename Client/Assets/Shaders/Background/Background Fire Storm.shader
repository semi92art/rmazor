//https://shadered.org/view?s=6Gny24_ojD
Shader "RMAZOR/Background/Fire Storm"
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
            #define T (_Time.x * _Speed)

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

            float2 random2(float2 p)
            {
                return frac(sin(float2(dot(p, float2(127.1, 311.7)), dot(p, float2(269.5, 183.3)))) * 43758.5453);
            }

            float grid(float2 p, float res, float k)
            {
                p.y -= T * k * 0.1;
                float2 u = frac(p * res) - .5;
                float2 e = floor(p * res);
                float r = random2(e).x * 0.25;
                float2 rnd = random2(floor(p.y + 2.) + e) - 0.5;
                u -= rnd * (1.0 - r * 2.0);
                float d = length(u);
                return smoothstep(r, r * 1.2, d);
            }

            fixed4 frag(v2f f_i) : SV_Target
            {
                float2 uv = f_i.uv * _ScreenParams.xy / _ScreenParams.y;
                float z = 1.0;
                for (float i = 3.; i < 7.0; i++)
                {
                    z *= grid(uv, i * 3., 2.0 - 0.3 * i);
                }
                return lerp(_Color1, _Color2, z);
            }
            ENDCG
        }
    }
}