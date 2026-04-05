using UnityEngine;
using System.Collections;

public class CornerOrbEncounter : MonoBehaviour
{
    [SerializeField] private LaneDodgerConfig config;
    [SerializeField] private GameObject orbPrefab;

    [Header("Spawn Area")]
    [SerializeField] private float spawnRadius = 7f;

    [Header("Optional Ellipse (override circle if enabled)")]
    [SerializeField] private bool useEllipse = true;
    [SerializeField] private Vector2 spawnRadiusXY = new Vector2(7f, 4f);

    [Header("Spawn Distribution")]
    [SerializeField] private float minAngleSeparation = 45f;

    [Header("Settings")]
    [SerializeField] private int orbCount = 3;
    [SerializeField] private float baseDelayBetweenOrbs = 2f;
    [SerializeField] private float delayShrinkStep = 0.1f;
    [SerializeField] private float destroyDelay = 2.0f;

    private bool registered = false;
    private float lastAngle = -999f;

    void Start()
    {
        ObstacleManager.Instance.RegisterObstacle(gameObject);
        registered = true;

        StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        for (int i = 0; i < orbCount; i++)
        {
            SpawnOrb();

            float delay = Mathf.Max(0.1f, baseDelayBetweenOrbs - delayShrinkStep * i);
            yield return new WaitForSeconds(delay);
        }

        yield return new WaitForSeconds(destroyDelay);

        Cleanup();
    }

    void SpawnOrb()
    {
        Vector2 pos = GetRandomCirclePosition();
        Instantiate(orbPrefab, pos, Quaternion.identity, transform);
    }

    Vector2 GetRandomCirclePosition()
    {
        float angle;

        // 🔥 Avoid spawning too close to previous angle
        int safety = 0;
        do
        {
            angle = Random.Range(0f, 360f);
            safety++;
        }
        while (Mathf.Abs(Mathf.DeltaAngle(angle, lastAngle)) < minAngleSeparation && safety < 10);

        lastAngle = angle;

        float rad = angle * Mathf.Deg2Rad;

        Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));

        // 🔥 Choose shape
        if (useEllipse)
        {
            return new Vector2(dir.x * spawnRadiusXY.x, dir.y * spawnRadiusXY.y);
        }
        else
        {
            return dir * spawnRadius;
        }
    }

    void Cleanup()
    {
        if (registered && ObstacleManager.Instance != null)
        {
            ObstacleManager.Instance.UnregisterObstacle(gameObject);
            Destroy(gameObject);
        }
    }
}