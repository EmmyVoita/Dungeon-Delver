using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class RouletteManager : MonoBehaviour
{
    public static RouletteManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private GameObject rouletteRoot;
    [SerializeField] private RectTransform rouletteRect;
    [SerializeField] private Roulette roulette;

    [Header("Entrance Animation")]
    [SerializeField] private float offscreenDistance = 1200f;
    [SerializeField] private float enterDuration = 0.6f;
    [SerializeField] private Ease enterEase = Ease.OutBack;

    [Header("Exit Animation")]
    [SerializeField] private float rewardHoldDuration = 0.6f;
    [SerializeField] private float exitDuration = 0.45f;
    [SerializeField] private Ease exitEase = Ease.InBack;

    private bool _isRouletteActive;
    private Vector2 _onscreenPosition;
    private Tween _movementTween;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (rouletteRect == null && rouletteRoot != null)
            rouletteRect = rouletteRoot.GetComponent<RectTransform>();

        if (rouletteRect != null)
            _onscreenPosition = rouletteRect.anchoredPosition;

        rouletteRoot.SetActive(false);
    }

    private void OnDestroy()
    {
        _movementTween?.Kill();

        if (Instance == this)
            Instance = null;
    }

    public void OpenRoulette(IReadOnlyList<RewardDefinition> rewards)
    {
        if (_isRouletteActive)
            return;

        if (rewards == null || rewards.Count == 0)
        {
            Debug.LogWarning("Cannot open roulette without rewards.");
            return;
        }

        if (rouletteRoot == null || rouletteRect == null || roulette == null)
        {
            Debug.LogError("RouletteManager is missing one or more references.");
            return;
        }

        _isRouletteActive = true;
        InputFocusManager.Claim(this);

        _movementTween?.Kill();

        rouletteRoot.SetActive(true);
        roulette.Initialize(rewards, HandleRewardSelected);

        Vector2 offscreenPosition =
            _onscreenPosition + Vector2.down * offscreenDistance;

        rouletteRect.anchoredPosition = offscreenPosition;

        _movementTween = rouletteRect
            .DOAnchorPos(_onscreenPosition, enterDuration)
            .SetEase(enterEase)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                _movementTween = null;

                if (_isRouletteActive)
                    roulette.Rotate();
            });
    }

    private void HandleRewardSelected(RewardDefinition reward)
    {
        if (!_isRouletteActive)
            return;

        reward?.Apply();

        CloseRoulette(reward.WinSound.clip.length * (1f / reward.WinSound.pitch));
    }

    private void CloseRoulette(float clipDuration)
    {
        if (!_isRouletteActive)
            return;

        _movementTween?.Kill();

        Vector2 offscreenPosition =
            _onscreenPosition + Vector2.down * offscreenDistance;

        Sequence closeSequence = DOTween.Sequence()
            .SetUpdate(true)
            .AppendInterval(Mathf.Max(0f,rewardHoldDuration + clipDuration))
            .Append(
                rouletteRect
                    .DOAnchorPos(offscreenPosition, exitDuration)
                    .SetEase(exitEase)
            )
            .OnComplete(FinishClosing);

        _movementTween = closeSequence;
    }

    private void FinishClosing()
    {
        _movementTween = null;
        _isRouletteActive = false;

        rouletteRoot.SetActive(false);

        // Reset it so it is ready for the next time it opens.
        rouletteRect.anchoredPosition = _onscreenPosition;

        InputFocusManager.Release(this);
    }
}