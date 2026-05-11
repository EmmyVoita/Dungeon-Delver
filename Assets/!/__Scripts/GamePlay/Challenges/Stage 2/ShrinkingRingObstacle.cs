using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ShrinkingRingObstacle : MonoBehaviour
{
    [Header("Center Target (Player)")]
    public Transform centerTarget;

    [Header("Ring Generation")]
    public GameObject destroyEffectPrefab;
    public GameObject segmentPrefab;
    public GameObject ringHolePrefab;
    public int segmentCount = 24;     // number of pieces making the ring
    public float ringRadius = 3f;     // spawn distance
    public float shrinkSpeed = 0.5f;  // units per second
    public int gapSize = 3;           // number of missing segments

    [Header("Rotation")]
    public bool autoRotate = true;
    public float rotationSpeed = 60f;
    public float rotationSpeedCountIncrease = 10f;

    [Header("Cleanup")]
    public SoundEffect passThroughSound;

    public AudioClip expireSound;
    public RingObstacleSpawner owner;

    [Header("Player Hit Settings")]
    public float hitRadius = 0.6f;   // When the ring shrinks this close, it hits
    public int damageOnFail = 1;
    public float ringStayTime = 0.5f;

    [Header("Audio")]
    [SerializeField] private float pitchStep = 0.05f;
    public SoundEffect failSound;


    private List<GameObject> segments = new List<GameObject>();
    private bool completed = false;
    private float currentRadius;
    private bool shouldShrink;
    private int ringNumber;
    private float finalRotationSpeed;


    void OnEnable()
    {
        RingHoleTrigger.RingHolePassedThrough += OnPassedThroughGap;
    }

    void OnDisable()
    {
        RingHoleTrigger.RingHolePassedThrough -= OnPassedThroughGap;
    }

    public void Initialize(int ringNumber)
    {
        this.ringNumber = ringNumber;
        finalRotationSpeed = rotationSpeed + (rotationSpeedCountIncrease * ringNumber);
    }

    void Start()
    {
        currentRadius = ringRadius;
        shouldShrink = true;

        SpawnRing();
        RandomizeStartRotation();
    }

    void Update()
    {
        if (autoRotate)
            transform.Rotate(Vector3.forward, finalRotationSpeed * Time.deltaTime);

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

                segments.Add(segment);
            } 
            else
            {
                float angle = (float)i / segmentCount * Mathf.PI * 2f;
                Vector3 pos = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * ringRadius;

                GameObject segment = Instantiate(segmentPrefab, transform);
                segment.transform.localPosition = pos;
                segment.transform.up = pos.normalized;
                segment.transform.localScale = new Vector3(0.75f, 0.75f, 1f );

                segments.Add(segment);
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
        if (!shouldShrink) return;

        currentRadius -= shrinkSpeed * Time.deltaTime;

        
        // 🔴 If it reaches danger zone and was not cleared → HIT PLAYER
        if (currentRadius <= hitRadius)
        {
            HandleFailedHit();
            return;
        }
            

        foreach (var segment in segments)
        {
            if (segment != null)
            {
                Vector3 dir = segment.transform.localPosition.normalized;
                segment.transform.localPosition = dir * currentRadius;
            }
        }
    }


    private void HandleFailedHit()
    {
        completed = true;
        shouldShrink = false;

        StopAllCoroutines();

        // Damage player
        if (Player.Instance != null)
            Player.Instance.DamageSelf(damageOnFail);

        // Play expire sound
        //if (expireSound != null)
            //AudioHelpers.PlayMyClipAtPoint(expireSound, AudioChannel.SFX, Camera.main.transform.position);\
        AudioHelpers.PlaySoundEffect(failSound, Camera.main.transform.position);

        // Notify owner
        owner?.OnRingResolved(this);

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
        if (completed) return;
        if (targetRing != gameObject) return;
        completed = true;

        float pitchScalar = 1.0f + ringNumber * pitchStep;
        AudioHelpers.PlaySoundEffect(passThroughSound,Camera.main.transform.position, pitchScalar);

        StartCoroutine(ResolveRing());
    }

    private IEnumerator ResolveRing()
    {
        yield return new WaitForSeconds(ringStayTime);

        if (this == null || completed == false)
            yield break;

        shouldShrink = false;
        foreach (var segment in segments)
        {
            if (segment == null) continue;

            SpikyBall s = segment.GetComponent<SpikyBall>();
            if (s != null)
                s.FadeOut();
        }


        owner?.OnRingResolved(this);
        HandleDestroy();
    }

    public void OnPlayerHitRing()
    {
        if (completed) return;
        completed = true;

        StopAllCoroutines();

        // Fade all segments
        foreach (var segment in segments)
        {
            var spiky = segment.GetComponent<SpikyBall>();
            if (spiky != null)
                spiky.FadeOut();
        }

        // Play fail sound
        if (expireSound != null)
            AudioHelpers.PlayMyClipAtPoint(expireSound, AudioChannel.SFX, Camera.main.transform.position);

        // Notify Spawner
        owner?.OnRingResolved(this);



        HandleDestroy();
    }


    private void HandleExpired()
    {
        if(completed) return;
        completed = true;
        if (expireSound != null)
            AudioHelpers.PlayMyClipAtPoint(expireSound, AudioChannel.SFX, Camera.main.transform.position);

        owner?.OnRingResolved(this);
        Destroy(gameObject);
    }

    private void RandomizeStartRotation()
    {
        transform.rotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));

        if (autoRotate && Random.value < 0.5f)
            finalRotationSpeed = -finalRotationSpeed;
    }

}
