using UnityEngine;

[RequireComponent(typeof(Camera))] [ExecuteAlways]
public class AspectRatioController : MonoBehaviour
{
    [Header("Native Aspect")]
    [SerializeField] private float nativeAspect = 1472f / 900f;

    [Header("Allowed Range")]
    [SerializeField] private float minAspect = 1.50f;
    [SerializeField] private float maxAspect = 1.78f;

    [Header("Cameras")]
    [SerializeField] private Camera leftCamera;
    [SerializeField] private Camera uiCamera;
    [SerializeField] private Camera centerCamera;
    [SerializeField] private Camera rightCamera;

    private readonly Rect leftNative =
        new Rect(0f, 0f, 0.160326f, 1f);

    private readonly Rect centerNative =
        new Rect(0.160326f, 0f, 0.679348f, 1f);

    private readonly Rect rightNative =
        new Rect(0.839674f, 0f, 0.160326f, 1f);

    private int _lastWidth;
    private int _lastHeight;

    private void Start()
    {
        Refresh();
    }

    private void Update()
    {
        if (Screen.width != _lastWidth ||
            Screen.height != _lastHeight)
        {
            Refresh();
        }
    }

    private void Refresh()
    {
        _lastWidth = Screen.width;
        _lastHeight = Screen.height;

        float windowAspect =
            (float)Screen.width / Screen.height;

        Rect presentationRect = GetPresentationRect(windowAspect);

        ApplyCameraRect(leftCamera, leftNative, presentationRect);
        ApplyCameraRect(centerCamera, centerNative, presentationRect);
        ApplyCameraRect(uiCamera, centerNative, presentationRect);
        ApplyCameraRect(rightCamera, rightNative, presentationRect);
    }

    private Rect GetPresentationRect(float windowAspect)
    {
        // Too wide -> pillarbox
        if (windowAspect > maxAspect)
        {
            float width = maxAspect / windowAspect;
            float x = (1f - width) * 0.5f;

            return new Rect(x, 0f, width, 1f);
        }

        // Too narrow -> letterbox
        if (windowAspect < minAspect)
        {
            float height = windowAspect / minAspect;
            float y = (1f - height) * 0.5f;

            return new Rect(0f, y, 1f, height);
        }

        return new Rect(0f, 0f, 1f, 1f);
    }

    private void ApplyCameraRect(
        Camera camera,
        Rect nativeRect,
        Rect presentationRect)
    {
        if (camera == null)
            return;

        camera.rect = new Rect(
            presentationRect.x +
            nativeRect.x * presentationRect.width,

            presentationRect.y +
            nativeRect.y * presentationRect.height,

            nativeRect.width * presentationRect.width,
            nativeRect.height * presentationRect.height
        );
    }
}