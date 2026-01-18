using UnityEngine;
using DG.Tweening;
using System.Collections;


public class OrbitingShieldObject : MonoBehaviour
{
    public static event System.Action<Vector2, Vector2, float> OnArrowCaught;

    [Header("Set in Inspector")]
    public Sprite[] shieldSprites;
    public int numHitsToBreak = 3;  
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

    private int hitsTaken = 0;
    private Tween activeShakeTween;
    private Tween vanishTween;
    private Vector3 baseLocalPos;
    private Vector3 baseScale;
    private bool isVanishing = false;

    private float flashDone = 0;
    private bool flashColor = false;
    private float spawnTime;
    private Coroutine tickRoutine;

    public int CurrentHitsTaken => hitsTaken;
    public bool IsFull => hitsTaken <= 0;


    //public Transform orbitingTransform;

    void Awake()
    {
        sRend = GetComponentInChildren<SpriteRenderer>();
        if (sRend == null)
        {
            Debug.LogError("❌ No SpriteRenderer found on goal!");
            enabled = false;
            return;
        }

        sRend.sprite = shieldSprites[0];
        baseLocalPos = transform.localPosition;
        baseScale = spriteObject.transform.localScale;
    }

    void Start()
    {
        // 🔊 Play start sound
        if (startSound != null)
            AudioHelpers.PlayMyClipAtPoint(startSound, AudioChannel.SFX, transform.position);
    }

    void OnEnable()
    {
        spawnTime = Time.time;
        CancelInvoke();
        //Invoke(nameof(Disappear), lifetime);
        //tickRoutine = StartCoroutine(PlayCountdownTicks());
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

        //if (!flashColor && !isVanishing)
        //sRend.sprite = defaultGoalSprite;

        if (!isVanishing)
        {
            float elapsed = Time.time - spawnTime;
            float t = Mathf.Clamp01(elapsed / lifetime);
            float curveT = lifetimeCurve.Evaluate(t);

            //sRend.color = Color.Lerp(startColor, endColor, curveT);
            float scaleFactor = Mathf.Lerp(1f, minScaleMultiplier, curveT);
            spriteObject.transform.localScale = baseScale * scaleFactor;
        }
    }

    public void RestoreShield()
    {
        hitsTaken = 0;
        sRend.sprite = shieldSprites[0];

        // 💚 Flash green feedback
        Color originalColor = sRend.color;
        sRend.color = Color.green;

        sRend.DOFade(0.3f, 0.05f).OnComplete(() =>
        {
            sRend.DOFade(1f, 0.3f);
        });

        // Smoothly lerp color back to original
        sRend.DOColor(originalColor, 0.4f)
            .SetDelay(0.1f)
            .SetEase(Ease.OutSine);

        // 🔊 Sound feedback
        AudioHelpers.PlayMyClipAtPoint(startSound, AudioChannel.SFX, transform.position);

        // 💥 Small scale pop
        spriteObject.transform
            .DOScale(baseScale * 1.2f, 0.15f)
            .SetEase(Ease.OutBack)
            .OnComplete(() => spriteObject.transform.DOScale(baseScale, 0.3f).SetEase(Ease.OutSine));

        Debug.Log("💚 Shield restored to full durability.");
    }
    
    // ------------------------------------------------------------
    // 🚀 Launch the shield outward in its current facing direction
    // ------------------------------------------------------------
    public void Launch(float speed = 6f, float fadeTime = 0.6f)
    {
        if (isVanishing) return;

        isVanishing = true; // prevents double activation
        transform.SetParent(null); // detach from orbit
        sRend.sortingOrder = 10;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb == null)
            rb = gameObject.AddComponent<Rigidbody2D>();

        rb.isKinematic = false;
        rb.gravityScale = 0f;
        rb.linearVelocity = transform.up * speed; // 🚀 launch along current facing direction

        // 💥 Quick feedback
        spriteObject.transform.DOScale(baseScale * 1.2f, 0.1f).SetEase(Ease.OutBack);
        //sRend.DOColor(Color.cyan, 0.1f).OnComplete(() => sRend.DOColor(Color.white, 0.3f));

        // 🔊 Launch sound
        if (goalCritSound != null)
            AudioHelpers.PlayMyClipAtPoint(goalCritSound, AudioChannel.SFX, transform.position, 1f, pitch: 1.1f);

        // 🕓 Fade and cleanup
        sRend.DOFade(0f, fadeTime)
            .SetEase(Ease.InSine)
            .OnComplete(() => Destroy(gameObject));

        Debug.Log($"🚀 Shield launched in direction: {transform.up}");
    }




    IEnumerator PlayCountdownTicks()
    {
        float elapsed = 0f;
        int tickCount = 0;

        while (elapsed < lifetime && !isVanishing)
        {
            float normalized = elapsed / lifetime;
            float pitch = basePitch * Mathf.Lerp(minPitch, maxPitch, normalized);

            if (tickSound != null)
                AudioHelpers.PlayClipWithVariation(tickSound, AudioChannel.SFX, transform.position, pitch);

            yield return new WaitForSeconds(tickInterval);
            elapsed += tickInterval;
            tickCount++;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        ArrowBase arrow = other.GetComponent<ArrowBase>();
        if (arrow == null || arrow.invincible) return;

        flashColor = true;
        flashDone = Time.time + flashDuration;
        //sRend.sprite = criticalCatchGoalSprite;

        //AddScore(Goal.GoalType.Critical, arrow.scoreValue);
        if (critCatchEffect != null) critCatchEffect.Play();

        arrow.OnArrowHit(1, Goal.GoalType.Critical, goalDirection);
        PlayImpactShake();

        //Player.Instance.AbilityCharge -= 1;



        hitsTaken++;
        int clampedhitsTaken = Mathf.Clamp(hitsTaken, 0, numHitsToBreak);
        clampedhitsTaken = Mathf.Clamp(hitsTaken, 0, shieldSprites.Length - 1);

        sRend.sprite = shieldSprites[clampedhitsTaken];
        

        
        if(hitsTaken >= numHitsToBreak)
        {
            Disappear();
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

    /*
    private void AddScore(Goal.GoalType type, int arrowScoreValue)
    {
        switch (type)
        {
            case Goal.GoalType.Normal:
                ScoreManager.Instance.AddScore(arrowScoreValue);
                //ComboManager.Instance.ResetCritCombo(true);
                break;
            case Goal.GoalType.Critical:
                ScoreManager.Instance.AddScore(arrowScoreValue);
                //ComboManager.Instance.AddCritHit();
                break;
        }
    }
    */

    private void Disappear()
    {
        if (isVanishing || sRend == null)
            return;



        isVanishing = true;
        flashColor = false;

        if (tickRoutine != null)
            StopCoroutine(tickRoutine);

        activeShakeTween?.Kill();

        // 🔊 Final break sound
        if (breakSound != null)
            AudioHelpers.PlayMyClipAtPoint(breakSound, AudioChannel.SFX, transform.position);

        vanishTween = DOTween.Sequence()
            .Append(spriteObject.transform.DOScale(baseScale * expandScale, vanishDuration * 0.3f).SetEase(Ease.OutQuad))
            .Append(spriteObject.transform.DOScale(Vector3.zero, vanishDuration * 0.7f).SetEase(Ease.InBack))
            .Join(sRend.DOFade(0f, vanishDuration * 0.8f).SetEase(Ease.OutSine))
            .OnComplete(() =>
            {
                //orbitingTransform.gameObject.SetActive(false);
                Destroy(transform.parent.gameObject);
            });
    }
}
