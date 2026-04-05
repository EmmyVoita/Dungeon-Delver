using UnityEngine;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule.Util;
public class VFXOverlayRendererFeature : ScriptableRendererFeature
{

   
        public Material blitCombineMaterial;
        public RenderTexture[] renderTextures;
        public RenderPassEvent CombineEvent = RenderPassEvent.AfterRenderingPostProcessing; 
        
        private BlitCombineBufferPass m_Pass;
        
        public override void Create()
        {
            m_Pass = new BlitCombineBufferPass();
            m_Pass.renderPassEvent = CombineEvent;
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            var cameraData = renderingData.cameraData;
            var camera = cameraData.camera;
            
            // if camera inst main camera, skip
            if (camera != Camera.main)
                return;

            // Since they have the same RenderPassEvent the order matters when enqueueing them.

            // Early exit if there are no materials.
            if (blitCombineMaterial == null || renderTextures.Length != 3)
            {
                Debug.LogWarning("Skipping MRTPass because the material is null or render textures doesn't have a size of 3.");
                return;
            }

            foreach (var rt in renderTextures)
            {
                if (rt == null)
                {
                    Debug.LogWarning("Skipping MRTPass because one of the render textures is null.");
                    return;
                }
            }

            m_Pass.Setup(blitCombineMaterial, renderTextures);
            renderer.EnqueuePass(m_Pass);
        }
}
