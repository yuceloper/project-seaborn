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
            #pragma target 3.0
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
                float age = saturate(input.color.r);
                float2 p = float2(input.uv.x * 5.0, input.uv.y * 1.9);
                float broad = Noise(p + float2(_Time.y * 0.035, 0));
                float fine = Noise(p * 3.7 + broad * 1.6);
                float detail = Noise(p * 9.0);
                float across = abs(input.uv.x * 2.0 - 1.0);
                float edge = 1.0 - smoothstep(0.60, 1.0, across + (broad - 0.5) * 0.20);
                float shoulderCenter = lerp(0.48, 0.62, age) + (broad - 0.5) * 0.08;
                float shoulderDistance = (across - shoulderCenter) * lerp(5.5, 4.2, age);
                float shoulders = exp(-shoulderDistance * shoulderDistance);
                float center = 1.0 - smoothstep(0.05, 0.55, across);
                float density = smoothstep(0.30, 0.72, broad * 0.45 + fine * 0.55);
                float detailFilter = 1.0 - smoothstep(0.5, 1.5,
                    max(fwidth(p.x * 9.0), fwidth(p.y * 9.0)));
                float bubbles = lerp(0.5, smoothstep(0.35, 0.65, detail), detailFilter);
                float tailBreakup = lerp(1.0, smoothstep(0.25, 0.65, broad),
                    smoothstep(0.2, 1.0, age) * 0.65);
                float foam = density * (0.30 + shoulders * 0.55 + center * lerp(0.16, 0.08, age));
                foam *= lerp(0.5, 1.0, bubbles) * tailBreakup;
                half alpha = saturate(foam * edge * input.color.a * _FoamColor.a);
                return half4(_FoamColor.rgb * lerp(0.84, 1.08, fine), alpha);
            }
            ENDHLSL
        }
    }
}
