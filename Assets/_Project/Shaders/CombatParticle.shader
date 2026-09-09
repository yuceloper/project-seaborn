Shader "Seaborn/Combat Particle"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (1, 1, 1, 1)
        _Softness ("Edge Softness", Range(0.01, 0.45)) = 0.18
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
            Name "CombatParticle"
            Tags { "LightMode" = "UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            HLSLPROGRAM

            #pragma target 3.0
            #pragma vertex ParticleVertex
            #pragma fragment ParticleFragment
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                float _Softness;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                half4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                half4 color : COLOR;
                float2 uv : TEXCOORD0;
                half fogFactor : TEXCOORD1;
            };

            Varyings ParticleVertex(Attributes input)
            {
                Varyings output;
                output.positionCS =
                    TransformObjectToHClip(input.positionOS.xyz);
                output.color = input.color * _BaseColor;
                output.uv = input.uv;
                output.fogFactor =
                    ComputeFogFactor(output.positionCS.z);
                return output;
            }

            half4 ParticleFragment(Varyings input) : SV_Target
            {
                float distanceFromCenter =
                    length(input.uv - float2(0.5, 0.5));

                half radialAlpha = 1.0h - smoothstep(
                    0.5h - _Softness,
                    0.5h,
                    distanceFromCenter
                );

                half alpha = input.color.a * radialAlpha;
                clip(alpha - 0.002h);

                half3 color = MixFog(
                    input.color.rgb,
                    input.fogFactor
                );

                return half4(color, alpha);
            }

            ENDHLSL
        }
    }

    FallBack Off
}
