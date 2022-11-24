//https://www.shadertoy.com/view/4tlXzM
Shader "RMAZOR/Background/Skyline"
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

            v2f vert(appdata v)
            {
                return vert_default(v);
            }

            fixed4 frag(v2f f_i) : SV_Target
            {
                float2 uv = f_i.uv;
                float x,c;
                fixed lc = 0;
                for (float i = 1.; i < 20.; i++)
                {
                    if (uv.y+.04*i < sin(c=floor(x= 2e2*uv.x/i + 9.*i + T * 100)))
                        lc += min(15.*((x-=c)-x*x),1.) *(i/20.-lc);
                }
                fixed4 frag_col = lerp(_Color1, _Color2, lc);
                return frag_col;
            }
            ENDCG
        }
    }
}