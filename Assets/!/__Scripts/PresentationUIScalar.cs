using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class PresentationUIScaler : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CanvasScaler canvasScaler;

    [Header("Reference")]
    [SerializeField] private Vector2 referenceResolution = new Vector2(1000f, 900f);

    [Header("Aspect Limits")]
    [SerializeField] private float minPresentationAspect = 1000f / 900f;
    [SerializeField] private float maxPresentationAspect = 16f / 9f;

    private int _lastWidth;
    private int _lastHeight;

    private void Awake()
    {
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
        Refresh();
    }

    private void Update()
    {
        if (Screen.width == _lastWidth &&
            Screen.height == _lastHeight)
            return;

        Refresh();
    }

    private void Refresh()
    {
        _lastWidth = Screen.width;
        _lastHeight = Screen.height;

        float windowAspect =
            (float)Screen.width / Screen.height;

        float effectiveAspect = Mathf.Clamp(
            windowAspect,
            minPresentationAspect,
            maxPresentationAspect
        );

        // Work out the effective pixel area that the center presentation
        // would occupy after aspect clamping.
        float effectiveWidth;
        float effectiveHeight;

        if (windowAspect > maxPresentationAspect)
        {
            // Pillarboxed: full height, limited width.
            effectiveHeight = Screen.height;
            effectiveWidth = effectiveHeight * maxPresentationAspect;
        }
        else if (windowAspect < minPresentationAspect)
        {
            // Letterboxed: full width, limited height.
            effectiveWidth = Screen.width;
            effectiveHeight = effectiveWidth / minPresentationAspect;
        }
        else
        {
            // Within supported range.
            effectiveWidth = Screen.width;
            effectiveHeight = Screen.height;
        }

        float widthScale =
            effectiveWidth / referenceResolution.x;

        float heightScale =
            effectiveHeight / referenceResolution.y;

        // Similar to Match = 0.5
        canvasScaler.scaleFactor =
            Mathf.Sqrt(widthScale * heightScale);
    }
}