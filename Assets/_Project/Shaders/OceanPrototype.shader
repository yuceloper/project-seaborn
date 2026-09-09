Shader "Seaborn/Ocean Prototype"
{
    Properties
    {
        _DeepColor ("Deep Color", Color) = (0.018, 0.09, 0.13, 1)
        _ShallowColor ("Shallow Color", Color) = (0.035, 0.24, 0.28, 1)
        _HorizonColor ("Horizon Color", Color) = (0.2, 0.42, 0.46, 1)
        _SunGlintColor ("Sun Glint Color", Color) = (1, 0.58, 0.26, 1)
        _WaveAmplitudeA ("Large Wave Height", Range(0, 1)) = 0.22
        _WaveAmplitudeB ("Small Wave Height", Range(0, 0.5)) = 0.08
        _WaveScaleA ("Large Wave Scale", Range(0.01, 2)) = 0.22
        _WaveScaleB ("Small Wave Scale", Range(0.01, 3)) = 0.46
        _WaveSpeedA ("Large Wave Speed", Range(-3, 3)) = 0.7
        _WaveSpeedB ("Small Wave Speed", Range(-3, 3)) = 1.15
        _FresnelPower ("Fresnel Power", Range(0.5, 8)) = 3.2
        _SunGlintPower ("Sun Glint Sharpness", Range(8, 256)) = 96
        _SunGlintStrength ("Sun Glint Strength", Range(0, 4)) = 1.35
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "Queue" = "Geometry"
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            Name "OceanForward"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM

            #pragma target 3.0
            #pragma vertex OceanVertex
            #pragma fragment OceanFragment
            #pragma multi_compile_fog
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _DeepColor;
                half4 _ShallowColor;
                half4 _HorizonColor;
                half4 _SunGlintColor;
                float _WaveAmplitudeA;
                float _WaveAmplitudeB;
                float _WaveScaleA;
                float _WaveScaleB;
                float _WaveSpeedA;
                float _WaveSpeedB;
                float _FresnelPower;
                float _SunGlintPower;
                float _SunGlintStrength;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                half3 normalWS : TEXCOORD1;
                half fogFactor : TEXCOORD2;
            };

            void EvaluateWaves(
                float2 positionXZ,
                out float height,
                out float2 slope)
            {
                float phaseA =
                    positionXZ.x * _WaveScaleA +
                    positionXZ.y * (_WaveScaleA * 0.62) +
                    _Time.y * _WaveSpeedA;

                float phaseB =
                    positionXZ.x * (-_WaveScaleB * 0.73) +
                    positionXZ.y * _WaveScaleB +
                    _Time.y * _WaveSpeedB;

                height =
                    sin(phaseA) * _WaveAmplitudeA +
                    sin(phaseB) * _WaveAmplitudeB;

                slope.x =
                    cos(phaseA) *
                    _WaveAmplitudeA *
                    _WaveScaleA +
                    cos(phaseB) *
                    _WaveAmplitudeB *
                    (-_WaveScaleB * 0.73);

                slope.y =
                    cos(phaseA) *
                    _WaveAmplitudeA *
                    (_WaveScaleA * 0.62) +
                    cos(phaseB) *
                    _WaveAmplitudeB *
                    _WaveScaleB;
            }

            Varyings OceanVertex(Attributes input)
            {
                Varyings output;

                float3 positionWS =
                    TransformObjectToWorld(input.positionOS.xyz);

                float waveHeight;
                float2 waveSlope;

                EvaluateWaves(
                    positionWS.xz,
                    waveHeight,
                    waveSlope
                );

                positionWS.y += waveHeight;

                output.positionWS = positionWS;
                output.normalWS = normalize(
                    half3(
                        -waveSlope.x,
                        1.0,
                        -waveSlope.y
                    )
                );
                output.positionCS =
                    TransformWorldToHClip(positionWS);
                output.fogFactor =
                    ComputeFogFactor(output.positionCS.z);

                return output;
            }

            half4 OceanFragment(Varyings input) : SV_Target
            {
                half3 normalWS = normalize(input.normalWS);
                half3 viewDirectionWS =
                    GetWorldSpaceNormalizeViewDir(
                        input.positionWS
                    );

                half fresnel = pow(
                    1.0h -
                    saturate(
                        dot(normalWS, viewDirectionWS)
                    ),
                    _FresnelPower
                );

                half heightTint = saturate(
                    input.positionWS.y * 0.55h + 0.45h
                );

                half3 waterColor = lerp(
                    _DeepColor.rgb,
                    _ShallowColor.rgb,
                    heightTint
                );

                waterColor = lerp(
                    waterColor,
                    _HorizonColor.rgb,
                    fresnel * 0.72h
                );

                Light mainLight = GetMainLight();

                half3 halfDirection = normalize(
                    viewDirectionWS +
                    mainLight.direction
                );

                half sunGlint = pow(
                    saturate(
                        dot(normalWS, halfDirection)
                    ),
                    _SunGlintPower
                ) * _SunGlintStrength;

                half diffuseLight =
                    saturate(
                        dot(normalWS, mainLight.direction)
                    ) * 0.2h + 0.8h;

                half3 color =
                    waterColor *
                    diffuseLight *
                    mainLight.color;

                color +=
                    _SunGlintColor.rgb *
                    sunGlint *
                    mainLight.color;

                color = MixFog(
                    color,
                    input.fogFactor
                );

                return half4(color, 1.0h);
            }

            ENDHLSL
        }
    }

    FallBack Off
}
