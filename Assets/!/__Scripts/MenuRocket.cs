using UnityEngine;

public class MenuRocket : MonoBehaviour
{
    public Vector2 velocity;

    [HideInInspector] public float depth;
    [HideInInspector] public float originalScale;

    public GameObject destroyEffect;
    public float centerPull = 0.4f;
    public float maxSpeed = 10f;
    public float minScale;

    float currentScale;

    void Start()
    {
        currentScale = originalScale;
    }

    void Update()
    {
        Vector2 pos = transform.position;

        // slight pull toward center
        Vector2 centerDir = -pos.normalized;
        velocity += centerDir * centerPull * Time.deltaTime;

        foreach (var hole in MenuBlackHole.All)
        {
            Vector2 dir = hole.Position - pos;
            float dist = dir.magnitude;

            if (dist < hole.influenceRadius)
            {
                float gravity =
                    hole.gravityStrength * depth /
                    Mathf.Max(dist * dist, 0.25f);

                velocity += dir.normalized * gravity * Time.deltaTime;

                // encourage orbit / slingshot
                Vector2 tangent = Vector2.Perpendicular(dir.normalized);
                velocity += tangent * 0.6f * depth * Time.deltaTime;

                // shrink when approaching blackhole
                float t = Mathf.InverseLerp(
                    hole.influenceRadius,
                    hole.consumeRadius,
                    dist);

                float newScale =
                    Mathf.Lerp(originalScale, originalScale * 0.25f, t);

                // only allow shrinking
                currentScale = Mathf.Min(currentScale, newScale);

                // optional spin
                transform.Rotate(0, 0, 300f * t * Time.deltaTime);
            }

            if (dist < hole.consumeRadius)
            {
                var effect = Instantiate(destroyEffect, transform.position, Quaternion.identity);
                effect.transform.localScale = this.transform.localScale;
                Destroy(gameObject);
                return;
            }
        }

        transform.localScale = Vector3.one * Mathf.Max(currentScale, minScale);

        velocity = Vector2.ClampMagnitude(velocity, maxSpeed);

        transform.position += (Vector3)(velocity * Time.deltaTime);

        RotateToVelocity();
    }

    void RotateToVelocity()
    {
        if (velocity.sqrMagnitude < 0.01f)
            return;

        float angle =
            Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg - 90f;

        transform.rotation = Quaternion.Euler(0, 0, angle);
    }
}