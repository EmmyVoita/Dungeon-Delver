using UnityEngine;
using System.Collections;
using Unity.VisualScripting;

public class BlackholeEmitter : MonoBehaviour
{
    [Header("References")]
    public Vector3 centerTarget;
    public GameObject spikyBallPrefab;

    [Header("Burst Settings")]
    public int ballsPerBurst = 6;
    public float burstSpacing = 0.08f;
    public float ballSpeed = 6f;

    [Header("Audio")]
    public SoundEffect shootSound;

    public void FireBurst()
    {
        StartCoroutine(BurstRoutine());
    }

    private IEnumerator BurstRoutine()
    {
        for (int i = 0; i < ballsPerBurst; i++)
        {
            SpawnBall();
            AudioHelpers.PlaySoundEffect(shootSound, this.transform.position);
            yield return new WaitForSeconds(burstSpacing);
        }
    }

    private void SpawnBall()
    {
        if (centerTarget == null) return;

        Vector2 dir = (centerTarget- transform.position).normalized;

        GameObject ball = Instantiate(spikyBallPrefab, transform.position, Quaternion.identity);

        ball.transform.up = dir;

        Rigidbody2D rb = ball.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = dir * ballSpeed;
        }
    }
}