using UnityEngine;

// Attached to your overlay camera
[ExecuteAlways]
public class OverlayCopyColor : MonoBehaviour
{
    public RenderTexture cameraOutputWithDepth;
    public RenderTexture colorOnlyCopy;

    void LateUpdate()
    {
        if (cameraOutputWithDepth == null || colorOnlyCopy == null) return;
        Graphics.Blit(cameraOutputWithDepth, colorOnlyCopy);
    }
}
