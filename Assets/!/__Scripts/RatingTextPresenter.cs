using TMPro;
using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(TextMeshProUGUI))]
public class RatingTextPresenter : MonoBehaviour
{
    private TextMeshProUGUI text;
    private RectTransform rect;

    private Gradient currentGradient;
    private float gradientOffset;
    private Tween gradientTween;
    private Tween scaleTween;

    [Header("Animation Settings")]
    [Tooltip("How fast the gradient scrolls.")]
    [SerializeField] private float gradientDuration = 1.2f;

    [Tooltip("How many times the gradient repeats across the text.")]
    [SerializeField] private float gradientTiling = 2.5f;

    [Tooltip("Enable subtle breathing scale.")]
    [SerializeField] private bool useBreathingScale = true;

    [SerializeField] private float breathingScaleAmount = 1.05f;
    [SerializeField] private float breathingDuration = 0.6f;

    private void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();
        rect = GetComponent<RectTransform>();
    }

    private void OnDisable()
    {
        gradientTween?.Kill();
        scaleTween?.Kill();
    }

    // ------------------------------------------------------
    // PUBLIC ENTRY POINT
    // ------------------------------------------------------

    public void ShowRating(RatingDisplayData data)
    {
        gradientTween?.Kill();
        scaleTween?.Kill();

        rect.localScale = Vector3.one;

        text.text = data.ratingText;
        text.ForceMeshUpdate();

        currentGradient = data.gradient;

        if (data.animateGradient)
        {
            StartGradientAnimation();
        }
        else
        {
            ApplyStaticGradient();
        }

        if (useBreathingScale && data.animateGradient)
        {
            StartBreathingScale();
        }
    }

    // ------------------------------------------------------
    // GRADIENT ANIMATION
    // ------------------------------------------------------

    private void StartGradientAnimation()
    {
        gradientOffset = 0f;

        gradientTween = DOTween.To(
                () => gradientOffset,
                x =>
                {
                    gradientOffset = x;
                    ApplyAnimatedGradient();
                },
                1f,
                gradientDuration
            )
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Restart);
    }

    private void ApplyAnimatedGradient()
    {
        var textInfo = text.textInfo;
        int charCount = textInfo.characterCount;

        if (charCount == 0)
            return;

        for (int i = 0; i < charCount; i++)
        {
            if (!textInfo.characterInfo[i].isVisible)
                continue;

            int matIndex = textInfo.characterInfo[i].materialReferenceIndex;
            int vertIndex = textInfo.characterInfo[i].vertexIndex;

            Color32[] vertexColors = textInfo.meshInfo[matIndex].colors32;

            float t =
                (((float)i / (charCount - 1)) * gradientTiling + gradientOffset) % 1f;

            Color color = currentGradient.Evaluate(t);

            vertexColors[vertIndex + 0] = color;
            vertexColors[vertIndex + 1] = color;
            vertexColors[vertIndex + 2] = color;
            vertexColors[vertIndex + 3] = color;
        }

        text.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
    }

    // ------------------------------------------------------
    // STATIC GRADIENT (for GOOD / OK etc)
    // ------------------------------------------------------

    private void ApplyStaticGradient()
    {
        var textInfo = text.textInfo;
        int charCount = textInfo.characterCount;

        if (charCount == 0)
            return;

        for (int i = 0; i < charCount; i++)
        {
            if (!textInfo.characterInfo[i].isVisible)
                continue;

            int matIndex = textInfo.characterInfo[i].materialReferenceIndex;
            int vertIndex = textInfo.characterInfo[i].vertexIndex;

            Color32[] vertexColors = textInfo.meshInfo[matIndex].colors32;

            float t = (float)i / (charCount - 1);

            Color color = currentGradient.Evaluate(t);

            vertexColors[vertIndex + 0] = color;
            vertexColors[vertIndex + 1] = color;
            vertexColors[vertIndex + 2] = color;
            vertexColors[vertIndex + 3] = color;
        }

        text.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
    }

    // ------------------------------------------------------
    // BREATHING SCALE
    // ------------------------------------------------------

    private void StartBreathingScale()
    {
        scaleTween = rect
            .DOScale(breathingScaleAmount, breathingDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }
}
