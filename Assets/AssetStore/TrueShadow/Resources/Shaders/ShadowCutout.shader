Shader "Hidden/ShadowCutout"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Offset ("Offset", Vector) = (0,0,0,0)
    }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Blend Zero One, One One
        BlendOp Add, RevSub
        ColorMask a

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            uniform sampler2D _MainTex;
            // uniform float4 _MainTex_TexelSize;
            uniform float2 _Offset;

            fixed4 frag (v2f i) : SV_Target
            {
                // uint2 pos = i.uv* _MainTex_TexelSize.zw;
                // float checker = (pos.x + pos.y%2)%2;
                // return fixed4(0,0,0, tex2D(_MainTex, i.uv + _Offset).a + checker);
                return fixed4(0,0,0, tex2D(_MainTex, i.uv + _Offset).a);
            }
            ENDCG
        }
    }
}
