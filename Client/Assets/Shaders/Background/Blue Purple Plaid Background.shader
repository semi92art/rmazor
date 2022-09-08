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
        _Gc1("Gradient Coefficient 1", Range(0, 1)) = 0.5
        _Gc2("Gradient Coefficient 2", Range(0, 1)) = 0.5
        _Mc1("Multiply Coefficient 1", Range(-100, 100)) = 0
        _Mc2("Multiply Coefficient 2", Range(-100, 100)) = 0
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
            float _Speed, _Scale, _Angle;
            fixed _Gc1, _Gc2, _Mc1, _Mc2;


            v2f vert(appdata v)
            {
                return vert_default(v);
            }

            float stroke(float x, float s, float w)
            {
                float feather = 0.005;
                // float d = step(s, x + w*0.5) - step(s, x - w*0.5);
                float d = smoothstep(s - feather, s + feather, x + w * 0.5) - smoothstep(
                    s - feather, s + feather, x - w * 0.5);
                return clamp(d, 0., 1.);
            }

            float sinN(float x)
            {
                return (sin(x) + 1.) / 2.;
            }


            fixed4 frag(v2f f_i) : SV_Target
            {
                float2 frag_coord = f_i.uv * _ScreenParams.xy;
                float2 R = _ScreenParams.xy;
                float2 pos = (2. * frag_coord - R) / min(R.x, R.y);

                float3 color = float3(0, 0, 0);

                float color_divisor = 6.;
                float w_max = .1;
                float nudge = .05;

                for (int i = 0; i < 10; i++)
                {
                    float w = sinN(T * 1.5 + pos.x - pos.y) * w_max;

                    float sdf = (pos.x + float(i) * (w_max + nudge) - pos.y);
                    color += stroke(sdf, 0., w) / color_divisor;

                    sdf = (pos.x - float(i) * (w_max + nudge) - pos.y);
                    color += stroke(sdf, 0., w) / color_divisor;
                }

                for (int i = 0; i < 10; i++)
                {
                    float w = sinN(T * 1.5 + pos.x + pos.y) * w_max;

                    float sdf_inv = (pos.x + float(i) * (w_max + nudge) + pos.y);
                    color += stroke(sdf_inv, 0., w) / color_divisor;

                    sdf_inv = (pos.x - float(i) * (w_max + nudge) + pos.y);
                    color += stroke(sdf_inv, 0., w) / color_divisor;
                }

                color *= float3(sinN(T * 0.65 + pos.y) * 0.65, 0., 0.5);


                // https://learnopengl.com/Advanced-Lighting/Gamma-Correction
                float gamma = 1.8;
                float c = 1 / gamma;
                color = pow(color, float3(c, c, c));
                return lerp(_Color1, _Color2, 1.5 * color.r);
                // return _Color1 + float4(color, 1.);
            }
            ENDCG
        }
    }
}