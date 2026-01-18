using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

public class BlitCombineBufferPass : ScriptableRenderPass
{
    
    private class PassData
    {
        public TextureHandle source;
        public TextureHandle destination;
        public TextureHandle[] rts;
        public Material material;
    }


    private Material m_BlitMaterial;
    RTHandle[] m_RTs = new RTHandle[3];
    RenderTargetInfo[] m_RTInfos = new RenderTargetInfo[3];

    public void Setup(Material mat, RenderTexture[] renderTextures)
    {
        m_BlitMaterial = mat;

        //Create RTHandles from the RenderTextures if they have changed.
        for (int i = 0; i < 3; i++)
        {
            if (m_RTs[i] == null || m_RTs[i].rt != renderTextures[i])
            {
                m_RTs[i]?.Release();
                m_RTs[i] = RTHandles.Alloc(renderTextures[i], $"ChannelTexture[{i}]");
                m_RTInfos[i] = new RenderTargetInfo()
                {
                    format = renderTextures[i].graphicsFormat,
                    height = renderTextures[i].height,
                    width = renderTextures[i].width,
                    bindMS = renderTextures[i].bindTextureMS,
                    msaaSamples = 1,
                    volumeDepth = renderTextures[i].volumeDepth,
                };
            }
        }
    }


    public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
    {
        const string passName = "CombineMainOverlayVFXPass";
        var resourceData = frameData.Get<UniversalResourceData>();


        var handles = new TextureHandle[3];
        // Imports the texture handles them in RenderGraph.
        for (int i = 0; i < 3; i++)
        {
            if (m_RTs[i] == null || m_RTs[i].rt == null || !m_RTs[i].rt.IsCreated())
            {
                Debug.LogWarning($"[BlitCombineBufferPass] Skipping invalid RT[{i}]");
                //return; // early exit or continue depending on needs
            }

            handles[i] = renderGraph.ImportTexture(m_RTs[i], m_RTInfos[i]);
        }

        // ✅ Create output
        var source = resourceData.activeColorTexture;
        var destDesc = renderGraph.GetTextureDesc(source);
        destDesc.name = "CameraColor-CombinedVFX";
        destDesc.clearBuffer = false;
        TextureHandle destination = renderGraph.CreateTexture(destDesc);

        using (var builder = renderGraph.AddRasterRenderPass<PassData>(passName, out var passData, new ProfilingSampler(passName)))
        {
            for (int i = 0; i < 3; i++)
            {
                builder.UseTexture(handles[i], AccessFlags.Read);
            }

           
            passData.rts = handles;
            passData.material = m_BlitMaterial;
            passData.destination = destination;

            // ✅ Explicit dependencies
            builder.SetRenderAttachment(destination, 0, AccessFlags.Write);
            builder.AllowPassCulling(false);

            builder.SetRenderFunc((PassData data, RasterGraphContext ctx) =>
            {
                using (new ProfilingScope(ctx.cmd, new ProfilingSampler("CombinePassExec")))
                {
                    //var actualTexture = ctx.renderGraph.GetTexture(data.rts[0]);
                    // IMPORTANT: bind textures here (inside the pass)
                    //data.material.SetTexture("_GameViewTex", data.rts[0]);
                    //data.material.SetTexture("_UICameraTex",  source);
                    //data.material.SetTexture("_OverlayVFXTex", source);

                    Blitter.BlitTexture(ctx.cmd, BuiltinRenderTextureType.None, new Vector4(1, 1, 0, 0), data.material, 0);
                }
            });

            resourceData.cameraColor = destination;
        }
    }
}
