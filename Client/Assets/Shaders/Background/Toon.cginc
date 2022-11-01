#include <HLSLSupport.cginc>

fixed4 toon_color(
    fixed4 color_1,
    fixed4 color_2,
    float  lerp_coefficient,
    float  toon_step)
{
    float lc = lerp_coefficient;
    for (float i = 0.; i < 1. + toon_step; i += toon_step)
    {
        if (lc > i) continue;
        lc = i; break;
    }
    lc = clamp(lc, 0., 1.);
    return lerp(color_1, color_2, lc);
}
