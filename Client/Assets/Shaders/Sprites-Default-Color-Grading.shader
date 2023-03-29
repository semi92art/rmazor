Shader "RMAZOR/Sprites Default Color Grading"
{
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)
		_Contrast("Contrast", Range(0, 2)) = 0
	}

	SubShader
	{
		Tags
		{ 
			"Queue"            ="Transparent" 
			"IgnoreProjector"  ="True" 
			"RenderType"       ="Transparent" 
			"PreviewType"      ="Plane"
			"CanUseSpriteAtlas"="True"
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
			#include "UnityCG.cginc"
			#include "Common.cginc"

			float _Contrast;
			fixed4 _Color;

			v2f vert(appdata v)
			{
				v2f OUT;
				OUT.vertex = UnityObjectToClipPos(v.vertex);
				OUT.uv = v.uv;
				OUT.color = v.color * _Color;
#ifdef PIXELSNAP_ON
				OUT.vertex = UnityPixelSnap (OUT.vertex);
#endif
				return OUT;
			}

			sampler2D _MainTex;

			fixed4 SampleSpriteTexture (float2 uv)
			{
				fixed4 color = tex2D (_MainTex, uv);
				return color;
			}

			half3 AdjustContrast(half3 color, half contrast) {
				#if !UNITY_COLORSPACE_GAMMA
				    color = LinearToGammaSpace(color);
				#endif
				    color = saturate(lerp(half3(0.5, 0.5, 0.5), color, contrast));
				#if !UNITY_COLORSPACE_GAMMA
				    color = GammaToLinearSpace(color);
				#endif
				    return color;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				fixed4 c = SampleSpriteTexture (i.uv) * i.color;
				fixed3 c_rgb = c.rgb;
				c_rgb = AdjustContrast(c_rgb, _Contrast);
				c.rgb = c_rgb;
				c.rgb *= c.a;
				return c;
			}
		ENDCG
		}
	}
}