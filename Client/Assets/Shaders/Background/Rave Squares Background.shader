//https://www.shadertoy.com/view/wlVBRR
Shader "RMAZOR/Background/Rave Squares"
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
            #define T (_Time.x * _Speed * 0.1)
            #define LINE_THICKNESS 0.001

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

            float2 rand2(in float2 p)
            {
                return frac(float2(sin(p.x * 591.32 + p.y * 154.077), cos(p.x * 391.32 + p.y * 49.077)));
            }

            float voronoi(in float2 x)
            {
                float2 p = floor(x);
                float2 f = frac(x);
                float minDistance = 1.;

                for (int j = -1; j <= 1; j ++)
                {
                    for (int i = -1; i <= 1; i ++)
                    {
                        float2 b = float2(i, j);
                        float2 rand = .5 + .5 * sin(T * 3. + 12. * rand2(p + b));
                        float2 r = float2(b) - f + rand;
                        minDistance = min(minDistance, length(r));
                    }
                }
                return minDistance;
            }

            fixed4 frag(v2f f_i) : SV_Target
            {
                float2 uv0 = f_i.uv;
                uv0.x *= screen_ratio();
                float2 uv = uv0;
                rotate(uv, _Angle * PI / 180 + T * 0.2);
                
                float val = pow(voronoi(uv * 8.) * 1.1, 8.) * 2.;
                
                float2 uv1;
                if (uv.x > 0)
                {
                    uv1.x = uv.x;
                }
                else
                {
                    uv1.x = 1.1+uv.x;
                }

                if (uv.y > 0)
                {
                    uv1.y = uv.y;
                }
                else
                {
                    uv1.y = 1.1+uv.y;
                }
                float2 grid = smoothstep(fmod(uv1, .05), float2(LINE_THICKNESS,LINE_THICKNESS),0.03);
                float d = dist(0.5,f_i.uv * _Gc2)*_Gc1*1.5;
                fixed4 main_col = lerp(_Color1, _Color2, d);
                return main_col + _Color2 * 0.9 * val * (grid.x + grid.y);
            }
            ENDCG
        }
    }
}