Shader "ScanLines"
{
    Properties
    {
        _Influence ("Influence", Range(0,1)) = 1
        _ScanlineLowerBound ("Scanline LowerBound", Range(0,1)) = 0.9
        _ScanlineFrequency ("Scanline Frequency", Range(1,1000)) = 500
        _Speed ("Scanline Speed", Range(0,10)) = 0.05
    }
   SubShader
   {
       Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline"}
       ZWrite Off Cull Off
       Pass
       {
           Name "ScanLinesPass"

           HLSLPROGRAM
           #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
           #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

           #pragma vertex Vert
           #pragma fragment Frag

           float _ScanlineLowerBound;
           float _ScanlineFrequency;
           float _Speed;
           float _Influence;


              
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


               //ScanLines
                float wave = sin(((uv.y - uv.x) + _Time.y * _Speed) * _ScanlineFrequency);
                //float remappedWave = RemapFloat(wave,-1,1,_ScanlineLowerBound, 1);
                float scan = step(0, wave);
                //float scan = smoothstep(_ScanlineLowerBound, 1.0, wave);
                float remappedWave = RemapFloat(scan,0,1,_ScanlineLowerBound, 1);


                return lerp(color, color * remappedWave, _Influence);
           }

        


           ENDHLSL
       }
   }
}