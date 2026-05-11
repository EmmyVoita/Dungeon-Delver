
using System.Collections;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class BounceBomb : MonoBehaviour
{
    public static event System.Action OnBombHit;
    public static event System.Action OnBombCleared;

    [Header("References")]
    [SerializeField] private SpriteRenderer sRend;
    [SerializeField] private EmberOrbitController emberController;
    [SerializeField] private GameObject movingWall;
    [SerializeField] private GameObject shockWavePrefab;
    [SerializeField] private GameObject explosionEffect;
    [SerializeField] private OrbBurst orbBurst;
    [SerializeField] private float speed = 5f;
    [SerializeField] private int hitsRequired = 3;
    [SerializeField] private float lifetime = 6f;

    [SerializeField] private float hitRandomDirInfluence = 0.2f;
    [SerializeField] private float hitSpeedMultiplier = 1.5f;
    [SerializeField] private float speedReturnRate = 5f;

    [SerializeField] private float hitCooldown = 0.15f;
    private float lastHitTime = -999f;

    private float currentSpeed;

    [Header("spawnedWall")]
    [SerializeField] private Vector3 spawnOffset = new Vector3(8f,0f,0f);
    [SerializeField] private float minWallSpeed = 6f;
    [SerializeField] private float maxWallSpeed = 6f;

    [Header("Audio")]
    [SerializeField] private SoundEffect hitSound;
    [SerializeField] private SoundEffect explodeSound;
    [SerializeField] private SoundEffect disarmSound;
    [SerializeField] private SoundEffect wallHitSound;

    private Rigidbody2D rb;
    private int currentHits = 0;
    private float timer;
    private bool interactable = true;
    private DG.Tweening.Sequence diffuseSequence;
    private DG.Tweening.Sequence explodeSequence;
    [SerializeField] private float scaleUpAmount = 1.2f;
    [SerializeField] private float scaleUpDuration = 0.15f;
    [SerializeField] private float shakeDuration = 0.25f;
    [SerializeField] private float shakeStrength = 0.15f;
    [SerializeField] private float scaleDownDuration = 0.2f;
    [SerializeField] private float shrinkDelay = 1.0f;

    [Header("Punch Scale on Hit")]
    [SerializeField] private Vector3 punch = new Vector3(0.3f,0.3f,0f);
    [SerializeField] private float punchDuration = 0.3f;
    [SerializeField] private float punchElasticity = 1.0f;
    [SerializeField] private int punchVibrato = 10;

    private Tween punchScaleTween;
    private Vector3 defaultScale;

    public float LifeTime => lifetime;
    public float HitsRequired => hitsRequired;

    void Awake()
    {
        defaultScale = transform.localScale;
        currentHits = 0;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        //Vector2 direction = Random.insideUnitCircle.normalized;
        //currentSpeed = speed;
        //rb.linearVelocity = direction * speed;
        StartCoroutine(StartSequence());

        timer = lifetime;

        interactable = true;
    }

    IEnumerator StartSequence()
    {
        yield return new WaitForSeconds(1.0f);
        Vector2 direction = Random.insideUnitCircle.normalized;
        rb.linearVelocity = direction * speed;
    }

    void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0f && interactable)
        {
            Explode();
        }

        currentSpeed = Mathf.Lerp(currentSpeed, speed, Time.deltaTime * speedReturnRate);

        rb.linearVelocity = rb.linearVelocity.normalized * currentSpeed;
    }

    public void OnPlayerHit(Vector2 playerDir)
    {
        if (Time.time - lastHitTime < hitCooldown)
            return;

        if(!interactable)
            return;

        OnBombHit?.Invoke();

        lastHitTime = Time.time;

        currentHits++;

        AudioHelpers.PlaySoundEffect(hitSound, transform.position);

        Vector2 newDir = playerDir.normalized;
        newDir += Random.insideUnitCircle * 0.2f;
        newDir.Normalize();

        currentSpeed = speed * hitSpeedMultiplier;
        rb.linearVelocity = newDir * currentSpeed;
        emberController.RemoveEmber();

        if (currentHits >= hitsRequired)
        {
            Defuse();
        }
        else
        {
            // stop old punch
            if (punchScaleTween != null && punchScaleTween.IsActive())
            {
                punchScaleTween.Kill();
            }

            // restore exact base scale first
            transform.localScale = defaultScale;

            // play fresh punch
            punchScaleTween = transform.DOPunchScale(
                punch,
                punchDuration,
                punchVibrato,
                punchElasticity
            )
            .SetLink(gameObject)
            .OnKill(() =>
            {
                // only force reset if we're not in some other state like defusing
                if (interactable)
                    transform.localScale = defaultScale;
            })
            .OnComplete(() =>
            {
                if (interactable)
                    transform.localScale = defaultScale;
            });
        }
    }

    void Explode()
    {
         interactable = false;

        // Stop movement
        rb.linearVelocity = Vector2.zero;

        // Kill any existing sequence (safety)
        explodeSequence?.Kill();

        explodeSequence = DOTween.Sequence();

        // 1. Scale up (anticipation)
        explodeSequence.Append(
            transform.DOScale(scaleUpAmount, scaleUpDuration)
                    .SetEase(Ease.OutQuad)
        );

        // 2. Shake (tension)
        explodeSequence.Append(
            transform.DOShakeScale(shakeDuration, shakeStrength, vibrato: 20, randomness: 90)
        );

        explodeSequence.AppendInterval(shrinkDelay);

        // 3. Play disarm sound right before vanish
        explodeSequence.AppendCallback(() =>
        {
            sRend.color = Color.clear;
            emberController.CleanUp();

            AudioHelpers.PlaySoundEffect(explodeSound, transform.position);
            if(shockWavePrefab)
            {
                Instantiate(shockWavePrefab, transform.position, Quaternion.identity);
            }

            if(movingWall)
            {
                GameObject obj = Instantiate(movingWall, spawnOffset, Quaternion.identity);
                MovingWall wall = obj.GetComponent<MovingWall>();
                wall.Init(
                    direction: Vector2.left,
                    _baseSpeed: CalculateWallSpeed(),
                    _lifeDuration: 3.0f,
                    speedMultiplier: 0.0f,
                    speedVariation: 0.0f
                );
            }

           
            
            if(explosionEffect)
            {
                Instantiate(explosionEffect,transform.position,Quaternion.identity);
            }
        });

        // 5. Destroy at the end
        explodeSequence.OnComplete(() =>
        {
            StartCoroutine(OnCompleteSequence());
        });
    }

    private IEnumerator OnCompleteSequence()
    {
        if(orbBurst)
        {
            yield return StartCoroutine(orbBurst.BurstRoutine(false, CalculateWallSpeed()));
            Destroy(gameObject);
        }
    }

    float CalculateWallSpeed()
    {
        float percent = (float)currentHits / hitsRequired;
        percent = Mathf.Clamp01(percent);

        return Mathf.Lerp(maxWallSpeed, minWallSpeed, percent);
    }

    void Defuse()
    {
        interactable = false;

        // Stop movement
        rb.linearVelocity = Vector2.zero;

        // Kill any existing sequence (safety)
        diffuseSequence?.Kill();

        diffuseSequence = DOTween.Sequence();

        // 1. Scale up (anticipation)
        diffuseSequence.Append(
            transform.DOScale(scaleUpAmount, scaleUpDuration)
                    .SetEase(Ease.OutQuad)
        );

        // 2. Shake (tension)
        diffuseSequence.Append(
            transform.DOShakeScale(shakeDuration, shakeStrength, vibrato: 20, randomness: 90)
        );

        diffuseSequence.AppendInterval(shrinkDelay);

        // 3. Play disarm sound right before vanish
        diffuseSequence.AppendCallback(() =>
        {
            AudioHelpers.PlaySoundEffect(disarmSound, transform.position);
        });

        // 4. Scale down to zero (disappear)
        diffuseSequence.Append(
            transform.DOScale(0f, scaleDownDuration)
                    .SetEase(Ease.InBack)
        );

        // 5. Destroy at the end
        diffuseSequence.OnComplete(() =>
        {
            OnBombCleared?.Invoke();
            Destroy(gameObject);
        });
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            float impact = collision.relativeVelocity.magnitude;

            if (impact > 1f) // threshold so tiny bumps don’t spam sound
            {
                AudioHelpers.PlaySoundEffect(wallHitSound, transform.position);
            }
        }
    }
}