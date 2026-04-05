using UnityEngine;
using System.Collections;
using Unity.VisualScripting;
using DG.Tweening;

public class OrbBurst : MonoBehaviour
{
    [SerializeField] private bool playOnAwake = true;
    [Header("Projectile")]
    [SerializeField] private GameObject projectilePrefab;

    [Header("Burst Settings")]
    [SerializeField] private int projectileCount = 12;
    [SerializeField] private float radius = 1.2f;

    [Header("Timing")]
    [SerializeField] private float windupTime = 1.0f;
    [SerializeField] private float releaseDelay = 0.3f;

    [Header("Visual")]
    [SerializeField] private GameObject destroyEffect;
    [SerializeField] private Transform visual;
    [SerializeField] private SpriteRenderer sRend;
    [SerializeField] private float endScale = 1.5f;
    [SerializeField] private float fadeOutTime = 0.3f;

    [Header("Audio")]
    [SerializeField] private AudioSource sizzleSource;
    [SerializeField] private SoundEffect orbExplodeSound;
    [SerializeField] private SoundEffect orbFireSound;
    [SerializeField] private SoundEffect tickSound;
    [SerializeField] private int tickCount = 2;
    [SerializeField] private float pitchStep = 0.1f;

    void Start()
    {
        if(playOnAwake)
            StartCoroutine(BurstRoutine());
    }

    public IEnumerator BurstRoutine(bool playWindup = true, float speedMultiplier = 1.0f)
    {
        if(playWindup)
        {
             sizzleSource.Play();
            // WINDUP (scale / glow / shake)
            yield return Windup();

            sizzleSource.Stop();

            AudioHelpers.PlaySoundEffect(orbExplodeSound, transform.position);

            if(destroyEffect)
            {
                Instantiate(destroyEffect, transform.position, Quaternion.identity);
            }

            sRend.color = Color.clear;
        }
       
        // Spawn circle
        GameObject[] spawned = SpawnCircle();

        if(tickCount == 0)
            yield return new WaitForSeconds(releaseDelay);

        for (int i = 0; i < tickCount; i++)
        {
            float pitchMult = 1.0f + pitchStep * i;
            AudioHelpers.PlaySoundEffect(tickSound, transform.position,pitchMult);
            yield return new WaitForSeconds(releaseDelay/(float)tickCount);
        }

        // RELEASE (activate tracking)
        foreach (var obj in spawned)
        {
            if (obj == null) continue;

            TrackingCharger charger = obj.GetComponent<TrackingCharger>();
            charger.Initialize(speedMultiplier: speedMultiplier);
        }

        AudioHelpers.PlaySoundEffect(orbFireSound, transform.position);



        Destroy(gameObject);
    }

    IEnumerator Windup()
    {
        float t = 0f;

        Vector3 startScale = Vector3.one;
        Vector3 endScale_ = Vector3.one * endScale;

        while (t < windupTime)
        {
            t += Time.deltaTime;
            float progress = t / windupTime;

            float eased = 1f - Mathf.Pow(1f - progress, 2f);

            visual.localScale = Vector3.Lerp(startScale, endScale_, eased);

            yield return null;
        }

     
    }

    GameObject[] SpawnCircle()
    {
        GameObject[] spawned = new GameObject[projectileCount];

        for (int i = 0; i < projectileCount; i++)
        {
            float angle = (i * 360f) / projectileCount;
            float rad = angle * Mathf.Deg2Rad;

            Vector2 offset = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * radius;
            Vector2 spawnPos = (Vector2)transform.position + offset;

            GameObject obj = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);

            // IMPORTANT: disable movement initially
            Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();
            rb.linearVelocity = Vector2.zero;

            spawned[i] = obj;
        }

        return spawned;
    }
}