Shader "StarWipe"
{
    Properties
    {
        _TransitionValue ("TransitionValue", Range(0,1)) = 0.0
        _StarTex ("Star Texture", 2D) = "white" {} // 👈 NEW
        _Repeat ("Repeat Count", Float) = 4.0
        _EdgeSoftness ("Edge Softness", Range(0.001,0.1)) = 0.02
        _RotationSpeed ("Rotation Speed", Float) = 1
        _SheenTex ("Sheen Texture", 2D) = "white" {} 
        _SheenColor ("Sheen Color", Color) = (1,1,1,1)
        _BackgroundColor ("Background Color", Color) = (1,1,1,1)
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        ZWrite Off Cull Off

        Pass
        {
            Name "StarWipe"

            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            #pragma vertex Vert
            #pragma fragment Frag

            float _TransitionValue;
            float _Repeat;
            float _EdgeSoftness;
            float _RotationSpeed;

            float4 _SheenColor;
            float4 _BackgroundColor;

            TEXTURE2D(_SheenTex);        // 👈 NEW
            SAMPLER(sampler_SheenTex);

            TEXTURE2D(_StarTex);        // 👈 NEW
            SAMPLER(sampler_StarTex);

            float2 rotateUV(float2 uv, float2 mid, float theta) {
                return float2(
                    cos(theta) * (uv.x - mid.x) - sin(theta) * (uv.y - mid.y) + mid.x,
                    sin(theta) * (uv.x - mid.x) + cos(theta) * (uv.y - mid.y) + mid.y
                );
            }


            float2 CalculateUV(float2 uv, float t)
            {
                 // growth curve (nice smooth expansion)
                float growth = smoothstep(0, 1, t);

                if(growth < 0.1) return float2(0,0);

                float scale = lerp(0.35, 25.0, 1-growth); // controlled expansion
                float2 uv_ = (uv - 0.5) * scale + 0.5;

                // rotation ramps up over time
                float rotRamp = pow(t, 2.5); // tweak this!
                float angle = rotRamp * _RotationSpeed;

                return rotateUV(uv_, float2(0.5,0.5), angle);
            }

            float4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float2 uv = input.texcoord.xy;

                
                float2 uv1 = CalculateUV(uv,_TransitionValue);
                float2 uv2 = CalculateUV(uv,lerp(saturate(_TransitionValue - 0.5),_TransitionValue,_TransitionValue));
                float2 uv3 = CalculateUV(uv,lerp(saturate(_TransitionValue - 1),_TransitionValue * 0.97,_TransitionValue));
                
                
                float mask1 = 1-SAMPLE_TEXTURE2D(_StarTex, sampler_StarTex, uv1).r;
                //float mask1 = SAMPLE_TEXTURE2D(_StarTex, sampler_StarTex, uv1).r;
                float mask2 = SAMPLE_TEXTURE2D(_StarTex, sampler_StarTex, uv2).r;
                float mask3 = SAMPLE_TEXTURE2D(_StarTex, sampler_StarTex, uv3).r;


                float3 col1 = mask1 * _BackgroundColor;// * float3(1,0,0);
                //float3 col1 = mask1 * float3(1,0,0);
                float3 col2 = mask2 * float3(0,1,0);
                float3 col3 = mask3 * float3(0,0,1);

                float4 finalStarCol = float4(col1,1);//float4(lerp(lerp(col1,col2,mask2),col3,mask3),1);



                // sample scene
                half4 color = SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearRepeat, uv, _BlitMipLevel);

                float sheenMask1 = SAMPLE_TEXTURE2D(_SheenTex, sampler_SheenTex, uv).r;

                //float combinedMask = max(mask1, max(mask2, mask3));
                float combinedMask = mask1;
                float4 finalCol = lerp(finalStarCol, color, 1 - combinedMask);
                finalCol += (sheenMask1 * combinedMask) * _SheenColor;

                return finalCol;
            }

            ENDHLSL
        }
    }
}