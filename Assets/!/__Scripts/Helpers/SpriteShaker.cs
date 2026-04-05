using UnityEngine;
using System.Collections;

public class SpriteShaker : MonoBehaviour
{
    public float shakeDuration = 0.2f;
    public float shakeMagnitude = 0.1f;
    public float shakeSpeed = 25f; // lower = slower shake, higher = faster

    private Vector3 originalLocalPos;
    private Coroutine shakeRoutine;

    void Awake()
    {
        originalLocalPos = transform.localPosition;
        Shake();
    }

    public void Shake()
    {
        if (shakeRoutine != null)
            StopCoroutine(shakeRoutine);

        shakeRoutine = StartCoroutine(DoShake());
    }

    IEnumerator DoShake()
    {
        float elapsed = 0f;
        float timeOffset = Random.value * 100f; // makes shakes feel less identical

        while (elapsed < shakeDuration)
        {
            elapsed += Time.deltaTime;
            
            // Smooth random shake using sine waves
            float x = Mathf.Sin((elapsed + timeOffset) * shakeSpeed) * shakeMagnitude;
            float y = Mathf.Cos((elapsed + timeOffset) * shakeSpeed * 1.3f) * shakeMagnitude;

            transform.localPosition = originalLocalPos + new Vector3(x, y, 0);
            yield return null;
        }

        transform.localPosition = originalLocalPos;
    }
}

