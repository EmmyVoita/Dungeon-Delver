using DG.Tweening;
using UnityEngine;

public class CanvasGroupStateVisibilityController : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;

    private void OnEnable()
    {
        GameStateManager.OnStateChanged += HandleStateChanged;
    }

    private void OnDisable()
    {
        GameStateManager.OnStateChanged -= HandleStateChanged;
    }

    private void HandleStateChanged(GameState previousState, GameState currentState)
    {
        canvasGroup.DOKill();

        Debug.Log("GameStateEffectManager.ShowScoreUI: " + GameStateEffectManager.ShowScoreUI);

        if (GameStateEffectManager.ShowScoreUI)
        {
            canvasGroup.DOFade(1f, 0.35f);
        }
        else
        {
            Debug.Log("Do fade 0 from canvas group");
            canvasGroup.DOFade(0f, 0.35f);
        }
    }
}