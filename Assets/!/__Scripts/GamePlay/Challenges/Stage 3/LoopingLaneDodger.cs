using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class LoopingLaneDodger : ChallengeBase
{
    [SerializeField] private LaneDodgerConfig config;

    [Header("Prefabs")]
    [SerializeField] private List<GameObject> spawnPrefabs;

    [Header("Wave Settings")]
    [SerializeField] private int projectileMinCount = 2;
    [SerializeField] private int projectileMaxCount = 3;

    [Header("Timing")]
    [SerializeField] private float baseSpawnInterval = 0.6f;
    [SerializeField] private float spawnIntervalStep = 0.05f;
    [SerializeField] private float minSpawnInterval = 0.2f;
    [SerializeField] private float obstacleActiveTime = 10f;

    [Header("Spawn Position")]
    [SerializeField] private float spawnX = 7f;

    [Header("IBossReactive")]
    [SerializeField] private bool reverse = false;
    [SerializeField] private float reverseAtTime = -1f;
    [SerializeField] private SoundEffect reverseSound;
    [SerializeField] private TimeSlowImpulseData impulseData;

    private List<BasicProjectile> _activeBalls = new List<BasicProjectile>();
    private List<IReversible> _reversibleObjects = new List<IReversible>();
    private Coroutine _spawnRoutine;
    private bool _spawnProjectiles = true;
    private float _elapsed = 0f;
    private bool _hasReversed = false;


    void Start()
    {
        Begin();
    }


    void Update()
    {
        if(!BossManager.Instance.IsBossActive || !reverse) return;

        if (!IsActive) return;

        _elapsed += Time.deltaTime;

        if (!_hasReversed && reverseAtTime >= 0f && _elapsed >= reverseAtTime)
        {
            ReverseAllProjectiles();
            _hasReversed = true;
        }
    }

    public override void Begin(object config = null)
    {
        base.Begin(this.config);

        _activeBalls.Clear();

        _spawnRoutine = StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        float currentInterval = baseSpawnInterval;
        float elapsed = 0f;

        while (elapsed < obstacleActiveTime)
        {
            if(_spawnProjectiles)
                SpawnWave();

            yield return new WaitForSeconds(currentInterval);

            elapsed += currentInterval;
            currentInterval = Mathf.Max(minSpawnInterval, currentInterval - spawnIntervalStep);
        }

        End();
    }

    // =========================
    // Wave Logic
    // =========================

    private void SpawnWave()
    {
        int laneCount = config.maxLanes;

        // Pick how many projectiles to spawn
        int projectileCount = Random.Range(projectileMinCount, projectileMaxCount + 1);

        // Ensure at least 1 safe lane
        projectileCount = Mathf.Min(projectileCount, laneCount - 1);

        int direction = Random.Range(0, 2) == 0 ? -1 : 1;

        List<int> lanes = GetRandomLanes(laneCount, projectileCount);

        foreach (int lane in lanes)
        {
            SpawnProjectileInLane(lane, direction);
        }
    }

    private List<int> GetRandomLanes(int maxLanes, int count)
    {
        List<int> available = new List<int>();

        for (int i = 0; i < maxLanes; i++)
            available.Add(i);

        // Shuffle
        for (int i = 0; i < available.Count; i++)
        {
            int rand = Random.Range(i, available.Count);
            (available[i], available[rand]) = (available[rand], available[i]);
        }

        return available.GetRange(0, count);
    }

    private void SpawnProjectileInLane(int lane, int direction)
    {
        float laneY = GetLaneY(lane);
        float x = direction * spawnX;

        Vector3 spawnPos = new Vector3(x, laneY, 0);

        GameObject prefab = spawnPrefabs[Random.Range(0, spawnPrefabs.Count)];

        GameObject obj = Instantiate(prefab, spawnPos, Quaternion.identity, transform);

        // Flip if coming from left
        if (direction == 1)
        {
            Vector3 scale = obj.transform.localScale;
            scale.x *= -1;
            obj.transform.localScale = scale;
        }

        LaneMover mover = obj.GetComponent<LaneMover>();
        if (mover != null)
        {
            mover.Initialize(-direction);
            _reversibleObjects.Add(mover); 
        }

        BasicProjectile ball = obj.GetComponent<BasicProjectile>();
        if (ball != null)
        {
            _activeBalls.Add(ball);
        }
    }

    private float GetLaneY(int lane)
    {
        float centerOffset = (config.maxLanes - 1) * 0.5f;
        return (lane - centerOffset) * config.laneSpacing;
    }

    private void ReverseAllProjectiles()
    {
        AudioHelpers.PlaySoundEffect(reverseSound,transform.position);

        _spawnProjectiles = false;

        for (int i = _reversibleObjects.Count - 1; i >= 0; i--)
        {
            var r = _reversibleObjects[i];

            if (r == null)
            {
                _reversibleObjects.RemoveAt(i);
                continue;
            }

            r.Reverse();
        }

        //TimeManager.Instance.PlayImpulseSlow(impulseData);

        var mod = new TimeScaleModifier("Impulse_FireWall", 1f);
        TimeManager.Instance.AddModifier(mod);

        DOTween.Sequence()
            .SetUpdate(true)

            .Append(DOTween.To(
                () => mod.Value,
                x => mod.SetValue(x),
                impulseData.slowMultiplier,
                impulseData.inDuration
            ).SetEase(Ease.OutSine))

            .AppendInterval(impulseData.holdDuration)

            .Append(DOTween.To(
                () => mod.Value,
                x => mod.SetValue(x),
                1f,
                impulseData.outDuration
            ).SetEase(Ease.InSine))

            .OnComplete(() =>
            {
                TimeManager.Instance.RemoveModifier(mod.Id);
            });
    }

    // =========================
    // Cleanup
    // =========================

    protected override void CleanUp()
    {
        if (_spawnRoutine != null)
        {
            StopCoroutine(_spawnRoutine);
            _spawnRoutine = null;
        }

        foreach (BasicProjectile ball in _activeBalls)
        {
            if (ball != null)
                Destroy(ball.gameObject);
        }

        _activeBalls.Clear();
    }

    public override void End()
    {
        base.End();
        Destroy(gameObject);
    }
}