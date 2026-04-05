using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SimpleLaneDodger : MonoBehaviour
{
    [SerializeField] private LaneDodgerConfig config;

    [Header("GeneralSettings")]
    [SerializeField] private List<GameObject> spawnPrefabs;
    public int ballCount = 3;
    public float spawnInterval = 0.6f;
    public float randomizeSpawnIntervalAmount = 0.2f;
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

        Player.Instance.SetPlayerControlState(Player.PlayerControlState.LaneDodger, config);

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

            Player.Instance.SetPlayerControlState(Player.PlayerControlState.Normal);
            Destroy(this.gameObject);
        }
    }

    float GetLaneY(int lane)
    {
        float centerOffset = (config.maxLanes - 1) * 0.5f;
        return (lane - centerOffset) * config.laneSpacing;
    }

    private void SpawnSingleBall()
    {
        int direction = Random.Range(0, 2) == 0 ? -1 : 1;

        int lane = Random.Range(0, config.maxLanes);
        float laneY = GetLaneY(lane);

        float spawnX = direction * spawnPosition.x;
        Vector3 adjustedSpawnPos = new Vector3(spawnX, laneY, 0);

        GameObject prefab = spawnPrefabs[Random.Range(0, spawnPrefabs.Count)];

        GameObject ball = Instantiate(
            prefab,
            adjustedSpawnPos,
            Quaternion.identity,
            transform
        );

        if (direction == -1)
        {
            Vector3 scale = ball.transform.localScale;
            scale.x *= -1;
            ball.transform.localScale = scale;
        }

        LaneMover mover = ball.GetComponent<LaneMover>();
        if (mover != null)
        {
            mover.Initialize(direction);
        }
        SpikyBall sb = ball.GetComponent<SpikyBall>();
        if (sb != null)
        {
            balls.Add(sb);
        }
    }

    private void OnDestroy()
    {
        if (registered && ObstacleManager.Instance != null)
        {
            ObstacleManager.Instance.UnregisterObstacle(gameObject);
        }
    }
}
