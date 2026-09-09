Shader "Seaborn/Wake Trail"
{
    Properties
    {
        _BaseColor ("Base Color", Color) =
            (0.68, 0.9, 0.92, 0.72)
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            Name "WakeTrail"
            Tags { "LightMode" = "UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            HLSLPROGRAM

            #pragma target 3.0
            #pragma vertex WakeVertex
            #pragma fragment WakeFragment
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                half4 color : COLOR;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                half4 color : COLOR;
                half fogFactor : TEXCOORD0;
            };

            Varyings WakeVertex(Attributes input)
            {
                Varyings output;
                output.positionCS =
                    TransformObjectToHClip(input.positionOS.xyz);
                output.color = input.color * _BaseColor;
                output.fogFactor =
                    ComputeFogFactor(output.positionCS.z);
                return output;
            }

            half4 WakeFragment(Varyings input) : SV_Target
            {
                half3 color = MixFog(
                    input.color.rgb,
                    input.fogFactor
                );
                return half4(color, input.color.a);
            }

            ENDHLSL
        }
    }

    FallBack Off
}
