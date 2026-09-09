Shader "Seaborn/Ocean Prototype"
{
    Properties
    {
        _DeepColor ("Deep Color", Color) = (0.025, 0.16, 0.2, 1)
        _ShallowColor ("Shallow Color", Color) = (0.06, 0.31, 0.35, 1)
        _HorizonColor ("Horizon Color", Color) = (0.18, 0.38, 0.42, 1)
        _SunGlintColor ("Sun Glint Color", Color) = (1, 0.72, 0.42, 1)
        _WaveAmplitudeA ("Large Wave Normal", Range(0, 1)) = 0.18
        _WaveAmplitudeB ("Small Wave Normal", Range(0, 0.5)) = 0.06
        _WaveScaleA ("Large Wave Scale", Range(0.01, 2)) = 0.22
        _WaveScaleB ("Small Wave Scale", Range(0.01, 3)) = 0.46
        _WaveSpeedA ("Large Wave Speed", Range(-3, 3)) = 0.7
        _WaveSpeedB ("Small Wave Speed", Range(-3, 3)) = 1.15
        _FresnelPower ("Fresnel Power", Range(0.5, 8)) = 3.6
        _SunGlintPower ("Sun Glint Sharpness", Range(8, 256)) = 128
        _SunGlintStrength ("Sun Glint Strength", Range(0, 2)) = 0.28
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
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                half fogFactor : TEXCOORD1;
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

                output.positionWS =
                    TransformObjectToWorld(input.positionOS.xyz);
                output.positionCS =
                    TransformWorldToHClip(output.positionWS);
                output.fogFactor =
                    ComputeFogFactor(output.positionCS.z);

                return output;
            }

            half4 OceanFragment(Varyings input) : SV_Target
            {
                float waveHeight;
                float2 waveSlope;

                EvaluateWaves(
                    input.positionWS.xz,
                    waveHeight,
                    waveSlope
                );

                half3 normalWS = normalize(
                    half3(
                        -waveSlope.x,
                        1.0h,
                        -waveSlope.y
                    )
                );

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

                half waveTint = saturate(
                    waveHeight * 1.8h + 0.5h
                );

                half3 waterColor = lerp(
                    _DeepColor.rgb,
                    _ShallowColor.rgb,
                    waveTint
                );

                waterColor = lerp(
                    waterColor,
                    _HorizonColor.rgb,
                    fresnel * 0.58h
                );

                Light mainLight = GetMainLight();

                half diffuseLight =
                    saturate(
                        dot(normalWS, mainLight.direction)
                    );

                half3 incomingLight =
                    -mainLight.direction;
                half3 reflectedLight = reflect(
                    incomingLight,
                    normalWS
                );

                half sunGlint = pow(
                    saturate(
                        dot(
                            reflectedLight,
                            viewDirectionWS
                        )
                    ),
                    _SunGlintPower
                ) * _SunGlintStrength;

                half lightingFactor =
                    0.82h + diffuseLight * 0.18h;

                half3 color =
                    waterColor * lightingFactor;

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
