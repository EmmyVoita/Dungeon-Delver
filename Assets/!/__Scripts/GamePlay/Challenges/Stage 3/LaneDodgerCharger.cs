using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LaneDodgerCharger : ChallengeBase
{
    [SerializeField] private LaneDodgerConfig config;

    [Header("General Settings")]
    [SerializeField] private GameObject chargerPrefab;

    [SerializeField] private float obstacleEndTime = 2f;

    [Header("Spawn Settings")]
    [SerializeField] private float spawnInterval = 1.2f;
    [SerializeField] private float speedUpStep = 0.05f;
    [SerializeField] private float randomizeSpawnIntervalAmount = 0.3f;
    [SerializeField] private int spawnCount = 6;

    [SerializeField] private float spawnX = 8f; // distance from center (left/right)

    private bool registered = false;

    private List<GameObject> _activeObjs = new List<GameObject>();

    void Start()
    {
        Begin();
    }

    private IEnumerator SpawnRoutine()
    {
        int spawned = 0;

        while (spawned < spawnCount)
        {
            SpawnCharger();
            spawned++;

            float offset = Random.Range(-randomizeSpawnIntervalAmount, randomizeSpawnIntervalAmount);
            yield return new WaitForSeconds((spawnInterval - speedUpStep * spawned) + offset);
        }

        // Let remaining chargers finish
        yield return new WaitForSeconds(obstacleEndTime);

        End();
    }

    private void SpawnCharger()
    {
        int direction = Random.Range(0, 2) == 0 ? -1 : 1;

        float spawnPosX = direction * spawnX;

        // Optional: small vertical randomness OR align to lanes
        int lane = Random.Range(0, config.maxLanes);
        float laneY = GetLaneY(lane);

        Vector3 spawnPos = new Vector3(spawnPosX, laneY, 0f);

        GameObject obj = Instantiate(chargerPrefab, spawnPos, Quaternion.identity, transform);

        _activeObjs.Add(obj);

        // Initialize charger
        TrackingCharger charger = obj.GetComponent<TrackingCharger>();
        if (charger != null)
        {
            charger.Initialize();
        }
    }

    float GetLaneY(int lane)
    {
        float centerOffset = (config.maxLanes - 1) * 0.5f;
        return (lane - centerOffset) * config.laneSpacing;
    }

    protected override void CleanUp()
    {
        foreach (GameObject obj in _activeObjs)
        {
            if (obj != null)
                Destroy(obj);
        }

        _activeObjs.Clear(); 
    }

    public override void Begin(object config = null)
    {
        base.Begin(this.config);
        StartCoroutine(SpawnRoutine());
    }

    public override void End()
    {
        base.End();
        Destroy(gameObject);
    }
}