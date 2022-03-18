Shader "RMAZOR/Background/Triangles" {
    Properties {
		_Color1 ("Color 1", Color) = (0,0,0,1)
		_Color2 ("Color 2", Color) = (1,1,1,1)
        _Size ("Size", Range(1, 10)) = 1
        _Ratio("Ratio", Range(1, 2)) = 1.224744
        _A("A", Float) = 0
        _B("B", Float) = 0
        _Tiling ("Tiling", Range(1, 500)) = 10
		_Direction ("Direction", Range(0, 1)) = 0
		_WarpScale ("Warp Scale", Range(0, 1)) = 0
		_WarpTiling ("Warp Tiling", Range(1, 10)) = 1
    }
    SubShader {
        Tags {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
        }
        Pass {
            CGPROGRAM
            
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "Common.cginc"
            
            fixed4 _Color1, _Color2;
            float _Size, _Ratio, _A, _B;
            int    _Tiling;
			float  _Direction;
			float  _WarpScale;
			float  _WarpTiling;

            inline float rand(float2 co) {
                return frac(dot(co, float2(_A, _B)));
            }

            inline float2 getTriangleCoords(float2 uv) {
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
                float2 pos = wrap_pos(i.uv, _Tiling, _Direction, _WarpScale, _WarpTiling);
                pos /= _Size * 0.02f;
                float2 triang = getTriangleCoords(pos);

                return rand(triang) < 0.5 ? _Color1 : _Color2;
            }
            
            ENDCG
        }
    }
}