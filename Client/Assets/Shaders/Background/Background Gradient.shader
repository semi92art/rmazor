Shader "RMAZOR/Background/Gradient"
{
    Properties
    {
        _Color1 ("Color 1", Color) = (0,0,0,1)
        _Color2 ("Color 2", Color) = (1,1,1,1)
        _Gc1("Gradient Coefficient 1", Range(0, 1)) = 0.5
        _Gc2("Gradient Coefficient 2", Range(0, 1)) = 0.5
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

            #include "../Common.cginc"

            fixed4 _Color1, _Color2;
            fixed _Gc1, _Gc2, _Mc1;

            v2f vert(appdata v)
            {
                return vert_default(v);
            }

            fixed4 frag(v2f vf) : SV_Target
            {
                float d = dist(0.5,vf.uv * _Gc2)*_Gc1*1.5;
				fixed4 frag_col = lerp(_Color1, _Color2, d);
                return frag_col;
            }
            ENDCG
        }
    }
}