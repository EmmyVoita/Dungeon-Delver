using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class SineWaveCorridorChallenge : ChallengeBase
{
    [SerializeField] private LaneDodgerConfig config;
    [SerializeField] private GameObject fireballPrefab;

    [Header("Wave Settings")]
    [SerializeField] private int waveCount = 2;
    [SerializeField] private float amplitude = 2f;
    [SerializeField] private float frequency = 1.5f;
    [SerializeField] private float waveSpeed = 1f;
    [SerializeField] private float verticalSpacing = 1.5f;

    [Header("Spawn Settings")]
    [SerializeField] private float spawnDelay = 2f;
    [SerializeField] private float spawnWidth = 7f;
    [SerializeField] private int samplePoints = 8;
    [SerializeField] private float projectileSpeed = 3f;

    [Header("Timing")]
    [SerializeField] private float activeTime = 6f;



    [Header("Wave Movement")]
    [SerializeField] private float moveStartDelay = 1.5f;
    [SerializeField] private float moveAmplitude = 1.5f;
    [SerializeField] private float moveSpeed = 2f;

    [SerializeField] private float spacingExpandAmount = 1.0f;

    [Header("IBossReactive")]
    [SerializeField] private float reverseAtTime = -1f;
    [SerializeField] private SoundEffect reverseSound;
    [SerializeField] private TimeSlowImpulseData impulseData;

    [Header("Spawn Safety")]
    [SerializeField] private float startSpacingMultiplier = 2f;
    [SerializeField] private float spacingLerpDuration = 1.0f;

    [Header("Audio")]
    [SerializeField] private SoundEffect spawnSound;

    private List<IReversible> _reversibleObjects = new();
    private float _time;
    private float _direction = 1f;
    private bool _hasReversed;
    private List<Transform> _parentObjs = new List<Transform>();
    private List<GameObject> _spawnedObjs = new List<GameObject>();
    private float _spacingLerpTime = 0f;
    private bool _isLerpingSpacing = false;

    private void Start()
    {
        Begin();
    }

    public override void Begin(object config = null)
    {
        base.Begin(this.config);

        _time = 0f;
        _direction = 1f;
        _hasReversed = false;

        StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        yield return new WaitForSeconds(spawnDelay);

        SpawnWave();

        yield return new WaitForSeconds(0.5f);

        _spacingLerpTime = 0f;
        _isLerpingSpacing = true;

        yield return new WaitForSeconds(activeTime);

        //StartCoroutine(MoveWavesRoutine());

        //yield return new WaitForSeconds(activeTime - moveStartDelay);

        AudioHelpers.PlaySoundEffect(spawnSound, transform.position);

        End();
    }

    /*
    private IEnumerator MoveWavesRoutine()
    {
        float t = 0f;

        while (true)
        {
            t += Time.deltaTime * moveSpeed;

            float offset = Mathf.Sin(t) * moveAmplitude;

            for (int i = 0; i < _parentObjs.Count; i++)
            {
                Transform parent = _parentObjs[i];
                if (parent == null) continue;

                // base spacing
                float baseOffset = (i - (waveCount - 1) * 0.5f) * verticalSpacing;

                // 🔥 expand spacing while moving
                float expandedSpacing = baseOffset + Mathf.Sign(baseOffset) * spacingExpandAmount;

                // blend between base and expanded
                float finalOffset = Mathf.Lerp(baseOffset, expandedSpacing, Mathf.Abs(Mathf.Sin(t)));

                parent.localPosition = new Vector3(
                    0f,
                    offset + finalOffset,
                    0f
                );
            }

            yield return null;
        }
    }
    */

    private void Update()
    {
        if (!IsActive) return;

        _time += Time.deltaTime;

        if (!_hasReversed && reverseAtTime >= 0f && _time >= reverseAtTime)
        {
            ReverseAllProjectiles();
            _hasReversed = true;
        }

        UpdateWaveSpacing();
    }

    private void UpdateWaveSpacing()
    {
        if (!_isLerpingSpacing) return;

        _spacingLerpTime += Time.deltaTime;

        float t = Mathf.Clamp01(_spacingLerpTime / spacingLerpDuration);

        float startSpacing = verticalSpacing * startSpacingMultiplier;
        float currentSpacing = Mathf.Lerp(startSpacing, verticalSpacing, t);

        for (int i = 0; i < _parentObjs.Count; i++)
        {
            if (_parentObjs[i] == null) continue;

            float baseOffset = (i - (waveCount - 1) * 0.5f) * currentSpacing;

            _parentObjs[i].localPosition = new Vector3(0f, baseOffset, 0f);
        }

        if (t >= 1f)
            _isLerpingSpacing = false;
    }

    private void SpawnWave()
    {
        for (int i = 0; i < waveCount; i++)
        {
            GameObject parentObj = new GameObject($"WaveParent_{i}");
            parentObj.transform.SetParent(transform, false);

            float startSpacing = verticalSpacing * startSpacingMultiplier;
            float baseOffset = (i - (waveCount - 1) * 0.5f) * startSpacing;

            parentObj.transform.localPosition = new Vector3(0f, baseOffset, 0f);

            _parentObjs.Add(parentObj.transform);
        }

        for (int i = 0; i < samplePoints; i++)
        {
            float t = (float)i / (samplePoints - 1);
            float x = Mathf.Lerp(-spawnWidth, spawnWidth, t);

            for(int j = 0; j < waveCount; j++)
            {
                SpawnProjectile(new Vector3(x, 0f, 0), x, j);
            }
        }

        AudioHelpers.PlaySoundEffect(spawnSound, transform.position);
    }



    private void SpawnProjectile(Vector3 pos, float x, int i)
    {
        GameObject obj = Instantiate(fireballPrefab, pos, Quaternion.identity, _parentObjs[i]);

        SineWaveMover mover = obj.GetComponent<SineWaveMover>();

        if (mover != null)
        {
            float waveDir = (i % 2 == 0) ? 1f : -1f;

            mover.Initialize(
                amplitude,
                frequency,
                waveDir: waveDir,
                x,
                _time
            );

            float y = mover.GetInitialY();
            obj.transform.localPosition = new Vector3(x, y, 0f);

            _reversibleObjects.Add(mover);
        }

        _spawnedObjs.Add(obj);
    }

    private void ReverseAllProjectiles()
    {
        _direction *= -1;
        AudioHelpers.PlaySoundEffect(reverseSound,transform.position);


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
        
        var mod = new TimeScaleModifier("Impulse_SineWave", 1f);
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
        foreach (Transform p in _parentObjs)
        {
            if (p != null)
                Destroy(p.gameObject);
        }

        foreach(GameObject obj in _spawnedObjs)
        {
            if(obj != null)
                Destroy(obj);
        }
    }

    public override void End()
    {
        base.End();
        Destroy(gameObject);
    }
}