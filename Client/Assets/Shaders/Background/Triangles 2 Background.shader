Shader "RMAZOR/Background/Triangles 2" {
    Properties {
		_Color1 ("Color 1", Color) = (0,0,0,1)
		_Color2 ("Color 2", Color) = (1,1,1,1)
        _Size ("Size", Range(1, 100)) = 1
        _Ratio("Ratio", Range(1, 100)) = 1.224744
        _A("A", Range(-2, 2)) = 0
        _B("B", Range(0, 0.5)) = 0
    	_C("C", Range(-10, 10)) = 1
    	_D("D", Range(0, 1)) = 1
    	_E("E", Range(0, 1)) = 1
    	[IntRange] _F("F", Range(0, 20)) = 1
    	[MaterialToggle] _Smooth ("Smooth", Float) = 1
    	[MaterialToggle] _Mirror ("Mirror", Float) = 1
    	[MaterialToggle] _Trunc ("Trunc", Float) = 1
    	[MaterialToggle] _TruncColor2 ("TruncColor2", Float) = 1
        _Direction ("Direction", Range(-1, 1)) = 1
    	_AnimationSpeed ("Animation Speed", Range(0, 10)) = 0
    	_AnimationRange ("Animation Range", Range(0.1, 1)) = 0.1
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
            #include "../Common.cginc"

			fixed4 _Color1,_Color2;
            float _Size, _Ratio, _A, _B, _C, _D, _E, _F, _Direction, _AnimationSpeed, _AnimationRange;
            bool _Smooth, _Mirror, _Trunc, _TruncColor2;

            float rand(float2 co) {
				float d = dot(co, float2(_A, _B));
            	float f = frac(d);
            	if (_Mirror)
            	{
					f = 2.0 * (f < 0.5 ? f : 1.0 - f); 
            		f = clamp(f, 0.0, 1.0);
            	}
            	return f;
            }

            float2 get_triangle_coords(float2 uv) {
                uv.y *= _Ratio;
                float2 coords = floor(uv);
                coords.x *= _C;
                coords.y *= _D;
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

            fixed4 lerp_colors(float lerp_coefficient)
            {
            	float lc = lerp_coefficient;
            	lc = clamp(lerp_coefficient + _AnimationRange * sin(_Time.y * .1 * _AnimationSpeed), 0., 1.);
	            return lerp(_Color1, _Color2, lc);
            }

            fixed4 frag(v2f i) : SV_Target {
            	if (all(_Color1 == _Color2))
					return _Color1;
                float2 pos = i.uv;
            	pos.x = lerp(i.uv.x, i.uv.y, _Direction);
				pos.y = lerp(i.uv.y, 1 - i.uv.x, _Direction);
            	pos.x *= _F;
                pos /= _Size * 0.02f;
                float2 triang = get_triangle_coords(pos);
            	float lerp_coeff = rand(triang);
            	if (_Smooth)
            	{
            		if (_Trunc && !_TruncColor2)
						return lerp_coeff < _E ? _Color1 : lerp_colors(lerp_coeff);
            		if (_Trunc && _TruncColor2)
						return lerp_coeff < _E ? lerp_colors(lerp_coeff) : _Color2;
            		return lerp_colors(lerp_coeff);
            	}
            	return lerp_coeff < _E ? _Color1 : _Color2;
            }
            
            ENDCG
        }
    }
}