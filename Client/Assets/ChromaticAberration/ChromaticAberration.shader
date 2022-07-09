Shader "SupGames/Mobile/ChromaticAberration"
{
	Properties
	{
		_MainTex("Base (RGB)", 2D) = "" {}
	}

	CGINCLUDE

#include "UnityCG.cginc"

	UNITY_DECLARE_SCREENSPACE_TEXTURE(_MainTex);
	fixed _RedX;
	fixed _RedY;
	fixed _GreenX;
	fixed _GreenY;
	fixed _BlueX;
	fixed _BlueY;
	fixed _Distortion;
	fixed _Coefficent;
	fixed4 _MainTex_TexelSize;

	struct appdata 
	{
		fixed4 pos : POSITION;
		fixed2 uv : TEXCOORD0;
		UNITY_VERTEX_INPUT_INSTANCE_ID
	};

	struct v2f 
	{
		fixed4 pos : POSITION;
		fixed4 uv : TEXCOORD0;
		fixed2 uv1 : TEXCOORD1;
		UNITY_VERTEX_OUTPUT_STEREO
	};


	v2f vert(appdata i)
	{
		v2f o;
		UNITY_SETUP_INSTANCE_ID(i);
		UNITY_INITIALIZE_OUTPUT(v2f, o);
		UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
		o.pos = UnityObjectToClipPos(i.pos);
		o.uv.x = i.uv.x + _RedX * _MainTex_TexelSize.x;
		o.uv.y = i.uv.y + _RedY * _MainTex_TexelSize.y;
		o.uv.z = i.uv.x + _GreenX * _MainTex_TexelSize.x - 0.5h;
		o.uv.w = i.uv.y + _GreenY * _MainTex_TexelSize.y - 0.5h;
		o.uv1.x = i.uv.x + _BlueX * _MainTex_TexelSize.x - 0.5h;
		o.uv1.y = i.uv.y + _BlueY * _MainTex_TexelSize.y - 0.5h;
		return o;
	}

	fixed4 frag(v2f i) : COLOR
	{
		UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
		fixed4 c = fixed4(0.0h,0.0h,0.0h,1.0h);
		c.r = tex2D(_MainTex, UnityStereoTransformScreenSpaceTex(i.uv.xy)).r;

		fixed r2 = i.uv.z * i.uv.z + i.uv.w * i.uv.w;
		fixed2 uv = (1.0h + r2 * (0.15h + _Distortion * sqrt(r2))) * i.uv.zw + fixed2(0.5h, 0.5h);
		c.g = tex2D(_MainTex, UnityStereoTransformScreenSpaceTex(uv)).g;

		r2 = i.uv1.x * i.uv1.x + i.uv1.y * i.uv1.y;
		uv = (1.0h + r2 * (0.15h + 1.5h*_Distortion * sqrt(r2))) * i.uv1.xy + fixed2(0.5h, 0.5h);
		c.b = tex2D(_MainTex, UnityStereoTransformScreenSpaceTex(uv)).b;
		return c;
	}
	ENDCG

	Subshader
	{
		Pass
		{
		  ZTest Always Cull Off ZWrite Off
		  Fog { Mode off }
		  CGPROGRAM
		  #pragma vertex vert
		  #pragma fragment frag
		  #pragma fragmentoption ARB_precision_hint_fastest
		  ENDCG
		}
	}
	Fallback off
}