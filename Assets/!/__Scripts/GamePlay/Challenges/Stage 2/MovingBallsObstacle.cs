using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MovingBallsObstacle : ChallengeBase
{
    [Header("GeneralSettings")]
    public GameObject ballPrefab;
    public int ballCount = 3;
    public float spawnInterval = 0.6f;
    public float randomizeSpawnIntervalAmount = 0.2f;
    public float travelDistance = 1.0f;
    public float speed = 1.0f;
    public float obstacleActiveTime = 10f;

    public Vector3 spawnPosition;

    [Header("Pattern Variation")]
    [Range(0f, 1f)]
    public float skipChance = 0.25f;   // 25% chance to skip a slot

    private List<BasicProjectile> _projectiles;
    private Coroutine _spawnRoutine;

    void Start()
    {
        _projectiles = new List<BasicProjectile>();
        Begin();
    }

   private IEnumerator SpawnBallsRoutine()
    {
        int spawned = 0;
        bool lastWasSkip = false;

        while (spawned < ballCount)
        {
            bool shouldSkip = false;

            // Only allow skip if last one was NOT a skip
            if (!lastWasSkip && Random.value < skipChance)
            {
                shouldSkip = true;
            }

            if (!shouldSkip)
            {
                SpawnSingleBall();
                spawned++;
                lastWasSkip = false;
            }
            else
            {
                lastWasSkip = true;
            }

            float offset = Random.Range(-randomizeSpawnIntervalAmount, randomizeSpawnIntervalAmount);
            yield return new WaitForSeconds(spawnInterval + offset);
        }

        yield return new WaitForSeconds(obstacleActiveTime);

    
        CleanUp();
        End();
    }

    protected override void CleanUp()
    {
        if(_spawnRoutine != null)
        {
            StopCoroutine(_spawnRoutine);
            _spawnRoutine = null;
        }

        foreach (BasicProjectile projectile in _projectiles)
        {
            if (projectile != null)
                projectile.DestroyProjectile(true);
        }

        base.CleanUp();
    }


    private void SpawnSingleBall()
    {
        GameObject obj = Instantiate(
            ballPrefab,
            spawnPosition,
            Quaternion.identity,
            transform
        );

        OscillateMovement movement = obj.gameObject.AddComponent<OscillateMovement>();

        movement.direction = Vector3.right;
        movement.distance = travelDistance;
        movement.speed = speed;

        BasicProjectile projectile = obj.GetComponent<BasicProjectile>();
        
        if(projectile != null)
        {
            _projectiles.Add(projectile);
        }
    }


    public override void Begin(object config = null)
    {
        base.Begin();
        _spawnRoutine = StartCoroutine(SpawnBallsRoutine());
    }

    public override void End()
    {
        base.End();
        Destroy(gameObject);
    }
}
