//https://www.shadertoy.com/view/lsBSRd
Shader "RMAZOR/Background/Sunflow"
{
    Properties
    {
        _Color1 ("Tint", Color) = (1,1,1,1)
        _Color2 ("Tint", Color) = (1,1,1,1)
        _Gc1("Gradient Coefficient 1", Range(0, 1)) = 0.5
        _Gc2("Gradient Coefficient 2", Range(0, 1)) = 0.5
        _Speed("Speed", Range(1,10)) = 1
        _Scale("Scale", Range(1,10)) = 1
    }
    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
        }
        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #define T (_Time.x * _Speed * 0.1)
            #define N 20
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ PIXELSNAP_ON
            #include "../Common.cginc"

            fixed4 _Color1, _Color2;
            fixed _Gc1, _Gc2;
            float _Scale, _Speed;

            v2f vert(appdata v)
            {
                return vert_default(v);
            }

            fixed4 frag(v2f f_i) : SV_Target
            {
                float2 uv0 = f_i.uv * _ScreenParams.xy;
                float2 uv = (uv0 + uv0 - _ScreenParams.xy) / (0.2 * _ScreenParams.y * _Scale);
                float r = length(uv),
                      a = atan2(uv.y, uv.x),
                      i = floor(r * N);
                a *= floor(pow(128., i / N));
                a += 10. * T + 123.34 * i;
                r += (.5 + .5 * cos(a)) / N;
                r = floor(N * r) / N;
                return lerp(_Color1, _Color2, r);
                return (1. - r) * fixed4(3, 2, 1, 1);
            }
            ENDCG
        }
    }
}