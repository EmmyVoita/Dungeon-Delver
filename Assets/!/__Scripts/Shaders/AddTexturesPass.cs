using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;


public class AddTexturesPass : ScriptableRenderPass
{
    class PassData
    {
    }

    // custom data class 
    public class CustomData : ContextItem 
    {
        public TextureHandle OverlayVFXTexture;

        public override void Reset()
        {
            OverlayVFXTexture = TextureHandle.nullHandle;
        }
    }

    private const string k_PassName = "AddTextures";
    private const string k_TextureNamePrefix = "CustomBuffer -";


    public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
    {
        using (var builder = renderGraph.AddRasterRenderPass<PassData>(k_PassName, out var passData))
        {
            var resourceData = frameData.Get<UniversalResourceData>();

            var cameraData = frameData.Get<UniversalCameraData>();
            int frameWidth = cameraData.cameraTargetDescriptor.width;
            int frameHeight = cameraData.cameraTargetDescriptor.height;

            var source = resourceData.activeColorTexture;

            RenderTextureFormat rgbaRenderFormat;
            rgbaRenderFormat = RenderTextureFormat.ARGBFloat;



            RenderTextureDescriptor screenResolutionColorDesc = new RenderTextureDescriptor(frameWidth, frameHeight, rgbaRenderFormat, 0);
            screenResolutionColorDesc.enableRandomWrite = true;


            // Post Passes
            TextureHandle overlayVFXTexture = UniversalRenderer.CreateRenderGraphTexture(renderGraph, screenResolutionColorDesc, $"{k_TextureNamePrefix} - overlay VFX", true);

            CustomData customData;
            if (!frameData.Contains<CustomData>())
            {
                customData = frameData.Create<CustomData>();
            }
            else
            {
                customData = frameData.Get<CustomData>();
            }
            
            customData.OverlayVFXTexture = overlayVFXTexture;

            builder.AllowPassCulling(false);
            builder.SetRenderFunc((PassData data, RasterGraphContext ctx) => { /* no-op */ });
        }
    }
}
