using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MovingBallsObstacle : MonoBehaviour
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


    private int ballsAlive = 0;
    private bool registered = false;
    private List<SpikyBall> balls;

    void Start()
    {
        balls = new List<SpikyBall>();
        ObstacleManager.Instance.RegisterObstacle(gameObject);
        registered = true;
        StartCoroutine(SpawnBallsRoutine());
        ballsAlive = ballCount;
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

        foreach (SpikyBall spikyBall in balls)
        {
            if (spikyBall != null)
                spikyBall.FadeOut();
        }

        if (registered && ObstacleManager.Instance != null)
        {
            ObstacleManager.Instance.UnregisterObstacle(gameObject);
            Destroy(this.gameObject);
        }
    }



    private void SpawnSingleBall()
    {
        GameObject ball = Instantiate(
            ballPrefab,
            spawnPosition,
            Quaternion.identity,
            transform
        );

        OscillateMovement movement = ball.gameObject.AddComponent<OscillateMovement>();

        movement.direction = Vector3.right;
        movement.distance = travelDistance;
        movement.speed = speed;

        SpikyBall sb = ball.GetComponent<SpikyBall>();
        if(sb != null)
        {
            balls.Add(sb);
        }

        //
    }

    /*
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
    */

    private void OnDestroy()
    {
        if (registered && ObstacleManager.Instance != null)
        {
            ObstacleManager.Instance.UnregisterObstacle(gameObject);
        }
    }
}
