using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;
using System.Reflection;

public class KeepColorBufferFeature : ScriptableRendererFeature
{
    class KeepColorBufferPass : ScriptableRenderPass
    {
        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            var cameraData = frameData.Get<UniversalCameraData>();
            if (cameraData == null || cameraData.camera == null)
                return;

            // Only affect our overlay VFX camera
            if (!cameraData.camera.CompareTag("OverlayVFX"))
                return;

            // Access UniversalRenderPipeline’s internal CameraColorInitPassData
            var resourceData = frameData.Get<UniversalResourceData>();
            if (resourceData == null)
                return;

            var colorHandle = resourceData.activeColorTexture;
            if (!colorHandle.IsValid())
                return;

            // Use reflection to access and override load/store on the underlying RT descriptor
            var handleField = typeof(TextureHandle).GetField("m_ResourceHandle",
                BindingFlags.NonPublic | BindingFlags.Instance);
            if (handleField == null) return;

            var rh = handleField.GetValue(colorHandle);
            if (rh == null) return;

            var descProp = rh.GetType().GetProperty("desc",
                BindingFlags.Public | BindingFlags.Instance);
            if (descProp == null) return;

            if (descProp.GetValue(rh) is not TextureDesc desc) return;

            desc.clearBuffer = false;
            desc.clearColor = Color.clear;

            descProp.SetValue(rh, desc);
        }
    }

    KeepColorBufferPass m_Pass;

    public override void Create()
    {
        m_Pass = new KeepColorBufferPass
        {
            renderPassEvent = RenderPassEvent.BeforeRendering
        };
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        // Only enqueue for OverlayVFX camera
        if (renderingData.cameraData.camera.CompareTag("OverlayVFX"))
            renderer.EnqueuePass(m_Pass);
    }
}
