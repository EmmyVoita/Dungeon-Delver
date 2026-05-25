using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class CanvasGroupFadeOnMenuState : MonoBehaviour
{
    [SerializeField] private MenuState activeState;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float fadeInDuration = 0.1f;
    [SerializeField] private float fadeOutDuration = 0.1f;

    private void OnEnable()
    {
        MenuManager.OnMenuOpened += HandleMenuOpened;
    }

    private void OnDisable()
    {
        MenuManager.OnMenuOpened -= HandleMenuOpened;
    }

    private void HandleMenuOpened(MenuState newState)
    {
        if(newState == activeState)
        {
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.DOFade(1,fadeInDuration);
        }
        else
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.DOFade(0,fadeOutDuration);
        }
    }
}