using UnityEngine;
using DG.Tweening;
using System.Collections;

public class ScreenShakeManager : MonoBehaviour
{
    public static ScreenShakeManager Instance { get; private set; }

    private Vector3 originalPos;
    private Coroutine shakeRoutine;

    private void Awake()
    {
        Instance = this;
        originalPos = transform.localPosition;
    }

    public void Shake(ScreenShakeRequest request)
    {
        if (request.directional)
        {
            ShakeDirectional(request);
        }
        else
        {
            ShakeRandom(request);
        }
    }

    private void ShakeDirectional(ScreenShakeRequest request)
    {
        Vector2 n = request.direction.normalized;
        Vector3 offset = new Vector3(-n.x, -n.y, 0) * request.magnitude;

        transform.DOKill();

        transform
            .DOPunchPosition(offset, request.duration, 8, 0.6f)
            .OnComplete(() => transform.localPosition = originalPos);
    }

    private void ShakeRandom(ScreenShakeRequest request)
    {
        if (shakeRoutine != null)
            StopCoroutine(shakeRoutine);

        shakeRoutine = StartCoroutine(
            DoShake(request.duration, request.magnitude, request.unscaled)
        );
    }

    private IEnumerator DoShake(float duration, float magnitude, bool unscaled)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
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