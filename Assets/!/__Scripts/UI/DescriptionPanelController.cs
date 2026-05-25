using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

public class DescriptionPanelController : MonoBehaviour
{
    [SerializeField] private List<GameState> hideStates;
    [SerializeField] private RectTransform rect;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TextTypewriter typewriter;

    [Header("Animation")]
    [SerializeField] private Ease easeCurve = Ease.OutBack;
    [SerializeField] private float hiddenY = -300f;
    [SerializeField] private float shownY = 0f;
    [SerializeField] private float duration = 0.4f;

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
        if(hideStates.Contains(newState))
        {
            Hide();
        }
    }

    private void Awake()
    {
        Hide();
    }

    public void Show(string text)
    {
        rect.DOKill();
        canvasGroup.DOKill();

        canvasGroup.alpha = 1;
        rect.DOAnchorPosY(shownY, duration)
            .SetEase(easeCurve);

        // start typing AFTER movement (feels better)
        typewriter.StartTyping(text);
    }

    public void Hide()
    {
        rect.DOKill();
        canvasGroup.DOKill();

        rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, hiddenY);
        canvasGroup.alpha = 0;
    }
}