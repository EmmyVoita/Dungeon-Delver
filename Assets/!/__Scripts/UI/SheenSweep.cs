using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

[RequireComponent(typeof(Image))]
public class SheenSweep : MonoBehaviour
{
    [Header("Sweep Motion")]
    [Tooltip("Direction the sheen moves in (e.g. (1,1) = up-right, (-1,1) = up-left)")]
    public Vector2 sweepDirection = new Vector2(1f, 1f);

    [Tooltip("How far the sheen travels along the sweep direction (UI units)")]
    public float sweepDistance = 240f;

    [Tooltip("How long one sweep takes (seconds)")]
    public float sweepDuration = 3.5f;

    [Tooltip("Pause between sweeps (seconds)")]
    public float pauseDuration = 2f;

    [Header("Easing")]
    public Ease sweepEase = Ease.InOutSine;

    [Header("Opacity Wobble")]
    public float baseAlpha = 0.25f;
    public float alphaWobbleAmount = 0.05f;
    public float alphaWobbleSpeed = 0.7f;

    [Header("Optional Tilt")]
    public float tiltAmount = 2f;
    public float tiltSpeed = 0.3f;

    private RectTransform rectTransform;
    private Image image;

    private Vector2 startAnchoredPos;
    private Sequence sweepSequence;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        image = GetComponent<Image>();
        startAnchoredPos = rectTransform.anchoredPosition;
    }

    void OnEnable()
    {
        StartSweep();
    }

    void OnDisable()
    {
        sweepSequence?.Kill();
    }

    void Update()
    {
        // 🫧 Soft alpha wobble
        float alpha = baseAlpha + Mathf.Sin(Time.time * alphaWobbleSpeed) * alphaWobbleAmount;
        Color c = image.color;
        c.a = Mathf.Clamp01(alpha);
        image.color = c;

        // 🫧 Subtle tilt wobble
        if (tiltAmount > 0f)
        {
            float tilt = Mathf.Sin(Time.time * tiltSpeed) * tiltAmount;
            rectTransform.localRotation = Quaternion.Euler(0f, 0f, tilt);
        }
    }

    void StartSweep()
    {
        sweepSequence?.Kill();

        Vector2 dir = sweepDirection.normalized;

        Vector2 startPos = startAnchoredPos - dir * sweepDistance;
        Vector2 endPos   = startAnchoredPos + dir * sweepDistance;

        rectTransform.anchoredPosition = startPos;

        sweepSequence = DOTween.Sequence();

        // Sweep across
        sweepSequence.Append(
            rectTransform
                .DOAnchorPos(endPos, sweepDuration)
                .SetEase(sweepEase)
        );

        // Pause
        sweepSequence.AppendInterval(pauseDuration);

        // Loop
        sweepSequence.SetLoops(-1, LoopType.Restart);
    }
}
