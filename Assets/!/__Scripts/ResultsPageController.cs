using System;
using DG.Tweening;
using UnityEngine;

public class ResultsPageController : MonoBehaviour
{
    public enum ResultsPage
    {
        Summary,
        Leaderboard,
        Inventory
    }

    [Header("References")]
    [SerializeField] private RectTransform pagesRoot;
    [SerializeField] private RectTransform viewport;

    [Header("Pages")]
    [SerializeField] private RectTransform summaryPage;
    [SerializeField] private RectTransform leaderboardPage;
    [SerializeField] private RectTransform inventoryPage;

    [Header("Transition")]
    [SerializeField] private float transitionDuration = 0.35f;
    [SerializeField] private Ease transitionEase = Ease.OutCubic;

    [Header("Starting Page")]
    [SerializeField] private ResultsPage startingPage = ResultsPage.Leaderboard;

    private Tween _pageTween;
    private ResultsPage _currentPage;
    private bool _transitioning;

    public ResultsPage CurrentPage => _currentPage;
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

        summaryPage.anchoredPosition = new Vector2(pageWidth, 0f);
        leaderboardPage.anchoredPosition = Vector2.zero;
        inventoryPage.anchoredPosition = new Vector2(-pageWidth, 0f);
    }

    public void ShowSummary()
    {
        ShowPage(ResultsPage.Summary);
    }

    public void ShowLeaderboard()
    {
        ShowPage(ResultsPage.Leaderboard);
    }

    public void ShowInventory()
    {
        ShowPage(ResultsPage.Inventory);
    }

    public void ShowPage(ResultsPage page, Action onComplete = null)
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

    private void SetPageInstant(ResultsPage page)
    {
        _pageTween?.Kill();

        Vector2 position = pagesRoot.anchoredPosition;
        position.x = GetRootPositionForPage(page);

        pagesRoot.anchoredPosition = position;

        _currentPage = page;
        _transitioning = false;
    }

    private float GetRootPositionForPage(ResultsPage page)
    {
        float pageWidth = viewport.rect.width;

        switch (page)
        {
            case ResultsPage.Summary:
                return -pageWidth;

            case ResultsPage.Leaderboard:
                return 0f;

            case ResultsPage.Inventory:
                return pageWidth;

            default:
                return 0f;
        }
    }
}