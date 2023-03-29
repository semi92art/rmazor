Shader "RMAZOR/Background/Synthwave"
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
            #pragma vertex vert
            #pragma fragment frag
            #define T (_Time.x * _Speed * 0.1)
            #pragma multi_compile _ PIXELSNAP_ON
            #pragma target 2.0

            #include "../Common.cginc"

            float _Speed, _Scale;
            fixed4 _Color1, _Color2;

            v2f vert(appdata v)
            {
                return vert_default(v);
            }

            float sun(float2 uv)
            {
                float val = smoothstep(0.7, 0.69, length(uv));
                float bloom = smoothstep(0.7, 0.0, length(uv));
                float cut = 5.0 * sin((uv.y + T * 0.2 * (1.02)) * 60.0)
                    + clamp(uv.y * 15.0, -6.0, 6.0);
                cut = clamp(cut, 0.0, 1.0);
                float sun_val = clamp(val * cut, 0.0, 1.0) + bloom * 0.6;
                sun_val *= 0.5;
                return sun_val;
            }

            float grid(float2 uv, float battery)
            {
                float2 size = float2(uv.y, uv.y * uv.y * 0.2) * 0.01;
                uv += float2(0.0, T * 4.0 * (battery + 0.05));
                uv = abs(frac(uv) - 0.5);
                float2 lines = smoothstep(size, float2(0, 0), uv);
                lines += smoothstep(size * 5.0, float2(0, 0), uv) * 0.4 * battery;
                return clamp(lines.x + lines.y, 0.0, 3.0);
            }

            fixed4 frag(v2f vf) : SV_Target
            {
                float2 frag_coord = vf.uv * _ScreenParams.xy;
                float2 uv = (2.0 * frag_coord.xy - _ScreenParams.xy) / _ScreenParams.y;
                float pixelate = 200.;
                uv = round(uv * pixelate) / pixelate;
                float battery = 1.0;
                fixed3 col = fixed3(0.0, 0.1, 0.2);
                if (uv.y < -0.2)
                {
                    uv.y = 3.0 / (abs(uv.y + 0.2) + 0.05);
                    uv.x *= uv.y * 1.0;
                    float grid_val = grid(uv, battery);
                    col = lerp(col, float3(1.0, 0.25, 0.5), grid_val);
                }
                else
                {
                    uv.y -= 0.34;
                    col = fixed3(1.0, 0.4, 0.4);
                    float sun_val = sun(uv);
                    col = lerp(col, fixed3(1.0, 0.85, 0.3), uv.y * 2.5 + 0.2);
                    col = lerp(float3(0.0, 0.0, 0.0), col, sun_val);
                    float ssr = 0.1 + 1.25 * (1.0 - smoothstep(-0.2, 0.8, 0.2 + uv.y));
                    float ssg = 0.15 * (1.0 - smoothstep(-0.2, 0.8, 0.2 + uv.y));
                    float ssb = 0.7 - 0.45 * smoothstep(-0.2, 0.8, 0.2 + uv.y);
                    col += fixed3(ssr, ssg, ssb);
                }
                float fog = smoothstep(0.2, -0.05, abs(uv.y + 0.2));
                col += fog * fog * fog;
                col = lerp(fixed3(0.75, 0.1, 0.45) * 0.2, col, battery * 0.7);
                col *= 0.8;
                return fixed4(col, 1.0);
            }
            ENDCG
        }
    }
}