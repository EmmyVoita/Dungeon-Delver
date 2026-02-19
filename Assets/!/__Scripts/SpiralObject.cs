using UnityEngine;
using System;

public class SpiralObject : MonoBehaviour
{
    public GameObject destroyEffectPrefab;  
    public SoundEffect destroySoundEffect;
    private Vector2 center;
    private float radius;
    private float angle;
    private float inwardSpeed;
    private float angularSpeed;

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

    void Update()
    {
        float dt = Time.deltaTime;

        angle += angularSpeed * dt;
        radius -= inwardSpeed * dt;

        Vector2 pos = center + new Vector2(
            Mathf.Cos(angle * Mathf.Deg2Rad),
            Mathf.Sin(angle * Mathf.Deg2Rad)
        ) * radius;

        transform.position = pos;

        if (radius <= 0.5f)
        {
            Consume();
        }
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
}
