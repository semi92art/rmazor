#define PI 3.14159265359
#define EPS 0.001
#include <HLSLSupport.cginc>
#include <UnityCG.cginc>

struct appdata {
    float4 vertex : POSITION;
    float4 color : COLOR;
    float2 uv : TEXCOORD0;
};

struct v2f {
    float4 vertex : SV_POSITION;
    fixed4 color : COLOR;
    float2 uv : TEXCOORD0;
};

float2 wrap_pos(
    float2 uv,
    float tiling,
    float direction,
    float wrap_scale,
    float wrap_tiling) {
    float2 pos = uv;
    if (abs(direction) > EPS) {
        pos.x = lerp(uv.x, uv.y, direction);
        pos.y = lerp(uv.y, 1 - uv.x, direction);
    }
    if (abs(wrap_tiling) > EPS && abs(wrap_scale) > EPS)
        pos.x += sin(pos.y * wrap_tiling * PI * 2.0) * wrap_scale;
    pos.x *= tiling;
    return pos;
}

float3 rgb(float r, float g, float b) {
    const float c = 0.003921568;
    return float3(r * c, g * c, b * c);
}

v2f vert_default(appdata IN) {
    v2f OUT;
    OUT.vertex = UnityObjectToClipPos(IN.vertex);
    OUT.uv = IN.uv;
    OUT.color = IN.color;
    #ifdef PIXELSNAP_ON
    OUT.vertex = UnityPixelSnap(OUT.vertex);
    #endif
    return OUT;
}

bool odd(int v) {
    return v % 2 == 1;
}

bool even(int v) {
    return v % 2 == 0;
}

float screen_ratio() {
    return _ScreenParams.x / _ScreenParams.y;
}

float clamp_inverse(float v, float min, float max) {
    return v > max ? min : v < min ? max : v;
}

float3 float_3(float v) {
    return float3(v,v,v);
}

float dist(float2 p0, float2 pf) {
    return sqrt((pf.x-p0.x)*(pf.x-p0.x)+(pf.y-p0.y)*(pf.y-p0.y));
}

void rotate(inout float2 p, float a) {
    p = cos(a) * p + sin(a) * float2(p.y, -p.x);
}
