using UnityEngine;
using System.Collections;

public class OrbitingBlackHoleArmSpawner : MonoBehaviour
{
    [Header("Prefab")]
    public OrbitingBlackHoleArm obstaclePrefab;

    [Header("Spawn")]
    public Vector3 centerTarget;
    public Vector3 spawnOffset = Vector3.zero;

    [Header("Lifetime")]
    public float lifetime = 6f;

    private bool registered;

    private void Start()
    {
        ObstacleManager.Instance.RegisterObstacle(gameObject);
        registered = true;

        Vector3 spawnPos = (centerTarget != null ? centerTarget : Vector3.zero) + spawnOffset;

        OrbitingBlackHoleArm obj = Instantiate(obstaclePrefab, spawnPos, Quaternion.identity, transform);
        obj.centerTarget = centerTarget;

        StartCoroutine(ExpireRoutine());
    }

    private IEnumerator ExpireRoutine()
    {
        yield return new WaitForSeconds(lifetime);

        if (registered && ObstacleManager.Instance != null)
        {
            ObstacleManager.Instance.UnregisterObstacle(gameObject);
            registered = false;
        }

        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (registered && ObstacleManager.Instance != null)
        {
            ObstacleManager.Instance.UnregisterObstacle(gameObject);
        }
    }
}