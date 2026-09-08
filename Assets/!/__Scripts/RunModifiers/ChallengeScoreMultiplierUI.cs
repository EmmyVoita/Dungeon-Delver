using DG.Tweening;
using TMPro;
using UnityEngine;

public class ChallengeScoreMultiplierUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TextMeshProUGUI multiplierText;
    [SerializeField] private RectTransform multiplierRect;

    [Header("Text")]
    [SerializeField] private string prefix = "SCORE x";

    [Header("Punch")]
    [SerializeField] private float punchScale = 0.2f;
    [SerializeField] private float punchRotation = 4f;
    [SerializeField] private float punchDuration = 0.25f;
    [SerializeField] private int vibrato = 6;
    [SerializeField] private float elasticity = 0.7f;

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

    private bool _flameActive;
    private float _currentMultiplier;
    private Vector3 _textBaseScale;
    private float _oldMultiplier;

    private void OnEnable()
    {
        RunModifierManager.OnValueChanged += HandleValueChanged;

        SetTextVisuals(false);
    }

    private void OnDisable()
    {
        RunModifierManager.OnValueChanged -= HandleValueChanged;

        multiplierRect?.DOKill();
    }

    private void Awake()
    {
        if (multiplierText != null)
            _textBaseScale = multiplierText.rectTransform.localScale;

        SetTextVisuals(false);

        _oldMultiplier = RunModifierManager.Instance.GetScoreMultiplier();
    }

    private void HandleValueChanged(RunModifierSelection selection)
    {
        
        //Refresh(true);
        SetTextVisuals(true);
        SetFlameVisuals(RunModifierManager.Instance.GetScoreMultiplier(), true);
    }

    /*
    private void Refresh(bool animate)
    {
        if (RunModifierManager.Instance == null)
            return;

        float newMultiplier = RunModifierManager.Instance.GetScoreMultiplier();

        bool changed =
            !Mathf.Approximately(
                newMultiplier,
                _currentMultiplier
            );

        _currentMultiplier = newMultiplier;

        multiplierText.text =
            $"{prefix}{_currentMultiplier:0.00}";

        if (animate && changed)
            PlayPunch();
    }
    */

    private void SetTextVisuals(bool animate)
    {
        if (multiplierText == null)
            return;


        float newMultiplier = RunModifierManager.Instance.GetScoreMultiplier();

        multiplierText.text = $"SCORE X{newMultiplier:0.00}";

        Color targetColor = GetMultiplierColor(newMultiplier);

        multiplierText.DOKill();

        if (!animate)
        {
            multiplierText.color =targetColor;
            multiplierText.rectTransform.localScale =_textBaseScale;
            return;
        }

        multiplierText .DOColor(targetColor, 0.12f).SetUpdate(true);

        RectTransform textRect = multiplierText.rectTransform;

        textRect.DOKill();

        textRect.localScale = _textBaseScale;

        if(newMultiplier != _oldMultiplier)
        {
            textRect.DOPunchScale(Vector3.one *textPunchAmount,textPunchDuration,6,0.45f).SetUpdate(true);

            if (animate)
                PlayPunch();
        }

       

        _oldMultiplier = newMultiplier;
    }

    private Color GetMultiplierColor(float multiplier)
    {
        if (multiplier <= neutralMultiplier)
        {
            // remap lower
            float t = Mathf.InverseLerp(lowMultiplier,neutralMultiplier,multiplier);
            return Color.Lerp(lowColor,neutralColor,t);
        }

        // remap higher
        float highT = Mathf.InverseLerp(neutralMultiplier,highMultiplier,multiplier);
        return Color.Lerp(neutralColor,highColor,highT);
    }

    private void PlayPunch()
    {
        if (multiplierRect == null)
            return;

        multiplierRect.DOKill();

        multiplierRect.localScale = Vector3.one;
        multiplierRect.localRotation = Quaternion.identity;

        multiplierRect.DOPunchScale(
            Vector3.one * punchScale,
            punchDuration,
            vibrato,
            elasticity
        )
        .SetUpdate(true);

        multiplierRect.DOPunchRotation(
            new Vector3(0f, 0f, Random.value < 0.5 ? punchRotation : -punchRotation),
            punchDuration,
            vibrato,
            elasticity
        )
        .SetUpdate(true);
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


    private void OnDestroy()
    {
        flameRect?.DOKill();

        if (multiplierText != null)
        {
            multiplierText.DOKill();
            multiplierText.rectTransform.DOKill();
        }

        flameCanvasGroup?.DOKill();
    }
}