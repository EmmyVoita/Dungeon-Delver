Shader "Custom/PausePixelOverlay"
{
    Properties
    {
        _MainTex ("Screen Texture", 2D) = "white" {}

        _TintColor ("Tint Color", Color) = (0.45, 0.2, 0.65, 1)
        _PatternColor ("Pattern Color", Color) = (0.15, 0.05, 0.25, 1)

        _Transition ("Transition", Range(0,1)) = 0
        _GridSize ("Grid Size", Float) = 12
        _PatternStrength ("Pattern Strength", Range(0,1)) = 0.45
        _BackgroundVisibility ("BackgroundVisibility", Range(0,1)) = 0
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "Queue"="Transparent"
        }

        Pass
        {
            ZTest Always
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex Vert
           #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"



            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            float4 _TintColor;
            float4 _PatternColor;

            float _Transition;
            float _GridSize;
            float _PatternStrength;
            float _BackgroundVisibility;

            float RemapFloat(float value, float inMin, float inMax, float outMin, float outMax)
            {
                return outMin + (value - inMin) * (outMax - outMin) / (inMax - inMin);
            }
        

            float4 Frag(Varyings input) : SV_Target0
            {
                // this is needed so we account XR platform differences in how they handle texture arrays
               UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);


                float2 uv = input.texcoord.xy;

                
                // Just standard center uv formula. Converts to -1 to 1 range.
                float2 centerUV = (uv - 0.5) * 2;


                // Multiply center uv by grid size to get the gric cells, 
                // we use the centeredUv so that the cells expand fromt the center.
                float2 cellUV = frac(centerUV * _GridSize);

                // To get the cell distance from the center for each we take transform it to
                // -0.5 to 0.5 range and then take the absolute value
                float2 cellCenterRadialDistance =  abs( frac(centerUV * _GridSize) - 0.5);
              
                //Sample the blit (screen) texture
                float4 screenColor = SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearClamp, uv, _BlitMipLevel);

                float angle = (1.0 - _Transition) * 1.5707963; // 90 degrees  
                
                
                float2 local = frac(centerUV * _GridSize) - 0.5;

                float s = sin(angle);
                float c = cos(angle);


                float2 rotated;
                
                rotated.x = local.x * c - local.y * s;
                rotated.y = local.x * s + local.y * c;

                float2 rotatedDistance = abs(rotated);

                // This function produces a square
                float rotatedSquareRadius = max(rotatedDistance.x, rotatedDistance.y);
                
                float rowTransition = saturate(_Transition * 2.0 - RemapFloat(uv.y + uv.x, 0, 2, 0, 1));
                
                // increase the size of the square over the transition duration
                float square = step(rotatedSquareRadius, rowTransition * 0.5);



                
                  
                //return screenColor;


                
                
                float2 gridUV = floor((centerUV + 1) * _GridSize);
                float checker = fmod(gridUV.x + gridUV.y, 2.0);

                float patternAmount = checker * square * _PatternStrength;


                //float4 color = lerp(screenColor, _PatternColor, patternAmount );

                //float patternAmount = checker * _PatternStrength * _Transition;

                /*
                

                

                
                */

                half3 tinted = lerp(
                    screenColor.rgb,
                    lerp(screenColor.rgb,  _TintColor.rgb, _BackgroundVisibility),
                    _Transition
                );



                tinted = lerp(
                    tinted,
                    _PatternColor.rgb,
                    patternAmount
                );

                //return color;

                //return half4(centerUV,0,1);

                return half4(tinted, 1);
            }

            ENDHLSL
        }
    }
}