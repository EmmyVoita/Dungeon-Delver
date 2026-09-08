using System;
using DG.Tweening;
using UnityEngine;

public class GameOverPagesController : MonoBehaviour
{
    public enum GameOverPage
    {
        Inventory,
        Results,
    }

    [Header("References")]
    [SerializeField] private RectTransform pagesRoot;
    [SerializeField] private RectTransform viewport;

    [Header("Pages")]
    [SerializeField] private RectTransform inventoryPage;
    [SerializeField] private RectTransform resultsPage;

    [Header("Transition")]
    [SerializeField] private float transitionDuration = 0.35f;
    [SerializeField] private Ease transitionEase = Ease.OutCubic;

    [Header("Starting Page")]
    [SerializeField] private GameOverPage startingPage = GameOverPage.Results;

    private Tween _pageTween;
    private  GameOverPage _currentPage;
    private bool _transitioning;

    public  GameOverPage CurrentPage => _currentPage;
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
        
        resultsPage.anchoredPosition = Vector2.zero;
        inventoryPage.anchoredPosition = new Vector2(-pageWidth, 0f);
    }

    public void ShowInventory()
    {
        ShowPage(GameOverPage.Inventory);
    }

    public void ShowResults()
    {
        ShowPage(GameOverPage.Results);
    }

    public void ShowPage(GameOverPage page, Action onComplete = null)
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

    private void SetPageInstant(GameOverPage page)
    {
        _pageTween?.Kill();

        Vector2 position = pagesRoot.anchoredPosition;
        position.x = GetRootPositionForPage(page);

        pagesRoot.anchoredPosition = position;

        _currentPage = page;
        _transitioning = false;
    }

    private float GetRootPositionForPage(GameOverPage page)
    {
        float pageWidth = viewport.rect.width;

        switch (page)
        {
            case GameOverPage.Results:
                return 0f;

            case GameOverPage.Inventory:
                return pageWidth;

            default:
                return 0f;
        }
    }
}