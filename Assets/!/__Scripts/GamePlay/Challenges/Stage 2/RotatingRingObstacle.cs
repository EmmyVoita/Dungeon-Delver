using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RotatingRingObstacle : MonoBehaviour
{
    [Header("Center Target (Player)")]
    public Transform centerTarget;

    [SerializeField] private LineRenderer outlineRenderer;
    [SerializeField] private int outlineResolution = 64;

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

    [Header("Gap Settings")]
    public int gapCount = 3;          // number of evenly spaced gaps
    public bool jitterGaps = true;    // small randomness inside each region

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
        //RingHoleTrigger.RingHolePassedThrough += OnPassedThroughGap;
    }

    void OnDisable()
    {
        //RingHoleTrigger.RingHolePassedThrough -= OnPassedThroughGap;
    }

    public void Initialize(int ringNumber, float customRadius, int direction)
    {
        this.ringNumber = ringNumber;
        this.ringRadius = customRadius;

        finalRotationSpeed = (rotationSpeed + 
            (rotationSpeedCountIncrease * ringNumber)) * direction;
    }

    void Start()
    {
        currentRadius = ringRadius;
        //shouldShrink = true;

        SpawnRing();
        SetupOutline();

        //RandomizeStartRotation();
    }

    private void SetupOutline()
    {
        if (outlineRenderer == null)
            return;

        outlineRenderer.positionCount = outlineResolution;

        for (int i = 0; i < outlineResolution; i++)
        {
            float angle = (float)i / outlineResolution * Mathf.PI * 2f;
            Vector3 pos = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * ringRadius;
            outlineRenderer.SetPosition(i, pos);
        }
    }

    void Update()
    {
        transform.Rotate(Vector3.forward, finalRotationSpeed * Time.deltaTime);
    }

    private void SpawnRing()
    {
        segments.Clear();

        // Safety clamp
        gapCount = Mathf.Clamp(gapCount, 1, segmentCount / gapSize);

        int regionSize = segmentCount / gapCount;

        HashSet<int> gapIndices = new HashSet<int>();

        for (int g = 0; g < gapCount; g++)
        {
            int regionStart = g * regionSize;

            int gapStart = regionStart;

            if (jitterGaps && regionSize > gapSize)
            {
                gapStart = regionStart + Random.Range(0, regionSize - gapSize);
            }

            for (int j = 0; j < gapSize; j++)
            {
                int index = (gapStart + j) % segmentCount;
                gapIndices.Add(index);
            }
        }

        for (int i = 0; i < segmentCount; i++)
        {
            float angle = (float)i / segmentCount * Mathf.PI * 2f;
            Vector3 pos = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * ringRadius;

            bool isGap = gapIndices.Contains(i);

            GameObject prefab = isGap ? ringHolePrefab : segmentPrefab;

            GameObject segment = Instantiate(prefab, transform);
            segment.transform.localPosition = pos;
            segment.transform.up = pos.normalized;

            if (!isGap)
            {
                segment.transform.localScale = new Vector3(0.75f, 0.75f, 1f);
            }

            segments.Add(segment);
        }
    }
}
