using UnityEngine;
using DG.Tweening;

public class LaserBeamHeightPulse : MonoBehaviour
{
    [Header("Height Settings")]
    [SerializeField] private float maxHeightScale = 1f;
    [SerializeField] private float growDuration = 0.12f;
    [SerializeField] private float shrinkDuration = 0.15f;

    [Header("Wobble Settings")]
    [SerializeField] private float wobbleAmount = 0.12f;
    [SerializeField] private float wobbleSpeed = 8f;

    [SerializeField] private Vector3 baseScale;
    private Tween wobbleTween;
    private Sequence pulseSequence;

    void Awake()
    {
        baseScale = transform.localScale;

        // Start hidden (height = 0)
        transform.localScale = new Vector3(baseScale.x, 0.1f, baseScale.z);
    }

    public void Play(float activeDuration)
    {
        
        pulseSequence?.Kill();
        wobbleTween?.Kill();

        pulseSequence = DOTween.Sequence();

        // 1) Grow in
        pulseSequence.Append(
            transform.DOScaleY(
                maxHeightScale,
                growDuration
            ).SetEase(Ease.OutQuad)
        );

        // 2) Start wobble
        pulseSequence.AppendCallback(() =>
        {
            wobbleTween = DOTween.To(
                () => transform.localScale.y,
                y =>
                {
                    transform.localScale = new Vector3(baseScale.x, y, baseScale.z);
                },
                maxHeightScale + wobbleAmount,
                1f / wobbleSpeed
            )
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
        });

        // 3) Stay active
        pulseSequence.AppendInterval(activeDuration);

        // 4) Stop wobble + shrink out
        pulseSequence.AppendCallback(() =>
        {
            wobbleTween?.Kill();
        });

        pulseSequence.Append(
            transform.DOScaleY(
                0f, 
                shrinkDuration
            ).SetEase(Ease.InQuad)
        );
        
    }

    void OnDisable()
    {
        pulseSequence?.Kill();
        wobbleTween?.Kill();
    }
}
