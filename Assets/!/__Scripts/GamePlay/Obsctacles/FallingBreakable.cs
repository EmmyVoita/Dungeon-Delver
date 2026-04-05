using System.Collections;
using DG.Tweening;
using UnityEngine;

public class FallingBreakable : MonoBehaviour, IShakeScreen
{
    public FallingBreakableSpawner owner;
    public float moveSpeed = 1f;
    public int maxHealth = 3;
    public int projectileCount = 3;
    public float projectileInterval = 0.25f;
    public GameObject destroyProjectile;

    

    [Header("Visuals")]
    public GameObject destroyEffect;
    public GameObject hitEffect;
    
    [Header("Audio")]
    public SoundEffect destroySound;
    public SoundEffect hitSound;

    [Header("Shake Settings")]
    public Transform shakeTransform;
    public float baseShakeStrength = 0.05f;
    public float maxShakeStrength = 0.2f;
    public float shakeDuration = 0.15f;

    [Header("Fan Settings")]
    public float fanAngle = 60f;          // Total spread angle
    public bool evenSpread = true;        // Evenly distribute across arc

    [Header("Screen Shake")]
    public float magnitude = 0.4f;
    public float duration = 0.15f;


    private Tween activeShake;
    private Vector3 basePosition;



    private float health;
    private bool notBroken;
    public float Health => health;

    public void TriggerShake(ScreenShakeRequest request)
    {
        ScreenShakeManager.Instance.Shake(request);
    }

    void Start()
    {
        health = maxHealth;
        basePosition = shakeTransform.localPosition;
        notBroken = true;
    }

    void Update()
    {
        transform.position += transform.up * moveSpeed * Time.deltaTime;
    }

    private void ShakeBasedOnHealth()
    {
        // Kill previous shake so they don't stack
        activeShake?.Kill();

        float damagePercent = 1f - (health / maxHealth);
        float strength = Mathf.Lerp(baseShakeStrength, maxShakeStrength, damagePercent);

        activeShake = shakeTransform
            .DOShakePosition(shakeDuration, strength, vibrato: 20, randomness: 90f)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                shakeTransform.localPosition = basePosition;
            });
    }


    public void Break()
    {
        notBroken = false;
        AudioHelpers.PlaySoundEffect(destroySound, transform.position);

        if(destroyEffect!= null)
        {
            Instantiate(destroyEffect, transform.position, Quaternion.identity);
        }

        TriggerShake(
            new ScreenShakeRequest(
                duration,
                magnitude,
                Vector2.zero,
                directional: false
            )
        );

        StartCoroutine(DestroySequence());
    }

    private IEnumerator DestroySequence()
    {
        Vector2 baseDirection = transform.up;

        for (int i = 0; i < projectileCount; i++)
        {
            float angleOffset;

            if (evenSpread && projectileCount > 1)
            {
                // Evenly distribute across the arc
                float t = i / (float)(projectileCount - 1);
                angleOffset = Mathf.Lerp(-fanAngle * 0.5f, fanAngle * 0.5f, t);
            }
            else
            {
                // Random within arc
                angleOffset = Random.Range(-fanAngle * 0.5f, fanAngle * 0.5f);
            }

            Vector2 rotatedDir = Quaternion.Euler(0f, 0f, angleOffset) * baseDirection;

            float angle = Mathf.Atan2(rotatedDir.y, rotatedDir.x) * Mathf.Rad2Deg - 90f;
            Quaternion rotation = Quaternion.Euler(0f, 0f, angle);

            GameObject projectile = Instantiate(
                destroyProjectile,
                transform.position,
                rotation
            );

            yield return new WaitForSeconds(projectileInterval);
        }

        KillBreakable();
    }


    void OnTriggerEnter2D(Collider2D col)
    {
        if(col.gameObject.GetComponent<PlayerProjectile>())
        {
            if(!notBroken) return;

            AudioHelpers.PlaySoundEffect(hitSound, transform.position);

            if(hitEffect != null)
            {
                Instantiate(hitEffect, transform.position,Quaternion.identity);
            }

            health = Mathf.Max(0, health-1);

            ShakeBasedOnHealth();  // 🔥 Add this line

            if(health == 0)
            {
                Break();
            }
        }

        if(col.transform.tag == "Player")
        {
            KillBreakable();
        }
    }

    public void ForceKill()
    {
        StopAllCoroutines();
        KillBreakable();
    }

    private void KillBreakable()
    {
        owner?.NotifyBreakableDestroyed(this);
        Destroy(gameObject);
    }
}
