using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class ScoreUIVisibilityController : MonoBehaviour
{
    [SerializeField] private ScoreDisplayView scoreDisplay;
    [SerializeField] private CanvasGroup scoreCanvasGroup;
    [SerializeField] private List<GameState> overrideShowStates;

    private void OnEnable()
    {
        GameStateManager.OnStateChanged +=  HandleStateChanged;
    }

    private void OnDisable()
    {
        GameStateManager.OnStateChanged -=  HandleStateChanged;
    }

    private void HandleStateChanged(GameState previousState, GameState currentState)
    {
        if (!scoreDisplay)
        {
            scoreDisplay = GetComponent<ScoreDisplayView>();
        }

        // Kill any existing tween to prevent stacking
        scoreCanvasGroup.DOKill();

        if (GameStateEffectManager.ShowScoreUI || overrideShowStates.Contains(currentState))
        {
            scoreDisplay.enabled = true;
            scoreCanvasGroup.gameObject.SetActive(true);

            scoreCanvasGroup.DOFade(1f, 0.35f)
                .SetEase(Ease.OutQuad);
        }
        else
        {
            scoreCanvasGroup.DOFade(0f, 0.35f)
                .SetEase(Ease.InQuad)
                .OnComplete(() =>
                {
                    //scoreDisplay.enabled = false;
                    scoreCanvasGroup.gameObject.SetActive(false);
                });
        }
    }
}