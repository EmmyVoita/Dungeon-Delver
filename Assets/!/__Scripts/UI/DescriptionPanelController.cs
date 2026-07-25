using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
using UnityEngine.UI;

public class DescriptionPanelController : MonoBehaviour
{
    [SerializeField] private List<GameState> hideStates;
    [SerializeField] private RectTransform rect;
    [SerializeField] private RectTransform layoutRoot;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TextTypewriter typewriter;
    [SerializeField] private TextTypewriter detailsTypewriter;

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

    private void RebuildLayout()
    {
        typewriter.textComponent.ForceMeshUpdate();

        LayoutRebuilder.ForceRebuildLayoutImmediate(
            typewriter.textComponent.rectTransform
        );

        LayoutRebuilder.ForceRebuildLayoutImmediate(
            layoutRoot
        );
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

        RebuildLayout();
    }

    public void ShowImmediate(string text)
    {
        /*
        typewriter.textComponent.ForceMeshUpdate();

        LayoutRebuilder.ForceRebuildLayoutImmediate(
            typewriter.textComponent.rectTransform
        );

        LayoutRebuilder.ForceRebuildLayoutImmediate(
            layoutRoot
        );
        */

        rect.DOKill();
        canvasGroup.DOKill();

        canvasGroup.alpha = 1;
        rect.DOAnchorPosY(shownY, duration)
            .SetEase(easeCurve);

        // start typing AFTER movement (feels better)
        typewriter.SetInstant(text);

        RebuildLayout();
    }

    public void ShowDetails(string text)
    {
        rect.DOKill();
        canvasGroup.DOKill();

        canvasGroup.alpha = 1;
        rect.DOAnchorPosY(shownY, duration)
            .SetEase(easeCurve);

        // start typing AFTER movement (feels better)
        detailsTypewriter.StartTyping(text);
    }

    public void ShowDetailsImmediate(string text)
    {
        rect.DOKill();
        canvasGroup.DOKill();

        canvasGroup.alpha = 1;
        rect.DOAnchorPosY(shownY, duration)
            .SetEase(easeCurve);

        // start typing AFTER movement (feels better)
        detailsTypewriter.SetInstant(text);
    }



    public void Hide()
    {
        rect.DOKill();
        canvasGroup.DOKill();

        rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, hiddenY);
        canvasGroup.alpha = 0;
    }
}