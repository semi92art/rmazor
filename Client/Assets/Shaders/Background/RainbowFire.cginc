#include <HLSLSupport.cginc>
#include <UnityShaderVariables.cginc>

float4 mod289(float4 x)
{
  return x - floor(x * (1.0 / 289.0)) * 289.0;
}

float4 permute(float4 x)
{
  return mod289(((x*34.0)+10.0)*x);
}

float noise(float2 n) {
    const float2 d = float2(0.0, 1.0);
    float2 b = floor(n), f = frac(n);//smoothstep(float2(0.0), float2(1.0), fract(n));
    
    float4 p = mod289(b.xyxy + float4(0,0,1,1));
    float4 v = permute(permute(p.xzxz) + p.yyww) / 289.0;
    return lerp(lerp(v.x, v.y, f.x), lerp(v.z, v.w, f.x), f.y);
}

float fbm(float2 n) {
    float total = 0.0, amplitude = 1.0;
    for (int i = 0; i < 5; i++) {
        // float val = 0.5 * cnoise(n) + 0.5;
        float val = noise(n);
        total += val * amplitude;
        n += n*1.7;
        amplitude *= 0.47;
    }
    return total / 1.84351981;
}

// Found on https://gist.github.com/983/e170a24ae8eba2cd174f
// Original: http://lolengine.net/blog/2013/07/27/rgb-to-hsv-in-glsl
float3 hsv2rgb(float3 c)
{
    float4 K = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
    float3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
    return c.z * lerp(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y);
}


fixed4 rainbow_fire(float2 uv) : SV_Target
{
    float iTime = _Time.x;
    float2 speed = float2(0.1, 0.9);
    float alpha = 1.0;
    
	float dist = 3.5-sin(iTime*0.4)/1.89;

    float2 p = uv * dist * _ScreenParams.xy / _ScreenParams.xx;
    p += sin(p.yx*4.0+float2(.2,-.3)*iTime)*0.04;
    p += sin(p.yx*8.0+float2(.6,+.1)*iTime)*0.01;
    
    //p.x -= iTime/1.1;
    float distort1 = fbm(p - iTime * 0.3+1.0*sin(iTime+0.5)/2.0);
    float distort2 = fbm(p - iTime * 0.9 - 10.0*cos(iTime)/15.0);
    float distort3 = fbm(p - iTime * 1.4 - 20.0*sin(iTime)/14.0);
    float distort = (distort1 - 2.0 * distort2 + 0.6 * distort3) / 3.8;
    
    float fire = fbm(p + distort - iTime * speed.y);
    fire *= clamp(0.7-0.4*p.y, 0.0, 1.0);
    fire *= 4.5;
    
    float highlights = 0.4*pow(max(0.0, fire - 0.3), 8.0);
    float lowlights = 0.3*pow(fire, 2.0);
    
    float hue = frac(0.41*(p.x + distort) + 0.23);
    float3 color = hsv2rgb(float3(hue, 0.9, 0.9));
    color *= highlights + lowlights;
    color /= 1.0 + max(float3(0,0,0), color);
    return fixed4(color, alpha);
}