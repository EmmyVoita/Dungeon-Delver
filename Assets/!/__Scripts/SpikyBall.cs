using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(Collider2D), typeof(SpriteRenderer))]
public class SpikyBall : MonoBehaviour
{
    [Header("Visual Effects")]
    public GameObject destroyEffectPrefab;
    public float fadeDuration = 0.2f;      // Quick fade time

    [Header("Audio")]
    public AudioClip hitSound;             // Optional
    public float soundPitch = 1f;

    private SpriteRenderer sRend;
    private Collider2D col;
    private bool hasBeenHit = false;



    void Awake()
    {
        sRend = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (hasBeenHit) return; // Prevent multiple triggers
        if (!other.CompareTag("Player")) return;

        // 🔹 Play sound (optional, if assigned)
        if (hitSound != null)
        {
            AudioHelpers.PlayClipWithVariation(
                hitSound,
                AudioChannel.SFX,
                Camera.main.transform.position,
                basePitch: soundPitch, pitchRange: 0.1f
            );
        }

        // 🔹 🔥 NOTIFY RING THAT IT FAILED!
        var ring = GetComponentInParent<ShrinkingRingObstacle>();
        if (ring != null)
            ring.OnPlayerHitRing();    // <- new function we will add!
        
        // 🔹 Destroy after fade & particle delay
        FadeOut();
    }

    
    public void FadeOut()
    {
        if (hasBeenHit) return; // Prevent multiple triggers
        hasBeenHit = true;

        // 🔹 Disable Collider Immediately
        col.enabled = false;

        // 🔹 Fade transparency using DOTween
        sRend.DOFade(0f, fadeDuration)
            .SetEase(Ease.OutQuad);

        if(destroyEffectPrefab != null)
        {
            Instantiate(destroyEffectPrefab, transform.position, transform.rotation);
        }

        // 🔹 Destroy after fade
        Destroy(gameObject, fadeDuration);
    }


}
