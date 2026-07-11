using UnityEngine;
using System;
using Unity.VisualScripting;

public class SpiralObject : MonoBehaviour
{
    [Header("Effects")]
    public GameObject destroyEffectPrefab;
    public GameObject destroyPlayerEffectPrefab;
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
    private Gradient originalGradient;

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
            originalGradient = line.colorGradient;
            //SetLineAlpha(0f);
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
     
    }

   void HandleLineFade()
    {
        float timeSinceSpawn = Time.time - spawnTime;

        float t = Mathf.InverseLerp(
            fadeInDelay,
            fadeInDelay + fadeInDuration,
            timeSinceSpawn
        );

        currentAlpha = Mathf.Clamp01(t);

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

    void SetLineAlpha(float alphaMultiplier)
    {
        Gradient gradient = new Gradient();

        GradientColorKey[] colorKeys = originalGradient.colorKeys;

        GradientAlphaKey[] oldAlphaKeys = originalGradient.alphaKeys;
        GradientAlphaKey[] newAlphaKeys = new GradientAlphaKey[oldAlphaKeys.Length];

        for (int i = 0; i < oldAlphaKeys.Length; i++)
        {
            newAlphaKeys[i] = new GradientAlphaKey(
                oldAlphaKeys[i].alpha * alphaMultiplier,
                oldAlphaKeys[i].time
            );
        }

        gradient.SetKeys(colorKeys, newAlphaKeys);

        line.colorGradient = gradient;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.transform.tag == "Player")
        {
            Instantiate(destroyPlayerEffectPrefab, transform.position, Quaternion.identity);
            AudioHelpers.PlaySoundEffect(destroySoundEffect, Camera.main.transform.position);
            Destroy(gameObject);
        }

        

        if (!collision.CompareTag("Center"))
            return;

       
        ConsumeWithDirection();
    }

    void ConsumeWithDirection()
    {
        OnConsumed?.Invoke();

        // Direction from center → object
        Vector2 normal = ((Vector2)transform.position - center).normalized;

        // Optional tangent (if you want swirl instead)
        Vector2 tangent = new Vector2(-normal.y, normal.x);

        if (destroyEffectPrefab != null)
        {
            // Choose which direction you want:
            Vector2 dir = normal; // or tangent

            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

            Instantiate(
                destroyEffectPrefab,
                transform.position,
                Quaternion.Euler(0, 0, angle)
            );
        }

        AudioHelpers.PlaySoundEffect(destroySoundEffect, Camera.main.transform.position);

        Destroy(gameObject);
    }

}
