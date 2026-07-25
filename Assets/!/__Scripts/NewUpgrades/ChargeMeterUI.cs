using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChargeMeterUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform parentRect;
    [SerializeField] private GameObject iconPrefab;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text chargeText;
    [SerializeField] private Color emptyColor = Color.grey;
    [SerializeField] private Color filledColor = Color.white;

    [Header("Fill Animation")]
    [SerializeField] private float individualDelay = 0.08f;
    [SerializeField] private float punchScaleUp = 0.2f;
    [SerializeField] private float punchDuration = 0.35f;
    [SerializeField] private int punchVibrato = 5;
    [SerializeField] private float elasticity = 0.5f;

    [Header("Full Hold")]
    [SerializeField] private float holdFullDuration = 0.15f;

    private readonly List<Image> _icons = new();

    private Coroutine _fillRoutine;
    private int _maxCharge;
    private bool _isPlayingFullAnimation;

    public bool IsPlayingFullAnimation => _isPlayingFullAnimation;

    public event Action OnFullAnimationCompleted;

    public void Initialize(
        string displayName,
        Sprite icon,
        int maxCharge)
    {
        _maxCharge = Mathf.Max(1, maxCharge);

        if (titleText != null)
            titleText.text = displayName;

        ClearExistingIcons();

        for (int i = 0; i < _maxCharge; i++)
        {
            GameObject newIcon = Instantiate(
                iconPrefab,
                parentRect
            );

            Image iconImage = newIcon.GetComponentInChildren<Image>();

            if (iconImage == null)
            {
                Destroy(newIcon);
                continue;
            }

            if (icon != null)
                iconImage.sprite = icon;

            iconImage.color = emptyColor;
            _icons.Add(iconImage);
        }

        UpdateChargeText(0);
    }

    public void SetCharge(int current, int maximum)
    {
        if (_isPlayingFullAnimation)
            return;

        _maxCharge = Mathf.Max(1, maximum);

        int clampedCharge = Mathf.Clamp(
            current,
            0,
            _maxCharge
        );

        SetIconsFilled(clampedCharge);
        UpdateChargeText(clampedCharge);

        if (clampedCharge >= _maxCharge)
            PlayFullAnimation();
    }

    private void PlayFullAnimation()
    {
        if (_isPlayingFullAnimation)
            return;

        if (_fillRoutine != null)
            StopCoroutine(_fillRoutine);

        _fillRoutine = StartCoroutine(FullAnimationRoutine());
    }

    private IEnumerator FullAnimationRoutine()
    {
        _isPlayingFullAnimation = true;

        // Keep the meter visually full during the celebration.
        SetIconsFilled(_maxCharge);
        UpdateChargeText(_maxCharge);

        for (int i = 0; i < _icons.Count; i++)
        {
            Image icon = _icons[i];

            if (icon == null)
                continue;

            RectTransform rect = icon.rectTransform;

            rect.DOKill();
            rect.localScale = Vector3.one;

            rect.DOPunchScale(
                    Vector3.one * punchScaleUp,
                    punchDuration,
                    punchVibrato,
                    elasticity
                )
                .SetUpdate(true);

            yield return new WaitForSecondsRealtime(
                individualDelay
            );
        }

        yield return new WaitForSecondsRealtime(
            holdFullDuration
        );

        // The logical charge was already consumed.
        // This catches the visuals up to the real value.
        SetIconsFilled(0);
        UpdateChargeText(0);

        _isPlayingFullAnimation = false;
        _fillRoutine = null;

        OnFullAnimationCompleted?.Invoke();
    }

    private void SetIconsFilled(int current)
    {
        for (int i = 0; i < _icons.Count; i++)
        {
            if (_icons[i] == null)
                continue;

            _icons[i].color =
                i < current
                    ? filledColor
                    : emptyColor;
        }
    }

    private void UpdateChargeText(int current)
    {
        if (chargeText != null)
            chargeText.text = $"{current}/{_maxCharge}";
    }

    private void ClearExistingIcons()
    {
        foreach (Image icon in _icons)
        {
            if (icon != null)
                Destroy(icon.gameObject);
        }

        _icons.Clear();
    }

    private void OnDisable()
    {
        if (_fillRoutine != null)
        {
            StopCoroutine(_fillRoutine);
            _fillRoutine = null;
        }

        foreach (Image icon in _icons)
        {
            if (icon == null)
                continue;

            icon.rectTransform.DOKill();
            icon.rectTransform.localScale = Vector3.one;
        }

        _isPlayingFullAnimation = false;
    }
}