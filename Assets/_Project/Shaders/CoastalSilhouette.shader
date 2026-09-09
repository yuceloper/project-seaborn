Shader "Seaborn/Coastal Silhouette"
{
    Properties
    {
        _BaseColor ("Base Color", Color) =
            (0.035, 0.075, 0.08, 1)
        _TopColor ("Top Color", Color) =
            (0.11, 0.14, 0.13, 1)
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
            Name "CoastalSilhouette"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM

            #pragma target 3.0
            #pragma vertex CoastVertex
            #pragma fragment CoastFragment
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                half4 _TopColor;
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

            Varyings CoastVertex(Attributes input)
            {
                Varyings output;
                output.positionWS =
                    TransformObjectToWorld(input.positionOS.xyz);
                output.positionCS =
                    TransformWorldToHClip(output.positionWS);
                output.normalWS =
                    TransformObjectToWorldNormal(input.normalOS);
                output.fogFactor =
                    ComputeFogFactor(output.positionCS.z);
                return output;
            }

            half4 CoastFragment(Varyings input) : SV_Target
            {
                Light mainLight = GetMainLight();
                half lightAmount = 0.48h +
                    saturate(
                        dot(
                            normalize(input.normalWS),
                            mainLight.direction
                        )
                    ) * 0.34h;

                half heightTint = saturate(
                    (input.positionWS.y - 0.7h) * 0.22h
                );

                half3 color = lerp(
                    _BaseColor.rgb,
                    _TopColor.rgb,
                    heightTint
                ) * lightAmount;

                color = MixFog(color, input.fogFactor);
                return half4(color, 1.0h);
            }

            ENDHLSL
        }
    }

    FallBack Off
}
