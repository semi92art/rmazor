// https://www.shadertoy.com/view/XdVfzd
Shader "RMAZOR/Background/Fibrous Ring"
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

            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ PIXELSNAP_ON
            #pragma target 2.0

            #include "UnityCG.cginc"
            #include "../Common.cginc"
            #include "Toon.cginc"

            fixed4 _Color1, _Color2;
            float _Speed, _Scale, _Angle;

            v2f vert(appdata v)
            {
                return vert_default(v);
            }

            fixed4 frag(v2f f_i) : SV_Target
            {
                float2 r = _ScreenParams.xy;
                float2 o = (f_i.uv-float2(.5,.47)) * r;
                fixed4 c = fixed4(0,0,0,0);
                o = float2(length(o) / r.y - .3, atan2(o.y,o.x));    
                float4 s = c.yzwx = (.05*_Scale)*cos(1.6*float4(0,1,2,3) + T + o.y + sin(o.y) * sin(T)*2.),
                f = min(o.x-s, c-o.x);
                c = dot(40.*(s-c), clamp(f*r.y, 0., 1.)) * (s-.1) - f;
                float cc = (c.r+c.g+c.b);
                cc = clamp(cc,0,1);
                return lerp(_Color2,_Color1, cc);
            }
            ENDCG
        }
    }
}