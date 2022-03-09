#define PI 3.14159265359
#define EPS 0.001
#include <HLSLSupport.cginc>

inline float2 wrap_pos(
    float2 uv,
    int tiling,
    float direction,
    float wrap_scale,
    float wrap_tiling)
{
    float2 pos;
    pos.x = lerp(uv.x, uv.y, direction);
    pos.y = lerp(uv.y, 1 - uv.x, direction);
    pos.x += sin(pos.y * wrap_tiling * PI * 2) * wrap_scale;
    pos.x *= tiling;
    return pos;
}

inline float3 rgb(float r, float g, float b)
{
    const float c = 0.003921568;
    return float3(r * c, g * c, b * c);
}
