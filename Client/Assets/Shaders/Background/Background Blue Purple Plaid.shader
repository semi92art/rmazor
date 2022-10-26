// https://www.shadertoy.com/view/tdXBDH
Shader "RMAZOR/Background/Blue Purple Plaid"
{
    Properties
    {
        _Color1 ("Color 1", Color) = (0,0,0,1)
        _Color2 ("Color 2", Color) = (1,1,1,1)
        _Speed("Speed", Range(1,10)) = 1
        _Scale("Scale", Range(1,10)) = 1
        _Angle("Angle", Range(0,2)) = 0
    }
    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderType"="Opaque"
            "PreviewType"="Plane"
        }
        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha
        LOD 10
        Pass
        {
            CGPROGRAM

            #define S smoothstep
            #define T (_Time.x * _Speed)

            #pragma vertex vert
            #pragma fragment frag
            // #pragma multi_compile _ PIXELSNAP_ON
            #pragma target 2.0

            #include "UnityCG.cginc"
            #include "../Common.cginc"

            fixed4 _Color1, _Color2;
            float _Speed, _Scale, _Angle;
            
            v2f vert(appdata v)
            {
                return vert_default(v);
            }

            float stroke(fixed x, fixed s, fixed w)
            {
                fixed feather = 0.005;
                // float d = step(s, x + w*0.5) - step(s, x - w*0.5);
                fixed d = smoothstep(s - feather, s + feather, x + w * 0.5) - smoothstep(
                    s - feather, s + feather, x - w * 0.5);
                return clamp(d, 0., 1.);
            }

            float sinN(float x)
            {
                return (sin(x) + 1.) / 2.;
            }

            fixed4 frag(v2f f_i) : SV_Target
            {
                fixed2 frag_coord = f_i.uv * _ScreenParams.xy;
                fixed2 R = _ScreenParams.xy;
                fixed2 pos = (2. * frag_coord - R) / min(R.x, R.y);

                fixed color = 0;

                fixed color_divisor = 6.;
                fixed w_max = .1;
                fixed nudge = .05;

                for (int i = 0; i < 10; i++)
                {
                    fixed w = sinN(T * 1.5 + pos.x - pos.y) * w_max;

                    fixed sdf = (pos.x + fixed(i) * (w_max + nudge) - pos.y);
                    color += stroke(sdf, 0., w) / color_divisor;

                    sdf = (pos.x - fixed(i) * (w_max + nudge) - pos.y);
                    color += stroke(sdf, 0., w) / color_divisor;
                }

                for (int i = 0; i < 10; i++)
                {
                    fixed w = sinN(T * 1.5 + pos.x + pos.y) * w_max;
                
                    fixed sdf_inv = (pos.x + fixed(i) * (w_max + nudge) + pos.y);
                    color += stroke(sdf_inv, 0., w) / color_divisor;
                
                    sdf_inv = (pos.x - fixed(i) * (w_max + nudge) + pos.y);
                    color += stroke(sdf_inv, 0., w) / color_divisor;
                }

                color *= sinN(T * 0.65 + pos.y) * 0.65, 0., 0.5;
                return lerp(_Color1, _Color2, 3 * color);
            }
            ENDCG
        }
    }
}