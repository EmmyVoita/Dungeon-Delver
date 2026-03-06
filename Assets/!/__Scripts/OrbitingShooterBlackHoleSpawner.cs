using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class OrbitingShooterBlackholeSpawner : MonoBehaviour
{
    [Header("Setup")]
    public Vector3 centerTarget;
    public GameObject blackholePrefab;

    [Header("Orbit Settings")]
    public int blackholeCount = 4;
    public float orbitRadius = 4f;
    public float orbitSpeed = 40f;

    [Header("Burst Settings")]
    public float burstInterval = 2f;
    public float lifetime = 10f;

    private List<BlackholeEmitter> emitters = new();
    private bool registered;

    void Start()
    {
        ObstacleManager.Instance.RegisterObstacle(gameObject);
        registered = true;

        SpawnBlackholes();
        StartCoroutine(BurstRoutine());
        StartCoroutine(LifetimeRoutine());
    }

    void Update()
    {
        if (centerTarget == null) return;

        transform.position = centerTarget;
        transform.Rotate(Vector3.forward, orbitSpeed * Time.deltaTime);
    }

    void SpawnBlackholes()
    {
        for (int i = 0; i < blackholeCount; i++)
        {
            float angle = (float)i / blackholeCount * Mathf.PI * 2f;
            Vector3 localPos = new Vector3(
                Mathf.Cos(angle),
                Mathf.Sin(angle),
                0f
            ) * orbitRadius;

            GameObject obj = Instantiate(blackholePrefab, transform);
            obj.transform.localPosition = localPos;

            BlackholeEmitter emitter = obj.GetComponent<BlackholeEmitter>();
            emitter.centerTarget = centerTarget;
            emitters.Add(emitter);
        }
    }

    IEnumerator BurstRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(burstInterval);

            if (emitters.Count == 0) continue;

            int randomIndex = Random.Range(0, emitters.Count);
            emitters[randomIndex].FireBurst();
        }
    }

    IEnumerator LifetimeRoutine()
    {
        yield return new WaitForSeconds(lifetime);

        if (registered)
        {
            ObstacleManager.Instance.UnregisterObstacle(gameObject);
            registered = false;
        }

        Destroy(gameObject);
    }

    void OnDestroy()
    {
        if (registered && ObstacleManager.Instance != null)
            ObstacleManager.Instance.UnregisterObstacle(gameObject);
    }
}