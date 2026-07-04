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
    float edgeSqueeze;
    float apexSqueeze;
    float handoverProgress;
    float tunnelClosing;
    float shadowTunnelSuction;
    float premiumRefraction;
    float tunnelAperture;
    float physicalGlass;
    float glassThickness;
    float chromaticEdge;
    float lensContactShadow;
    float glassCaustics;
    float specularGlassSweeps;
    float2 objectMotion;
};

float4 Main(float4 position : SV_POSITION, float2 uv : TEXCOORD) : SV_TARGET
{
    float2 normalized = (uv - lensCenter) / max(lensRadius, float2(0.001, 0.001));
    float radius = length(normalized);
    float inside = saturate(1.0 - radius);

    float squeeze = smoothstep(0.0, 1.0, edgeSqueeze);
    float apex = smoothstep(0.0, 1.0, apexSqueeze);
    float aperture = smoothstep(0.0, 1.0, tunnelAperture);
    float premium = smoothstep(0.0, 1.0, premiumRefraction);
    float wave = sin((radius * (34.0 + (premium * 32.0))) - (timeSeconds * 1.35)) * (0.003 + (premium * 0.002));
    float edgeDraw = smoothstep(0.18, 0.95, edgeContact) * portalPull;
    float sink = inside * inside * (0.060 + (tunnelDepth * 0.084) + (edgeDraw * 0.035) + (squeeze * 0.032) + (apex * 0.022));
    float2 pull = normalize(normalized + float2(0.0001, 0.0001)) * sink;
    float2 motionBend = objectMotion * inside * (0.006 + (edgeDraw * 0.008) + (squeeze * 0.006));
    float fresnel = smoothstep(0.62, 1.0, radius) * inside;
    float physical = smoothstep(0.0, 1.0, physicalGlass);
    float thickness = smoothstep(0.0, 1.0, glassThickness);
    float2 physicalBend = normalized * inside * physical * (0.010 + (fresnel * 0.018) + (thickness * 0.012));
    float2 sampleUv = uv - pull - physicalBend + motionBend + (wave * normalized);

    float4 desktop = desktopInput.Sample(desktopSampler, sampleUv);
    float chroma = smoothstep(0.0, 1.0, chromaticEdge) * fresnel;
    float4 desktopRed = desktopInput.Sample(desktopSampler, sampleUv + normalized * chroma * 0.0022);
    float4 desktopBlue = desktopInput.Sample(desktopSampler, sampleUv - normalized * chroma * 0.0022);
    desktop.r = lerp(desktop.r, desktopRed.r, chroma * 0.55);
    desktop.b = lerp(desktop.b, desktopBlue.b, chroma * 0.55);

    float rim = smoothstep(0.72, 1.0, radius) * inside;
    float innerWall = smoothstep(0.18, 0.78, inside) * smoothstep(1.0, 0.28, radius);
    float handoverDepth = smoothstep(0.0, 1.0, handoverProgress);
    float closingFade = smoothstep(0.0, 1.0, tunnelClosing);
    float depth = inside * inside * (tunnelDepth + (portalPull * 0.35) + (handoverDepth * 0.18) + (aperture * 0.12));
    float3 glassTint = float3(0.92, 0.98, 1.0);
    float3 color = lerp(desktop.rgb, desktop.rgb * glassTint, 0.16 * inside);
    float caustic = smoothstep(0.0, 1.0, glassCaustics) * inside * (0.5 + (0.5 * sin((uv.x + uv.y + timeSeconds * 0.12) * 90.0)));
    float sweep = smoothstep(0.0, 1.0, specularGlassSweeps) * inside * saturate(1.0 - abs((normalized.y - normalized.x * 0.34) * 2.4));
    color += rim * float3(0.20, 0.29, 0.32);
    color += physical * fresnel * float3(0.20, 0.24, 0.23);
    color += thickness * rim * float3(0.13, 0.17, 0.17);
    color += caustic * float3(0.055, 0.052, 0.040);
    color += sweep * float3(0.060, 0.070, 0.066);
    color += (edgeDraw + (squeeze * 0.34)) * inside * float3(0.05, 0.08, 0.09);
    color += premium * inside * rim * float3(0.08, 0.09, 0.08);
    color += aperture * innerWall * float3(0.035, 0.040, 0.038);
    color -= depth * innerWall * float3(0.10, 0.12, 0.13);
    color -= shadowTunnelSuction * innerWall * float3(0.035, 0.038, 0.040);
    color = lerp(color, desktop.rgb, closingFade * 0.76);

    float alpha = inside * (0.16 + (rim * (0.38 + (premium * 0.12) + (physical * 0.16))) + (depth * 0.20) + (edgeDraw * 0.08) + (squeeze * 0.04) + (apex * 0.04));
    alpha += lensContactShadow * smoothstep(0.52, 0.95, radius) * inside * 0.035;
    alpha *= 1.0 - (closingFade * 0.82);
    return float4(color, alpha);
}
