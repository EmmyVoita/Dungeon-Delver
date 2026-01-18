using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(Collider2D), typeof(SpriteRenderer))]
public class SpikyBall : MonoBehaviour
{
    [Header("Visual Effects")]
    public ParticleSystem hitParticles;    // Assign in Inspector
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

        hasBeenHit = true;

        // 🔹 Disable Collider Immediately
        col.enabled = false;

        // 🔹 Fade transparency using DOTween
        sRend.DOFade(0f, fadeDuration)
            .SetEase(Ease.OutQuad);

        // 🔹 Play particle effect
        if (hitParticles != null)
        {
            ParticleSystem ps = Instantiate(
                hitParticles,
                transform.position,
                Quaternion.identity
            );
            ps.Play();
        }

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
        Destroy(gameObject, fadeDuration + 0.1f);
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

        // 🔹 Destroy after fade
        Destroy(gameObject, fadeDuration);
    }


}
