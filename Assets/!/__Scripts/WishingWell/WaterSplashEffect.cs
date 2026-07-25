using DG.Tweening;
using UnityEngine;

public class WaterSplashEffect : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SpriteRenderer splashRenderer;
    [SerializeField] private SpriteRenderer rippleRenderer;
    [SerializeField] private ParticleSystem droplets;

    [Header("Splash")]
    [SerializeField] private float splashDuration = 0.28f;
    [SerializeField] private float splashStartScale = 0.25f;
    [SerializeField] private float splashPeakScale = 1f;

    [Header("Ripple")]
    [SerializeField] private float rippleDuration = 0.55f;
    [SerializeField] private Vector3 rippleStartScale =
        new Vector3(0.2f, 0.08f, 1f);

    [SerializeField] private Vector3 rippleEndScale =
        new Vector3(1.4f, 0.35f, 1f);

    public void Play()
    {
        if (droplets != null)
            droplets.Play();

        PlaySplash();
        PlayRipple();
    }

    private void PlaySplash()
    {
        if (splashRenderer == null)
            return;

        Transform splash = splashRenderer.transform;

        splash.localScale = Vector3.one * splashStartScale;
        splashRenderer.color =
            new Color(
                splashRenderer.color.r,
                splashRenderer.color.g,
                splashRenderer.color.b,
                1f
            );

        Sequence sequence = DOTween.Sequence();

        sequence.Append(
            splash.DOScale(splashPeakScale, splashDuration * 0.4f)
                .SetEase(Ease.OutBack)
        );

        sequence.Append(
            splash.DOScaleY(0f, splashDuration * 0.6f)
                .SetEase(Ease.InQuad)
        );

        sequence.Join(
            splashRenderer.DOFade(0f, splashDuration * 0.6f)
        );
    }

    private void PlayRipple()
    {
        if (rippleRenderer == null)
            return;

        Transform ripple = rippleRenderer.transform;

        ripple.localScale = rippleStartScale;
        rippleRenderer.color =
            new Color(
                rippleRenderer.color.r,
                rippleRenderer.color.g,
                rippleRenderer.color.b,
                1f
            );

        Sequence sequence = DOTween.Sequence();

        sequence.Join(
            ripple.DOScale(rippleEndScale, rippleDuration)
                .SetEase(Ease.OutQuad)
        );

        sequence.Join(
            rippleRenderer.DOFade(0f, rippleDuration)
                .SetEase(Ease.InQuad)
        );

        sequence.OnComplete(() =>
        {
            Destroy(gameObject);
        });
    }
}