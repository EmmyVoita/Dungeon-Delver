using System;
using DG.Tweening;
using UnityEngine;

public class PlayPagesController : MonoBehaviour
{
    public enum PlayPage
    {
        AbilitySelect,
        ChallengeSelect
    }

    [Header("References")]
    [SerializeField] private RectTransform pagesRoot;
    [SerializeField] private RectTransform viewport;

    [Header("Pages")]
    [SerializeField] private RectTransform abilitySelectPage;
    [SerializeField] private RectTransform challengeSelectPage;

    [Header("Transition")]
    [SerializeField] private float transitionDuration = 0.35f;
    [SerializeField] private Ease transitionEase = Ease.OutCubic;

    [Header("Starting Page")]
    [SerializeField] private PlayPage startingPage = PlayPage.AbilitySelect;

    private Tween _pageTween;
    private PlayPage _currentPage;
    private bool _transitioning;

    public PlayPage CurrentPage => _currentPage;
    public bool IsTransitioning => _transitioning;

    private void Start()
    {
        SetupPages();

        _currentPage = startingPage;

        SetPageInstant(startingPage);
    }

    private void OnDestroy()
    {
        _pageTween?.Kill();
    }

    private void SetupPages()
    {
        float pageWidth = viewport.rect.width;

        abilitySelectPage.anchoredPosition = Vector2.zero;
        challengeSelectPage.anchoredPosition = new Vector2(pageWidth, 0f);
    }

    public void ShowAbilitySelect()
    {
        ShowPage(PlayPage.AbilitySelect);
    }

    public void ShowChallengeSelect()
    {
        ShowPage(PlayPage.ChallengeSelect);
    }


    public void ShowPage(PlayPage page, Action onComplete = null)
    {
        if (_transitioning)
            return;

        if (page == _currentPage)
        {
            onComplete?.Invoke();
            return;
        }

        float targetX = GetRootPositionForPage(page);

        _transitioning = true;
        _currentPage = page;

        _pageTween?.Kill();

        _pageTween = pagesRoot
            .DOAnchorPosX(targetX, transitionDuration)
            .SetEase(transitionEase)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                _transitioning = false;
                _pageTween = null;
                onComplete?.Invoke();
            });
    }

    private void SetPageInstant(PlayPage page)
    {
        _pageTween?.Kill();

        Vector2 position = pagesRoot.anchoredPosition;
        position.x = GetRootPositionForPage(page);

        pagesRoot.anchoredPosition = position;

        _currentPage = page;
        _transitioning = false;
    }

    private float GetRootPositionForPage(PlayPage page)
    {
        float pageWidth = viewport.rect.width;

        switch (page)
        {
            case PlayPage.AbilitySelect:
                return 0f;

            case PlayPage.ChallengeSelect:
                return -pageWidth;

            default:
                return 0f;
        }
    }
}