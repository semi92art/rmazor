#include <HLSLSupport.cginc>

void rotate(inout float2 p, float a) {
    p = cos(a) * p + sin(a) * float2(p.y, -p.x);
}

float circle(float2 p, float r) {
    return (length(p / r) - 1.0) * r;
}

float rand_parallax(float2 c) {
    return frac(sin(dot(c.xy, float2(12.9898, 78.233))) * 43758.5453);
}

void bokeh_layer(inout fixed3 color, float2 p, fixed3 c) {
    float wrap = 450.0;
    if (fmod(floor(p.y / wrap + 0.5), 2.0) == 0.0)
    {
        p.x += wrap * 0.5;
    }

    float2 p2 = fmod(p + 0.5 * wrap, wrap) - 0.5 * wrap;
    float2 cell = floor(p / wrap + 0.5);
    float cell_r = rand_parallax(cell);

    c *= frac(cell_r * 3.33 + 3.33);
    float radius = lerp(30.0, 70.0, frac(cell_r * 7.77 + 7.77));
    p2.x *= lerp(0.9, 1.1, frac(cell_r * 11.13 + 11.13));
    p2.y *= lerp(0.9, 1.1, frac(cell_r * 17.17 + 17.17));

    float sdf = circle(p2, radius);
    float circle = 1.0 - smoothstep(0.0, 1.0, sdf * 0.04);
    float glow = exp(-sdf * 0.025) * 0.3 * (1.0 - circle);
    color += c * (circle+glow);
}

fixed4 parallax_color(v2f i, float size, fixed4 color1, fixed4 color2)
{
    float2 p = (2.0 * i.uv * size * _ScreenParams.xy - _ScreenParams.xy) / _ScreenParams.x * 1000.0;
    float3 col = float3(0,0,0);
    float time = 50 * sin(_Time.x * 0.2) - 100;
    float3 col1 = lerp(color1.rgb, color2.rgb, 0.2);
    float3 col2 = lerp(color1.rgb, color2.rgb, 0.4);
    float3 col3 = lerp(color1.rgb, color2.rgb, 0.6);
    float3 col4 = lerp(color1.rgb, color2.rgb, 0.8);
    rotate(p, 0.2 + time * 0.03);
    bokeh_layer(col, p + float2(-50.0 * time + 0.0, 0.0), col1);
    rotate(p, 0.3 - time * 0.05);
    bokeh_layer(col, p + float2(-70.0 * time + 33.0, -33.0), col2);
    rotate(p, 0.5 + time * 0.07);
    bokeh_layer(col, p + float2(-60.0 * time + 55.0, 55.0), col3);
    rotate(p, 0.9 - time * 0.03);
    bokeh_layer(col, p + float2(-25.0 * time + 77.0, 77.0), col4);
    return fixed4(col, 0.0);
}
