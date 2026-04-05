using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BlackHoleSpawner : MonoBehaviour
{
    [Header("References")]
    public GameObject obstaclePrefab;

    [Header("Spawn Settings")]
    public bool registerObstacle = true;
    public float spawnRadius = 12f;
    public float spawnInterval = 0.6f;
    public int maxActiveObjects = 25;
    public float runTime = 10f;
    public bool runInfinite = false;

    [Header("Spiral Motion")]
    public float inwardSpeed = 2.5f;
    public float angularSpeed = 90f; // degrees per second

    [Header("Difficulty Control")]
    [Tooltip("Minimum angular spacing between spawns (deg)")]
    public float minAngleSpacing = 60f;

    private float lastSpawnAngle;
    private List<SpiralObject> active = new List<SpiralObject>();

    private Vector2 center;

    void OnEnable()
    {
        center = transform.position; // or Camera.main.transform.position
        StartCoroutine(SpawnLoop());
        if(registerObstacle) ObstacleManager.Instance?.RegisterObstacle(gameObject);
    }

    void OnDisable()
    {
        StopAllCoroutines();
        foreach (var obj in active)
        {
            if (obj != null)
                Destroy(obj.gameObject);
        }

        active.Clear();
    }

    IEnumerator SpawnLoop()
    {
        float currentTime = Time.time;

        while (runTime > Time.time - currentTime || runInfinite)
        {
            if (active.Count < maxActiveObjects)
            {
                SpawnSpiralObject();
            }

            yield return new WaitForSeconds(spawnInterval);
        }

        OnExit();
    }

    void OnExit()
    {
        foreach (var obj in active)
        {
            if (obj != null)
                Destroy(obj.gameObject);
        }
        if(registerObstacle) ObstacleManager.Instance?.UnregisterObstacle(gameObject);
        Destroy(gameObject, 0f);
    }

    void SpawnSpiralObject()
    {
        float angle = GetSafeSpawnAngle();

        Vector2 pos = center + new Vector2(
            Mathf.Cos(angle * Mathf.Deg2Rad),
            Mathf.Sin(angle * Mathf.Deg2Rad)
        ) * spawnRadius;

        GameObject obj = Instantiate(obstaclePrefab, pos, Quaternion.identity);

        SpiralObject spiral = obj.GetComponent<SpiralObject>();
        spiral.Init(center, spawnRadius, angle, inwardSpeed, angularSpeed);

        active.Add(spiral);

        spiral.OnConsumed += () =>
        {
            active.Remove(spiral);
        };

        lastSpawnAngle = angle;
    }

    float GetSafeSpawnAngle()
    {
        float angle;

        int safety = 0;
        do
        {
            angle = Random.Range(0f, 360f);
            safety++;
        }
        while (Mathf.Abs(Mathf.DeltaAngle(angle, lastSpawnAngle)) < minAngleSpacing && safety < 10);

        return angle;
    }
}
