Shader "RMAZOR/Background/Parallax Circles"
{
    Properties
    {
        _Color1 ("Tint", Color) = (1,1,1,1)
        _Color2 ("Tint", Color) = (1,1,1,1)
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
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ PIXELSNAP_ON
            #include "../Common.cginc"

            fixed4 _Color1, _Color2;
            float _Scale;

            v2f vert(appdata v)
            {
                return vert_default(v);
            }

            fixed3 linear_to_srgb(fixed3 color)
            {
                float v = 1.0 / 2.2;
                return pow(color, float3(v, v, v));
            }

            float gradient_noise(in float2 uv)
            {
                const float3 magic = float3(0.06711056, 0.00583715, 52.9829189);
                return frac(magic.z * frac(dot(uv, magic.xy)));
            }

            float circle(float2 p, float r)
            {
                return (length(p / r) - 1.0) * r;
            }

            float rand_parallax(float2 c)
            {
                return frac(sin(dot(c.xy, float2(12.9898, 78.233))) * 43758.5453);
            }

            void bokeh_layer(inout fixed color, float2 p, fixed3 c, float r_min, float r_max)
            {
                float wrap = 450.0;
                if (fmod(floor(p.y / wrap + 0.5), 2.0) == 0.0)
                {
                    p.x += wrap * 0.5;
                }

                float2 p2 = fmod(p + 0.5 * wrap, wrap) - 0.5 * wrap;
                float2 cell = floor(p / wrap + 0.5);
                float cell_r = rand_parallax(cell);

                c *= frac(cell_r * 3.33 + 3.33);
                float radius = lerp(r_min, r_max, frac(cell_r * 7.77 + 7.77));
                p2.x *= lerp(0.9, 1.1, frac(cell_r * 11.13 + 11.13));
                p2.y *= lerp(0.9, 1.1, frac(cell_r * 17.17 + 17.17));

                float sdf = circle(p2, radius);
                float circle = 1.0 - smoothstep(0.0, 1.0, sdf * 0.04);
                float glow = exp(-sdf * 0.025) * 0.3 * (1.0 - circle);
                color += circle + glow;
            }

            fixed4 parallax_color(v2f i, float size, fixed4 color1, fixed4 color2)
            {
                float2 p = (2.0 * i.uv * size * _ScreenParams.xy - _ScreenParams.xy) / _ScreenParams.x * 1000.0;
                float time = 50 * sin(_Time.x * 0.2) - 100;
                float3 col1 = color2.rgb;
                float3 col2 = color2.rgb;
                float3 col3 = color2.rgb;
                float3 col4 = color2.rgb;
                rotate(p, 0.2 + time * 0.03);
                fixed col = 0;
                bokeh_layer(col, p + float2(-50.0 * time + 0.0, 0.0), col1, 50, 90);
                rotate(p, 0.3 - time * 0.05);
                bokeh_layer(col, p + float2(-70.0 * time + 33.0, -33.0), col2, 10, 200);
                rotate(p, 0.5 + time * 0.07);
                bokeh_layer(col, p + float2(-60.0 * time + 55.0, 55.0), col3, 50, 150);
                rotate(p, 0.9 - time * 0.03);
                bokeh_layer(col, p + float2(-25.0 * time + 77.0, 77.0), col4, 35, 66);

                return lerp(color1, color2, col * .5);
            }

            fixed4 frag(v2f i) : SV_Target
            {
                i.uv /= _Scale * 0.1f;
                fixed4 parallax_col = parallax_color(i, 1.0, _Color1, _Color2);
                return parallax_col;
            }
            ENDCG
        }
    }
}