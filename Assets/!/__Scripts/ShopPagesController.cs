using System;
using DG.Tweening;
using UnityEngine;

public class ShopPagesController : MonoBehaviour
{
    public enum ShopPage
    {
        Inventory,
        Shop,
        DrawPool
    }

    [Header("References")]
    [SerializeField] private RectTransform pagesRoot;
    [SerializeField] private RectTransform viewport;

    [Header("Pages")]
    [SerializeField] private RectTransform inventoryPage;
    [SerializeField] private RectTransform shopPage;
    [SerializeField] private RectTransform drawPoolPage;

    [Header("Transition")]
    [SerializeField] private float transitionDuration = 0.35f;
    [SerializeField] private Ease transitionEase = Ease.OutCubic;

    [Header("Starting Page")]
    [SerializeField] private ShopPage startingPage = ShopPage.Shop;

    private Tween _pageTween;
    private ShopPage _currentPage;
    private bool _transitioning;

    public ShopPage CurrentPage => _currentPage;
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
        
        drawPoolPage.anchoredPosition = new Vector2(pageWidth, 0f);
        shopPage.anchoredPosition = Vector2.zero;
        inventoryPage.anchoredPosition = new Vector2(-pageWidth, 0f);
    }

    public void ShowInventory()
    {
        ShowPage(ShopPage.Inventory);
    }

    public void ShowPage()
    {
        ShowPage(ShopPage.Shop);
    }

    public void ShowDrawPool()
    {
        ShowPage(ShopPage.DrawPool);
    }

    public void ShowPage(ShopPage page, Action onComplete = null)
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

    private void SetPageInstant(ShopPage page)
    {
        _pageTween?.Kill();

        Vector2 position = pagesRoot.anchoredPosition;
        position.x = GetRootPositionForPage(page);

        pagesRoot.anchoredPosition = position;

        _currentPage = page;
        _transitioning = false;
    }

    private float GetRootPositionForPage(ShopPage page)
    {
        float pageWidth = viewport.rect.width;

        switch (page)
        {
            case ShopPage.DrawPool:
                return -pageWidth;

            case ShopPage.Shop:
                return 0f;

            case ShopPage.Inventory:
                return pageWidth;

            default:
                return 0f;
        }
    }
}