using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

[System.Serializable]
public struct CanvasGroupGameStateFadeSetting
{
    public GameState state;
    public float fadeInDelay;
}

public class GameStateControlsCanvasGroupAdvanced : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Visible States")]
    [SerializeField] private List<CanvasGroupGameStateFadeSetting> showStates;

    [Header("Timing")]
    [SerializeField] private float fadeDuration = 0.35f;
    [SerializeField] private float defaultFadeInDelay = 0f;

    [Header("Behavior")]
    [SerializeField] private bool refreshFadeInWhenEnteringShowState = true;
    [SerializeField] private bool setInactiveOnFadeOut = false;

    private readonly Dictionary<GameState, float> fadeDelayLookup = new();
    private Tween fadeTween;

    private void Awake()
    {
        BuildLookup();

        if (canvasGroup != null)
            canvasGroup.alpha = 0f;
    }

    private void OnEnable()
    {
        GameStateManager.OnStateChanged += HandleStateChanged;
    }

    private void OnDisable()
    {
        GameStateManager.OnStateChanged -= HandleStateChanged;
        fadeTween?.Kill();
    }

    private void BuildLookup()
    {
        fadeDelayLookup.Clear();

        foreach (var setting in showStates)
        {
            fadeDelayLookup[setting.state] = setting.fadeInDelay;
        }
    }

    private void HandleStateChanged(GameState previousState, GameState newState)
    {
        bool shouldShow = fadeDelayLookup.ContainsKey(newState);

        if (shouldShow)
        {
            float delay = fadeDelayLookup.TryGetValue(newState, out float customDelay)
                ? customDelay
                : defaultFadeInDelay;

            FadeIn(delay);
        }
        else
        {
            FadeOut();
        }
    }

    private void FadeIn(float delay)
    {
        if (canvasGroup == null)
            return;

        if (!refreshFadeInWhenEnteringShowState && canvasGroup.alpha >= 1f)
            return;

        gameObject.SetActive(true);

        fadeTween?.Kill();

        if (refreshFadeInWhenEnteringShowState)
            canvasGroup.alpha = 0f;

        fadeTween = canvasGroup
            .DOFade(1f, fadeDuration)
            .SetDelay(delay);
    }

    private void FadeOut()
    {
        if (canvasGroup == null)
            return;

        fadeTween?.Kill();

        fadeTween = canvasGroup
            .DOFade(0f, fadeDuration)
            .OnComplete(() =>
            {
                if (setInactiveOnFadeOut)
                    gameObject.SetActive(false);
            });
    }
}