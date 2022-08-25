Shader "RMAZOR/Transition/TriaHex" {
    Properties {
        _BackgroundColor ("Background Color", Color) = (0,0,0,1)
        _EdgesColor("Edges Color", Color) = (1,1,1,1)
        _Scale("Scale", Range(1, 10)) = 3
        _TransitionValue1("Transition Value 1", Range(0, 1)) = 0
        _TransitionValue2("Transition Value 2", Range(0, 1)) = 0
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

			fixed4 _BackgroundColor, _EdgesColor;
			float _Scale, _TransitionValue1, _TransitionValue2;
			
			float line_distance(in float2 p, in float2 a, in float2 b) {
			    float2 pa = p - a, ba = b - a;
				float h = clamp(dot(pa,ba) / dot(ba,ba), 0., 1.);	
				return length(pa - ba * h);
			}

			bool right_side(in float2 p, in float2 a, in float2 b) {
				float tv = .5 * (1 - _TransitionValue1);
			    return (p.x-a.x)*(b.y-a.y)-(p.y-a.y)*(b.x-a.x) > tv;
			}
  
            fixed4 frag (v2f f_i) : SV_Target {
       			float2 pos = f_i.uv;
       			pos.x *= screen_ratio();
            	pos.x *= -1.;
                float s32 = sqrt(3.) / 2.;
            	float2 uv = _Scale * pos;
            	
			    // Tile vertically~diagonally along a triangle edge:
			    // uv.y/s32 is the V coordinate in a UV frame shaped like /_
			    uv -= floor(uv.y / s32) * float2(.5, s32);
			    // And tile by 1 horizontally
			    uv.x = uv.x - floor(uv.x);
			    
			    // Look at the three triangle edges individually.
			    // Here's the horizontal _ one that becomes / when t==1
            	float tv = _TransitionValue2 * .5;
			    float2 orig = float2(0, 0);
			    float2 end = tv * float2(.5, s32) + float2(1 - tv, 0);
			    
			    // Distance to the line, and are we on its right side
			    float dist1 = line_distance(uv, orig, end);
			    bool right1 = right_side(uv, orig, end);
			    
			    // Transform uv into the second edge's local frame
			    float2 uv2 = uv - float2(1, 0);
			    uv2 = float2(-.5 * uv2.x + s32 * uv2.y, -s32 * uv2.x - .5 * uv2.y);
			    // Then same computation
			    float dist2 = line_distance(uv2, orig, end);
			    bool right2 = right_side(uv2, orig, end);
       
			    // Transform into the third edge's frame
			    float2 uv3 = uv - float2(.5, s32);
			    uv3 = float2(-.5 * uv3.x - s32 * uv3.y, s32 * uv3.x - .5 * uv3.y);
			    // And again
			    float dist3 = line_distance(uv3, orig, end);
			    bool right3 = right_side(uv3, orig, end);
			    
			    // Background
			    fixed4 frag_color = _BackgroundColor;
            	frag_color *= _BackgroundColor.a;
            	fixed4 edges_color = _EdgesColor;
            	edges_color *= _EdgesColor.a;
       
			    // Inside triangles
			    if ((right1 && right2 && right3) || (!right1 && !right2 && !right3))
			        frag_color = fixed4(0, 0, 0, 0);
			    // Triangle edges
			    // Truncated
			    dist1 = !right3 && right2 ? 10 : dist1;
			    dist2 = !right1 && right3 ? 10 : dist2;
			    dist3 = !right2 && right1 ? 10 : dist3;
       
			    float distToTriangle = min(min(dist1, dist2), dist3);
       	
			    // Anti-aliasing
			    frag_color = lerp(
			    	frag_color,
			    	edges_color,
			    	smoothstep(2 * _Scale / _ScreenParams.y, 0, distToTriangle));

            	return frag_color;
            }
            ENDCG
        }
    }
}