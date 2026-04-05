using UnityEngine;
using System.Collections;

public class TrackingCharger : MonoBehaviour
{
    public enum SteeringMode
    {
        TowardPlayer,
        TowardDirection
    }

    [Header("Settings")]
    public float chargeSpeed = 8f;
    [SerializeField] private AnimationCurve speedOverTime = AnimationCurve.Linear(0, 1, 1, 1);
    public float windupTime = 0.5f;
    public float lifetime = 2f;

    [Header("Steering")]
    public bool enableSteering = true;
    public float steeringStrength = 2f;
    [SerializeField] private AnimationCurve steeringOverTime = AnimationCurve.Linear(0, 1, 1, 1);

    [Header("Steering Mode")]
    [SerializeField] private SteeringMode steeringMode = SteeringMode.TowardPlayer;
    [SerializeField] private Vector2 globalSteeringDirection = Vector2.right;

    [Header("Direction Blending (Optional)")]
    [SerializeField] private bool useDirectionBlendCurve = false;
    [SerializeField] private AnimationCurve directionBlendOverTime = AnimationCurve.Linear(0, 0, 1, 1);

    [Header("Audio")]
    public SoundEffect spawnSound;
    public SoundEffect fireSound;

    [Header("Sprite")]
    [SerializeField] private Transform spriteTransform;
    [SerializeField] private float startAngle = 90f;

    private Rigidbody2D rb;

    // Direction system
    private Vector2 lockedDirection;
    private Vector2? overrideStartDirection = null;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // 🔥 Initialize with optional starting direction (for fan patterns)
    public void Initialize(Vector2? startDirection = null, float speedMultiplier = 1.0f)
    {
        overrideStartDirection = startDirection;
        chargeSpeed *= speedMultiplier;

        AudioHelpers.PlaySoundEffect(spawnSound, transform.position);
        StartCoroutine(ChargeRoutine());
    }

    private IEnumerator ChargeRoutine()
    {
        spriteTransform.localRotation = Quaternion.Euler(0, 0, startAngle);

        yield return new WaitForSeconds(windupTime);

        // --- INITIAL DIRECTION ---
        Vector2 playerPos = Player.Instance.transform.position;
        Vector2 playerDirection = (playerPos - (Vector2)transform.position).normalized;

        lockedDirection = overrideStartDirection.HasValue
            ? overrideStartDirection.Value.normalized
            : playerDirection;

        AudioHelpers.PlaySoundEffect(fireSound, transform.position);

        float timer = 0f;

        while (timer < lifetime)
        {
            timer += Time.deltaTime;
            float t = timer / lifetime;

            // --- SPEED ---
            float speedMultiplier = speedOverTime.Evaluate(t);
            float currentSpeed = chargeSpeed * speedMultiplier;

            // --- CURRENT DIR ---
            Vector2 currentDir = rb.linearVelocity.normalized;
            if (rb.linearVelocity.magnitude < 0.1f)
                currentDir = lockedDirection;

            // --- TARGET DIR ---
            Vector2 targetDir = lockedDirection;

            if (enableSteering)
            {
                Vector2 desiredDir = lockedDirection;

                if (steeringMode == SteeringMode.TowardPlayer)
                {
                    Vector2 playerDir = ((Vector2)Player.Instance.transform.position - rb.position).normalized;

                    // Slight bias toward player (not full lock)
                    desiredDir = Vector2.Lerp(lockedDirection, playerDir, 0.3f);
                }
                else if (steeringMode == SteeringMode.TowardDirection)
                {
                    desiredDir = globalSteeringDirection.normalized;
                }

                float steeringMultiplier = steeringOverTime.Evaluate(t);

                float blend = useDirectionBlendCurve
                    ? directionBlendOverTime.Evaluate(t)
                    : steeringMultiplier;

                targetDir = Vector2.Lerp(lockedDirection, desiredDir, blend);
            }

            // --- APPLY STEERING ---
            float steering = steeringStrength * Time.deltaTime;
            Vector2 newDir = Vector2.Lerp(currentDir, targetDir, steering).normalized;

            rb.linearVelocity = newDir * currentSpeed;

            // --- ROTATION ---
            float angle = Mathf.Atan2(newDir.y, newDir.x) * Mathf.Rad2Deg;
            spriteTransform.localRotation = Quaternion.Euler(0, 0, angle);

            yield return null;
        }

        Destroy(gameObject);
    }
}