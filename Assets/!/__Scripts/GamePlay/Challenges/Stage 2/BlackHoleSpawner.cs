using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BlackHoleSpawner : ChallengeBase
{
    [Header("References")]
    public List<GameObject> obstaclePrefabs;

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
        Begin();
    }

    void OnDisable()
    {
        StopAllCoroutines();
        CleanUp();
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

        End();
    }

    protected override void CleanUp()
    {
        foreach (var obj in active)
        {
            if (obj != null)
                Destroy(obj.gameObject);
        }
    }

    void SpawnSpiralObject()
    {
        float angle = GetSafeSpawnAngle();

        Vector2 pos = center + new Vector2(
            Mathf.Cos(angle * Mathf.Deg2Rad),
            Mathf.Sin(angle * Mathf.Deg2Rad)
        ) * spawnRadius;

        GameObject spawnObj = obstaclePrefabs.GetRandom();

        GameObject obj = Instantiate(spawnObj, pos, Quaternion.identity);

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

    public override void Begin(object config = null)
    {
        base.Begin();
        StartCoroutine(SpawnLoop());
    }

    public override void End()
    {
        base.End();
        Destroy(gameObject);
    }
}
