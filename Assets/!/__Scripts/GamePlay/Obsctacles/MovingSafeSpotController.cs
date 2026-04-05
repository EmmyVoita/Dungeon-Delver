using UnityEngine;
using System.Collections;
using DG.Tweening;

public class MovingSafeSpotController : MonoBehaviour
{
    public AudioClip windupSound;
    public AudioClip ringSpawnSound;
    public AudioClip successSound;
    public AudioClip failSound;

    [Header("References")]
    public Transform safeSpotTransform;
    public GameObject collapsingRingPrefab;

    [Header("Center & Radius Settings")]
    //public Transform center;
    public float baseRadius = 2.5f;
    public float radiusVariance = 1f;

    [Header("Motion Timing")]
    public float segmentMoveTime = 0.7f;
    public AnimationCurve moveCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Sequence Settings")]
    public int collapseCount = 3;
    public float delayBetweenCollapses = 0.4f;

    [Header("Spawn Behavior")]
    public bool spawnRingAtTargetBeforeArrival = false;
    public float preSpawnLeadTime = 0.15f;

    [Header("Extra Spin Settings")]
    public bool allowExtraSpin = true;
    public int maxExtraRevolutions = 1; // how many extra full circles max


    private float currentAngle = 0f;
    private float startAngle = 0f;
    private float targetAngle = 0f;

    private float currentRadius;
    private float startRadius;
    private float targetRadius;

    private bool isMoving = false;
    private int cyclesCompleted = 0;

    private int lastDirectionIndex = -1;

    private bool tookDamageThisCycle = false;
    private bool hasFadedIn = false;



    private static readonly float[] CARDINAL_ANGLES = new float[]
    {
        90f,   // Up
        0f,    // Right
        270f,  // Down
        180f   // Left
    };

    void OnEnable()
    {
        Player.OnDamageTaken += HandlePlayerDamaged;
    }

    void OnDisable()
    {
        Player.OnDamageTaken -= HandlePlayerDamaged;
    }

    void HandlePlayerDamaged(int damageAmount)
    {
        if (isMoving && !tookDamageThisCycle)
        {
            tookDamageThisCycle = true;
            AudioHelpers.PlayMyClipAtPoint(failSound, AudioChannel.SFX, Camera.main.transform.position);
        }
    }


    void Start()
    {
        currentRadius = baseRadius;
        currentAngle = CARDINAL_ANGLES[Random.Range(0, 4)];

        StartCoroutine(SequenceRoutine());

        SpriteRenderer sr = safeSpotTransform.GetComponent<SpriteRenderer>();
        Color c = sr.color;
        c.a = 0f;
        sr.color = c;

    }


    IEnumerator SequenceRoutine()
    {
        cyclesCompleted = 0;
        ObstacleManager.Instance.RegisterObstacle(gameObject);

        while (cyclesCompleted < collapseCount)
        {
            PickNewTarget();

            // Optional early-spawn BEFORE safe circle arrives
            if (spawnRingAtTargetBeforeArrival)
            {
                yield return new WaitForSeconds(preSpawnLeadTime);

                AudioHelpers.PlayMyClipAtPoint(ringSpawnSound, AudioChannel.SFX, Camera.main.transform.position);

                // Pre-spawn the collapsing ring at target position
                Vector3 predictedPos = CalculatePosition(targetAngle, targetRadius);


                GameObject earlyRing = Instantiate(collapsingRingPrefab, predictedPos, Quaternion.identity);
                earlyRing.GetComponent<CollapsingRing>().Init(safeSpotTransform, () =>
                {
                    if (!tookDamageThisCycle)
                    {
                        AudioHelpers.PlayMyClipAtPoint(successSound, AudioChannel.SFX, Camera.main.transform.position);
                    }
                });
            }

            // Move safe spot via smooth curve
            yield return StartCoroutine(MoveToTargetRoutine());

            // Normal spawn AFTER arrival (only if early-spawn is disabled)
            if (!spawnRingAtTargetBeforeArrival)
            {
                GameObject ring = Instantiate(collapsingRingPrefab, safeSpotTransform.position, Quaternion.identity);
                ring.GetComponent<CollapsingRing>().Init(safeSpotTransform);
            }

            cyclesCompleted++;
            yield return new WaitForSeconds(delayBetweenCollapses);
        }

        ObstacleManager.Instance.UnregisterObstacle(gameObject);
        Destroy(gameObject);
    }



    void PickNewTarget()
    {
        // Choose a different cardinal direction
        int index;
        do
        {
            index = Random.Range(0, CARDINAL_ANGLES.Length);
        }
        while (index == lastDirectionIndex);

        lastDirectionIndex = index;

        startAngle = currentAngle;
        targetAngle = CARDINAL_ANGLES[index];

        // Pick radius variation
        startRadius = currentRadius;
        targetRadius = baseRadius + Random.Range(-radiusVariance, radiusVariance);

        // Normalize base angles
        startAngle = Mathf.Repeat(startAngle, 360f);
        targetAngle = Mathf.Repeat(targetAngle, 360f);

        // 💫 Add optional extra spin
        if (allowExtraSpin)
        {
            int extraRevs = Random.Range(0, maxExtraRevolutions + 1); // 0, 1, or more
            if (extraRevs > 0)
            {
                // Clockwise or counterclockwise depends on the shortest path
                float signedDelta = Mathf.DeltaAngle(startAngle, targetAngle);

                if (signedDelta > 0)
                    targetAngle += 360f * extraRevs;
                else
                    targetAngle -= 360f * extraRevs;
            }
        }
    }



    IEnumerator MoveToTargetRoutine()
    {
        AudioHelpers.PlayMyClipAtPoint(windupSound, AudioChannel.SFX, Camera.main.transform.position);

         // ⭐ Fade in once — at first movement
        if (!hasFadedIn)
        {
            hasFadedIn = true;

            SpriteRenderer sr = safeSpotTransform.GetComponent<SpriteRenderer>();
            sr.DOFade(1f, 0.4f).SetEase(Ease.OutQuad);
        }

        tookDamageThisCycle = false;
        isMoving = true;

        float timer = 0f;

        while (timer < segmentMoveTime)
        {
            timer += Time.deltaTime;
            float t = moveCurve.Evaluate(timer / segmentMoveTime);

            // ❗ REAL FIX: do NOT use LerpAngle for multi-rotation
            currentAngle = Mathf.Lerp(startAngle, targetAngle, t);
            currentRadius = Mathf.Lerp(startRadius, targetRadius, t);

            UpdateSafeSpotPosition();
            yield return null;
        }

        currentAngle = targetAngle;
        currentRadius = targetRadius;

        UpdateSafeSpotPosition();
        isMoving = false;
    }



    private void UpdateSafeSpotPosition()
    {
        safeSpotTransform.position = CalculatePosition(currentAngle, currentRadius);
    }


    private Vector3 CalculatePosition(float angleDegrees, float radius)
    {
        float rad = angleDegrees * Mathf.Deg2Rad;
        Vector3 offset = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0) * radius;
        return Vector3.zero + offset;
    }

    void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        // Draw path line toward target
        Vector3 predictedPos = CalculatePosition(targetAngle, targetRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(Vector3.zero, predictedPos);

        // Draw small crosshair at predictedPos
        float s = 0.2f;
        Gizmos.DrawLine(predictedPos + Vector3.left * s, predictedPos + Vector3.right * s);
        Gizmos.DrawLine(predictedPos + Vector3.up * s, predictedPos + Vector3.down * s);

        // Draw a small circle
        Gizmos.color = Color.cyan;
        DrawCircle(predictedPos, 0.15f);
    }

    void DrawCircle(Vector3 center, float radius)
    {
        int segments = 20;
        float angleStep = 360f / segments;

        Vector3 prev = center + new Vector3(Mathf.Cos(0f), Mathf.Sin(0f)) * radius;

        for (int i = 1; i <= segments; i++)
        {
            float theta = angleStep * i * Mathf.Deg2Rad;
            Vector3 next = center + new Vector3(Mathf.Cos(theta), Mathf.Sin(theta)) * radius;
            Gizmos.DrawLine(prev, next);
            prev = next;
        }
    }

}
