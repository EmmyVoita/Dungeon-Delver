using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[ExecuteAlways]
public class FinalOverlayPost : MonoBehaviour
{
    [Header("Composite Settings")]
    public Material combineMaterial;           // Your combine shader material
    public RenderTexture overlayVFXTexture;    // The texture with your VFX overlay

    private static readonly ProfilingSampler sampler = new ProfilingSampler("Global Final Overlay");

    private void OnEnable()
    {
        RenderPipelineManager.endContextRendering += OnEndContextRendering;
    }

    private void OnDisable()
    {
        RenderPipelineManager.endContextRendering -= OnEndContextRendering;
    }

    private void OnEndContextRendering(ScriptableRenderContext context, List<Camera> cameras)
    {
        if (combineMaterial == null || overlayVFXTexture == null)
            return;

        // 🧠 Find the *final* camera that renders to the Game display (not offscreen RTs)
        Camera lastCam = null;
        foreach (var cam in cameras)
        {
            if (cam.cameraType == CameraType.Game && cam.targetTexture == null)
                lastCam = cam;
        }

        // None found → skip
        if (lastCam == null)
            return;

        // ✅ Debug output
        Debug.Log($"[FinalOverlayPost] Running after ALL cameras at frame {Time.frameCount} | Final cam: {lastCam.name}");

        // 🧾 Now we’re truly after the full composite
        using (var cmd = CommandBufferPool.Get("FinalOverlayGlobal"))
        using (new ProfilingScope(cmd, new ProfilingSampler("Global Final Overlay")))
        {
            combineMaterial.SetTexture("_OverlayVFXTex", overlayVFXTexture);
            Blitter.BlitTexture(cmd, BuiltinRenderTextureType.CameraTarget, new Vector4(1, 1, 0, 0), combineMaterial, 0);
            context.ExecuteCommandBuffer(cmd);
        }
    }


}
