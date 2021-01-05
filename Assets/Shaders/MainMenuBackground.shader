Shader "Clickers/MainMenuBackground"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color1("Color 1", Color) = (1,1,1,1)
        _Color2("Color 2", Color) = (1,1,1,1)
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }
        
        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha


        Pass
        {
            Name "Default"
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord  : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
            };

            sampler2D _MainTex;
 
            float _AlphaCoeff;
            float4 _MainTex_ST;
            fixed4 _Color1;
            fixed4 _Color2;

            v2f vert(appdata_t v)
            {
                v2f OUT;
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
                OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                OUT.color = v.color;

                // Effects start
                float realXcoord = v.texcoord.x / 10;
                float realYcoord = v.texcoord.y / 10; 
                float crdC = (realXcoord + realYcoord) * 0.5;
                fixed4 resCol = crdC * _Color1 + (1 - crdC) * _Color2;
                OUT.color = resCol;
                // Effects end
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                half4 color = (tex2D(_MainTex, IN.texcoord)) * IN.color;

                #ifdef UNITY_UI_ALPHACLIP
                clip (color.a - 0.001);
                #endif

                return color;
            }
        ENDCG
        }
    }
}