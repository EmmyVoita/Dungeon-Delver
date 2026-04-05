using UnityEngine;

public class BreakSpawnProjectile : MonoBehaviour
{
    public float moveSpeed = 3f;
    public float lifetime = 4f;

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.position += transform.up * moveSpeed * Time.deltaTime;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Let player handle damage logic
            Destroy(gameObject);
        }
    }
}
