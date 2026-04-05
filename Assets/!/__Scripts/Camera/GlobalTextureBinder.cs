using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[ExecuteAlways]
public class GlobalTextureBinder : MonoBehaviour
{
    [SerializeField] private RenderTexture gameView;
    [SerializeField] private RenderTexture uiCamera;
    [SerializeField] private RenderTexture overlay;

    private void OnEnable()
    {
        RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
    }

    private void OnDisable()
    {
        RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
    }

    private void OnBeginCameraRendering(ScriptableRenderContext ctx, Camera cam)
    {
        // set once for all shaders before each camera renders
        Shader.SetGlobalTexture("_GameViewTex", gameView);
        Shader.SetGlobalTexture("_UICameraTex", uiCamera);
        Shader.SetGlobalTexture("_OverlayVFXTex", overlay);
    }
}
