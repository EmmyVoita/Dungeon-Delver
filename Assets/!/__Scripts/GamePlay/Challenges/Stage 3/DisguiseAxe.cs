using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class DisguiseAxe : ChallengeBase
{
    [SerializeField] private LaneDodgerConfig config;

    [Header("Axe Prefab")]
    [SerializeField] private GameObject axePrefab;

    [Header("Audio")]
    [SerializeField] private SoundEffect axeAppearSound;
    [SerializeField] private SoundEffect axeExitSound;

    [Header("Memory Axe Phase")]
    [SerializeField] private int axeSpawnCount = 3;
    [SerializeField] private float memoryDisplayX = 5.5f;
    [SerializeField] private float memoryOffscreenX = 9f;
    [SerializeField] private float axeSpawnDelay = 0.12f;
    [SerializeField] private float axeYSpacing = 2f;
    [SerializeField] private float memoryHoldTime = 0.9f;
    [SerializeField] private float memoryExitDuration = 0.35f;
    [SerializeField] private float transparentAlpha = 0.35f;

    [Header("Sweep Axe Phase")]
    [SerializeField] private float sweepTriggerTime = 2.2f;
    [SerializeField] private float sweepStartX = 8f;
    [SerializeField] private bool randomizeSweepDirection = true;

    [Header("Fireball Phase")]
    [SerializeField] private List<GameObject> spawnPrefabs;
    [SerializeField] private int ballCount = 8;
    [SerializeField] private float spawnInterval = 0.6f;
    [SerializeField] private float randomizeSpawnIntervalAmount = 0.2f;
    [SerializeField] private float obstacleActiveTime = 5f;
    [SerializeField] private Vector3 spawnPosition;

    [Header("Boss Reverse")]
    [SerializeField] private float reverseAtTime = -1f;
    [SerializeField] private SoundEffect reverseSound;
    [SerializeField] private TimeSlowImpulseData impulseData;

    private readonly List<SpikyBall> _balls = new();
    private readonly List<GameObject> _memoryAxes = new();
    private readonly List<GameObject> _hazardAxes = new();
    private readonly List<IReversible> _reversibleObjects = new();

    private Coroutine _mainRoutine;
    private Coroutine _fireballRoutine;

    private int _safeLaneIndex = -1;
    private float _elapsed = 0f;
    private bool _hasReversed = false;
    private bool _spawnProjectiles = true;
    private bool _trackReverseTimer = false;

    void Start()
    {
        Begin();
    }

    void Update()
    {
        if (!IsActive || !_trackReverseTimer)
            return;

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

        _elapsed = 0f;
        _hasReversed = false;
        _spawnProjectiles = true;
        _trackReverseTimer = false;

        _balls.Clear();
        _memoryAxes.Clear();
        _hazardAxes.Clear();
        _reversibleObjects.Clear();

        _mainRoutine = StartCoroutine(RunSequence());
    }

    private IEnumerator RunSequence()
    {
        yield return StartCoroutine(ShowMemoryAxesSequence());

        _elapsed = 0f;
        _trackReverseTimer = true;

        _fireballRoutine = StartCoroutine(SpawnBallsRoutine());

        if (sweepTriggerTime > 0f)
            yield return new WaitForSeconds(sweepTriggerTime);

        SpawnSweepAxes();

        float remaining = Mathf.Max(0f, obstacleActiveTime - sweepTriggerTime);
        yield return new WaitForSeconds(remaining);

        End();
    }

    private IEnumerator ShowMemoryAxesSequence()
    {
        int laneCount = Mathf.Min(axeSpawnCount, config.maxLanes);
        _safeLaneIndex = Random.Range(0, laneCount);

        for (int i = 0; i < laneCount; i++)
        {
            float laneY = GetMemoryLaneY(i);
            Vector3 spawnPos = new Vector3(memoryDisplayX, laneY, 0f);

            GameObject axeObj = Instantiate(axePrefab, spawnPos, Quaternion.identity, transform);
            DisableHazardsForMemoryDisplay(axeObj);

            AudioHelpers.PlaySoundEffect(axeAppearSound, transform.position);

            if (i == _safeLaneIndex)
                SetAxeAlpha(axeObj, transparentAlpha);

            _memoryAxes.Add(axeObj);

            yield return new WaitForSeconds(axeSpawnDelay);
        }

        yield return new WaitForSeconds(memoryHoldTime);

        AudioHelpers.PlaySoundEffect(axeExitSound, transform.position);

        foreach (GameObject axe in _memoryAxes)
        {
            if (axe == null) continue;

            axe.transform.DOMoveX(memoryOffscreenX, memoryExitDuration)
                .SetEase(Ease.InQuad);
        }

        yield return new WaitForSeconds(memoryExitDuration);

        foreach (GameObject axe in _memoryAxes)
        {
            if (axe != null)
                Destroy(axe);
        }

        _memoryAxes.Clear();
    }

    private IEnumerator SpawnBallsRoutine()
    {
        int spawned = 0;

        while (spawned < ballCount)
        {
            if (_spawnProjectiles)
                SpawnSingleBall();

            spawned++;

            float offset = Random.Range(-randomizeSpawnIntervalAmount, randomizeSpawnIntervalAmount);
            float delay = Mathf.Max(0.05f, spawnInterval + offset);
            yield return new WaitForSeconds(delay);
        }
    }

    private void SpawnSweepAxes()
    {
        int laneCount = Mathf.Min(axeSpawnCount, config.maxLanes);

        for (int i = 0; i < laneCount; i++)
        {
           

            float laneY = GetMemoryLaneY(i);
            Vector3 spawnPos = new Vector3(sweepStartX, laneY, 0f);

            GameObject axeObj = Instantiate(axePrefab, spawnPos, Quaternion.identity, transform);

            if(i == _safeLaneIndex)
                axeObj.GetComponent<DamageEffect>().enabled = false;

            LaneMover mover = axeObj.GetComponent<LaneMover>();
            if (mover != null)
            {
                mover.Initialize(-1);
                _reversibleObjects.Add(mover);
            }

            DisguiseAxeProjectile projectile = axeObj.GetComponent<DisguiseAxeProjectile>();

            if(projectile != null)
            {
                projectile.Initialize(i == _safeLaneIndex);
            }

            _hazardAxes.Add(axeObj);
        }
    }

    private void SpawnSingleBall()
    {
        int direction = Random.Range(0, 2) == 0 ? -1 : 1;

        int lane = Random.Range(0, config.maxLanes);
        float laneY = GetLaneY(lane);

        float spawnX = direction * spawnPosition.x;
        Vector3 adjustedSpawnPos = new Vector3(spawnX, laneY, 0f);

        GameObject prefab = spawnPrefabs[Random.Range(0, spawnPrefabs.Count)];
        GameObject ball = Instantiate(prefab, adjustedSpawnPos, Quaternion.identity, transform);

        if (direction == 1)
        {
            Vector3 scale = ball.transform.localScale;
            scale.x *= -1f;
            ball.transform.localScale = scale;
        }

        LaneMover mover = ball.GetComponent<LaneMover>();
        if (mover != null)
        {
            mover.Initialize(-direction);
            _reversibleObjects.Add(mover);
        }

        SpikyBall sb = ball.GetComponent<SpikyBall>();
        if (sb != null)
            _balls.Add(sb);
    }

    private void ReverseAllProjectiles()
    {
        AudioHelpers.PlaySoundEffect(reverseSound, transform.position);

        _spawnProjectiles = false;

        for (int i = _reversibleObjects.Count - 1; i >= 0; i--)
        {
            IReversible reversible = _reversibleObjects[i];

            if (reversible == null)
            {
                _reversibleObjects.RemoveAt(i);
                continue;
            }

            reversible.Reverse();
        }

        //TimeManager.Instance.PlayImpulseSlow(impulseData);

        var mod = new TimeScaleModifier("Impulse_DisguiseAxe", 1f);
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

    private float GetLaneY(int lane)
    {
        float centerOffset = (config.maxLanes - 1) * 0.5f;
        return (lane - centerOffset) * config.laneSpacing;
    }

    private float GetMemoryLaneY(int laneIndex)
    {
        int laneCount = Mathf.Min(axeSpawnCount, config.maxLanes);
        float centerOffset = (laneCount - 1) * 0.5f;
        return (laneIndex - centerOffset) * config.laneSpacing * axeYSpacing;
    }

    private void SetAxeAlpha(GameObject axeObj, float alpha)
    {
        SpriteRenderer[] renderers = axeObj.GetComponentsInChildren<SpriteRenderer>();

        foreach (SpriteRenderer sr in renderers)
        {
            Color c = sr.color;
            c.a = alpha;
            sr.color = c;
        }
    }

    private void DisableHazardsForMemoryDisplay(GameObject axeObj)
    {
        Collider2D[] colliders = axeObj.GetComponentsInChildren<Collider2D>();

        foreach (Collider2D col in colliders)
            col.enabled = false;

        Rigidbody2D[] bodies = axeObj.GetComponentsInChildren<Rigidbody2D>();

        foreach (Rigidbody2D body in bodies)
            body.simulated = false;
    }

    protected override void CleanUp()
    {
        if (_mainRoutine != null)
        {
            StopCoroutine(_mainRoutine);
            _mainRoutine = null;
        }

        if (_fireballRoutine != null)
        {
            StopCoroutine(_fireballRoutine);
            _fireballRoutine = null;
        }

        foreach (SpikyBall ball in _balls)
        {
            if (ball != null)
                Destroy(ball.gameObject);
        }

        foreach (GameObject axe in _memoryAxes)
        {
            if (axe != null)
                Destroy(axe);
        }

        foreach (GameObject axe in _hazardAxes)
        {
            if (axe != null)
                Destroy(axe);
        }

        _balls.Clear();
        _memoryAxes.Clear();
        _hazardAxes.Clear();
        _reversibleObjects.Clear();
    }

    public override void End()
    {
        base.End();
        Destroy(gameObject);
    }
}