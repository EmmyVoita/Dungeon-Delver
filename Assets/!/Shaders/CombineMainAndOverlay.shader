Shader "Hidden/CombineMainAndOverlayVFX"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline" }
        ZWrite Off
        Cull Off
        ZTest Always
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            Name "CombineMainAndOverlay"
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            TEXTURE2D(_GameViewTex);
            TEXTURE2D(_UICameraTex);
            TEXTURE2D(_OverlayVFXTex);

            half4 Frag(Varyings input) : SV_Target
            {
                float2 uv = input.texcoord.xy;

                // NEW: 1200x1080 game view region inside 1920x1080 frame
                float2 offset = float2(0.1875, 0.0);
                float2 scale  = float2(0.625, 1.0);

                float2 gameViewUV = (uv - offset) / scale;

                // Check if inside valid game view area
                bool inBounds = all(gameViewUV >= 0.0) && all(gameViewUV <= 1.0);

                half4 gameView = 0;
                if (inBounds)
                    gameView = SAMPLE_TEXTURE2D(_GameViewTex, sampler_PointClamp, gameViewUV);

                half4 vfx = SAMPLE_TEXTURE2D(_OverlayVFXTex, sampler_PointClamp, uv);
                half4 ui  = SAMPLE_TEXTURE2D(_UICameraTex, sampler_PointClamp, uv);

                half4 color = ui;

                if (inBounds)
                {
                    color.rgb = lerp(color.rgb, gameView.rgb, gameView.a);
                    color.a   = saturate(color.a + gameView.a * (1 - color.a));
                }

                color.rgb = lerp(color.rgb, vfx.rgb, vfx.a);
                color.a   = saturate(color.a + vfx.a * (1 - color.a));

                return color;


            }
            ENDHLSL
        }
    }
}
