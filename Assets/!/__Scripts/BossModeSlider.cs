using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class BossModeSlider : MonoBehaviour, IPointerClickHandler
{
    public static event Action<bool> OnBossModeChanged;

    [Header("References")]
    [SerializeField] private RectTransform knobRect;
    [SerializeField] private TMP_Text stateText;

    [Header("Positions")]
    [SerializeField] private float offX = -30f;
    [SerializeField] private float onX = 30f;

    [Header("Animation")]
    [SerializeField] private float slideDuration = 0.18f;
    [SerializeField] private Ease slideEase = Ease.OutBack;

    [SerializeField] private float punchScale = 0.18f;
    [SerializeField] private float punchDuration = 0.18f;

    [SerializeField] private float rotationAmount = 12f;
    [SerializeField] private float rotationDuration = 0.15f;

    [Header("State")]
    [SerializeField] private bool bossModeEnabled = false;

    [Header("Audio")]
    [SerializeField] private SoundEffect toggleSound;

    private Tween _moveTween;
    private Tween _scaleTween;
    private Tween _rotationTween;

    public bool BossModeEnabled => bossModeEnabled;

    private void Awake()
    {
        RefreshInstant();
    }

    // ----------------------------------------------------
    // Mouse Input
    // ----------------------------------------------------

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        if (InputModeManager.Instance == null ||
            InputModeManager.Instance.CurrentMode != InputModeManager.InputMode.Mouse)
        {
            return;
        }

        Toggle();
    }

    // ----------------------------------------------------
    // Value
    // ----------------------------------------------------

    public void Toggle()
    {
        SetValue(!bossModeEnabled);
    }

    public void SetValue(bool enabled, bool animate = true)
    {
        if (bossModeEnabled == enabled)
            return;

        bossModeEnabled = enabled;

        if (animate)
            AnimateValue();
        else
            RefreshInstant();

        AudioHelpers.PlaySoundEffect(
            toggleSound,
            Camera.main.transform.position
        );

        OnBossModeChanged?.Invoke(bossModeEnabled);
    }

    public void AdjustValue(int direction)
    {
        if (direction > 0)
            SetValue(true);
        else if (direction < 0)
            SetValue(false);
    }

    // ----------------------------------------------------
    // Visuals
    // ----------------------------------------------------

    private void AnimateValue()
    {
        _moveTween?.Kill();
        _scaleTween?.Kill();
        _rotationTween?.Kill();

        float targetX = bossModeEnabled ? onX : offX;

        _moveTween = knobRect
            .DOAnchorPosX(targetX, slideDuration)
            .SetEase(slideEase)
            .SetUpdate(true);

        knobRect.localScale = Vector3.one;

        _scaleTween = knobRect
            .DOPunchScale(
                Vector3.one * punchScale,
                punchDuration,
                vibrato: 5,
                elasticity: 0.7f
            )
            .SetUpdate(true);

        float rotation = bossModeEnabled
            ? -rotationAmount
            : rotationAmount;

        knobRect.localRotation = Quaternion.identity;

        Sequence rotationSequence = DOTween.Sequence();
        rotationSequence.SetUpdate(true);

        rotationSequence.Append(
            knobRect.DORotate(
                new Vector3(0f, 0f, rotation),
                rotationDuration * 0.5f
            )
        );

        rotationSequence.Append(
            knobRect.DORotate(
                Vector3.zero,
                rotationDuration * 0.5f
            )
            .SetEase(Ease.OutBack)
        );

        _rotationTween = rotationSequence;

        RefreshText();
    }

    private void RefreshInstant()
    {
        if (knobRect != null)
        {
            Vector2 pos = knobRect.anchoredPosition;
            pos.x = bossModeEnabled ? onX : offX;

            knobRect.anchoredPosition = pos;
            knobRect.localScale = Vector3.one;
            knobRect.localRotation = Quaternion.identity;
        }

        RefreshText();
    }

    private void RefreshText()
    {
        if (stateText != null)
            stateText.text = bossModeEnabled ? "ON" : "OFF";
    }

    private void OnDestroy()
    {
        _moveTween?.Kill();
        _scaleTween?.Kill();
        _rotationTween?.Kill();
    }
}