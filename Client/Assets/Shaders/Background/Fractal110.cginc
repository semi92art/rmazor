#include <UnityShaderVariables.cginc>

float3 R(float3 p,float3 a, float t) {
    return lerp(a*dot(p,a),p,cos(t))+sin(t)*cross(p,a);
}
            
float3 H(float h) {
    return cos(h*6.3+float3(1,1,1))*.5+.5;
}

fixed4 fractal(float2 uv, float speed, float alpha)
{
    float3 p = float3(_ScreenParams.xy, 0);
    float3 r = float3(_ScreenParams.xy, 0);
    float3 c = float3(0,0,0);
    float3 pos = float3(uv * _ScreenParams.xy, 0);
    float3 d = normalize(float3((pos-.5*r.xy)/r.y,1));
    for(float i=0.,s,e,g=0.,t=speed*_Time.x;i++<90.;){
        p=g*d;
        p.z-=.4;
        p=R(p,H(t*.01),t*.2);
        p=abs(p);
        s=3.;
        for(int j=0;j++<6;)
            s*=e=max(1./dot(p,p),3.),
            p=p.x<p.y?p.zxy:p.zyx,
            p=abs(p*e-float3(5,1,5)),
            p=R(p,normalize(float3(2,2,1)),2.1);
        g+=e=length(p.xz)/s+1e-4;
        c += lerp(float3(1,1,1), H(log(s)*.3),.4) * .019/exp(.2*i*i*e);
    }
    c=c*c*c*c*alpha;
    return fixed4(c,alpha);
}