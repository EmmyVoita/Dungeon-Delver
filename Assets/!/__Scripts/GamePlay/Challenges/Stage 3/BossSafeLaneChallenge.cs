using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class BossSafeLaneChallenge : ChallengeBase
{
    [SerializeField] private LaneDodgerConfig config;


    [Header("Prefabs")]
    [SerializeField] private List<GameObject> spawnPrefabs;

    [Header("Timing")]
    [SerializeField] private float safeLaneInterval = 1.5f;
    [SerializeField] private float safeLaneWarningDuration = 0.75f;
    [SerializeField] private float obstacleActiveTime = 10f;

    [Header("Spawn")]
    [SerializeField] private float spawnX = 7f;


    private List<BasicProjectile> _activeBalls = new();
    private List<IReversible> _reversibleObjects = new();

    private Coroutine _spawnRoutine;

    private bool _spawnProjectiles = true;
    private bool _hasReversed = false;

    private float _elapsed;

    private int _currentSafeLane = -1;



    void Start()
    {
        Begin();
    }

    public override void Begin(object config = null)
    {
        //base.Begin(this.config);

        _activeBalls.Clear();

    
        _elapsed = 0f;
        _hasReversed = false;

        if (BossManager.Instance.IsBossActive)
        {
            _spawnRoutine = StartCoroutine(
                SpawnRoutine()
            );
        }
    }


    void Update()
    {
        if (!BossManager.Instance.IsBossActive)
            return;

        if (!IsActive)
            return;

        _elapsed += Time.deltaTime;
    }


    private IEnumerator SpawnRoutine()
    {
        yield return null;

        float elapsed = 0f;

        while (elapsed < obstacleActiveTime)
        {
            SelectSafeLane();

            yield return new WaitForSeconds(
                safeLaneWarningDuration
            );

            if (_spawnProjectiles)
            {
                SpawnSafeLaneWave();
            }

            yield return new WaitForSeconds(
                safeLaneInterval
            );

            elapsed +=
                safeLaneWarningDuration +
                safeLaneInterval;
        }

        End();
    }


    private void SelectSafeLane()
    {
        LaneVisualizer.RequestClearLaneHighlights();

        int laneCount = LaneState.MaxLanes;

        int previous = _currentSafeLane;

        do
        {
            _currentSafeLane =
                Random.Range(0, laneCount);

        }
        while (
            laneCount > 1 &&
            _currentSafeLane == previous
        );

        LaneVisualizer.RequestHighlightLane(_currentSafeLane);
    }


    private void SpawnSafeLaneWave()
    {
        int laneCount = LaneState.MaxLanes;

        int direction =
            Random.Range(0, 2) == 0
            ? -1
            : 1;

        for (int lane = 0; lane < laneCount; lane++)
        {
            if (lane == _currentSafeLane)
                continue;

            SpawnProjectileInLane(
                lane,
                direction
            );
        }
    }


    private void SpawnProjectileInLane(
        int lane,
        int direction)
    {
        float laneY = GetLaneY(lane);

        float x = direction * spawnX;

        Vector3 spawnPos =
            new Vector3(
                x,
                laneY,
                0
            );

        GameObject prefab =
            spawnPrefabs[
                Random.Range(
                    0,
                    spawnPrefabs.Count
                )
            ];

        GameObject obj =
            Instantiate(
                prefab,
                spawnPos,
                Quaternion.identity,
                transform
            );

        if (direction == 1)
        {
            Vector3 scale =
                obj.transform.localScale;

            scale.x *= -1;

            obj.transform.localScale =
                scale;
        }

        LaneMover mover =
            obj.GetComponent<LaneMover>();

        if (mover != null)
        {
            mover.Initialize(
                -direction
            );

            _reversibleObjects.Add(
                mover
            );
        }

        BasicProjectile ball =
            obj.GetComponent<BasicProjectile>();

        if (ball != null)
        {
            _activeBalls.Add(
                ball
            );
        }
    }


    private float GetLaneY(int lane)
    {
        float centerOffset =
            (LaneState.MaxLanes - 1)
            * 0.5f;

        return
            (lane - centerOffset)
            * LaneState.LaneSpacing;
    }



    protected override void CleanUp()
    {
        if (_spawnRoutine != null)
        {
            StopCoroutine(
                _spawnRoutine
            );

            _spawnRoutine = null;
        }

        foreach (var ball in _activeBalls)
        {
            if (ball != null)
                Destroy(
                    ball.gameObject
                );
        }

        _activeBalls.Clear();
    }


    public override void End()
    {
        //base.End();
        CleanUp();

        Destroy(gameObject);
    }
}