using UnityEngine;
using DG.Tweening;

public class ShockwaveEffect : MonoBehaviour
{
    public float expandDuration = 0.4f;
    public float fadeDuration = 0.2f;
    public SpriteRenderer spriteRenderer;
    public AudioClip shockwaveSound;

    public void Initialize(float targetRadius)
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        transform.localScale = Vector3.zero;
        Color c = spriteRenderer.color;
        c.a = 1f;
        spriteRenderer.color = c;

        // play optional sound
        if (shockwaveSound != null)
            AudioHelpers.PlayMyClipAtPoint(shockwaveSound, AudioChannel.SFX, transform.position);

        // Animate outward and fade
        Sequence seq = DOTween.Sequence();
        seq.Append(transform.DOScale(Vector3.one * targetRadius, expandDuration).SetEase(Ease.OutQuad));
        seq.Join(spriteRenderer.DOFade(0f, fadeDuration).SetEase(Ease.InSine).SetDelay(expandDuration * 0.8f));
        seq.OnComplete(() => Destroy(gameObject));
    }

    void OnTriggerEnter2D(Collider2D coll)
    {
        ArrowBase arrow = coll.GetComponent<ArrowBase>();
        if (arrow != null)
        {
            arrow.OnArrowHit();
        }
    }
}
