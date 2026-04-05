using UnityEngine;
using System.Collections;

public class MenuBackgroundController : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject rocketPrefab;
    public GameObject blackHolePrefab;

    [Header("Rocket Spawn")]
    public Vector2 rocketSpawnInterval = new Vector2(0.5f, 1.2f);
    public float rocketSpawnRadius = 10f;

    [Header("Rocket Settings")]
    public Vector2 rocketSizeRange = new Vector2(0.5f, 1.5f);
    public Vector2 rocketSpeedRange = new Vector2(2f, 6f);

    [Header("Blackhole Spawn")]
    public Vector2 blackholeSpawnInterval = new Vector2(5f, 8f);
    public float blackholeSpeed = 1.5f;

    void Start()
    {
        StartCoroutine(SpawnRockets());
        StartCoroutine(SpawnBlackholes());
    }

    IEnumerator SpawnRockets()
    {
        while (true)
        {
            SpawnRocket();

            yield return new WaitForSeconds(
                Random.Range(rocketSpawnInterval.x, rocketSpawnInterval.y));
        }
    }

    IEnumerator SpawnBlackholes()
    {
        while (true)
        {
            SpawnBlackhole();

            yield return new WaitForSeconds(
                Random.Range(blackholeSpawnInterval.x, blackholeSpawnInterval.y));
        }
    }

    void SpawnRocket()
    {
        Vector2 spawnPos = Random.insideUnitCircle.normalized * rocketSpawnRadius;

        GameObject obj = Instantiate(rocketPrefab, spawnPos, Quaternion.identity);

        MenuRocket rocket = obj.GetComponent<MenuRocket>();

        float depth = Random.value;

        float size = Mathf.Lerp(rocketSizeRange.x, rocketSizeRange.y, depth);
        float speed = Mathf.Lerp(rocketSpeedRange.x, rocketSpeedRange.y, depth);

        rocket.transform.localScale = Vector3.one * size;
        rocket.originalScale = size;
        rocket.depth = depth;

        Vector2 dirToCenter = (-spawnPos).normalized;
        rocket.velocity = dirToCenter * speed;
    }

    void SpawnBlackhole()
    {
        Vector2 spawnPos = Random.insideUnitCircle.normalized * rocketSpawnRadius;

        GameObject obj = Instantiate(blackHolePrefab, spawnPos, Quaternion.identity);

        MenuBlackHole hole = obj.GetComponent<MenuBlackHole>();

        Vector2 dirToCenter = (-spawnPos).normalized;
        hole.velocity = dirToCenter * blackholeSpeed;
    }
}