using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;
using UnityEngine.UIElements;



public class BlitOverlayVFXPass : ScriptableRenderPass
{
        private LayerMask _layerMask;
        private Material _blitMaterial;
        public void Setup(LayerMask layerMask, Material blitMaterial = null)
        {
        _layerMask = layerMask;
        _blitMaterial = blitMaterial;
    }

        class NormalsPassData
        {
            internal RendererListHandle objectsToDraw;
        }
        
        public BlitOverlayVFXPass()
        {
            //The pass will read the current color texture. That needs to be an intermediate texture. It's not supported to use the BackBuffer as input texture. 
            //By setting this property, URP will automatically create an intermediate texture. 
            //It's good practice to set it here and not from the RenderFeature. This way, the pass is selfcontaining and you can use it to directly enqueue the pass from a monobehaviour without a RenderFeature.
            requiresIntermediateTexture = true;
        }

        // This is where the renderGraph handle can be accessed.
        // Each ScriptableRenderPass can use the RenderGraph handle to add multiple render passes to the render graph
        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            using (var builder = renderGraph.AddRasterRenderPass<NormalsPassData>("BlitOverlayToTexture", out var passData))
            {
                var customData = frameData.Get<AddTexturesPass.CustomData>();
                var overlayVFXBuffer = customData.OverlayVFXTexture;
                
                
                if(!overlayVFXBuffer.IsValid())
                {
                    Debug.LogError("Outlines: Invalid texture handle.");
                    return;
                }

                UniversalRenderingData renderingData = frameData.Get<UniversalRenderingData>();
                UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
                UniversalLightData lightData = frameData.Get<UniversalLightData>();
                SortingCriteria sortFlags = SortingCriteria.CommonTransparent;
                RenderQueueRange renderQueueRange = RenderQueueRange.transparent;
                FilteringSettings filterSettings = new FilteringSettings(renderQueueRange, _layerMask);
                
                

                var shaderTags = new[]
                {
                    new ShaderTagId("UniversalForward"),
                    new ShaderTagId("UniversalForwardOnly"),
                    new ShaderTagId("SRPDefaultUnlit")
                };


                // Create drawing settings
                DrawingSettings drawSettings = RenderingUtils.CreateDrawingSettings(shaderTags[0], renderingData, cameraData, lightData, sortFlags);

                // Add the override material to the drawing settings
                drawSettings.overrideMaterial = null; //_blitMaterial;

                // Create the list of objects to draw
                var rendererListParameters = new RendererListParams(renderingData.cullResults, drawSettings, filterSettings);
                passData.objectsToDraw = renderGraph.CreateRendererList(rendererListParameters);


                builder.SetRenderAttachment(overlayVFXBuffer, 0, AccessFlags.Write); // Color attachment

                builder.UseRendererList(passData.objectsToDraw);

                builder.AllowPassCulling(false);

                builder.SetRenderFunc((NormalsPassData data, RasterGraphContext rgContext) =>  {
                    
                    rgContext.cmd.ClearRenderTarget(RTClearFlags.Color | RTClearFlags.Depth, Color.clear, 1,0);
                    rgContext.cmd.DrawRendererList(data.objectsToDraw);
                });
            }
        }
    }
