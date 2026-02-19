using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RingObstacleSpawner : MonoBehaviour
{
    [Header("Ring Settings")]
    public ShrinkingRingObstacle ringPrefab;
    public int ringCount = 3;
    public float spawnInterval = 0.6f;

    public Transform centerTarget;

    private int ringsAlive = 0;
    private bool registered = false;

    void Start()
    {
        ObstacleManager.Instance.RegisterObstacle(gameObject);
        registered = true;
        StartCoroutine(SpawnRingsRoutine());
        ringsAlive = ringCount;
    }

    private IEnumerator SpawnRingsRoutine()
    {
        for (int i = 0; i < ringCount; i++)
        {
            SpawnSingleRing(i);
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void SpawnSingleRing(int i)
    {
        Vector3 spawnPos = centerTarget != null ? centerTarget.position : Vector3.zero;

        ShrinkingRingObstacle ring = Instantiate(
            ringPrefab,
            spawnPos,
            Quaternion.identity,
            this.transform // Parent rings under the spawner
        );

        ring.Initialize(i);

        ring.owner = this;
        if (centerTarget != null)
            ring.centerTarget = centerTarget;

    }

    public void OnRingResolved(ShrinkingRingObstacle ring)
    {
        ringsAlive--;

        Debug.Log($"Ring resolved. Rings remaining: {ringsAlive}");

        if (ringsAlive <= 0)
        {
            if (registered)
            {
                ObstacleManager.Instance.UnregisterObstacle(gameObject);
                registered = false;
            }

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
