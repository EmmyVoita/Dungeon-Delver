Shader "Hidden/TestFinalOverlay"
{
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" }
        Pass
        {
            ZTest Always Cull Off ZWrite Off
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes { float4 positionOS : POSITION; };
            struct Varyings { float4 pos : SV_POSITION; };

            Varyings Vert(Attributes v)
            {
                Varyings o;
                o.pos = TransformObjectToHClip(v.positionOS.xyz);
                return o;
            }

            half4 Frag(Varyings i) : SV_Target
            {
                return half4(1,0,0,1); // bright red
            }
            ENDHLSL
        }
    }
}
