Shader "RMAZOR/Background/Triangles" {
    Properties {
		_Color1 ("Color 1", Color) = (0,0,0,1)
		_Color2 ("Color 2", Color) = (1,1,1,1)
        _Size ("Size", Range(1, 10)) = 1
        _Ratio("Ratio", Range(1, 2)) = 1.224744
        _A("A", Float) = 0
        _B("B", Float) = 0
        [IntRange] _Tiling ("Tiling", Range(1, 10)) = 1
		_Direction ("Direction", Range(0, 1)) = 0
		_WrapScale ("Wrap Scale", Range(0, 1)) = 0
		_WrapTiling ("Wrap Tiling", Range(1, 10)) = 1
    }
    SubShader {
		Tags { 
			"Queue"="Transparent" 
			"IgnoreProjector"="True" 
			"RenderType"="Opaque" 
			"PreviewType"="Plane"
			"CanUseSpriteAtlas"="False"
		}
		Cull Off
		Lighting Off
		ZWrite Off
		Blend One OneMinusSrcAlpha
        Pass {
            CGPROGRAM
            
            #pragma vertex vert
            #pragma fragment frag
            #include "Common.cginc"

			fixed4 _Color1,_Color2;
			float  _Direction, _WrapScale, _WrapTiling, _Tiling;
            float _Size, _Ratio, _A, _B;

            float rand(float2 co) {
                return frac(dot(co, float2(_A, _B)));
            }

            float2 get_triangle_coords(float2 uv) {
                uv.y *= _Ratio;
                float2 coords = floor(uv);
                coords.x *= 2.0;
                float fmody = floor(fmod(coords.y, 2.0));
                float2 local = float2(frac(uv.x + fmody * 0.5) - 0.5, frac(uv.y));
                if (local.y > abs(local.x) * 2.0)
                    coords.x += local.x < 0.0 ? 1.0 : -1.0;
                if (local.x >= 0.0 && fmody == 0.0)
                    coords.x += 2.0;
                return coords;
            }

            v2f vert(appdata v) {
                return vert_default(v);
            }

            fixed4 frag(v2f i) : SV_Target {
            	if (all(_Color1 == _Color2))
					return _Color1;
                float2 pos = wrap_pos(i.uv, _Tiling, _Direction, _WrapScale, _WrapTiling);
                pos /= _Size * 0.02f;
                float2 triang = get_triangle_coords(pos);
                return rand(triang) < 0.5 ? _Color1 : _Color2;
            }
            
            ENDCG
        }
    }
}