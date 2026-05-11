using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

[System.Serializable]
public struct RingEmitterData
{
    public Vector3[] emitterPoints;
}

public class MultiEmitterRingChallenge : ChallengeBase
{
    [SerializeField] private GameObject indicatorPrefab;
    [SerializeField] private GameObject fireballPrefab;
    [SerializeField] private List<RingEmitterData> ringEmitterData = new List<RingEmitterData>();

    [Header("Ring Settings")]
    [SerializeField] private int ringOrbCount = 24;
    [SerializeField] private int ringsPerRound = 4;
    [SerializeField] private float delayBetweenRings = 0.4f;
    [SerializeField] private float ringSpeed = 4f;

    [Header("Indicator Settings")]
    [SerializeField] private float indicatorDuration = 1.0f;

    [Header("Timing")]
    [SerializeField] private float endDelay = 2f;
    [SerializeField] private float spawnInterval = 1f;

    [Header("IBossReactive")]
    [SerializeField] private float reverseDelay = 1.5f;
    [SerializeField] private SoundEffect reverseSound;
    [SerializeField] private TimeSlowImpulseData impulseData;

    private List<IReversible> _reversibleObjects = new();
    private List<GameObject> _spawnedObjs = new();
    private List<GameObject> _indicatorObjs = new();

    void Start()
    {
        Begin();
    }

    public override void Begin(object config = null)
    {
        base.Begin(config);
        StartCoroutine(RunSequence());
    }

    private IEnumerator RunSequence()
    {

        for(int i = 0; i < ringEmitterData.Count; i++)
        {
            yield return StartCoroutine(SpawnRound(ringEmitterData[i]));

            // 🔥 Reverse before impact
            yield return new WaitForSeconds(reverseDelay);
            ReverseAll();

            yield return new WaitForSeconds(endDelay);

            CleanUp();

            yield return new WaitForSeconds(spawnInterval);
        }

        

        End();
    }

    private IEnumerator SpawnRound(RingEmitterData ringData)
    {

        for (int i = 0; i < ringData.emitterPoints.Length; i++)
        {
            SpawnRingIndicator(ringData.emitterPoints[i]);

            yield return null;
        }

        yield return new WaitForSeconds(indicatorDuration);

        foreach(GameObject indicator in _indicatorObjs)
        {
            if(indicator != null)
                Destroy(indicator);
        }

        for (int i = 0; i < ringData.emitterPoints.Length; i++)
        {
            SpawnRing(ringData.emitterPoints[i]);

            yield return new WaitForSeconds(delayBetweenRings);
        }
    }

    private void SpawnRingIndicator(Vector3 center)
    {
        GameObject indicator = Instantiate(indicatorPrefab, center, Quaternion.identity, transform);
        _indicatorObjs.Add(indicator);
    }

    private void SpawnRing(Vector3 center)
    {
        float angleStep = 360f / ringOrbCount;

        for (int i = 0; i < ringOrbCount; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

            GameObject orb = Instantiate(fireballPrefab, center, Quaternion.identity, transform);

            ExpandingRingOrb mover = orb.GetComponent<ExpandingRingOrb>();
            if (mover != null)
            {
                mover.Initialize(dir, ringSpeed);
                _reversibleObjects.Add(mover);
            }

            _spawnedObjs.Add(orb);
        }
    }

    private void ReverseAll()
    {
        AudioHelpers.PlaySoundEffect(reverseSound,transform.position);

        foreach (var r in _reversibleObjects)
        {
            if (r != null)
                r.Reverse();
        }

        var mod = new TimeScaleModifier("Impulse_EmitterRings", 1f);
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
        foreach (var obj in _spawnedObjs)
        {
            if (obj != null)
                Destroy(obj);
        }

        _spawnedObjs.Clear();
        _reversibleObjects.Clear();
        _indicatorObjs.Clear();
    }

    public override void End()
    {
        base.End();
        Destroy(gameObject);
    }
}