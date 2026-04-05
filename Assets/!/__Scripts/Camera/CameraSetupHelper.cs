using UnityEngine;
using UnityEngine.Rendering.Universal;

[ExecuteAlways]
public class CameraSetupHelper : MonoBehaviour
{
    private UniversalAdditionalCameraData urpData;

    void OnEnable()
    {
        var cam = GetComponent<Camera>();
        cam.depth = 50;

        if (cam.TryGetComponent<UniversalAdditionalCameraData>(out urpData))
        {
            // Tell URP not to clear the color buffer.
            urpData.renderPostProcessing = false;
            urpData.requiresColorOption = CameraOverrideOption.Off;
            urpData.requiresDepthOption = CameraOverrideOption.Off;
            urpData.SetRenderer(0); // optional, use the first renderer slot
        }

        cam.clearFlags = CameraClearFlags.Nothing; // built-in safety
    }
}
