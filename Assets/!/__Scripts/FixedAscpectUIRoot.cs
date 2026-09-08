using UnityEngine;

public class FixedAspectUIRoot : MonoBehaviour
{
    [SerializeField] private RectTransform root;
    [SerializeField] private RectTransform canvasRect;

    [SerializeField] private float minAspect = 1000f / 900f;
    [SerializeField] private float maxAspect = 16f / 9f;

    private Vector2 _lastCanvasSize;

    private void LateUpdate()
    {
        Vector2 size = canvasRect.rect.size;

        if (size == _lastCanvasSize)
            return;

        _lastCanvasSize = size;

        Refresh(size);
    }

    private void Refresh(Vector2 availableSize)
    {
        float availableAspect =
            availableSize.x / availableSize.y;

        float targetAspect = Mathf.Clamp(
            availableAspect,
            minAspect,
            maxAspect
        );

        float targetWidth;
        float targetHeight;

        if (availableAspect > targetAspect)
        {
            // Too wide.
            // Keep full height and limit width.
            targetHeight = availableSize.y;
            targetWidth = targetHeight * targetAspect;
        }
        else
        {
            // Too narrow.
            // Keep full width and limit height.
            targetWidth = availableSize.x;
            targetHeight = targetWidth / targetAspect;
        }

        root.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Horizontal,
            targetWidth
        );

        root.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Vertical,
            targetHeight
        );

        root.anchoredPosition = Vector2.zero;
    }
}