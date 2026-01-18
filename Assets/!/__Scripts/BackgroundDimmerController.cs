using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class BackgroundDimmerController : MonoBehaviour
{
    [SerializeField] private Image dimImage;
    [SerializeField] private float fadeDuration = 0.3f;
    [SerializeField] private float maxAlpha = 0.6f;

    private Tween currentTween;

    void Awake()
    {
        if (dimImage != null)
            dimImage.color = new Color(dimImage.color.r, dimImage.color.g, dimImage.color.b, 0f);

        gameObject.SetActive(false);
    }

    public void FadeIn()
    {
        currentTween?.Kill();

        gameObject.SetActive(true);
        dimImage.DOFade(maxAlpha, fadeDuration)
            .SetEase(Ease.OutSine);
    }

    public void FadeOut()
    {
        currentTween?.Kill();

        dimImage.DOFade(0f, fadeDuration)
            .SetEase(Ease.InSine)
            .OnComplete(() =>
            {
                gameObject.SetActive(false);
            });
    }
}
