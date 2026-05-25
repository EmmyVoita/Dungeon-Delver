using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(Collider2D))]
public class ProjectileShieldShot : MonoBehaviour
{
    [Header("Projectile Settings")]
    public float speed = 12f;
    public float fadeOutSpeed = 1f;
    public float lifetime = 2f;
    public float fadeOutTime = 0.2f;
    public int maxPierceCount = 1;
    [Tooltip("Maximum distance the projectile can travel before despawning.")]
    public float maxTravelDistance = 10f;

    [Header("Upgrade Effects")]
    public bool projectileCrits = false;
    public bool useFreezeEffect = false;
    public bool enableEmpowerEffect = false;
    public float freezeDuration = 2f;
    public float empowerRadius = 3f;
    public int bonusScoreMultiplier = 5;

    [Header("Visual & Audio")]
    public GameObject breakEffectPrefab;
    public AudioClip breakSound;
    public AudioClip hitArrowSound;
    public AudioClip fadeOutSound;
    public SpriteRenderer spriteRenderer;

    private bool isFadingOut = false;
    private bool isBreaking = false;
    private int arrowsPierced = 0;
    private Vector3 startPosition;

    public ParticleSystem[] particleSystems;

    private void Start()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (particleSystems == null || particleSystems.Length == 0)
            particleSystems = GetComponentsInChildren<ParticleSystem>();

        startPosition = transform.position;

        // Backup destroy timer
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        if (isBreaking) return;

        transform.Translate(Vector3.up * speed * Time.deltaTime, Space.Self);

        // 🔹 Check travel distance
        float traveled = Vector3.Distance(startPosition, transform.position);
        if (traveled >= maxTravelDistance)
        {
            //BreakProjectile(breakSilently: true);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("ProjectileShieldShot hit: " + other.name);

        if (isBreaking || other.CompareTag("Player"))
            return;

        if (other.CompareTag("ScreenBounds"))
        {
            FadeOutProjectile();
            //BreakProjectile(breakSilently: true);
        }

        ArrowBase arrow = other.GetComponent<ArrowBase>();
        if (arrow != null && !arrow.Invincible)
        {
            if (useFreezeEffect)
            {
                //arrow.Freeze(freezeDuration);
            }
            else
            {
                arrow.OnArrowHit(1, Goal.GoalType.Critical, transform.up);
                //ScoreManager.Instance.AddScore(arrow.scoreValue);

                if (projectileCrits)
                    Player.Instance.AbilityCharge += 2;

                arrowsPierced++;
            }

            if (hitArrowSound != null)
                AudioHelpers.PlayMyClipAtPoint(hitArrowSound, AudioChannel.SFX, transform.position);

           

            if (arrowsPierced >= maxPierceCount)
            {
                BreakProjectile();
                return;
            }
        }
    }

    private void BreakProjectile(bool breakSilently = false)
    {
        if (isBreaking) return;
        isBreaking = true;

        if (enableEmpowerEffect)
            EmpowerNearbyArrows(transform.position);

        if (!breakSilently)
        {
            if (breakEffectPrefab != null)
                Instantiate(breakEffectPrefab, transform.position, Quaternion.identity);

            if (breakSound != null)
                AudioHelpers.PlayMyClipAtPoint(breakSound, AudioChannel.SFX, transform.position);
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.DOFade(0f, fadeOutTime)
                .SetEase(Ease.OutSine)
                .OnComplete(() => Destroy(gameObject));
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void FadeOutProjectile()
    {
        if (isFadingOut) return;
        isFadingOut = true;

        // stop movement
        speed = fadeOutSpeed;

        // 🔊 optional sound
        if (fadeOutSound != null)
            AudioHelpers.PlayMyClipAtPoint(fadeOutSound, AudioChannel.SFX, transform.position);

        // ✨ stop all particles gradually
        foreach (var ps in particleSystems)
        {
            var emission = ps.emission;
            emission.enabled = false; // stop new particles
            ps.Stop(withChildren: true, ParticleSystemStopBehavior.StopEmitting);
        }

        // 🎨 fade the material color instead of the SpriteRenderer color
        if (spriteRenderer != null && spriteRenderer.material.HasProperty("_Color"))
        {
            Color startColor = spriteRenderer.material.GetColor("_Color");
            Color endColor = startColor;
            endColor.a = 0f;

            // Tween material color alpha
            DOTween.To(
                () => spriteRenderer.material.GetColor("_Color"),
                c => spriteRenderer.material.SetColor("_Color", c),
                endColor,
                fadeOutTime
            )
            .SetEase(Ease.OutSine)
            .OnComplete(() => Destroy(gameObject));
        }
        else
        {
            // fallback (no sprite or material)
            Destroy(gameObject, fadeOutTime + 0.2f);
        }
    }


    private void EmpowerNearbyArrows(Vector3 center)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(center, empowerRadius);
        foreach (var hit in hits)
        {
            ArrowBase arrow = hit.GetComponent<ArrowBase>();
            if (arrow != null && !arrow.Invincible)
                arrow.SetGolden();
        }
    }
}
