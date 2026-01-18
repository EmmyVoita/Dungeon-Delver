using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;

public class NoClearFeature : ScriptableRendererFeature
{
    class NoClearPass : ScriptableRenderPass
    {
        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            // Get camera data
            UniversalCameraData camData = frameData.Get<UniversalCameraData>();
            if (camData == null)
                return;

            // Only apply for cameras with our special tag
            if (camData.camera == null || !camData.camera.CompareTag("OverlayVFX"))
                return;

            // Set the load action to Store so the color buffer is kept
            // (so nothing overwrites what was already rendered)
            ConfigureClear(ClearFlag.Color, Color.black);   // false = no clear on color; true = store result
        }
    }

    NoClearPass m_Pass;

    public override void Create()
    {
        m_Pass = new NoClearPass
        {
            renderPassEvent = RenderPassEvent.BeforeRendering
        };
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        // Queue the pass for this renderer
        renderer.EnqueuePass(m_Pass);
    }
}
