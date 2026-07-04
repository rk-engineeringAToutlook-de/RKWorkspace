// RK Workspace Living Lens material shader.
// WPF ShaderEffect target: ps_3_0. This is the first active native shader
// stage for the Living Lens renderer.

sampler2D implicitInput : register(s0);

float lensCenterX : register(c0);
float lensCenterY : register(c1);
float lensRadiusX : register(c2);
float lensRadiusY : register(c3);
float openAmount : register(c4);
float intensityAmount : register(c5);
float lookMode : register(c6);
float timeSeconds : register(c7);

float4 main(float2 uv : TEXCOORD) : COLOR
{
    float2 center = float2(lensCenterX, lensCenterY);
    float2 radius = max(float2(lensRadiusX, lensRadiusY), float2(0.001, 0.001));
    float2 normalized = (uv - center) / radius;
    float distance = length(normalized);
    float inside = saturate(1.0 - distance);
    float mask = smoothstep(1.0, 0.90, distance) * intensityAmount;

    if (mask <= 0.001)
    {
        return float4(0.0, 0.0, 0.0, 0.0);
    }

    float glassLook = saturate(1.0 - abs(lookMode - 0.0));
    float wormholeLook = saturate(1.0 - abs(lookMode - 1.0));
    float hybridLook = saturate(1.0 - abs(lookMode - 2.0));
    float fresnel = smoothstep(0.56, 1.0, distance) * mask;
    float throat = smoothstep(0.80, 0.08, distance) * openAmount;
    float ripple = sin((distance * (42.0 + wormholeLook * 38.0)) - (timeSeconds * (0.72 + hybridLook * 0.28))) * 0.0035;
    float2 bend = normalized * inside * (0.030 + openAmount * 0.025 + fresnel * 0.020 + wormholeLook * 0.022);
    float2 swirl = float2(-normalized.y, normalized.x) * throat * (0.008 + wormholeLook * 0.010);
    float2 sampleUv = uv - bend + swirl + normalized * ripple;

    float chroma = fresnel * (0.0018 + glassLook * 0.0014 + hybridLook * 0.0010);
    float4 baseSample = tex2D(implicitInput, sampleUv);
    float4 redSample = tex2D(implicitInput, sampleUv + normalized * chroma);
    float4 blueSample = tex2D(implicitInput, sampleUv - normalized * chroma);
    baseSample.r = lerp(baseSample.r, redSample.r, 0.48);
    baseSample.b = lerp(baseSample.b, blueSample.b, 0.48);

    float sweepA = saturate(1.0 - abs((normalized.y - normalized.x * 0.42 + sin(timeSeconds * 0.22) * 0.08) * 2.6));
    float sweepB = saturate(1.0 - abs((normalized.y + normalized.x * 0.34 - 0.26 + cos(timeSeconds * 0.18) * 0.06) * 3.2));
    float caustic = inside * (0.5 + 0.5 * sin((uv.x * 54.0) + (uv.y * 62.0) + (timeSeconds * 0.42)));
    float3 color = baseSample.rgb;
    color += fresnel * float3(0.110, 0.125, 0.120);
    color += sweepA * inside * float3(0.070, 0.082, 0.078) * (0.68 + glassLook * 0.42);
    color += sweepB * inside * float3(0.048, 0.058, 0.056) * (0.42 + hybridLook * 0.32);
    color += caustic * float3(0.018, 0.019, 0.014) * (glassLook * 0.80 + hybridLook * 0.48);
    color -= throat * wormholeLook * float3(0.040, 0.046, 0.050);
    color = lerp(color, baseSample.rgb, 0.40 - (openAmount * 0.16));

    float alpha = mask * (0.14 + fresnel * 0.44 + inside * 0.12 + openAmount * 0.05);
    return float4(color, saturate(alpha));
}
