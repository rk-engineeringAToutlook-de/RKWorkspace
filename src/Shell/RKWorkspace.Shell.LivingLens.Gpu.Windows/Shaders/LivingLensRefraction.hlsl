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
    float portalPull;
    float edgeContact;
    float2 objectMotion;
};

float4 Main(float4 position : SV_POSITION, float2 uv : TEXCOORD) : SV_TARGET
{
    float2 normalized = (uv - lensCenter) / max(lensRadius, float2(0.001, 0.001));
    float radius = length(normalized);
    float inside = saturate(1.0 - radius);

    float wave = sin((radius * 34.0) - (timeSeconds * 1.8)) * 0.004;
    float edgeDraw = smoothstep(0.18, 0.95, edgeContact) * portalPull;
    float sink = inside * inside * (0.060 + (tunnelDepth * 0.084) + (edgeDraw * 0.035));
    float2 pull = normalize(normalized + float2(0.0001, 0.0001)) * sink;
    float2 motionBend = objectMotion * inside * (0.006 + (edgeDraw * 0.008));
    float2 sampleUv = uv - pull + motionBend + (wave * normalized);

    float4 desktop = desktopInput.Sample(desktopSampler, sampleUv);
    float rim = smoothstep(0.72, 1.0, radius) * inside;
    float innerWall = smoothstep(0.18, 0.78, inside) * smoothstep(1.0, 0.28, radius);
    float depth = inside * inside * (tunnelDepth + (portalPull * 0.35));
    float3 glassTint = float3(0.92, 0.98, 1.0);
    float3 color = lerp(desktop.rgb, desktop.rgb * glassTint, 0.16 * inside);
    color += rim * float3(0.26, 0.36, 0.40);
    color += edgeDraw * inside * float3(0.05, 0.08, 0.09);
    color -= depth * innerWall * float3(0.10, 0.12, 0.13);

    float alpha = inside * (0.18 + (rim * 0.44) + (depth * 0.20) + (edgeDraw * 0.08));
    return float4(color, alpha);
}
