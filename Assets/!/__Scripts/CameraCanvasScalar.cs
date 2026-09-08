using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class CameraCanvasScaler : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;
    [SerializeField] private CanvasScaler canvasScaler;

    [SerializeField] private Vector2 referenceResolution =
        new Vector2(1000f, 900f);

    private Vector2 _lastSize;

    private void Awake()
    {
        canvasScaler.uiScaleMode =
            CanvasScaler.ScaleMode.ConstantPixelSize;

        Refresh();
    }

    private void Update()
    {
        Vector2 size = new Vector2(
            targetCamera.pixelWidth,
            targetCamera.pixelHeight
        );

        if (size == _lastSize)
            return;

        Refresh();
    }

    private void Refresh()
    {
        float width = targetCamera.pixelWidth;
        float height = targetCamera.pixelHeight;

        _lastSize = new Vector2(width, height);

        float widthScale =
            width / referenceResolution.x;

        float heightScale =
            height / referenceResolution.y;

        canvasScaler.scaleFactor =
            Mathf.Sqrt(widthScale * heightScale);
    }
}