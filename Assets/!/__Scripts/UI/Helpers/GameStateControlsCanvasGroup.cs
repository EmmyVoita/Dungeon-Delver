using System.Collections.Generic;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;

public class GameStateControlsCanvasGroup : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private List<GameState> showStates;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Timing")]
    [SerializeField] private float fadeDuration = 0.35f;
    [SerializeField] private float fadeInDelay = 0.0f;

    private void OnEnable()
    {
        GameStateManager.OnStateChanged += HandleStateChanged;
    }

    private void OnDisable()
    {
        GameStateManager.OnStateChanged -= HandleStateChanged;
    }

    private void HandleStateChanged(GameState previousState, GameState newState)
    {
        if(showStates.Contains(newState))
            FadeIn();
        else
            FadeOut();
    }

    private void FadeIn()
    {
        canvasGroup.DOKill();

        canvasGroup.DOFade(1f, fadeDuration)
            .SetDelay(fadeInDelay);
    }

    private void FadeOut()
    {
        canvasGroup.DOKill();

        canvasGroup.DOFade(0f, fadeDuration);
    }
}