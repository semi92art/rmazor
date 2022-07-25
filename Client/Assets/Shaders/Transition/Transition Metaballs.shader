//https://www.shadertoy.com/view/WsdSzr
Shader "RMAZOR/Transition/Metaballs" {
    Properties {
        _Color1 ("Color", Color) = (0,0,0,1)
        _MainTex ("Texture", 2D) = "white" {}
        _TransitionValue("Transition Value", Range(0, 1)) = 0
    	_Ball1Phase1("Ball 1 Phase1", Range(-3.14, 3.14)) = 0 
    	_Ball2Phase1("Ball 2 Phase1", Range(-3.14, 3.14)) = 0 
    	_Ball3Phase1("Ball 3 Phase1", Range(-3.14, 3.14)) = 0 
    	_Ball1Phase("Ball 1 Phase", Range(0, 3.14)) = 0 
    	_Ball2Phase("Ball 2 Phase", Range(0, 3.14)) = 0 
    	_Ball3Phase("Ball 3 Phase", Range(0, 3.14)) = 0 
    	_Ball1Amp("Ball 1 Amplitude", Range(100, 5000)) = 200 
    	_Ball2Amp("Ball 2 Amplitude", Range(100, 5000)) = 200
    	_Ball3Amp("Ball 3 Amplitude", Range(100, 5000)) = 200 
		_Ball1Size("Ball 1 Size", Range(100, 5000)) = 200
    	_Ball2Size("Ball 2 Size", Range(100, 5000)) = 200 
    	_Ball3Size("Ball 3 Size", Range(100, 5000)) = 200 
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

			fixed4 _Color1, _EdgesColor;
			float _TransitionValue;
			float _Ball1Phase, _Ball2Phase, _Ball3Phase;
			float _Ball1Phase1, _Ball2Phase1, _Ball3Phase1;
			float _Ball1Amp, _Ball2Amp, _Ball3Amp;
			float _Ball1Size, _Ball2Size, _Ball3Size;

			float blob(in float2 p, in float2 o, in float r) {
			    p -= o;
			    return r * r / dot(p, p);
			}

			bool point_in(in float2 p) {
			    float2 center = 0.5 * _ScreenParams.xy;
				float tv = _TransitionValue * 10;
			    float t1 = tv * _Ball1Phase + _Ball1Phase1;
				float t2 = tv * _Ball2Phase + _Ball2Phase1;
				float t3 = tv * _Ball3Phase + _Ball3Phase1;
			    float c1 = cos(t1), s1 = sin(t1);
			    float c2 = cos(t2), s2 = sin(t2);
			    float c3 = cos(t3), s3 = sin(t3);

			    float2 mb1 = center + float2(c2, s1) * _Ball1Amp * screen_ratio();
			    float2 mb2 = center + float2(c1, s3) * _Ball2Amp * screen_ratio();
			    float2 mb3 = center + float2(c3, s2) * _Ball3Amp * screen_ratio();

				float b1s = (_Ball1Size + 300) * _TransitionValue;
				float b2s = (_Ball2Size + 300) * _TransitionValue;
				float b3s = (_Ball3Size + 300) * _TransitionValue;
				
			    float blob1 = blob(p, mb1, b1s);
			    float blob2 = blob(p, mb2, b2s);
			    float blob3 = blob(p, mb3, b3s);

			    float d = blob1 + blob2 + blob3;
			    return d > 0.5;
			}

			fixed4 frag (v2f i) : SV_Target {
				const float aa = 10;
				float2 pos = i.uv * _ScreenParams.xy;
				fixed4 frag_col = fixed4(0,0,0,0);
			    for (float k = 0.0; k < aa; k++) {
			        for (float j=0.0; j < aa; j++) {
			            frag_col += float(point_in(pos + float2(k, j) / aa));
			        }
			    }
			    frag_col /= aa * aa;
				frag_col.rgb = _Color1.rgb;
				frag_col.rgb *= frag_col.a;
			    return frag_col;
			}
			ENDCG
		}
    }
}