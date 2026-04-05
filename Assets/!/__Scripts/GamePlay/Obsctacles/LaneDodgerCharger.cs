using UnityEngine;
using System.Collections;

public class LaneDodgerCharger : MonoBehaviour
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

    void Start()
    {
        ObstacleManager.Instance.RegisterObstacle(gameObject);
        registered = true;

        Player.Instance.SetPlayerControlState(Player.PlayerControlState.LaneDodger, config);

        StartCoroutine(SpawnRoutine());
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

        Cleanup();
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

        // Initialize charger
        TrackingCharger charger = obj.GetComponent<TrackingCharger>();
        if (charger != null)
        {
            charger.Initialize();
        }
        /*
        // Flip sprite to face inward
        if (direction == -1)
        {
            Vector3 scale = obj.transform.localScale;
            scale.x = -Mathf.Abs(scale.x);
            obj.transform.localScale = scale;
        }
        else
        {
            Vector3 scale = obj.transform.localScale;
            scale.x = Mathf.Abs(scale.x);
            obj.transform.localScale = scale;
        }
        */
    }

    float GetLaneY(int lane)
    {
        float centerOffset = (config.maxLanes - 1) * 0.5f;
        return (lane - centerOffset) * config.laneSpacing;
    }

    private void Cleanup()
    {
        if (registered && ObstacleManager.Instance != null)
        {
            ObstacleManager.Instance.UnregisterObstacle(gameObject);

            Player.Instance.SetPlayerControlState(Player.PlayerControlState.Normal);

            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (registered && ObstacleManager.Instance != null)
        {
            ObstacleManager.Instance.UnregisterObstacle(gameObject);
        }
    }
}