using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FanShooterObstacle : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private LaneDodgerConfig config;

    [Header("Projectile")]
    [SerializeField] private GameObject projectilePrefab;

    [Header("Fan Settings")]
    [SerializeField] private int projectileCountA = 3;
    [SerializeField] private int projectileCountB = 5;
    [SerializeField] private bool alternateProjectileCount = true;
    [SerializeField] private float spreadAngle = 60f;
    [SerializeField] private bool spawnRandomWithinAngle = false;

    [Header("Spawn Settings")]
    [SerializeField] private bool aimAtPlayer = true;
    [SerializeField] private float spawnX = 8f;
    [SerializeField] private bool spawnFromLeft = true;

    [Header("Timing")]
    [SerializeField] private float startDelay = 1f;
    [SerializeField] private float fireInterval = 1.5f;
    [SerializeField] private int burstCount = 5;
    [SerializeField] private float delayBetweenShots = 0f;

    [Header("Tracking")]
    [SerializeField] private float followSmoothTime = 0.3f;
    [SerializeField] private float maxFollowSpeed = 10f;

    [Header("Prediction")]
    [SerializeField] private bool predictPlayerMovement = true;
    [SerializeField] private float predictionTime = 0.4f;
    [SerializeField] private float maxPredictionOffset = 3f;

    [Header("Lifetime")]
    [SerializeField] private float obstacleEndDelay = 2f;

    [Header("Audio")]
    [SerializeField] private SoundEffect fireSound;

    private float velocityY = 0f;
    private bool registered = false;
    private List<GameObject> activeProjectiles;
    private bool useA = true;
    private float lastPlayerY;

    void Start()
    {
        lastPlayerY = Player.Instance.transform.position.y;
        //ObstacleManager.Instance.RegisterObstacle(gameObject);
        //registered = true;
        activeProjectiles = new List<GameObject>();

        //Player.Instance.SetPlayerControlState(Player.PlayerControlState.LaneDodger, config);
        TrackPlayerY();

        StartCoroutine(FireRoutine());
    }

    void Update()
    {
        TrackPlayerY();
    }

    // -------------------------------------
    // TRACKING
    // -------------------------------------

    void TrackPlayerY()
    {
        if (Player.Instance == null) return;

        float playerY = Player.Instance.transform.position.y;

        // --- Calculate velocity ---
        float playerVelocityY = (playerY - lastPlayerY) / Time.deltaTime;
        lastPlayerY = playerY;

        float targetY = playerY;

        if (predictPlayerMovement)
        {
            float predictedOffset = playerVelocityY * predictionTime;

            // clamp so it doesn't go crazy
            predictedOffset = Mathf.Clamp(predictedOffset, -maxPredictionOffset, maxPredictionOffset);

            targetY += predictedOffset;
        }

        float newY = Mathf.SmoothDamp(
            transform.position.y,
            targetY,
            ref velocityY,
            followSmoothTime,
            maxFollowSpeed
        );

        transform.position = new Vector3(spawnX, newY, transform.position.z);
    }

    // -------------------------------------
    // FIRING
    // -------------------------------------

    IEnumerator FireRoutine()
    {
        yield return new WaitForSeconds(startDelay);
        
        for (int i = 0; i < burstCount; i++)
        {
            yield return StartCoroutine(FireFan());

            AudioHelpers.PlaySoundEffect(fireSound, transform.position);

            yield return new WaitForSeconds(fireInterval);
        }

        yield return new WaitForSeconds(obstacleEndDelay);

        Cleanup();
    }

    IEnumerator FireFan()
    {
        Vector2 origin = transform.position;

        // Determine base direction (left → right OR right → left)
        Vector2 baseDir = spawnFromLeft ? Vector2.right : Vector2.left;
        baseDir = aimAtPlayer ? Player.Instance.transform.position - transform.position : baseDir;

        int currentCount = projectileCountA;

        if (alternateProjectileCount)
        {
            currentCount = useA ? projectileCountA : projectileCountB;
            useA = !useA; // 🔥 flip for next time
        }

        currentCount = Mathf.Max(1, currentCount); // safety

        float angleStep = currentCount > 1 ? spreadAngle / (currentCount - 1) : 0f;
        float startAngle = currentCount > 1 ? -spreadAngle * 0.5f : 0f;

        for (int i = 0; i < currentCount; i++)
        {
            float angle = spawnRandomWithinAngle ? Random.Range(-spreadAngle,spreadAngle) : startAngle + angleStep * i;

            

            Vector2 dir = RotateVector(baseDir, angle);

            GameObject obj = Instantiate(projectilePrefab, transform.position, Quaternion.identity);

            //FanProjectile proj = obj.GetComponent<FanProjectile>();
            TrackingCharger proj = obj.GetComponent<TrackingCharger>();
            if (proj != null)
            {
                //proj.Initialize(dir);
                proj.Initialize(dir);
            }

            activeProjectiles.Add(obj);

            yield return new WaitForSeconds(delayBetweenShots);
        }
    }

    // -------------------------------------
    // MATH
    // -------------------------------------

    Vector2 RotateVector(Vector2 v, float degrees)
    {
        float rad = degrees * Mathf.Deg2Rad;

        float cos = Mathf.Cos(rad);
        float sin = Mathf.Sin(rad);

        return new Vector2(
            v.x * cos - v.y * sin,
            v.x * sin + v.y * cos
        );
    }

    // -------------------------------------
    // CLEANUP
    // -------------------------------------

    void Cleanup()
    {
        Destroy(gameObject);

     
        /*
        if (registered && ObstacleManager.Instance != null)
        {
            ObstacleManager.Instance.UnregisterObstacle(gameObject);

            Player.Instance.SetPlayerControlState(Player.PlayerControlState.Normal);

            Destroy(gameObject);
        }
        */
    }

    void OnDestroy()
    {
        foreach(GameObject projectile in activeProjectiles)
        {
            if(projectile != null)
            {
                Destroy(projectile);
            }
        }
        /*
        if (registered && ObstacleManager.Instance != null)
        {
            ObstacleManager.Instance.UnregisterObstacle(gameObject);
        }
        */
    }
}