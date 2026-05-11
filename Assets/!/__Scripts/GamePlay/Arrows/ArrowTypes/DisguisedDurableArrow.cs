using UnityEngine;
using System.Collections;

public class DisguisedDurableArrow : ArrowBase
{
    // =====================================================
    // Bounce / Durability
    // =====================================================

    [Header("Bounce Settings")]
    [Tooltip("How many beats the whole out-and-back bounce should last.")]
    public float bounceTimeBeats = 4f;

    [Tooltip("How far (in world units) the arrow moves away at the peak of the bounce.\n" +
             "If <= 0, it will be derived from speed & duration.")]
    public float maxBounceDistance = 0f;

    public AudioClip bounceSound;

    [Header("Durability Settings")]
    public int hitsRequired = 2;
    public float invincibilityDuration = 0.8f;

    [Header("Runtime")]
    [SerializeField] private int hitsTaken = 0;
    [SerializeField] private float invincibleDone = 0f;

    private Coroutine bounceRoutine;

    // =====================================================
    // Disguise
    // =====================================================

    [Header("Disguise Settings")]
    [Tooltip("Sprite shown before the arrow reveals its true type")]
    [SerializeField] private Sprite disguisedSprite;
    [SerializeField] private GameObject disguiseEffectPrefab;

    [Tooltip("When the arrow reveals itself (0 = immediately, 1 = at hit zone)")]
    [Range(0f, 1f)]
    [SerializeField] private float revealFraction = 0.7f;

    private Sprite realSprite;
    private bool isDisguised = false;
    private bool hasRevealed = false;
    private float revealTimer = -1f;

    // =====================================================
    // Lifecycle
    // =====================================================

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Update()
    {
        base.Update();
        // ------------------------------
        // Invincibility window
        // ------------------------------
        if (invincible && Time.time > invincibleDone)
            invincible = false;

        // ------------------------------
        // Disguise reveal timing
        // ------------------------------
        if (isDisguised && !hasRevealed)
        {
            revealTimer -= Time.deltaTime;
            if (revealTimer <= 0f)
                RevealDisguise();
        }
    }

    // =====================================================
    // Fire / Disguise Init
    // =====================================================

    /*public override void Fire(Vector2 direction, float speed)
    {
        //base.Fire(direction, speed);

        TryApplyDisguise();
    }
    */

    private void TryApplyDisguise()
    {
        //if (!BossContext.IsBossActive)
            //return;

        //if (!BossContext.HasEffect(BossEffectType.ModifyArrows))
            //return;


        if (disguisedSprite == null)
            return;

        var sr = GetComponentInChildren<SpriteRenderer>();
        if (sr == null)
            return;

        realSprite = sr.sprite;
        sr.sprite = disguisedSprite;

        isDisguised = true;
        hasRevealed = false;

        // Speed-safe reveal timing
        float travelTime = ArrowSpawner.Instance.SpawnDistance / speed;
        revealTimer = travelTime * Mathf.Clamp01(revealFraction);
    }

    private void RevealDisguise()
    {
        if (hasRevealed) return;

        hasRevealed = true;
        isDisguised = false;

        var sr = GetComponentInChildren<SpriteRenderer>();
        if (sr != null && realSprite != null)
            sr.sprite = realSprite;

        Instantiate(disguiseEffectPrefab, transform.position, Quaternion.identity, this.gameObject.transform);
        // Optional polish:
        // - small flash
        // - scale pop
        // - sound cue
    }

    // =====================================================
    // Hit Logic
    // =====================================================

    public override void OnArrowHit(
        float damage = 1f,
        Goal.GoalType goalType = Goal.GoalType.Normal,
        Vector2 hitDirection = default)
    {
        if (invincible) return;

        hitsTaken++;

        if (hitsTaken >= hitsRequired)
        {
            Die(goalType, true, hitDirection);
        }
        else
        {
            invincible = true;
            invincibleDone = Time.time + invincibilityDuration;

            if (bounceSound != null)
            {
                AudioHelpers.PlayClipWithVariation(
                    bounceSound,
                    AudioChannel.SFX,
                    Camera.main.transform.position
                );
            }

            if (bounceRoutine != null)
                StopCoroutine(bounceRoutine);

            bounceRoutine = StartCoroutine(BounceParabola());
        }
    }

    // =====================================================
    // Bounce Motion
    // =====================================================

    private IEnumerator BounceParabola()
    {
        // --- Timing from BPM ---
        float bpm = (ArrowSpawner.Instance != null)
            ? ArrowSpawner.Instance.ActiveBPM
            : 120f;

        float secondsPerBeat = 60f / bpm;
        float duration = bounceTimeBeats * secondsPerBeat;
        if (duration <= 0f)
            duration = 0.25f;

        // --- Setup ---
        Vector3 startPos = transform.position;
        Vector3 awayDir = (Vector3)direction.normalized;

        float distance;
        if (maxBounceDistance > 0f)
            distance = maxBounceDistance;
        else
            distance = speed * (duration * 0.5f);

        // Pause physics
        rb.linearVelocity = Vector2.zero;

        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            float u = Mathf.Clamp01(t);

            // Parabola (0 → 1 → 0)
            float h = 4f * u * (1f - u);

            transform.position = startPos + awayDir * (h * distance);
            yield return null;
        }

        // Snap back
        transform.position = startPos;

        // Resume motion toward player
        rb.linearVelocity = -direction * speed;

        bounceRoutine = null;
    }
}
