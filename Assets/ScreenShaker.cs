using UnityEngine;
using DG.Tweening;

public class ScreenShaker : MonoBehaviour
{
    public static ScreenShaker Instance;

    public float shakeStrength = 0.3f;
    public float shakeDuration = 0.15f;

    private Vector3 originalPos;

    void Awake()
    {
        Instance = this;
        originalPos = transform.localPosition;
    }

    public void ShakeInDirection(Vector2 dir)
    {
        // Normalize direction so shake amount stays consistent
        Vector2 n = dir.normalized;

        // Convert to 3D vector
        Vector3 offset = new Vector3(-n.x, -n.y, 0) * shakeStrength;

        // Kill old shake if still running
        transform.DOKill();

        // Punch position and return to center automatically
        transform.DOPunchPosition(offset, shakeDuration, vibrato: 8, elasticity: 0.6f)
            .OnComplete(() => transform.localPosition = originalPos);
    }
}
