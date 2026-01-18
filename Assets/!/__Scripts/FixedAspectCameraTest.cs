using UnityEngine;

[RequireComponent(typeof(Camera))]
[ExecuteAlways]
public class FixedAspectCameraTest : MonoBehaviour
{
    public float targetAspect = 16f / 9f;

    void Start()
    {
        UpdateCamera();
    }

    void Update()
    {
        // If resized, re-apply
        if (Screen.width != lastW || Screen.height != lastH)
        {
            lastW = Screen.width;
            lastH = Screen.height;
            UpdateCamera();
        }
    }

    private int lastW, lastH;

    void UpdateCamera()
    {
        float windowAspect = (float)Screen.width / Screen.height;
        float scaleHeight = windowAspect / targetAspect;

        Camera cam = GetComponent<Camera>();

        if (scaleHeight < 1.0f)
        {
            // Add top/bottom letterbox
            Rect rect = cam.rect;
            rect.width = 1.0f;
            rect.height = scaleHeight;
            rect.x = 0;
            rect.y = (1.0f - scaleHeight) / 2.0f;
            cam.rect = rect;
        }
        else
        {
            // Add left/right pillarbox
            float scaleWidth = 1.0f / scaleHeight;
            Rect rect = cam.rect;
            rect.width = scaleWidth;
            rect.height = 1.0f;
            rect.x = (1.0f - scaleWidth) / 2.0f;
            rect.y = 0;
            cam.rect = rect;
        }
    }
}
