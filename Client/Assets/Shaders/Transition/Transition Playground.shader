Shader "RMAZOR/Transition/Playground" {
    Properties {
        _Color1 ("Color", Color) = (0,0,0,1)
        _MainTex ("Texture", 2D) = "white" {}
        _TransitionValue("Transition Value", Range(0, 1)) = 0
    	_Size("Size", Range(0, 10)) = 1
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

			fixed4 _Color1;
			float _TransitionValue, _Size;

			struct color_fun
			{
			    fixed4 color;
			    float2 fun;
			};

			color_fun resolve_section(float2 uv)
			{
			    float2 arg = uv * 128.0;
			    float2 adj_arg = float2(2.0 * arg.x - 0.5 * arg.y, -0.5 * arg.x - 2.0 * arg.y);
				color_fun res;
				res.color = _Color1;
				res.fun = sin(adj_arg);
				return res;
			}

			float create_band_mask(float x, float blur_radius, float width, float base)
			{
			    width -= 2. * blur_radius;
			    return smoothstep(0.0, blur_radius, abs(x - base - width / 2. - blur_radius) - width / 2.);
			}

			fixed4 frag (v2f i) : SV_Target {
			    float adj_time = _TransitionValue * 10;
			    float2 uv = i.uv * _ScreenParams.xy / (_ScreenParams.x * _Size);
			    float streak_angle = (-1.0 / 3.0 * uv.x - 3.0 * uv.y) / 6.0; // As with the checkerboard, we want the wave at an angle, but a different one.
			    float anim_progress = streak_angle + 0.10 * adj_time; // Let's animate it
			    anim_progress = frac(anim_progress);
			    color_fun cf = resolve_section(uv);
			    // Get the checkerboard.
			    float mask = (cf.fun.x + cf.fun.y) / 4.0 - 0.5;
			    // Now, we will offset the checkerboard with a smoothed line.
			    // This will create the effect of the checkerboard dots appearing
			    // On the wavefront.
			    // Scaling was used so that when there is no wave, the whole checkerboard is < 0, and behind the front it's > 1
			    float shiftVal = 2.0 * (1.0 - create_band_mask(anim_progress, 0.3, 0.7, 0.0));
			    float alpha = mask + shiftVal;
			    // https://github.com/glslify/glsl-aastep
			    float aawidth = length(float2(ddx(alpha), ddy(alpha))) / sqrt(2.0);
			    alpha = smoothstep(0.5 - aawidth, 0.5 + aawidth, alpha);
			    float result = clamp(alpha, 0.0, 1.0);
			    fixed4 frag_col = cf.color * result + cf.color * (1.0 - result) * 0.5;
				if (frag_col.a < 1)
					frag_col = fixed4(0,0,0,0);
				return frag_col;
			}
			ENDCG
		}
    }
}