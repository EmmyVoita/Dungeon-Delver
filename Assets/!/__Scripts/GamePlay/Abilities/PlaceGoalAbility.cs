using UnityEngine;
using DG.Tweening;
using System.Collections;

public class PlaceGoalAbility : MonoBehaviour
{
    public static event System.Action<Vector2, Vector2, float> OnArrowCaught;

    [Header("Beam Absorb Settings")]
    public GameObject beamPrefab;
    public int absorbToFire = 4;               // number of absorbed arrows to trigger beam
    public AudioClip beamReadySound;
    public AudioClip beamFireSound;
    public Color chargeColor = Color.cyan;
    public float beamDuration = 0.6f;
    public float beamDelay = 0.15f;            // small anticipation before firing
    [SerializeField] private GameObject goldenWavePrefab;



    [Header("Golden Pulse Settings")]
    [SerializeField] private bool pulseOnSpawn = true;      // true = trigger on placement
    [SerializeField] private bool pulseOnDisappear = false; // or trigger when disappearing
    [SerializeField] private float pulseRadius = 4f;
    [SerializeField] private int maxGoldenArrows = 10;
    [SerializeField] private float goldenScoreBoost = 1.5f;
    [SerializeField] private LayerMask arrowLayer;


    private int absorbedCount = 0;
    private bool isFiring = false;

    [Header("Set in Inspector")]
    public AudioClip goalSound;
    public AudioClip goalCritSound;
    public ParticleSystem critCatchEffect;
    public Vector2 goalDirection = Vector2.up;
    public GameObject spriteObject;
    public float critWindow = 0.2f;
    public float flashDuration = 0.5f;
    public Sprite defaultGoalSprite;
    public Sprite normalCatchGoalSprite;
    public Sprite criticalCatchGoalSprite;

    [Header("Goal Lifetime Settings")]
    [SerializeField] private float lifetime = 6f;
    [SerializeField] private float vanishDuration = 0.5f;
    [SerializeField] private float expandScale = 1.2f;

    [Header("Lifetime Visual Feedback")]
    [SerializeField] private Color startColor = Color.white;
    [SerializeField] private Color endColor = Color.red;
    [SerializeField] private AnimationCurve lifetimeCurve = AnimationCurve.Linear(0, 0, 1, 1);
    [SerializeField] private float minScaleMultiplier = 0.9f;

    [Header("Audio Feedback")]
    [SerializeField] private AudioClip startSound;
    [SerializeField] private AudioClip tickSound;
    [SerializeField] private float basePitch = 1f;
    [SerializeField] private float tickInterval = 1f;
    [SerializeField] private float minPitch = 0.8f;
    [SerializeField] private float maxPitch = 1.4f;
    [SerializeField] private AudioClip breakSound;

    [Header("References")]
    [SerializeField] private SpriteRenderer sRend;

    private Tween activeShakeTween;
    private Tween vanishTween;
    private Vector3 baseLocalPos;
    private Vector3 baseScale;
    private bool isVanishing = false;
    private float flashDone = 0;
    private bool flashColor = false;
    private float spawnTime;
    private Coroutine tickRoutine;

    void Awake()
    {
        sRend = GetComponentInChildren<SpriteRenderer>();
        if (sRend == null)
        {
            Debug.LogError("❌ No SpriteRenderer found on goal!");
            enabled = false;
            return;
        }

        sRend.sprite = defaultGoalSprite;
        baseLocalPos = transform.localPosition;
        baseScale = spriteObject.transform.localScale;
    }

    void Start()
    {
        if (startSound != null)
            AudioHelpers.PlayMyClipAtPoint(startSound, AudioChannel.SFX, transform.position);
    }

    void OnEnable()
    {
        spawnTime = Time.time;
        CancelInvoke();
        Invoke(nameof(Disappear), lifetime);
        tickRoutine = StartCoroutine(PlayCountdownTicks());
    }

    void OnDisable()
    {
        vanishTween?.Kill();
        activeShakeTween?.Kill();
        if (tickRoutine != null)
            StopCoroutine(tickRoutine);
    }

    void Update()
    {
        if (flashColor && Time.time > flashDone)
            flashColor = false;

        if (!flashColor && !isVanishing)
            sRend.sprite = defaultGoalSprite;

        if (!isVanishing)
        {
            float elapsed = Time.time - spawnTime;
            float t = Mathf.Clamp01(elapsed / lifetime);
            float curveT = lifetimeCurve.Evaluate(t);

            sRend.color = Color.Lerp(startColor, endColor, curveT);
            float scaleFactor = Mathf.Lerp(1f, minScaleMultiplier, curveT);
            spriteObject.transform.localScale = baseScale * scaleFactor;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        ArrowBase arrow = other.GetComponent<ArrowBase>();
        if (arrow == null || arrow.Invincible) return;

        absorbedCount++;
        flashColor = true;
        flashDone = Time.time + flashDuration;
        sRend.sprite = normalCatchGoalSprite;

        // Basic arrow consume
        arrow.OnArrowHit(1, Goal.GoalType.Normal, goalDirection);

        // Pulse feedback
        sRend.DOKill();
        sRend.DOColor(chargeColor, 0.1f).OnComplete(() => sRend.DOColor(Color.white, 0.3f));

        PlayImpactShake();

        // Absorb sound w/ slight pitch up
        if (goalSound != null)
            AudioHelpers.PlayMyClipAtPoint(goalSound, AudioChannel.SFX, transform.position, 0.9f, 1f + absorbedCount * 0.05f);

        
    }

    private IEnumerator FireBeam()
    {
        isFiring = true;
        //absorbedCount = 0;

        // 🔊 Charging sound cue
        if (beamReadySound != null)
            AudioHelpers.PlayMyClipAtPoint(beamReadySound, AudioChannel.SFX, transform.position, 1f, 1.1f);

        yield return new WaitForSeconds(beamDelay);

        /*

        if (beamPrefab != null)
        {
            GameObject beam = Instantiate(beamPrefab, transform.position, transform.rotation);
            Destroy(beam, beamDuration);
        }

        if (beamFireSound != null)
            AudioHelpers.PlayMyClipAtPoint(beamFireSound, AudioChannel.SFX, transform.position, 1f, 1f);

        */

        if (pulseOnDisappear)
            PulseGoldenArrows();


        //Player.Instance.HealPlayer(1);

        // Flash feedback
        sRend.DOFade(0.5f, 0.1f).OnComplete(() => sRend.DOFade(1f, 0.3f));
        yield return new WaitForSeconds(0.3f);

        isFiring = false;
    }

    private void PulseGoldenArrows()
    {
        var arrow = Instantiate(goldenWavePrefab, transform.position, Quaternion.identity);
        var goldenWave = arrow.GetComponent<GoldenWave>();
        if (goldenWave != null)
        {
            goldenWave.Initalize(absorbedCount);
        }
        /*
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, pulseRadius, arrowLayer);
        int converted = 0;

        foreach (var hit in hits)
        {
            ArrowBase arrow = hit.GetComponent<ArrowBase>();
            if (arrow != null && !arrow.IsEmpowered) // optional flag check
            {
                arrow.SetEmpowered(goldenScoreBoost);
                converted++;
                if (converted >= maxGoldenArrows)
                    break;
            }
        }

        if (converted > 0)
        {
            Debug.Log($"✨ Goal pulse empowered {converted} arrows!");
            AudioHelpers.PlayMyClipAtPoint(goalCritSound, AudioChannel.SFX, transform.position);
        }

        // simple visual burst feedback
        sRend.DOKill();
        //sRend.DOPunchScale(Vector3.one * 0.25f, 0.3f, 3, 0.5f);
        */
    }


    private IEnumerator PlayCountdownTicks()
    {
        float elapsed = 0f;
        while (elapsed < lifetime && !isVanishing)
        {
            float normalized = elapsed / lifetime;
            float pitch = basePitch * Mathf.Lerp(minPitch, maxPitch, normalized);

            if (tickSound != null)
                AudioHelpers.PlayClipWithVariation(tickSound, AudioChannel.SFX, transform.position, pitch);

            yield return new WaitForSeconds(tickInterval);
            elapsed += tickInterval;
        }
    }

    public void PlayImpactShake(float shakeStrength = 0.05f, float shakeDuration = 0.15f, int shakeVibrato = 2)
    {
        if (activeShakeTween != null && activeShakeTween.IsActive())
        {
            activeShakeTween.Kill();
            transform.localPosition = baseLocalPos;
        }

        Sequence shakeSeq = DOTween.Sequence();

        for (int i = 0; i < shakeVibrato; i++)
        {
            float intensity = Mathf.Sin(i / (float)shakeVibrato * Mathf.PI) * shakeStrength;
            Vector3 offset = transform.up * (intensity * (i % 2 == 0 ? 1 : -1));

            shakeSeq.Append(transform.DOLocalMove(baseLocalPos + offset, shakeDuration / shakeVibrato / 2).SetEase(Ease.OutSine));
            shakeSeq.Append(transform.DOLocalMove(baseLocalPos, shakeDuration / shakeVibrato / 2).SetEase(Ease.InSine));
        }

        shakeSeq.OnComplete(() =>
        {
            transform.localPosition = baseLocalPos;
            activeShakeTween = null;
        });

        activeShakeTween = shakeSeq;
    }

    private void Disappear()
    {
        if (isVanishing || sRend == null)
            return;

        // Beam trigger
        if (absorbedCount >= absorbToFire && !isFiring)
            StartCoroutine(FireBeam());

        isVanishing = true;
        flashColor = false;

        if (tickRoutine != null)
            StopCoroutine(tickRoutine);

        activeShakeTween?.Kill();

        // 🔊 Break sound
        if (breakSound != null)
            AudioHelpers.PlayMyClipAtPoint(breakSound, AudioChannel.SFX, transform.position);

        vanishTween = DOTween.Sequence()
            .Append(spriteObject.transform.DOScale(baseScale * expandScale, vanishDuration * 0.3f).SetEase(Ease.OutQuad))
            .Append(spriteObject.transform.DOScale(Vector3.zero, vanishDuration * 0.7f).SetEase(Ease.InBack))
            .Join(sRend.DOFade(0f, vanishDuration * 0.8f).SetEase(Ease.OutSine))
            .OnComplete(() =>
            {
                Destroy(gameObject);
            });
    }
}
