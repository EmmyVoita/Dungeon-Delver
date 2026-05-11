using UnityEngine;
using System.Collections;

public class RingFormationSpawner : ChallengeBase
{
    public enum RotationPattern
    {
        AllSameDirection,
        MiddleOpposite
    }

    [Header("Rotation Pattern")]
    public RotationPattern rotationPattern = RotationPattern.AllSameDirection;

    [Header("Ring Setup")]
    public RotatingRingObstacle ringPrefab;
    public int ringCount = 3;

    public float minRadius = 2f;
    public float maxRadius = 4f;
    public float spawnDelay = 0.3f;

    [Header("Movement")]
    public Vector3 startPosition;
    public Vector3 endPosition;
    public float moveDuration = 3f;

    private Vector3 moveDirection;
    private float totalDistance;

    void Start()
    {
        // Force exact start position
        transform.position = startPosition;

        // Precompute movement data
        totalDistance = Vector3.Distance(startPosition, endPosition);
        moveDirection = (endPosition - startPosition).normalized;

        rotationPattern = (Random.value < 0.5f)
            ? RotationPattern.AllSameDirection
            : RotationPattern.MiddleOpposite;

        Begin();
    }

    private IEnumerator SpawnRoutine()
    {
        for (int i = 0; i < ringCount; i++)
        {
            float t = (ringCount == 1) ? 0f : (float)i / (ringCount - 1);
            float radius = Mathf.Lerp(minRadius, maxRadius, t);

            SpawnRing(i, radius);

            yield return new WaitForSeconds(spawnDelay);
        }
    }

    private void SpawnRing(int index, float radius)
    {
        RotatingRingObstacle ring = Instantiate(
            ringPrefab,
            transform.position,
            Quaternion.identity,
            this.transform
        );

        int direction = 1;

        if (rotationPattern == RotationPattern.MiddleOpposite)
        {
            int middleIndex = ringCount / 2;

            if (index == middleIndex)
                direction = -1;
        }

        ring.Initialize(index, radius, direction);
    }

    private IEnumerator MoveRoutine()
    {
        float speed = totalDistance / moveDuration;
        float traveled = 0f;

        while (traveled < totalDistance)
        {
            float step = speed * Time.deltaTime;
            transform.position += moveDirection * step;

            traveled += step;

            yield return null;
        }

        // Snap exactly to end
        transform.position = endPosition;

        End();
    }

    public override void Begin(object config = null)
    {
        base.Begin();

        StartCoroutine(SpawnRoutine());
        StartCoroutine(MoveRoutine());
    }

    public override void End()
    {
        base.End();
        Destroy(gameObject);
    }
}