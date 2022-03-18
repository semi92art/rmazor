#define PI 3.14159265359
#define EPS 0.001
#include <HLSLSupport.cginc>

struct appdata
{
    float4 vertex : POSITION;
    float4 color : COLOR;
    float2 uv : TEXCOORD0;
};

struct v2f
{
    float4 vertex : SV_POSITION;
    fixed4 color : COLOR;
    float2 uv : TEXCOORD0;
};

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

inline v2f vert_default(appdata IN){
    v2f OUT;
    OUT.vertex = UnityObjectToClipPos(IN.vertex);
    OUT.uv = IN.uv;
    OUT.color = IN.color;
    #ifdef PIXELSNAP_ON
    OUT.vertex = UnityPixelSnap (OUT.vertex);
    #endif

    return OUT;
}