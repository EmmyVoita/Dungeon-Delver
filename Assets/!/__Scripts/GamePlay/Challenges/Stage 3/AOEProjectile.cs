using UnityEngine;
using System.Collections;
using DG.Tweening;

public class AOEProjectile : MonoBehaviour
{
    public CircleCollider2D coll;

    [Header("Movement")]
    public float speed = 6f;

    [Header("Explosion")]
    public float fuseTime = 1.0f;
    public float explosionRadius = 1.5f;
    public float hitboxRadius = 1.5f;

    [Header("Visual")]
    [SerializeField] private Transform warningCircle;
    [SerializeField] private Transform sprite;

    [Header("Audio")]
    public SoundEffect spawnSound;
    public SoundEffect explodeSound;

    [SerializeField] private float scaleUpAmount;
     [SerializeField] private float shakeStrength;
     [SerializeField] private float shakeFrequency = 10f;
    [SerializeField] private Vector3 baseLocalPos;

    private Rigidbody2D rb;
    private bool exploded = false;

    private Sequence diffuseSequence;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Initialize(Vector2 direction)
    {
        AudioHelpers.PlaySoundEffect(spawnSound, transform.position);

        rb.linearVelocity = direction.normalized * speed;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle - 90f);

        StartCoroutine(FuseRoutine());
    }

    IEnumerator FuseRoutine()
    {
        float t = 0f;

        baseLocalPos = sprite.localPosition;

        // Kill any existing sequence (safety)
        diffuseSequence?.Kill();

        diffuseSequence = DOTween.Sequence();

        // 1. Scale up (anticipation)
        diffuseSequence.Append(
            sprite.DOScale(scaleUpAmount, fuseTime)
                    .SetEase(Ease.OutQuad)
        );

        // 2. Shake (tension)
        diffuseSequence.Join(
            DOTween.To(() => 0f, t =>
            {
                float strength = Mathf.Lerp(0f, shakeStrength, t);

                float time = Time.time;

                float x = (Mathf.PerlinNoise(time * shakeFrequency, 0f) - 0.5f) * 2f;
                float y = (Mathf.PerlinNoise(0f, time * shakeFrequency) - 0.5f) * 2f;

                Vector2 offset = new Vector2(x, y) * strength;

                sprite.localPosition = baseLocalPos + (Vector3)offset;

            }, 1f, fuseTime)
            .SetEase(Ease.InQuad) // ramp up
        );

        while (t < fuseTime)
        {
            t += Time.deltaTime;

            float progress = t / fuseTime;

            // Ease-out (fast start, slow end)
            float eased = 1f - Mathf.Pow(1f - progress, 2f);

            if (warningCircle != null)
            {
                float scale = Mathf.Lerp(0.2f, explosionRadius, eased);
                warningCircle.localScale = Vector3.one * scale;
            }

            yield return null;
        }

        Explode();
    }

    void Explode()
    {
        if (exploded) return;
        exploded = true;

        AudioHelpers.PlaySoundEffect(explodeSound, transform.position);

        // TODO: damage logic (overlap circle)
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, hitboxRadius);

        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                Player.Instance.DamageSelf(1);
                // damage player
            }
        }

        Destroy(gameObject);
    }
}