Shader "RMAZOR/Transition/Texture Bubbles" {
    Properties {
        _TransitionValue("Transition Value", Range(0, 1)) = 0
    	_MainText("Main Texture", 2D) = "white"
    	_SecondTex("Second Texture", 2D) = "white"
    	_TransitionTex("Transition Texture", 2D) = "white"
    }
    SubShader {
		Tags { 
			"Queue"             = "Transparent-1"
			"IgnoreProjector"   = "True" 
			"RenderType"        = "Opaque" 
			"PreviewType"       = "Plane"
			"CanUseSpriteAtlas" = "False"
		}
		Cull Off
		Lighting Off
		ZWrite Off
		Blend One OneMinusSrcAlpha
		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "../Common.cginc"

            v2f vert (appdata v) {
				return vert_default(v);
			}

			#define S(v) smoothstep(0.,1.5*fwidth(v), v )
			
			float _TransitionValue;
			sampler2D _MainTex, _SecondTex, _TransitionTex;

			// Mirror every side
			float2 mirror(float2 v) {
			    // The progress is added to floattor making it 0 to 2
			    // Se we mod by 2
			  float2 m = fmod(v, 2.0);
			    // Not sure about this one
			  return lerp(m, 2.0 - m, step(1.0, m));
			}
			float cubicInOut(float t) {
			  return t < 0.5
			    ? 4.0 * t * t * t
			    : 0.5 * pow(2.0 * t - 2.0, 3.0) + 1.0;
			}

			bool keyToggle(int ascii) 
			{
				return (tex2D(_TransitionTex,float2((.5+float(ascii))/256.,0.75)).x > 0.);
			}

			fixed4 frag (v2f i) : SV_Target {
			    float2 uv = i.uv;			    
			    // float progress = cubicInOut(smoothstep(0.1,0.9,sin(t * 2.) * 0.5 + 0.5));
			    float p = _TransitionValue;
			    float mask = tex2D(_TransitionTex, uv).r;
			    
			    float stepMask = S(mask - p);
			    fixed4 img1 = tex2D(_SecondTex, mirror(float2(uv.x - (1. - p) * mask,uv.y)));
			    // fixed4 img1 = fixed4(0,0,0,0);
			    // fixed4 img2 = tex2D(_MainTex, mirror(float2(uv.x + p * mask,uv.y)));
			    fixed4 img2 = fixed4(0,0,0,0);

			    // Output to screen
			    return lerp(img1,img2,stepMask);
			}
			ENDCG
		}
    }
}