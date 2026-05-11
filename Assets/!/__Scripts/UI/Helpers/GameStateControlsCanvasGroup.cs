using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;

public class GameStateControlsCanvasGroup : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Timing")]
    [SerializeField] private float fadeDuration = 0.35f;

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
        if(GameStateEffectManager.ShowScoreUI)
            FadeIn();
        else
            FadeOut();
    }

    private void FadeIn()
    {
        canvasGroup.DOFade(1f, 0.35f);
    }

    private void FadeOut()
    {
        canvasGroup.DOFade(0f, 0.35f);
    }
}