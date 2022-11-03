//https://www.shadertoy.com/view/ftcBRM
Shader "RMAZOR/Background/Lambmeow Sin Squares"
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

            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ PIXELSNAP_ON
            #pragma target 2.0

            #include "UnityCG.cginc"
            #include "../Common.cginc"
            #include "Toon.cginc"

            fixed4 _Color1, _Color2;
            float _Speed, _Scale;

            v2f vert(appdata v)
            {
                return vert_default(v);
            }

            fixed4 frag(v2f f_i) : SV_Target
            {
                float2 f = f_i.uv * _ScreenParams.xy;
                float2 r = _ScreenParams.xy, u = (2. * f - r)/r.x;
                float ss = 10.;
                float time = 1.;

                fixed4 c = fixed4(0.,0.1,.1,1);
                for(int i = 0; i < 20; i++)
                {
                    float2 flu = floor(u * ss);
                    float3 color = (1. - float3(cos(flu.y/ss + T + float(i)), 1. - abs(sin(flu.y/ss + T + float(i))), 0)); 
                    c.rgb += color.rgb * step(abs(u.x), abs(cos(flu.y/ss + T * time) + sin(T + flu.y/ss) *0.25));
                    time *= .88;
                }
                c *= 0.02f;
                float lc = c.r + c.g + c.b;
                lc = clamp(lc, 0, 1);
                return lerp(_Color2, _Color1, lc);
            }
            ENDCG
        }
    }
}