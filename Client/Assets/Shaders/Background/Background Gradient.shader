Shader "RMAZOR/Background/Gradient"
{
    Properties
    {
        _Color1 ("Color 1", Color) = (0,0,0,1)
        _Color2 ("Color 2", Color) = (1,1,1,1)
        _A1("A1", Range(-1,1)) = 0
        _A2("A2", Range(-1,1)) = 0
        _B1("B1", Range(-1,1)) = 0
        _B2("B2", Range(-1,1)) = 0
        
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
            #pragma multi_compile _ PIXELSNAP_ON
            #pragma target 2.0

            // Gradient noise from Jorge Jimenez's presentation:
            // http://www.iryoku.com/next-generation-post-processing-in-call-of-duty-advanced-warfare
            float gradient_noise(in float2 uv)
            {
                const float3 magic = float3(0.06711056, 0.00583715, 52.9829189);
                return frac(magic.z * frac(dot(uv, magic.xy)));
            }

            #include "../Common.cginc"

            fixed4 _Color1, _Color2;
            fixed _Gc1, _Gc2;
            fixed _A1, _A2, _B1, _B2;

            v2f vert(appdata v)
            {
                return vert_default(v);
            }

            fixed4 frag(v2f vf) : SV_Target
            {
                float2 frag_coord = vf.uv * _ScreenParams.xy;
                float2 a = float2(_A1, _A2) * _ScreenParams.xy;
                float2 b = float2(_B1, _B2) * _ScreenParams.xy;
                // Calculate interpolation factor with floattor projection.
                float2 ba = b - a;
                float t = dot(frag_coord - a, ba) / dot(ba, ba);
                // Saturate and apply smoothstep to the factor.
                t = smoothstep(0.0, 1.0, clamp(t, 0.0, 1.0));
                // Interpolate.
                float3 color = lerp(_Color1, _Color2, t);
                // Add gradient noise to reduce banding.
                color += (1.0/255.0) * gradient_noise(frag_coord) - (0.5/255.0);

                return float4(color, 1.0);
            }
            ENDCG
        }
    }
}