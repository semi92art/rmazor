//https://shadered.org/view?s=6Gny24_ojD
Shader "RMAZOR/Background/Toon Rain"
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

            // Perlin noise variant of "metafall" https://shadertoy.com/view/wllGzB
            #define hash(p) ( 2.* frac(sin(mul(p,float2x2(127.1,311.7,269.5,183.3)))*43758.5453123) -1.)

            fixed4 frag(v2f f_i) : SV_Target
            {
                float2 frag_coord = f_i.uv * _ScreenParams.xy;
                float2 R = _ScreenParams.xy,
                       U = frag_coord / R.y, V = 15. * U;
                V.y += T;
                float2 C, v = smoothstep(0., 1., frac(V)); // Perlin noise
                #define Px(x,y) dot( hash( C = floor(V) + float2(x,y) ), V-C )
                #define Py(y)   lerp( Px(0,y), Px(1,y), v.x)
                float p = lerp(Py(0), Py(1), v.y);
                p = .55 + .75 * p;
                float lc = .3 + 0.8 * U.y - p;
                lc = clamp(lc, 0, 1);
                lc += 0.1;
                lc *= 1.3;
                return toon_color(_Color2, _Color1, lc, .2);
            }
            ENDCG
        }
    }
}