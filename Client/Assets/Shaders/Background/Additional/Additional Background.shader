Shader "RMAZOR/Background/Additional Background" {
	Properties
	{
		_MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)
		[MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
		[IntRange] _StencilRef ("Stencil Reference Value", Range(0,255)) = 0
	}

	SubShader
	{
		Tags
		{ 
			"Queue"             = "Transparent" 
			"IgnoreProjector"   = "True" 
			"RenderType"        = "Transparent" 
			"PreviewType"       = "Plane"
			"CanUseSpriteAtlas" = "True"
		}

		Cull Off
		Lighting Off
		ZWrite Off
		Blend One OneMinusSrcAlpha
		
		Stencil {
			Ref [_StencilRef]
			Comp Equal
		}

		Pass
		{
		CGPROGRAM
			#pragma vertex   vert
			#pragma fragment frag
			#pragma multi_compile _ PIXELSNAP_ON
			#include "../../Common.cginc"
		
			v2f vert(appdata v) {
				return vert_default(v);
			}

	     	fixed4    _Color;
			sampler2D _MainTex;
			float     _AlphaSplitEnabled;

			fixed4 sample_sprite_texture (float2 uv)
			{
				fixed4 color = tex2D (_MainTex, uv);
#if UNITY_TEXTURE_ALPHASPLIT_ALLOWED
				if (_AlphaSplitEnabled)
					color.a = tex2D (_AlphaTex, uv).r;
#endif
				return color;
			}

			fixed4 frag(v2f IN) : SV_Target
			{
				fixed4 c = sample_sprite_texture (IN.uv) * IN.color;
				c.rgb *= c.a;
				return c;
			}
		ENDCG
		}
	}
}