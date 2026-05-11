Shader "OpenShutterTransition"
{
    Properties
    {
        _BladeCount ("BladeCount", Range(0,10)) = 6
        _CurveStrength ("CurveStrength", Range(0,10)) = 2
        _TransitionValue ("TransitionValue", Range(0,1)) = 0.0
        _SheenTex ("Sheen Texture", 2D) = "white" {} 
        _SheenColor ("Sheen Color", Color) = (1,1,1,1)
        _BackgroundColor ("Background Color", Color) = (1,1,1,1)
    }
   SubShader
   {
       Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline"}
       ZWrite Off Cull Off
       Pass
       {
           Name "OpenShutterTransition"

           HLSLPROGRAM
           #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
           #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

           #pragma vertex Vert
           #pragma fragment Frag

           float _TransitionValue;
           float _BladeCount;
           float _CurveStrength;
            float4 _SheenColor;
            float4 _BackgroundColor;

            TEXTURE2D(_SheenTex);        // 👈 NEW
            SAMPLER(sampler_SheenTex);



              
            float RemapFloat(float value, float inMin, float inMax, float outMin, float outMax)
            {
                return outMin + (value - inMin) * (outMax - outMin) / (inMax - inMin);
            }

            float PolarEffect(float2 uv)
            {
                // 0 is closed, 1 is open
                float t = 1.0 - _TransitionValue; // 0 → 1

        
                // Move uv origin to center
                float2 p = uv - 0.5;
                
                
                // raidus is calculated as the distance of the position from the center
                float radius = length(p);

                float radiusMask = radius < _TransitionValue ? 1 : 0;

                
                float angleRad = atan2(p.y, p.x);

                // apply rotation here
                float rotation = lerp(0.0, 3.0, t);
                angleRad += rotation;

                // now derive angle01 FROM rotated angle
                float angle01 = (angleRad + PI) / (2 * PI);
                

                float bladeIndex = floor(angle01 * _BladeCount)+ 1.0;
                float bladeCenter = (bladeIndex + 0.5) / _BladeCount * 2 * PI;
                

                float localAngle = angleRad - bladeCenter;
                float rotatedAngle = localAngle - rotation;
                float curvedAngle = rotatedAngle + pow(radius, 1.5) * _CurveStrength;


                float circleRadius = _TransitionValue * 0.5;
                float circleMask = 1.0 - smoothstep(circleRadius, circleRadius + 0.02, radius+0.025);

            

                float bladeShape = smoothstep(0.8, 0.0, abs(curvedAngle * -0.1));
                float bladeMask = smoothstep(radius, radius + 0.02, bladeShape);
                float bladeFade = smoothstep(0.3, 0.6, _TransitionValue);

                float sheenMask1 = SAMPLE_TEXTURE2D(_SheenTex, sampler_SheenTex, uv).r;

                float finalMask = lerp(circleMask, saturate(circleMask+ bladeMask), bladeFade);
       
                return saturate(finalMask);
            }

            
           // Out frag function takes as input a struct that contains the screen space coordinate we are going to use to sample our texture. It also writes to SV_Target0, this has to match the index set in the UseTextureFragment(sourceTexture, 0, …) we defined in our render pass script.   
           float4 Frag(Varyings input) : SV_Target0
           {
               // this is needed so we account XR platform differences in how they handle texture arrays
               UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                // sample the texture using the SAMPLE_TEXTURE2D_X_LOD
                float2 uv = input.texcoord.xy;
                half4 color = SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearRepeat, uv, _BlitMipLevel);

                float mask = PolarEffect(uv);

                
                
                float sheenMask1 = SAMPLE_TEXTURE2D(_SheenTex, sampler_SheenTex, uv).r;

                color = lerp(_BackgroundColor, color * mask, mask);
                color += (sheenMask1 * (1-mask)) * _SheenColor;

                return color;//* mask;
           }


           ENDHLSL
       }
   }
}