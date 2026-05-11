Shader "ScanLinesWipe"
{
    Properties
    {
        _ScanlineLowerBound ("Scanline LowerBound", Range(0,1)) = 0.9
        _ScanlineFrequency ("Scanline Frequency", Range(1,1000)) = 500
        _Speed ("Scanline Speed", Range(0,10)) = 0.05
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
           Name "ScanLinesWipePass"

           HLSLPROGRAM
           #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
           #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

           #pragma vertex Vert
           #pragma fragment Frag

           float _ScanlineLowerBound;
           float _ScanlineFrequency;
           float _Speed;
           float _TransitionValue;
           float4 _SheenColor;
           float4 _BackgroundColor;

            TEXTURE2D(_SheenTex);        // 👈 NEW
            SAMPLER(sampler_SheenTex);


              
            float RemapFloat(float value, float inMin, float inMax, float outMin, float outMax)
            {
                return outMin + (value - inMin) * (outMax - outMin) / (inMax - inMin);
            }
            

           // Out frag function takes as input a struct that contains the screen space coordinate we are going to use to sample our texture. It also writes to SV_Target0, this has to match the index set in the UseTextureFragment(sourceTexture, 0, …) we defined in our render pass script.   
           float4 Frag(Varyings input) : SV_Target0
           {
               // this is needed so we account XR platform differences in how they handle texture arrays
               UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

               // sample the texture using the SAMPLE_TEXTURE2D_X_LOD
               float2 uv = input.texcoord.xy;
               half4 color = SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearRepeat, uv, _BlitMipLevel);

                float center = abs(uv.x - 0.5) <= 1-_TransitionValue * 2 ? 0 : 1;

                float centerblack = abs(uv.x - 0.5) <= max(_TransitionValue - 0.5,0) ? 0 : 1;

                //center *= -1;

               //ScanLines
                float wave = sin(((uv.y)) * _ScanlineFrequency);
                //float remappedWave = RemapFloat(wave,-1,1,_ScanlineLowerBound, 1);
                float scan = step(0, wave);
                //float scan = smoothstep(_ScanlineLowerBound, 1.0, wave);
                float remappedWave = RemapFloat(scan,0,1,_ScanlineLowerBound, 1);


                float sheenMask1 = SAMPLE_TEXTURE2D(_SheenTex, sampler_SheenTex, uv).r;
                //centerblack += sheenMask1;

                //color = lerp(color,color*remappedWave,center);
                color = lerp(color,_BackgroundColor,center*remappedWave);
                //color *= centerblack;//lerp(color*centerblack,_BackgroundColor,centerblack);
                color = lerp(_BackgroundColor,color,centerblack);
                color += (sheenMask1 * (1-centerblack)) * _SheenColor;
                //color = half4(1, 1, 1, 1) * mask;
                return color;
               
               // Modify the sampled color
               return half4(0, 1, 0, 1) * color;
           }

        


           ENDHLSL
       }
   }
}