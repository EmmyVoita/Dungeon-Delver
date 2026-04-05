using UnityEngine;
using System.Collections;

public class ScreenShake : MonoBehaviour
{
    private Vector3 originalPos;
    private Coroutine shakeRoutine;
    [SerializeField] private float gameOverShakeDuration = 0.5f;
    [SerializeField] private float gameOverShakeMagnitude = 0.3f;

    public static ScreenShake Instance { get; private set; }

    void OnEnable()
    {
        UIManager.OnGameOver += HandleGameOverShake;
    }

    void OnDisable()
    {
        UIManager.OnGameOver -= HandleGameOverShake;
    }

    private void HandleGameOverShake()
    {
        Shake(gameOverShakeDuration, gameOverShakeMagnitude);
    }


    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void Shake(float duration = 0.3f, float magnitude = 0.4f, bool unscaled = true)
    {
        if (shakeRoutine != null)
            StopCoroutine(shakeRoutine);

        shakeRoutine = StartCoroutine(DoShake(duration, magnitude, unscaled));
    }

    private IEnumerator DoShake(float duration, float magnitude, bool unscaled)
    {
        originalPos = transform.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            // use unscaledTime if you want shake to work while paused
            float delta = unscaled ? Time.unscaledDeltaTime : Time.deltaTime;
            elapsed += delta;

            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            transform.localPosition = originalPos + new Vector3(x, y, 0);

            yield return null;
        }

        transform.localPosition = originalPos;
        shakeRoutine = null;
    }
}
