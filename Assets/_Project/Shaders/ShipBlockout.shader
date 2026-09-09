Shader "Seaborn/Ship Blockout"
{
    Properties
    {
        _BaseColor ("Base Color", Color) =
            (0.35, 0.2, 0.11, 1)
        _TopLight ("Top Light", Range(0, 1)) = 0.28
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
            Name "ShipBlockout"
            Tags { "LightMode" = "UniversalForward" }

            Cull Off
            ZWrite On

            HLSLPROGRAM

            #pragma target 3.0
            #pragma vertex ShipVertex
            #pragma fragment ShipFragment
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                float _TopLight;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                half3 normalWS : TEXCOORD0;
                half fogFactor : TEXCOORD1;
            };

            Varyings ShipVertex(Attributes input)
            {
                Varyings output;
                output.positionCS =
                    TransformObjectToHClip(input.positionOS.xyz);
                output.normalWS =
                    TransformObjectToWorldNormal(input.normalOS);
                output.fogFactor =
                    ComputeFogFactor(output.positionCS.z);
                return output;
            }

            half4 ShipFragment(Varyings input) : SV_Target
            {
                Light mainLight = GetMainLight();
                half3 normalWS = normalize(input.normalWS);
                half directLight = saturate(
                    dot(normalWS, mainLight.direction)
                );
                half topLight = saturate(normalWS.y) * _TopLight;
                half lighting = 0.58h +
                    directLight * 0.24h +
                    topLight;

                half3 color =
                    _BaseColor.rgb *
                    lighting *
                    lerp(
                        half3(1, 1, 1),
                        mainLight.color,
                        0.18h
                    );

                color = MixFog(color, input.fogFactor);
                return half4(color, 1);
            }

            ENDHLSL
        }
    }

    FallBack Off
}
