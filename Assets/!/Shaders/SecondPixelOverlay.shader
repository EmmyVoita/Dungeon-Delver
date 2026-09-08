Shader "Custom/PausePixelOverlayUI"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}

        _TintColor ("Tint Color", Color) = (0.45, 0.2, 0.65, 1)
        _PatternColor ("Pattern Color", Color) = (0.15, 0.05, 0.25, 1)

        _Transition ("Transition", Range(0,1)) = 0
        _GridSize ("Grid Size", Float) = 12
        _PatternStrength ("Pattern Strength", Range(0,1)) = 0.45
        _BackgroundVisibility ("Background Visibility", Range(0,1)) = 0
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Transparent"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            float4 _TintColor;
            float4 _PatternColor;

            float _Transition;
            float _GridSize;
            float _PatternStrength;
            float _BackgroundVisibility;

            Varyings vert(Attributes input)
            {
                Varyings output;

                output.positionHCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                output.color = input.color;

                return output;
            }

            float4 frag(Varyings input) : SV_Target
            {
                float2 uv = input.uv;

                float2 centerUV = (uv - 0.5) * 2.0;

                // -----------------------------
                // Cell rotation
                // -----------------------------

                float angle = (1.0 - _Transition) * 1.5707963;

                float2 local = frac(centerUV * _GridSize) - 0.5;

                float s = sin(angle);
                float c = cos(angle);

                float2 rotated;

                rotated.x = local.x * c - local.y * s;
                rotated.y = local.x * s + local.y * c;

                float2 rotatedDistance = abs(rotated);

                float rotatedSquareRadius = max(
                    rotatedDistance.x,
                    rotatedDistance.y
                );

                // -----------------------------
                // Alternating blinds
                // -----------------------------

                float row = floor(uv.y * _GridSize);
                float blinds = fmod(row, 2.0);

                float rowTransition = saturate(
                    lerp(
                        _Transition * 2.0 - (1.0 - uv.x),
                        _Transition * 2.0 - uv.x,
                        blinds
                    )
                );

                // Square grows from center.
                float square = step(
                    rotatedSquareRadius,
                    rowTransition * 0.5
                );

                // -----------------------------
                // Checker pattern
                // -----------------------------

                float2 gridUV = floor((centerUV + 1.0) * _GridSize);
                float checker = fmod(gridUV.x + gridUV.y, 2.0);

                // -----------------------------
                // Final UI overlay
                // -----------------------------

                float patternAlpha =
                    checker *
                    square *
                    _PatternStrength;

                float backgroundAlpha =
                    _BackgroundVisibility *
                    _Transition;

                float3 finalColor = lerp(
                    _TintColor.rgb,
                    _PatternColor.rgb,
                    patternAlpha
                );

                float finalAlpha = max(
                    backgroundAlpha,
                    patternAlpha
                );

                return float4(
                    finalColor,
                    finalAlpha * input.color.a
                );
            }

            ENDHLSL
        }
    }
}