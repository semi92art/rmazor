Shader "RMAZOR/Background/Texture Only"
{
    Properties
    {
        _MainTex("Main Texture", 2D) = "white"
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

            sampler2D _MainTex;

            v2f vert(appdata v)
            {
                return vert_default(v);
            }

            fixed4 frag(v2f vf) : SV_Target
            {
                return tex2D(_MainTex, vf.uv);
            }
            ENDCG
        }
    }
}