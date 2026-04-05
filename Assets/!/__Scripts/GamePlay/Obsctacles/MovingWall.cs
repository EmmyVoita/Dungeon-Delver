using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class MovingWall : MonoBehaviour
{
    private Rigidbody2D rb;
    private float moveSpeed;
    private float lifeDuration;

    private Vector2 horizontalDir = Vector2.left;
    private Vector2 perpendicularDir;

    private float baseSpeed;
    private Vector2 moveDirection;

    public void Init(Vector2 direction, float _baseSpeed, float _lifeDuration, float speedMultiplier, float speedVariation)
    {
        rb = GetComponent<Rigidbody2D>();
        lifeDuration = _lifeDuration;

    
        // 🔹 Randomized speed from variation
        float variationFactor = Random.Range(1 - speedVariation, 1 + speedVariation);

        baseSpeed = _baseSpeed;

        // 🔹 Final base speed
        moveSpeed = _baseSpeed * variationFactor * speedMultiplier;

         // 🔹 Normalize direction
        moveDirection = direction.normalized;

        // get the vector perpendicular to the movement direction. Linear transformation to rotate 90 counterclosckwise:
        perpendicularDir = Random.value > 0.5f ? new Vector2(-moveDirection.y, moveDirection.x) : new Vector2(moveDirection.y, -moveDirection.x);


        // 🔹 Rotate wall to face its move direction (2D friendly)
        float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        StartCoroutine(StartMovement());
    }

    IEnumerator StartMovement()
    {
        float timer = 0f;

        while (timer < lifeDuration)
        {
            // 🔹 Combined movement (mostly left, small vertical drift)
            Vector2 finalVelocity = (moveDirection * baseSpeed) + (perpendicularDir * moveSpeed * 0.25f);

            rb.linearVelocity = finalVelocity;

            timer += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }

    /*
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Player.Instance.DamageSelf(1);
        }
    }
    */
}
