using UnityEngine;
using System;
using Unity.VisualScripting;

public class SpiralObject : MonoBehaviour
{
    [Header("Effects")]
    public GameObject destroyEffectPrefab;
    public SoundEffect destroySoundEffect;

    [Header("Spiral Line Settings")]
    public int lineResolution = 40;

    [Header("Fade Settings")]
    public float fadeInDelay = 0.15f;
    public float fadeInDuration = 0.25f;
    public float fadeOutDuration = 0.2f;
    public float fadeOutRadius = 1.0f;

    private LineRenderer line;

    private Vector2 center;
    private float radius;
    private float angle;
    private float inwardSpeed;
    private float angularSpeed;

    private float spawnTime;
    private float currentAlpha = 0f;
    private bool fadingOut = false;

    public event Action OnConsumed;

    public void Init(
        Vector2 center,
        float startRadius,
        float startAngle,
        float inwardSpeed,
        float angularSpeed
    )
    {
        this.center = center;
        this.radius = startRadius;
        this.angle = startAngle;
        this.inwardSpeed = inwardSpeed;
        this.angularSpeed = angularSpeed;
    }

    void Awake()
    {
        line = GetComponent<LineRenderer>();

        if(line)
        {
            line.positionCount = lineResolution;
            line.useWorldSpace = true;
            SetLineAlpha(0f);
        }
 
        spawnTime = Time.time;
        
    }

    void Update()
    {
        float dt = Time.deltaTime;

        // Spiral motion
        angle += angularSpeed * dt;
        radius -= inwardSpeed * dt;

        Vector2 pos = center + new Vector2(
            Mathf.Cos(angle * Mathf.Deg2Rad),
            Mathf.Sin(angle * Mathf.Deg2Rad)
        ) * radius;

        transform.position = pos;

        // Update line fade + drawing
        if(line) 
        {
            HandleLineFade();
            DrawSpiralPath();
        }
     

        // Consume when close
        if (radius <= 0.5f)
        {
            Consume();
        }
    }

    void HandleLineFade()
    {
        float timeSinceSpawn = Time.time - spawnTime;

        // Fade In
        if (!fadingOut)
        {
            if (timeSinceSpawn > fadeInDelay)
            {
                float t = Mathf.InverseLerp(
                    fadeInDelay,
                    fadeInDelay + fadeInDuration,
                    timeSinceSpawn
                );

                currentAlpha = Mathf.Clamp01(t);
            }

            // Trigger fade-out near center
            if (radius <= fadeOutRadius)
            {
                fadingOut = true;
            }
        }

        // Fade Out
        if (fadingOut)
        {
            currentAlpha -= Time.deltaTime / fadeOutDuration;
            currentAlpha = Mathf.Clamp01(currentAlpha);
        }

        SetLineAlpha(currentAlpha);
    }

    void DrawSpiralPath()
    {
        float simRadius = radius;
        float simAngle = angle;

        for (int i = 0; i < lineResolution; i++)
        {
            float t = i / (float)(lineResolution - 1);

            float futureRadius = Mathf.Lerp(simRadius, 0f, t);
            float timeToCenter = (simRadius - futureRadius) / inwardSpeed;

            float futureAngle = simAngle + angularSpeed * timeToCenter;

            Vector2 point = center + new Vector2(
                Mathf.Cos(futureAngle * Mathf.Deg2Rad),
                Mathf.Sin(futureAngle * Mathf.Deg2Rad)
            ) * futureRadius;

            line.SetPosition(i, point);
        }
    }

    void SetLineAlpha(float alpha)
    {
        Gradient gradient = new Gradient();

        gradient.SetKeys(
            new GradientColorKey[]
            {
                new GradientColorKey(Color.white, 0f),
                new GradientColorKey(Color.white, 1f),
            },
            new GradientAlphaKey[]
            {
                new GradientAlphaKey(alpha, 0f),
                new GradientAlphaKey(alpha * 0.4f, 1f) // fade toward center
            }
        );

        line.colorGradient = gradient;
    }

    void Consume()
    {
        OnConsumed?.Invoke();

        if (destroyEffectPrefab != null)
        {
            Instantiate(destroyEffectPrefab, transform.position, Quaternion.identity);
        }

        AudioHelpers.PlaySoundEffect(destroySoundEffect, Camera.main.transform.position);

        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
       if(collision.transform.tag != "Player") return;

       Destroy(gameObject);
    }
}
