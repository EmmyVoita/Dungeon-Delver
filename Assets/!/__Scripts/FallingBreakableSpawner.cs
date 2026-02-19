using System.Collections;
using UnityEngine;

public class FallingBreakableSpawner : MonoBehaviour
{
    public GameObject breakablePrefab;
    public GameObject playerProjectile;
    public SoundEffect shootSoundEffect;

    public float spawnDistance = 6f;
    public float spawnInterval = 1.5f;
    public int spawnCount = 4;

    [Header("Shooting Settings")]
    public float minFireDelay = 0.4f;
    private float nextAllowedFireTime = 0f;
    private int lastDirectionIndex = -1;

    public float unregisterDelay = 8f;



    private Vector2[] directions =
    {
        Vector2.up,
        Vector2.down,
        Vector2.left,
        Vector2.right
    };

    void Update()
    {
        if (InputBindingManager.Instance.GetKeyDown(InputActionType.Confirm))
        {
            if (Time.time >= nextAllowedFireTime)
            {
                nextAllowedFireTime = Time.time + minFireDelay;

                AudioHelpers.PlaySoundEffect(shootSoundEffect, Player.Instance.transform.position);
                Player.Instance.ShootProjectile(playerProjectile.GetComponent<PlayerProjectile>());
            }
        }

    }

    void Start()
    {
        ObstacleManager.Instance.RegisterObstacle(gameObject);
        Player.Instance.SetPlayerControlState(Player.PlayerControlState.Shooter);
        StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        for (int i = 0; i < spawnCount; i++)
        {
            SpawnOne();
            yield return new WaitForSeconds(spawnInterval);
        }

        yield return new WaitForSeconds(unregisterDelay);

        ObstacleManager.Instance.UnregisterObstacle(gameObject);

        Player.Instance.SetPlayerControlState(Player.PlayerControlState.Normal);

        Destroy(gameObject);
    }

    void SpawnOne()
    {
        int newIndex = Random.Range(0, directions.Length - 1);

        // Shift index if it matches last
        if (newIndex >= lastDirectionIndex)
            newIndex++;

        lastDirectionIndex = newIndex;

        Vector2 dir = directions[newIndex];

        Vector3 spawnPos = dir * spawnDistance;

        Vector2 toCenter = (-spawnPos).normalized;

        float angle = Mathf.Atan2(toCenter.y, toCenter.x) * Mathf.Rad2Deg - 90f;
        Quaternion rot = Quaternion.Euler(0, 0, angle);

        Instantiate(breakablePrefab, spawnPos, rot);
    }


}
