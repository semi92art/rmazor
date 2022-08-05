// https://www.shadertoy.com/view/MtBSWR
Shader "RMAZOR/Background/Squigly Circled Wave" {
    Properties {
        _Color ("Background", Color) = (1,1,1,1)
        _Ring1Color("Ring 1", Color) = (1,1,1,1)
        _Ring2Color("Ring 2", Color) = (1,1,1,1)
        _Ring3Color("Ring 3", Color) = (1,1,1,1)
        _TimeAlt ("Time", Float) = 0
    }
    SubShader {
        Tags {
            "Queue"       = "Transparent"
            "RenderType"  = "Transparent"
            "PreviewType" = "Plane"
        }
        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ PIXELSNAP_ON
            #include "../Common.cginc"

            fixed4 _Color, _Ring1Color, _Ring2Color, _Ring3Color;
            float _TimeAlt;

			v2f vert(appdata v) {
                return vert_default(v);
            }

            fixed4 frag(v2f i) : SV_Target {
	            float radius = 0.4 * screen_ratio();
				float lineWidth = 5.0; // in pixels
				float glowSize = 3; // in pixels
			    
			    float pixelSize = 1.0/min(_ScreenParams.x, _ScreenParams.y);
				lineWidth *= pixelSize;
				glowSize *= pixelSize;
			    glowSize *= 2.0;
			    
  				float2 uv = i.uv - float2(.5,.5);
			    uv.x *= screen_ratio();
			    
			    float len = length(uv);
				float angle = atan2(uv.y, uv.x);
			    
				float fallOff = frac(-0.5*(angle/PI)-_TimeAlt*0.5);
			    
			    lineWidth = (lineWidth-pixelSize)*0.5*fallOff;
				float c = smoothstep(pixelSize, 0.0, abs(radius - len) - lineWidth)*fallOff;
				c += smoothstep(glowSize*fallOff, 0.0, abs(radius - len) - lineWidth)*fallOff*0.5;    
			   
				fixed4 frag_col = fixed4(c,c,c,c);
            	return frag_col + _Color;
            }
            ENDCG
        }
    }
}