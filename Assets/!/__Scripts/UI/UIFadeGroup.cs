using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(CanvasGroup))]
public class UIFadeGroup : MonoBehaviour
{
    [SerializeField] private float fadeInDuration = 0.25f;
    [SerializeField] private float fadeOutDuration = 0.25f;

    private CanvasGroup canvasGroup;
    private Tween fadeTween;

    private void Awake()
    {
        EnsureCanvasGroup();
    }

    private void EnsureCanvasGroup()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();
    }


    public void Show(bool instant = false)
    {
        EnsureCanvasGroup();

        fadeTween?.Kill();

        gameObject.SetActive(true);

        if (instant)
        {
            canvasGroup.alpha = 1f;
            return;
        }

        canvasGroup.alpha = 0f;
        fadeTween = canvasGroup
            .DOFade(1f, fadeInDuration)
            .SetEase(Ease.OutSine);
    }

    public void Hide(bool instant = false)
    {
        EnsureCanvasGroup();
        
        fadeTween?.Kill();

        if (instant)
        {
            canvasGroup.alpha = 0f;
            gameObject.SetActive(false);
            return;
        }

        fadeTween = canvasGroup
            .DOFade(0f, fadeOutDuration)
            .SetEase(Ease.InSine)
            .OnComplete(() => gameObject.SetActive(false));
    }
}
