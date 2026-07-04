// RK Workspace Living Lens refraction contract.
// This is the shader target for the Direct2D/Win2D renderer path. The current
// WPF composition slice mirrors these equations in managed code until the
// native HLSL pipeline is activated.

Texture2D desktopInput : register(t0);
SamplerState desktopSampler : register(s0);

cbuffer LivingLensConstants : register(b0)
{
    float2 lensCenter;
    float2 lensRadius;
    float tunnelDepth;
    float absorption;
    float timeSeconds;
    float shadowSuction;
    float2 objectMotion;
};

float4 Main(float4 position : SV_POSITION, float2 uv : TEXCOORD) : SV_TARGET
{
    float2 normalized = (uv - lensCenter) / max(lensRadius, float2(0.001, 0.001));
    float radius = length(normalized);
    float inside = saturate(1.0 - radius);

    float wave = sin((radius * 28.0) - (timeSeconds * 1.6)) * 0.006;
    float sink = inside * inside * (0.055 + (tunnelDepth * 0.070));
    float2 pull = normalize(normalized + float2(0.0001, 0.0001)) * sink;
    float2 motionBend = objectMotion * inside * 0.010;
    float2 sampleUv = uv - pull + motionBend + (wave * normalized);

    float4 desktop = desktopInput.Sample(desktopSampler, sampleUv);
    float rim = smoothstep(0.72, 1.0, radius) * inside;
    float depth = inside * inside * tunnelDepth;
    float3 glassTint = float3(0.92, 0.98, 1.0);
    float3 color = lerp(desktop.rgb, desktop.rgb * glassTint, 0.16 * inside);
    color += rim * float3(0.26, 0.36, 0.40);
    color -= depth * float3(0.08, 0.10, 0.11);

    float alpha = inside * (0.20 + (rim * 0.42) + (depth * 0.18));
    return float4(color, alpha);
}
