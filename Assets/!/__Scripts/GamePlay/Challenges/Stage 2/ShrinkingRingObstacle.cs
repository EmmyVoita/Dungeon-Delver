using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ShrinkingRingObstacle : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private DamageEffect dEf;
    [SerializeField] private GameObject destroyEffectPrefab;
    [SerializeField] private GameObject segmentPrefab;
    [SerializeField] private GameObject ringHolePrefab;


    [Header("Ring Generation")]
    [SerializeField] private int segmentCount = 24;     // number of pieces making the ring
    [SerializeField] private float ringRadius = 3f;     // spawn distance
    [SerializeField] private float shrinkSpeed = 0.5f;  // units per second
    [SerializeField] private int gapSize = 3;           // number of missing segments


    [Header("Rotation")]
    [SerializeField] private bool autoRotate = true;
    [SerializeField] private float rotationSpeed = 60f;
    [SerializeField] private float rotationSpeedCountIncrease = 10f;

  
    [Header("Player Hit Settings")]
    [SerializeField] private float hitRadius = 0.6f;   // When the ring shrinks this close, it hits


    [Header("Audio")]
    [SerializeField] private float pitchStep = 0.05f;
    [SerializeField] private SoundEffect failSound;
    [SerializeField] private SoundEffect passThroughSound;

    

    private List<GameObject> _segments = new List<GameObject>();
    private bool _completed = false;
    private float _currentRadius;
    private bool _shouldShrink;
    private int _ringNumber;
    private float _finalRotationSpeed;
    private Transform _centerTarget;
    private RingObstacleSpawner _owner;


    void OnEnable()
    {
        RingHoleTrigger.RingHolePassedThrough += OnPassedThroughGap;
        BasicProjectile.OnProjectileHit += OnPlayerHitRing;
    }

    void OnDisable()
    {
        RingHoleTrigger.RingHolePassedThrough -= OnPassedThroughGap;
        BasicProjectile.OnProjectileHit -= OnPlayerHitRing;
    }

    public void Initialize(int ringNumber, RingObstacleSpawner owner, Transform centerTarget)
    {
        this._ringNumber = ringNumber;
        _finalRotationSpeed = rotationSpeed + (rotationSpeedCountIncrease * ringNumber);
        _centerTarget = centerTarget;
        _owner = owner;
    }

    void Start()
    {
        _currentRadius = ringRadius;
        _shouldShrink = true;

        SpawnRing();
        RandomizeStartRotation();
    }

    void Update()
    {
        if (autoRotate)
            transform.Rotate(Vector3.forward, _finalRotationSpeed * Time.deltaTime);

        ShrinkTowardsCenter();
    }

    private void SpawnRing()
    {
        int gapStartIndex = Random.Range(0, segmentCount);

        for (int i = 0; i < segmentCount; i++)
        {
            if (IsInGap(i, gapStartIndex))
            {
                float angle = (float)i / segmentCount * Mathf.PI * 2f;
                Vector3 pos = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * ringRadius;

                GameObject segment = Instantiate(ringHolePrefab, transform);
                segment.transform.localPosition = pos;
                segment.transform.up = pos.normalized;

                _segments.Add(segment);
            } 
            else
            {
                float angle = (float)i / segmentCount * Mathf.PI * 2f;
                Vector3 pos = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * ringRadius;

                GameObject segment = Instantiate(segmentPrefab, transform);
                segment.transform.localPosition = pos;
                segment.transform.up = pos.normalized;
                segment.transform.localScale = new Vector3(0.75f, 0.75f, 1f );

                _segments.Add(segment);
            }
        }
    }

    private bool IsInGap(int index, int gapStart)
    {
        int distance = (index - gapStart + segmentCount) % segmentCount;
        return distance < gapSize;
    }


    private void ShrinkTowardsCenter()
    {
        if (!_shouldShrink) return;

        _currentRadius -= shrinkSpeed * Time.deltaTime;

        
        if (_currentRadius <= hitRadius)
        {
            HandleFailedHit();
            return;
        }
            

        foreach (var segment in _segments)
        {
            if (segment != null)
            {
                Vector3 dir = segment.transform.localPosition.normalized;
                segment.transform.localPosition = dir * _currentRadius;
            }
        }
    }


    private void HandleFailedHit()
    {
        _completed = true;
        _shouldShrink = false;

        StopAllCoroutines();

        // Damage player
        if (Player.Instance != null)
            Player.Instance.DamageSelf(dEf.damage, dEf.sourceName);

        AudioHelpers.PlaySoundEffect(failSound, Camera.main.transform.position);

        // Notify owner
        _owner?.OnRingResolved(this);

        HandleDestroy();
    }

    private void HandleDestroy()
    {
        if(destroyEffectPrefab != null)
        {
            Instantiate(destroyEffectPrefab);
        }
        // Destroy
        Destroy(gameObject);
    }


    public void OnPassedThroughGap(GameObject targetRing)
    {
        if (_completed) return;
        if (targetRing != gameObject) return;
        _completed = true;

        float pitchScalar = 1.0f + _ringNumber * pitchStep;
        AudioHelpers.PlaySoundEffect(passThroughSound,Camera.main.transform.position, pitchScalar);

        StartCoroutine(ResolveRing());
    }

    private IEnumerator ResolveRing()
    {
        if (this == null || _completed == false)
            yield break;

        _shouldShrink = false;
        foreach (var segment in _segments)
        {
            if (segment == null) continue;

            BasicProjectile s = segment.GetComponent<BasicProjectile>();
            if (s != null)
                s.DestroyProjectile();
        }

        _owner?.OnRingResolved(this);
        HandleDestroy();
    }

    public void OnPlayerHitRing()
    {
        if (_completed) return;
        _completed = true;

        StopAllCoroutines();

        // Fade all segments
        foreach (var segment in _segments)
        {
            var spiky = segment.GetComponent<BasicProjectile>();
            if (spiky != null)
                spiky.DestroyProjectile();
        }

        // Notify Spawner
        _owner?.OnRingResolved(this);

        HandleDestroy();
    }



    private void RandomizeStartRotation()
    {
        transform.rotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));

        if (autoRotate && Random.value < 0.5f)
            _finalRotationSpeed = -_finalRotationSpeed;
    }

}
