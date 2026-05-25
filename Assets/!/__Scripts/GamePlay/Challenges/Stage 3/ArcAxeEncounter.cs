using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class ArcAxeEncounter : ChallengeBase
{
    [SerializeField] private LaneDodgerConfig config;
    [Header("Prefab")]
    [SerializeField] private GameObject axePrefab;

    [SerializeField] private SoundEffect spawnSound;

    [Header("Spawn Settings")]
    [SerializeField] private Vector2 spawnX = new Vector2(6,8);
    [SerializeField] private float minY = -3f;
    [SerializeField] private float maxY = 3f;

    [Header("Arc Settings")]
    [SerializeField] private float arcHeight = 2.5f;
    [SerializeField] private float arcYOffset = 0f;
    [SerializeField] private float travelDuration = 1.2f;

    [Header("Timing")]
    [SerializeField] private float baseInterval = 1.2f;
    [SerializeField] private float intervalStep = 0.1f;
    [SerializeField] private float minInterval = 0.4f;
    [SerializeField] private float activeTime = 10f;

    private List<GameObject> _axes = new List<GameObject>();
    private List<IReversible> _reversibleObjects = new List<IReversible>();
    private Coroutine _routine;
    private bool _spawnProjectiles = true;

    [Header("IBossReactive")]
    [SerializeField] private bool reverse = false;
    [SerializeField] private float reverseAtTime = -1f;
    [SerializeField] private SoundEffect reverseSound;
    [SerializeField] private TimeSlowImpulseData impulseData;

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

        _axes.Clear();
        _routine = StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        float currentInterval = baseInterval;
        float elapsed = 0f;

        while (elapsed < activeTime)
        {
            if(_spawnProjectiles)
                SpawnAxe();

            yield return new WaitForSeconds(currentInterval);

            elapsed += currentInterval;
            currentInterval = Mathf.Max(minInterval, currentInterval - intervalStep);
        }

        End();
    }

    private void SpawnAxe()
    {
        int direction = Random.value < 0.5f ? -1 : 1;

        float startY = Random.Range(minY, maxY);

        float _spawnX = Random.Range(spawnX.x,spawnX.y);

        Vector3 start = new Vector3(direction * _spawnX, startY, 0);
        Vector3 end = new Vector3(-direction * _spawnX, startY, 0);

        GameObject axe = Instantiate(axePrefab, start, Quaternion.identity, transform);

        ArcProjectile proj = axe.GetComponent<ArcProjectile>();
        if (proj != null)
        {
            proj.Initialize(start, end, travelDuration, arcHeight, arcYOffset);
            _reversibleObjects.Add(proj);
        }

        AudioHelpers.PlaySoundEffect(spawnSound, transform.position);

        _axes.Add(axe);
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

        var mod = new TimeScaleModifier("Impulse_ArcAxe", 1f);
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

    protected override void CleanUp()
    {
        if (_routine != null)
        {
            StopCoroutine(_routine);
            _routine = null;
        }

        foreach (var axe in _axes)
        {
            if (axe != null)
                Destroy(axe);
        }

        _axes.Clear();
    }

    public override void End()
    {
        base.End();
        Destroy(gameObject);
    }
}