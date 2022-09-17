//https://www.shadertoy.com/view/wlVBRR
Shader "RMAZOR/Background/Funny Rain"
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

            float sin_n(float v)
            {
                return sin(v) * 0.5 + 0.5;
            }

            // shadertoy.com/view/4ssXRX
            float rand(float2 n)
            {
                return frac(sin(dot(n.xy, float2(12.9898, 78.233))) * 43758.5453);
            }


            fixed4 frag(v2f f_i) : SV_Target
            {
                float time = _Time.x * _Speed;
                float2 uv = f_i.uv / _Scale * 10;
                uv.x *= screen_ratio();
                uv *= 40.0;

                uv += float2(234.0, 6.0 * time * rand(floor(uv.xx + 123.0)));
                float2 id = floor(uv);
                float2 gv = frac(uv);
                gv -= 0.5 + float2(sin((id.y + 2.0) * (id.x + 2.0) + time * 10.0) * 0.1, 0.0);

                float c = 1.0 - smoothstep(0.05, sin_n(time + id.x * id.y) * 0.2 + 0.1, length(gv));
                float3 col = c * _Color2.rgb;

                c *= rand(id);

                fixed4 dots_col = fixed4(col,1);
                return lerp(_Color1,dots_col,c);
            }
            ENDCG
        }
    }
}