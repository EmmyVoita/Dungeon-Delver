using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SafeRingSpawner : MonoBehaviour
{
    public SafeZoneRing ringPrefab;
    public Transform playerCenter;
    public float spawnInterval = 3f;
    public float baseRadius = 2.5f;

    private List<Vector2> directions = new List<Vector2>
    {
        Vector2.up, Vector2.down, Vector2.left, Vector2.right
    };

    void Start()
    {
        StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        yield return new WaitForSeconds(1f);

        while (true)
        {
            SpawnRing();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    void SpawnRing()
    {
        Vector2 dir = directions[Random.Range(0, directions.Count)];
        RingDistance dist = (RingDistance)Random.Range(0, 3);

        SafeZoneRing ring = Instantiate(ringPrefab);
        ring.Init(playerCenter, dir, dist, baseRadius);
        ring.OnRingCollapsed += HandleRingCollapse;
    }

    void HandleRingCollapse(SafeZoneRing ring)
    {
        /*bool correctDir = Player.Instance.LastJumpDir == ring.direction;
        bool correctDist = Player.Instance.LastJumpDistanceTier == ring.distanceTier;

        if (correctDir && correctDist)
        {
            Debug.Log("Success! Jumped into safe zone!");
        }
        else
        {
            Debug.Log("Failed - wrong distance or direction!");
            Player.Instance.DamageSelf(1);
        }
        */
    }
}
