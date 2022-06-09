// https://www.shadertoy.com/view/MtBSWR
Shader "RMAZOR/Background/Squigly Circled Wave" {
    Properties {
        _Color ("Color", Color) = (1,1,1,1)
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

            fixed4 _Color;
            float _TimeAlt;

			v2f vert(appdata v) {
                return vert_default(v);
            }

            fixed4 frag(v2f i) : SV_Target {
                float tau = 2. * PI;
                //Scaling
                float2 pos = i.uv;
                float2 scaled = (pos * _ScreenParams.xy * 2. - _ScreenParams.xy) / (_ScreenParams.y * 1.7 * screen_ratio());
                float t = 2. / _ScreenParams.y;
                //Info
                float l = length(scaled);
                float angle = atan2(scaled.y, scaled.x) + PI; //[-Pi,Pi]->[0,2Pi]
                float wave = fmod(_TimeAlt * 2., tau); //[0,2Pi]
                //Background
                fixed3 color = _Color;
                //Calculating
                float angle_difference = abs(wave - angle);
                float distance_to_wave = min(angle_difference, tau - angle_difference);
                float final_multiplier = pow(max(1., distance_to_wave), -4.);
                //Rings
                float ring1 = .50 + .03 * cos(angle * 7.) * final_multiplier;
                float ring2 = .485 + .03 * cos(angle * 7. + tau / 3.) * final_multiplier;
                float ring3 = .47 + .03 * cos(angle * 7. - tau / 3.) * final_multiplier;
                //Drawing
                fixed3 color_ring1 = rgb(10, 205, 203);
                fixed3 color_ring2 = rgb(46, 72, 82);
                fixed3 color_ring3 = rgb(54, 147, 147);
                color = lerp(color, color_ring1, smoothstep(.01 + t, .01, abs(ring1 - l)));
                color = lerp(color, color_ring2, smoothstep(.01 + t, .01, abs(ring2 - l)));
                color = lerp(color, color_ring3, smoothstep(.01 + t, .01, abs(ring3 - l)));
                //Final
                return fixed4(color, 1.);
            }
            ENDCG
        }
    }
}