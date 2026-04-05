using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerProjectile : MonoBehaviour
{
    public float initialSpeed = 10f;
    public float lifetime = 3f;

    private Rigidbody2D rb;

    public void Initialize(Vector2 dir)
    {
        rb = GetComponent<Rigidbody2D>();

        Vector2 normalizedDir = dir.normalized;

        rb.linearVelocity = normalizedDir * initialSpeed;


        Destroy(gameObject, lifetime);
    }


    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Obstacle"))
        {
            Destroy(gameObject);
        }
    }
}
