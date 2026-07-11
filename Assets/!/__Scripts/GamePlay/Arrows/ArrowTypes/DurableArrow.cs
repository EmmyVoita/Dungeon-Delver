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

    private float bounceStartTime;
    private float bounceEndTime;
    private Vector2 bounceStartPos;
    private float bounceDistance;
    private bool isBouncing = false;



    protected override void Update()
    {
        if (_invincible && Time.time > invincibleDone)
            _invincible = false;

        if (_isDead) return;

        float elapsed = (float)MusicManager.Instance.ScaledElapsedTime;

        // --- BOUNCE PHASE ---
        if (isBouncing)
        {
            float t = Mathf.InverseLerp(bounceStartTime, bounceEndTime, elapsed);
            t = Mathf.Clamp01(t);

            float h = 4f * t * (1f - t); // parabola

            Vector2 awayDir = _direction.normalized;

            Vector2 targetPos = bounceStartPos + awayDir * (h * bounceDistance);

            SmoothTranslate(targetPos);

            if (elapsed >= bounceEndTime)
            {
                isBouncing = false;

                // IMPORTANT: shift arrow timeline forward
                float delay = bounceEndTime - bounceStartTime;
                _spawnTime += delay;
                _arrivalTime += delay;
            }

            return;
        }

        // --- NORMAL MOVEMENT ---
        base.Update();
    }


    public override void Init(Vector2 direction, float speed, float spawnTime, float arrivalTime, Vector3 startPos, Vector3 endPos)
    {
        base.Init(direction, speed, spawnTime, arrivalTime, startPos, endPos);
        
        float secondsPerBeat = 60f / ArrowSpawner.Instance.ActiveBPM;
        float duration = bounceTimeBeats * secondsPerBeat;

        float secondArrivalTime = arrivalTime + duration;
        /*
        float totalDistance = Vector2.Distance(startPos, endPos);

        float clampedStopDistance = Mathf.Min(totalDistance - stopDistance, totalDistance);
        float stopRatio = clampedStopDistance / totalDistance;

        stopTime = Mathf.Lerp(spawnTime, arrivalTime, stopRatio);

        float secondsPerBeat = 60f / ArrowSpawner.Instance.ActiveBPM;
        float delayDuration = delayBeats * secondsPerBeat;

        resumeTime = stopTime + delayDuration;
        this.arrivalTime = delayDuration + arrivalTime;

        // ✅ Correct stop position
        stopPos = Vector2.Lerp(startPos, endPos, stopRatio);
        */
    }


    public override void OnArrowHit(float damage = 1f,
                                    Goal.GoalType goalType = Goal.GoalType.Normal,
                                    Vector2 hitDirection = default)
    {
        if (_invincible) return;

        hitsTaken++;

        if (hitsTaken >= hitsRequired)
        {
            Die(goalType, true, hitDirection);
        }
        else
        {

            _invincible = true;
            invincibleDone = Time.time + invincibilityDuration;

            base.PlayAudio(goalType);

            //if (bounceRoutine != null)
               // StopCoroutine(bounceRoutine);

            //bounceRoutine = StartCoroutine(BounceParabola());

            float secondsPerBeat = 60f / ArrowSpawner.Instance.ActiveBPM;
            float duration = bounceTimeBeats * secondsPerBeat;

            bounceStartTime = (float)MusicManager.Instance.ScaledElapsedTime;
            bounceEndTime = bounceStartTime + duration;

            bounceStartPos = transform.position;

            bounceDistance = maxBounceDistance > 0f
                ? maxBounceDistance
                : _speed * (duration * 0.5f);

            isBouncing = true;

        }
    }

    /*
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
    */
}
