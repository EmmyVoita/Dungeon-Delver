using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class PagesController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform pagesRoot;
    [SerializeField] private RectTransform viewport;

    [Header("Pages")]
    [SerializeField] private List<RectTransform> pages;

    [Header("Transition")]
    [SerializeField] private float transitionDuration = 0.35f;
    [SerializeField] private Ease transitionEase = Ease.OutCubic;

    [Header("Starting Page")]
    [SerializeField] private int startingPage;

    private Tween _pageTween;
    private int _currentPage;
    private bool _transitioning;

    public int CurrentPage => _currentPage;
    public int PageCount => pages.Count;
    public bool IsTransitioning => _transitioning;

    private float _pageWidth;


    public void Initialize()
    {
        Canvas.ForceUpdateCanvases();

        SetupPages();

        _currentPage = Mathf.Clamp(startingPage, 0, pages.Count - 1);
        SetPageInstant(_currentPage);
    }


    private void Start()
    {
        Canvas.ForceUpdateCanvases();

        SetupPages();

        _currentPage = Mathf.Clamp(startingPage, 0, pages.Count - 1);
        SetPageInstant(_currentPage);
    }

    private void OnDestroy()
    {
        _pageTween?.Kill();
    }

    private void SetupPages()
    {
        _pageWidth = viewport.rect.width;

        for (int i = 0; i < pages.Count; i++)
        {
            pages[i].anchoredPosition = new Vector2(
                i * _pageWidth,
                0f
            );
        }
    }

    public void ShowPage(int pageIndex, Action onComplete = null)
    {
        if (_transitioning)
            return;

        if (!IsValidPage(pageIndex))
            return;

        if (pageIndex == _currentPage)
        {
            onComplete?.Invoke();
            return;
        }

        _transitioning = true;
        _currentPage = pageIndex;

        _pageTween?.Kill();

        _pageTween = pagesRoot
            .DOAnchorPosX(GetRootPosition(pageIndex), transitionDuration)
            .SetEase(transitionEase)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                _transitioning = false;
                _pageTween = null;

                onComplete?.Invoke();
            });
    }

    public void ShowNextPage(Action onComplete = null)
    {
        ShowPage(_currentPage + 1, onComplete);
    }

    public void ShowPreviousPage(Action onComplete = null)
    {
        ShowPage(_currentPage - 1, onComplete);
    }

    public void SetPageInstant(int pageIndex)
    {
        if (!IsValidPage(pageIndex))
            return;

        _pageTween?.Kill();

        Vector2 position = pagesRoot.anchoredPosition;
        position.x = GetRootPosition(pageIndex);

        pagesRoot.anchoredPosition = position;

        _currentPage = pageIndex;
        _transitioning = false;
    }

    public bool HasNextPage()
    {
        return _currentPage < pages.Count - 1;
    }

    public bool HasPreviousPage()
    {
        return _currentPage > 0;
    }

    private float GetRootPosition(int pageIndex)
    {
        return -(pageIndex * _pageWidth);
    }

    private bool IsValidPage(int pageIndex)
    {
        return pageIndex >= 0 && pageIndex < pages.Count;
    }
}