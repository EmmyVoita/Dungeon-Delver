using UnityEngine;
using DG.Tweening;
using System.Collections;

[RequireComponent(typeof(SpriteRenderer))]
public class BeltBoostButton : MonoBehaviour
{
    public FoodAssemblyChallenge parentChallenge;
    [Header("Button Sprites")]
    public Sprite upSprite;         // default unpressed sprite
    public Sprite downSprite;       // sprite while pressed

    [Header("Boost Settings")]
    public float boostAmount = 2f;      // speed multiplier
    public float boostDuration = 1f;    // seconds the boost lasts
    public AudioClip hitSound;

    [Header("Belts Affected")]
    public ConveyorBelt[] belts;


    [Header("Animation Settings")]
    public float pressDepth = 0.4f;
    public float pressScale = 0.9f;
    public float pressTime = 0.07f;
    public float bounceTime = 0.12f;


    [Header("Death Animation Settings")]
    public GameObject deathEffect;
    public float popScale = 1.25f;      // how big it gets
    public float reboundScale = 0.85f;  // after rubberband
    public float popTime = 0.15f;
    public float reboundTime = 0.12f;
    public float fadeTime = 0.15f;

    private SpriteRenderer sr;
    private bool coolingDown = false;
    private bool colliderActive = true;

    void Awake()
    {
        colliderActive = false;
        sr = GetComponent<SpriteRenderer>();
        sr.sprite = upSprite;
        sr.color = new Color(1f, 1f, 1f, 0f);


    }

    public void FadeOutSprite(float fadeTime = 0.3f)
    {
        colliderActive = false;
        sr.DOKill(); // stop any belt animation tweens
        sr.DOColor(new Color(sr.color.r, sr.color.g, sr.color.b, 0f), fadeTime)
        .SetEase(Ease.OutSine);
    }

    public void FadeInSprite(float duration = 0.5f)
    {
        Color c = sr.color;
        sr.color = new Color(c.r, c.g, c.b, 0f);
        sr.DOFade(1f, duration);

        colliderActive = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (coolingDown || !colliderActive) return;

        coolingDown = true;

        // SFX
        if (hitSound)
            AudioHelpers.PlayMyClipAtPoint(hitSound, AudioChannel.SFX, transform.position);

        // Visual update
        sr.sprite = downSprite;
        PlayPressAnimation();

        // Apply boost to all belts
        foreach (var belt in parentChallenge.ConveyorBelts)
            belt.TemporaryBoost(boostAmount, boostDuration);

        // Start cooldown (sprite returns to normal after)
        StartCoroutine(CooldownRoutine());
    }

    IEnumerator CooldownRoutine()
    {
        // Wait until the boost is over
        yield return new WaitForSeconds(boostDuration);

        // Return sprite
        sr.sprite = upSprite;

        // Small bounce on release for extra juice (optional)
        PlayReleaseBounce();

        // A small buffer prevents immediate re-trigger
        yield return new WaitForSeconds(0.15f);

        coolingDown = false;
    }

    // ------------------------------------------------------------
    //  Button Press Animation (Nintendo-style squash + dip + bounce)
    // ------------------------------------------------------------
    void PlayPressAnimation()
    {


        Vector3 originalPos = transform.localPosition;
        Vector3 pressedPos = originalPos + Vector3.down * pressDepth;

        Sequence seq = DOTween.Sequence();

        // Step 1 — press
        seq.Append(transform.DOScale(new Vector3(pressScale, pressScale, 1), pressTime).SetEase(Ease.OutQuad));
        seq.Join(transform.DOLocalMove(pressedPos, pressTime).SetEase(Ease.OutQuad));

        // Step 2 — bounce up slightly (the actual bounce happens after release)
        seq.Append(transform.DOLocalMove(originalPos, bounceTime).SetEase(Ease.OutBack));
        seq.Join(transform.DOScale(Vector3.one * 1.03f, bounceTime).SetEase(Ease.OutBack));
    }

    // ------------------------------------------------------------
    // Optional: small bounce after cooldown ends
    // ------------------------------------------------------------
    void PlayReleaseBounce()
    {
        float time = 0.1f;

        Sequence seq = DOTween.Sequence();
        seq.Append(transform.DOScale(Vector3.one * 1.1f, time).SetEase(Ease.OutQuad));
        seq.Append(transform.DOScale(Vector3.one, time).SetEase(Ease.InQuad));
    }


    public void OnDeath()
    {
        colliderActive = false;

        Sequence seq = DOTween.Sequence();

        // 1. BIG POP
        seq.Append(transform.DOScale(popScale, popTime).SetEase(Ease.OutBack));

        // 2. RUBBERBAND — snaps small
        seq.Append(transform.DOScale(reboundScale, reboundTime).SetEase(Ease.InQuad));

        // 3. SHRINK + FADE OUT
        seq.Append(
            transform.DOScale(0f, fadeTime).SetEase(Ease.InQuad)
        );
        seq.Join(
            sr.DOFade(0f, fadeTime)
        );

        // 4. Destroy after animation
        seq.OnComplete(() => 
        {
            if(deathEffect)
                Instantiate(deathEffect, transform.position, Quaternion.identity);
            Destroy(gameObject, 0.2f);
        }
       );
    }

}
