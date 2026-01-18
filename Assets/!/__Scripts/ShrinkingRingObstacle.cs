using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ShrinkingRingObstacle : MonoBehaviour
{
    [Header("Center Target (Player)")]
    public Transform centerTarget;

    [Header("Ring Generation")]
    public GameObject segmentPrefab;
    public GameObject ringHolePrefab;
    public int segmentCount = 24;     // number of pieces making the ring
    public float ringRadius = 3f;     // spawn distance
    public float shrinkSpeed = 0.5f;  // units per second
    public int gapSize = 3;           // number of missing segments

    [Header("Rotation")]
    public bool autoRotate = true;
    public float rotationSpeed = 60f;

    [Header("Cleanup")]
    public AudioClip passThroughSound;
    public AudioClip expireSound;
    public RingObstacleSpawner owner;

    [Header("Player Hit Settings")]
    public float hitRadius = 0.6f;   // When the ring shrinks this close, it hits
    public int damageOnFail = 1;


    private List<GameObject> segments = new List<GameObject>();
    private bool completed = false;
    private float currentRadius;

    void OnEnable()
    {
        RingHoleTrigger.RingHolePassedThrough += OnPassedThroughGap;
    }

    void OnDisable()
    {
        RingHoleTrigger.RingHolePassedThrough -= OnPassedThroughGap;
    }

    void Awake()
    {
        currentRadius = ringRadius;

        SpawnRing();
        RandomizeStartRotation();
    }

    void Update()
    {
        if (autoRotate)
            transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);

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
        return index >= gapStart && index < gapStart + gapSize;
    }

    private void ShrinkTowardsCenter()
    {
        if (completed) return;

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

        // Damage player
        if (Player.Instance != null)
            Player.Instance.DamageSelf(damageOnFail);

        // Play expire sound
        if (expireSound != null)
            AudioHelpers.PlayMyClipAtPoint(expireSound, AudioChannel.SFX, Camera.main.transform.position);

        // Notify owner
        owner?.OnRingResolved(this);

        // Destroy
        Destroy(gameObject);
    }


    public void OnPassedThroughGap(GameObject targetRing)
    {
        if (completed) return;
        if (targetRing != gameObject) return;
        completed = true;

        if (passThroughSound != null)
            AudioHelpers.PlayMyClipAtPoint(passThroughSound, AudioChannel.SFX, Camera.main.transform.position);

        foreach (var segment in segments)
        {
            SpikyBall s = segment.GetComponent<SpikyBall>();
            if (s != null)
                s.FadeOut();
        }

        owner?.OnRingResolved(this);
        Destroy(gameObject, 1f);
    }

    public void OnPlayerHitRing()
    {
        if (completed) return;
        completed = true;

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

        Destroy(gameObject, 0.5f);
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
            rotationSpeed = -rotationSpeed;
    }

}
