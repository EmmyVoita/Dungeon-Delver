using DG.Tweening;
using TMPro;
using UnityEngine;

public class ScoreMultiplierSlider : MonoBehaviour
{
    [SerializeField] private string suffix = " Score";

    [Header("References")]
    [SerializeField] private RectTransform sliderBounds;
    [SerializeField] private RectTransform knobRect;
    [SerializeField] private TextMeshProUGUI multiplierText;

    [Header("Movement")]
    [SerializeField] private float moveDuration = 0.15f;
    [SerializeField] private Ease moveEase = Ease.OutCubic;

    [Header("Visual Range")]
    [Range(0f, 0.2f)]
    [SerializeField] private float edgePadding = 0.05f;

    [Header("Text Colors")]
    [SerializeField] private float lowMultiplier = 0.5f;
    [SerializeField] private float neutralMultiplier = 1f;
    [SerializeField] private float highMultiplier = 2.5f;

    [SerializeField] private Color lowColor = new Color(0.55f, 1f, 0.65f);
    [SerializeField] private Color neutralColor =Color.white;
    [SerializeField] private Color highColor =new Color(1f, 0.25f, 0.1f);

    [Header("Text Punch")]
    [SerializeField] private float textPunchAmount = 0.08f;
    [SerializeField] private float textPunchDuration = 0.18f;

    [Header("Knob Punch")]
    [SerializeField] private float knobPunchMin = 0.04f;
    [SerializeField] private float knobPunchMax = 0.12f;
    [SerializeField] private float knobPunchDuration = 0.18f;

    [Header("Flame")]
    [SerializeField] private SoundEffect igniteSound;
    [SerializeField] private RectTransform flameRect;
    [SerializeField] private CanvasGroup flameCanvasGroup;

    [SerializeField] private float flameStartMultiplier = 1.25f;
    [SerializeField] private float flameMaxMultiplier = 2.5f;

    [SerializeField] private float flameMinScale = 0.75f;
    [SerializeField] private float flameMaxScale = 1.5f;

    [SerializeField] private float flameTweenDuration = 0.12f;
    [SerializeField] private float flameFadeDuration = 0.1f;

    [Header("Flame Ignition")]
    [SerializeField] private float ignitionStartScale = 0.25f;
    [SerializeField] private float ignitionOvershoot = 1.12f;
    [SerializeField] private float ignitionUpDuration = 0.07f;
    [SerializeField] private float ignitionSettleDuration = 0.08f;

    private float _currentDifficulty;
    private bool _flameActive;

    private Vector3 _knobBaseScale;
    private Vector3 _textBaseScale;

    private void Awake()
    {
        if (knobRect != null)
            _knobBaseScale = knobRect.localScale;

        if (multiplierText != null)
            _textBaseScale = multiplierText.rectTransform.localScale;

        if (flameRect != null)
            flameRect.localScale = Vector3.zero;

        if (flameCanvasGroup != null)
            flameCanvasGroup.alpha = 0f;
    }

    public void SetMultiplier(
        float multiplier,
        bool animate = true)
    {
        multiplier = Mathf.Max(0.01f, multiplier);

        // Logarithmic position:
        //
        // 0.5x = -1
        // 1.0x =  0
        // 2.0x = +1
        // 4.0x = +2
        _currentDifficulty =
            Mathf.Log(multiplier, 2f);

        float compressed =
            _currentDifficulty /
            (1f + Mathf.Abs(_currentDifficulty));

        float normalized =
            (compressed + 1f) * 0.5f;

        normalized = Mathf.Lerp(
            edgePadding,
            1f - edgePadding,
            normalized
        );

        SetKnobPosition(normalized, animate);
        SetTextVisuals(multiplier, animate);
        SetFlameVisuals(multiplier, animate);

        if (animate)
            PlayKnobFeedback(multiplier);
    }

    private void SetKnobPosition(
        float normalized,
        bool animate)
    {
        if (sliderBounds == null ||
            knobRect == null)
        {
            return;
        }

        float targetX = Mathf.Lerp(
            sliderBounds.rect.xMin,
            sliderBounds.rect.xMax,
            normalized
        );

        knobRect.DOKill();

        if (!animate)
        {
            Vector2 position =
                knobRect.anchoredPosition;

            position.x = targetX;

            knobRect.anchoredPosition =
                position;

            return;
        }

        knobRect
            .DOAnchorPosX(
                targetX,
                moveDuration
            )
            .SetEase(moveEase)
            .SetUpdate(true);
    }

    private void SetTextVisuals(
        float multiplier,
        bool animate)
    {
        if (multiplierText == null)
            return;

        multiplierText.text =
            $"x{multiplier:0.00}{suffix}";

        Color targetColor =
            GetMultiplierColor(multiplier);

        multiplierText.DOKill();

        if (!animate)
        {
            multiplierText.color =
                targetColor;

            multiplierText.rectTransform.localScale =
                _textBaseScale;

            return;
        }

        multiplierText
            .DOColor(targetColor, 0.12f)
            .SetUpdate(true);

        RectTransform textRect =
            multiplierText.rectTransform;

        textRect.DOKill();

        textRect.localScale =
            _textBaseScale;

        textRect
            .DOPunchScale(
                Vector3.one *
                textPunchAmount,
                textPunchDuration,
                6,
                0.45f
            )
            .SetUpdate(true);
    }

    private Color GetMultiplierColor(
        float multiplier)
    {
        if (multiplier <= neutralMultiplier)
        {
            float t = Mathf.InverseLerp(
                lowMultiplier,
                neutralMultiplier,
                multiplier
            );

            return Color.Lerp(
                lowColor,
                neutralColor,
                t
            );
        }

        float highT = Mathf.InverseLerp(
            neutralMultiplier,
            highMultiplier,
            multiplier
        );

        return Color.Lerp(
            neutralColor,
            highColor,
            highT
        );
    }

    private void SetFlameVisuals(
        float multiplier,
        bool animate)
    {
        if (flameRect == null)
            return;

        float flameT =
            Mathf.InverseLerp(
                flameStartMultiplier,
                flameMaxMultiplier,
                multiplier
            );

        bool shouldShow =
            multiplier >
            flameStartMultiplier;

        if (!shouldShow)
        {
            _flameActive = false;

            flameRect.DOKill();

            if (flameCanvasGroup != null)
                flameCanvasGroup.DOKill();

            if (!animate)
            {
                flameRect.localScale =
                    Vector3.zero;

                if (flameCanvasGroup != null)
                    flameCanvasGroup.alpha = 0f;

                return;
            }

            flameRect
                .DOScale(
                    0f,
                    flameTweenDuration
                )
                .SetEase(Ease.InCubic)
                .SetUpdate(true);

            if (flameCanvasGroup != null)
            {
                flameCanvasGroup
                    .DOFade(
                        0f,
                        flameFadeDuration
                    )
                    .SetUpdate(true);
            }

            return;
        }

        float targetScale =
            Mathf.Lerp(
                flameMinScale,
                flameMaxScale,
                flameT
            );

        if (flameCanvasGroup != null)
        {
            flameCanvasGroup.DOKill();

            float targetAlpha =
                Mathf.Lerp(
                    0.65f,
                    1f,
                    flameT
                );

            if (animate)
            {
                flameCanvasGroup
                    .DOFade(
                        targetAlpha,
                        flameFadeDuration
                    )
                    .SetUpdate(true);
            }
            else
            {
                flameCanvasGroup.alpha =
                    targetAlpha;
            }
        }

        flameRect.DOKill();

        if (!animate)
        {
            _flameActive = true;

            flameRect.localScale =
                Vector3.one *
                targetScale;

            return;
        }

        // Newly crossing the ignition threshold.
        if (!_flameActive)
        {
            _flameActive = true;

            AudioHelpers.PlaySoundEffect(igniteSound, Camera.main.transform.position);

            flameRect.localScale =
                Vector3.one *
                ignitionStartScale;

            flameRect
                .DOScale(
                    targetScale *
                    ignitionOvershoot,
                    ignitionUpDuration
                )
                .SetEase(Ease.OutQuad)
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    if (flameRect == null)
                        return;

                    flameRect
                        .DOScale(
                            targetScale,
                            ignitionSettleDuration
                        )
                        .SetEase(Ease.OutCubic)
                        .SetUpdate(true);
                });

            return;
        }

        // Already burning: smoothly resize.
        flameRect
            .DOScale(
                targetScale,
                flameTweenDuration
            )
            .SetEase(Ease.OutCubic)
            .SetUpdate(true);
    }

    private void PlayKnobFeedback(
        float multiplier)
    {
        if (knobRect == null)
            return;

        float intensity =
            Mathf.InverseLerp(
                neutralMultiplier,
                highMultiplier,
                multiplier
            );

        float punch =
            Mathf.Lerp(
                knobPunchMin,
                knobPunchMax,
                intensity
            );

        // Important: don't DOKill here,
        // because SetKnobPosition is tweening
        // this RectTransform's anchored position.
        knobRect
            .DOPunchScale(
                Vector3.one * punch,
                knobPunchDuration,
                6,
                0.5f
            )
            .SetUpdate(true);
    }

    private void OnDestroy()
    {
        knobRect?.DOKill();
        flameRect?.DOKill();

        if (multiplierText != null)
        {
            multiplierText.DOKill();
            multiplierText.rectTransform.DOKill();
        }

        flameCanvasGroup?.DOKill();
    }
}