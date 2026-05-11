using UnityEngine;
using System.Collections;

public class OrbitingBlackHoleArmSpawner : ChallengeBase
{
    [Header("Prefab")]
    public OrbitingBlackHoleArm obstaclePrefab;

    [Header("Spawn")]
    public Vector3 centerTarget;
    public Vector3 spawnOffset = Vector3.zero;

    [Header("Lifetime")]
    public float lifetime = 6f;

    private void Start()
    {
        Vector3 spawnPos = (centerTarget != null ? centerTarget : Vector3.zero) + spawnOffset;

        OrbitingBlackHoleArm obj = Instantiate(obstaclePrefab, spawnPos, Quaternion.identity, transform);
        obj.centerTarget = centerTarget;

        Begin();
    }

    private IEnumerator ExpireRoutine()
    {
        yield return new WaitForSeconds(lifetime);
        End();
    }

    public override void Begin(object config = null)
    {
        base.Begin();

        StartCoroutine(ExpireRoutine());
    }

    public override void End()
    {
        base.End();
        Destroy(gameObject);
    }
}