Shader "Seaborn/Surface Wake"
{
    Properties
    {
        _FoamColor ("Foam", Color) = (0.68, 0.84, 0.81, 0.72)
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent+20" "RenderType"="Transparent" }
        Pass
        {
            Tags { "LightMode"="SRPDefaultUnlit" }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            ZTest LEqual
            Cull Off
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial)
                half4 _FoamColor;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                half4 color : COLOR;
            };
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                half4 color : COLOR;
            };
            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                output.color = input.color;
                return output;
            }
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
                f = f * f * (3.0 - 2.0 * f);
                return lerp(lerp(Hash(cell), Hash(cell + float2(1,0)), f.x),
                            lerp(Hash(cell + float2(0,1)), Hash(cell + 1), f.x), f.y);
            }
            half4 Frag(Varyings input) : SV_Target
            {
                float2 p = float2(input.uv.x * 5.0, input.uv.y * 1.9);
                // Slow local breakup, not scrolling stripes or camera-facing puffs.
                float broad = Noise(p + float2(_Time.y * 0.035, 0));
                float fine = Noise(p * 3.7 + broad * 1.6);
                float detail = Noise(p * 9.0);
                float across = abs(input.uv.x * 2.0 - 1.0);
                float edge = 1.0 - smoothstep(0.68, 1.0, across + (broad - 0.5) * 0.17);
                float shoulders = exp(-pow((across - 0.52) * 5.5, 2.0));
                float center = 1.0 - smoothstep(0.05, 0.55, across);
                float density = smoothstep(0.30, 0.72, broad * 0.45 + fine * 0.55);
                float bubbles = smoothstep(0.35, 0.65, detail);
                float foam = density * (0.30 + shoulders * 0.55 + center * 0.16);
                foam *= lerp(0.5, 1.0, bubbles);
                half alpha = saturate(foam * edge * input.color.a * _FoamColor.a);
                return half4(_FoamColor.rgb * lerp(0.84, 1.08, fine), alpha);
            }
            ENDHLSL
        }
    }
}
