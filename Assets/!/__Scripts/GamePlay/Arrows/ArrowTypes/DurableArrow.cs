using UnityEngine;
using System.Collections;

public class DurableArrow : ArrowBase
{
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
    [SerializeField] private float invincibleDone = 0;

    private Coroutine bounceRoutine;


    void Update()
    {
        if (invincible && Time.time > invincibleDone)
            invincible = false;
    }

    public override void OnArrowHit(float damage = 1f,
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

            base.PlayAudio(goalType);

            if (bounceRoutine != null)
                StopCoroutine(bounceRoutine);

            bounceRoutine = StartCoroutine(BounceParabola());
        }
    }

    private IEnumerator BounceParabola()
    {
        // --- 1. Setup timing from BPM ---
        float bpm = (ArrowSpawner.Instance != null) ? ArrowSpawner.Instance.ActiveBPM : 120f;
        float secondsPerBeat = 60f / bpm;
        float duration = bounceTimeBeats * secondsPerBeat;
        if (duration <= 0f)
            duration = 0.25f;

        // --- 2. Setup distances ---
        Vector3 startPos = transform.position;

        // direction: from ArrowBase, normalized already in Fire()
        Vector3 awayDir = (Vector3)direction.normalized;  // away from the player
        float distance;

        if (maxBounceDistance > 0f)
        {
            distance = maxBounceDistance;
        }
        else
        {
            // Roughly: how far it would go in half the duration at this speed
            distance = speed * (duration * 0.5f);
        }

        // Stop physics while we manually control the position
        rb.linearVelocity = Vector2.zero;

        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            float u = Mathf.Clamp01(t);

            // Parabola 0 → 1 → 0 (like jump arc in 1D)
            float h = 4f * u * (1f - u);

            // Move away then back along the same line
            transform.position = startPos + awayDir * (h * distance);

            yield return null;
        }

        // Snap back to exact start (in case of tiny float drift)
        transform.position = startPos;

        // Restore normal motion: back toward the player / center
        rb.linearVelocity = -direction * speed;

        bounceRoutine = null;
    }
}
