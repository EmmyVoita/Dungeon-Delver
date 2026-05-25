using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RingObstacleSpawner : ChallengeBase
{
    [Header("Ring Settings")]
    public ShrinkingRingObstacle ringPrefab;
    public int ringCount = 3;
    public float spawnInterval = 0.6f;

    public Transform centerTarget;

    private int ringsAlive = 0;

    void Start()
    {
        ringsAlive = ringCount;

        Begin();
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

        ring.Initialize(i,this,centerTarget);
    }

    public void OnRingResolved(ShrinkingRingObstacle ring)
    {
        ringsAlive--;

        if (ringsAlive <= 0)
        {
            End();
        }
    }


    public override void Begin(object config = null)
    {
        base.Begin();

        StartCoroutine(SpawnRingsRoutine());
    }

    public override void End()
    {
        base.End();

        /*
        foreach (Transform child in transform)
        {
            var challenge = child.GetComponent<ChallengeBase>();
            if (challenge != null)
            {
                challenge.End();
            }
        }
        */

        Destroy(gameObject);
    }
}
