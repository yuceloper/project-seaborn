Shader "Seaborn/Ocean Prototype"
{
    Properties
    {
        _DeepColor ("Deep Color", Color) = (0.014, 0.075, 0.15, 1)
        _ShallowColor ("Water Body Color", Color) = (0.04, 0.24, 0.30, 1)
        _HorizonColor ("Sky Reflection Tint", Color) = (0.26, 0.43, 0.56, 1)
        _SunGlintColor ("Sun Glint Color", Color) = (1, 0.87, 0.68, 1)
        _WaveAmplitudeA ("Swell Normal", Range(0, 1)) = 0.18
        _WaveAmplitudeB ("Cross Wave Normal", Range(0, 0.5)) = 0.06
        _WaveScaleA ("Swell Scale", Range(0.01, 2)) = 0.22
        _WaveScaleB ("Cross Wave Scale", Range(0.01, 3)) = 0.46
        _WaveSpeedA ("Swell Speed", Range(-3, 3)) = 0.7
        _WaveSpeedB ("Cross Wave Speed", Range(-3, 3)) = 1.15
        _FresnelPower ("Fresnel Power", Range(0.5, 8)) = 4.5
        _SunGlintPower ("Sun Glint Sharpness", Range(8, 256)) = 48
        _SunGlintStrength ("Sun Glint Strength", Range(0, 2)) = 0.30
        _RippleStrength ("Fine Ripple Normal", Range(0, 0.5)) = 0.17
        _RippleScale ("Ripple Scale", Range(0.2, 4)) = 1.15
        _SkyReflection ("Sky Reflection", Range(0, 1)) = 0.34
        _WhitecapStrength ("Sparse Crest Foam", Range(0, 0.5)) = 0.028
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" "RenderPipeline"="UniversalPipeline" }
        Pass
        {
            Name "OceanForward"
            Tags { "LightMode"="UniversalForward" }
            ZWrite On
            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex OceanVertex
            #pragma fragment OceanFragment
            #pragma multi_compile_fog
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _DeepColor, _ShallowColor, _HorizonColor, _SunGlintColor;
                float _WaveAmplitudeA, _WaveAmplitudeB, _WaveScaleA, _WaveScaleB;
                float _WaveSpeedA, _WaveSpeedB, _FresnelPower;
                float _SunGlintPower, _SunGlintStrength;
                float _RippleStrength, _RippleScale, _SkyReflection, _WhitecapStrength;
            CBUFFER_END

            struct Attributes { float4 positionOS : POSITION; };
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                half fogFactor : TEXCOORD1;
            };

            float Hash(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }
            float Noise(float2 p)
            {
                float2 cell = floor(p);
                float2 f = frac(p);
                f = f * f * f * (f * (f * 6.0 - 15.0) + 10.0);
                return lerp(lerp(Hash(cell), Hash(cell + float2(1,0)), f.x),
                            lerp(Hash(cell + float2(0,1)), Hash(cell + 1), f.x), f.y);
            }
            // Continuous derivatives keep reflection normals smooth at cell boundaries.
            float3 NoiseGradient(float2 p)
            {
                float2 cell = floor(p);
                float2 f = frac(p);
                float2 u = f * f * f * (f * (f * 6.0 - 15.0) + 10.0);
                float2 du = 30.0 * f * f * (f - 1.0) * (f - 1.0);
                float a = Hash(cell);
                float b = Hash(cell + float2(1, 0));
                float c = Hash(cell + float2(0, 1));
                float d = Hash(cell + 1);
                float k = a - b - c + d;
                return float3(a + (b-a)*u.x + (c-a)*u.y + k*u.x*u.y,
                    du.x * (b-a + k*u.y), du.y * (c-a + k*u.x));
            }
            float2 RippleGradient(float2 p, float scale, float2 drift)
            {
                float2 q = float2(p.x * 0.8 + p.y * 0.6, -p.x * 0.6 + p.y * 0.8);
                q = q * scale + drift;
                float filter = 1.0 - smoothstep(0.35, 1.4, max(fwidth(q.x), fwidth(q.y)));
                float2 g = NoiseGradient(q).yz;
                return float2(g.x * 0.8 - g.y * 0.6, g.x * 0.6 + g.y * 0.8) * filter;
            }
            // Long swell remains directional; fine reflection normals are aperiodic.
            void Wave(float2 p, float2 direction, float frequency, float speed,
                      float amplitude, float phaseOffset, inout float height, inout float2 slope)
            {
                direction = normalize(direction);
                float phase = dot(p, direction) * frequency - _Time.y * speed + phaseOffset;
                float filter = 1.0 - smoothstep(0.6, 2.8, fwidth(phase));
                height += sin(phase) * amplitude * filter;
                slope += direction * cos(phase) * amplitude * filter;
            }
            Varyings OceanVertex(Attributes input)
            {
                Varyings output;
                // Deliberately flat geometry: matches wake surface, waterline and colliders.
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.positionCS = TransformWorldToHClip(output.positionWS);
                output.fogFactor = ComputeFogFactor(output.positionCS.z);
                return output;
            }
            half4 OceanFragment(Varyings input) : SV_Target
            {
                float2 p = input.positionWS.xz;
                float drift = Noise(p * 0.15 + float2(_Time.y * 0.015, 0));
                float driftB = Noise(p * 0.19 + float2(17.3, -_Time.y * 0.018));
                float2 warped = p + (float2(drift, driftB) - 0.5) * 2.6;
                float height = 0;
                float2 slope = 0;
                Wave(warped, float2(1,0.42), _WaveScaleA * 3.0, _WaveSpeedA, _WaveAmplitudeA, 0, height, slope);
                Wave(warped, float2(-0.65,1), _WaveScaleB * 2.4, _WaveSpeedB, _WaveAmplitudeB, 1.7, height, slope);
                Wave(warped, float2(0.85,0.68), _WaveScaleA * 5.8, _WaveSpeedA * 1.32, _WaveAmplitudeA * 0.40, 3.2, height, slope);
                Wave(warped, float2(-0.3,1), _WaveScaleB * 4.1, _WaveSpeedB * 0.85, _WaveAmplitudeB * 0.5, 0.9, height, slope);
                float swellHeight = height;
                // Independent rotations break intersecting periodic glint stripes.
                float2 rippleSlope = RippleGradient(p, _RippleScale * 0.72,
                    float2(-_Time.y * _WaveSpeedA * 0.13, _Time.y * 0.045)) * 0.70;
                float2 rotated = float2(p.x * 0.36 - p.y * 0.93295, p.x * 0.93295 + p.y * 0.36);
                float2 crossSlope = RippleGradient(rotated, _RippleScale * 1.57,
                    float2(9.7 + _Time.y * 0.07, -_Time.y * _WaveSpeedB * 0.11));
                rippleSlope += float2(crossSlope.x * 0.36 + crossSlope.y * 0.93295,
                    -crossSlope.x * 0.93295 + crossSlope.y * 0.36) * 0.32;
                rippleSlope += RippleGradient(p, _RippleScale * 3.13,
                    float2(32.4 - _Time.y * 0.08, 15.2 + _Time.y * 0.12)) * 0.14;
                slope += rippleSlope * _RippleStrength * lerp(0.85, 1.15, drift);

                half3 normalWS = normalize(half3(-slope.x, 1, -slope.y));
                half3 viewWS = GetWorldSpaceNormalizeViewDir(input.positionWS);
                half facing = saturate(dot(normalWS, viewWS));
                half fresnel = 0.02h + 0.98h * pow(1.0h - facing, _FresnelPower);

                // Restrained large-scale tint; ripples primarily change reflected light.
                float patch = Noise(p * 0.045);
                half bodyBlend = saturate(0.36 + (patch - 0.5) * 0.2 + swellHeight * 0.23);
                half3 water = lerp(_DeepColor.rgb, _ShallowColor.rgb, bodyBlend);
                half3 reflectedView = reflect(-viewWS, normalWS);
                half skyElevation = saturate(reflectedView.y * 0.5h + 0.5h);
                half3 sky = lerp(_HorizonColor.rgb * 0.65h, _HorizonColor.rgb * 1.25h, skyElevation);
                float skyCloud = Noise(reflectedView.xz * 3.0 + p * 0.012);
                sky *= lerp(0.88h, 1.12h, skyCloud);
                water = lerp(water, sky, saturate(_SkyReflection * (0.25h + fresnel)));

                float4 shadowCoord;
                #if defined(_MAIN_LIGHT_SHADOWS_SCREEN)
                    shadowCoord = ComputeScreenPos(TransformWorldToHClip(input.positionWS));
                #else
                    shadowCoord = TransformWorldToShadowCoord(input.positionWS);
                #endif
                Light sun = GetMainLight(shadowCoord);
                half ndl = saturate(dot(normalWS, sun.direction));
                half shade = lerp(0.68h, 1.0h, sun.shadowAttenuation);
                water *= (0.82h + ndl * 0.18h) * shade;

                half3 halfDirection = SafeNormalize(sun.direction + viewWS);
                float ndh = saturate(dot(normalWS, halfDirection));
                float variance = dot(ddx(normalWS), ddx(normalWS)) + dot(ddy(normalWS), ddy(normalWS));
                float power = lerp(_SunGlintPower, 16.0, saturate(variance * 7));
                float energy = sqrt(power / max(8.0, _SunGlintPower));
                float glint = pow(ndh, power) * 0.58 * energy + pow(ndh, 12.0) * 0.07;
                glint *= lerp(0.70, 1.0, driftB);
                // Avoid multiplying two strongly orange tints.
                half3 glintTint = lerp(half3(1, 1, 1), _SunGlintColor.rgb, 0.55h);
                water += glintTint * sun.color * glint *
                    _SunGlintStrength * sun.shadowAttenuation * ndl;

                float crests = smoothstep(0.72, 0.98,
                    swellHeight / max(0.02, _WaveAmplitudeA + _WaveAmplitudeB) * 0.5 + 0.5);
                float2 foamFlow = warped + float2(-_Time.y * 0.08, _Time.y * 0.025);
                float patches = smoothstep(0.52, 0.78, Noise(foamFlow * 0.31 + 23.7));
                float foamDetail = Noise(foamFlow * 2.7 + driftB);
                float aa = max(0.025, fwidth(foamDetail));
                float flecks = smoothstep(0.74 - aa, 0.9 + aa, foamDetail);
                float foam = crests * patches * flecks * _WhitecapStrength;
                water = lerp(water, half3(0.56, 0.75, 0.76) * shade, foam);
                return half4(MixFog(water, input.fogFactor), 1);
            }
            ENDHLSL
        }
    }
    FallBack Off
}
