using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class FinalOverlayFeature : ScriptableRendererFeature
{
    class Pass : ScriptableRenderPass
    {
        private Material mat;

        public Pass(Material mat)
        {
            this.mat = mat;
            renderPassEvent = RenderPassEvent.AfterRendering + 10; // after FinalBlit
        }

        [System.Obsolete]
        public override void Execute(ScriptableRenderContext context, ref RenderingData data)
        {
            var cmd = CommandBufferPool.Get("Final Overlay Pass");
            using (new ProfilingScope(cmd, new ProfilingSampler("Final Overlay")))
            {
                // This will actually show up in RenderDoc
                CoreUtils.DrawFullScreen(cmd, mat);
            }
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }

    public Material overlayMaterial;
    private Pass pass;

    public override void Create()
    {
        pass = new Pass(overlayMaterial);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData data)
    {
        if (overlayMaterial == null) return;
        renderer.EnqueuePass(pass);
    }
}
