using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class SimpleLaneDodger : ChallengeBase
{
    [SerializeField] private LaneDodgerConfig config;

    [Header("GeneralSettings")]
    [SerializeField] private List<GameObject> spawnPrefabs;
    public int ballCount = 3;
    public float spawnInterval = 0.6f;
    public float randomizeSpawnIntervalAmount = 0.2f;
    public float obstacleActiveTime = 10f;

    public Vector3 spawnPosition;


    [Header("IBossReactive")]
    [SerializeField] private bool reverse = false;
    [SerializeField] private float reverseAtTime = -1f;
    [SerializeField] private SoundEffect reverseSound;
    [SerializeField] private TimeSlowImpulseData impulseData;


    private int ballsAlive = 0;
    //private bool registered = false;
    private List<BasicProjectile> balls = new List<BasicProjectile>();

    private float _elapsed = 0f;
    private bool _hasReversed = false;
    private bool _spawnProjectiles = true;

    private List<IReversible> _reversibleObjects = new List<IReversible>();

    void Start()
    {
        balls = new List<BasicProjectile>();
        ballsAlive = ballCount;

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

        var mod = new TimeScaleModifier("Impulse_SimpleLaneDodger", 1f);
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


   private IEnumerator SpawnBallsRoutine()
    {
        
        int spawned = 0;
        

        while (spawned < ballCount)
        {

            if (_spawnProjectiles)
            {
                SpawnSingleBall();
            }

            spawned++;

            float offset = Random.Range(-randomizeSpawnIntervalAmount, randomizeSpawnIntervalAmount);
            yield return new WaitForSeconds(spawnInterval + offset);
        }
        

        yield return new WaitForSeconds(obstacleActiveTime);
        
        End();
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
            _reversibleObjects.Add(mover);
        }
        BasicProjectile sb = ball.GetComponent<BasicProjectile>();
        if (sb != null)
        {
            balls.Add(sb);
        }
    }

       

    protected override void CleanUp()
    {
        foreach (BasicProjectile spikyBall in balls)
        {
            if (spikyBall != null)
                Destroy(spikyBall);
        }

        balls.Clear();
    }

    public override void Begin(object config = null)
    {
        base.Begin(this.config);
        StartCoroutine(SpawnBallsRoutine());
    }

    public override void End()
    {
        base.End();
        Destroy(gameObject);
    }
}
