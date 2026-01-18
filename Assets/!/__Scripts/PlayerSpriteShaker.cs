using UnityEngine;
using DG.Tweening;

public class PlayerSpriteShaker : MonoBehaviour
{
    private Tween activeShakeTween;
    private Vector3 baseLocalPos;

    void Awake()
    {
        baseLocalPos = transform.localPosition;
    }

    public void Shake(float strength = 0.05f, float duration = 0.15f, int vibrato = 4)
    {
        // Reset if something was running
        if (activeShakeTween != null && activeShakeTween.IsActive())
        {
            activeShakeTween.Kill();
            transform.localPosition = baseLocalPos;
        }

        Sequence shakeSeq = DOTween.Sequence();

        for (int i = 0; i < vibrato; i++)
        {
            float t = i / (float)vibrato;
            float intensity = Mathf.Sin(t * Mathf.PI) * strength;

            Vector3 offset = transform.right * (intensity * (i % 2 == 0 ? 1 : -1));

            shakeSeq.Append(transform.DOLocalMove(baseLocalPos + offset, duration / vibrato / 2f)
                            .SetEase(Ease.OutSine));
            shakeSeq.Append(transform.DOLocalMove(baseLocalPos, duration / vibrato / 2f)
                            .SetEase(Ease.InSine));
        }

        shakeSeq.OnComplete(() =>
        {
            transform.localPosition = baseLocalPos;
            activeShakeTween = null;
        });

        activeShakeTween = shakeSeq;
    }
}
