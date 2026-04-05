using System.Collections;
using UnityEngine;

public class BackgroundPulseOnJump : MonoBehaviour
{
    [Header("Pulse Settings")]
    public float growAmount = 0.2f;    // how MUCH the background increases per up-jump
    public float maxScale = 1.6f;      // max scale limit
    public float shrinkSpeed = 0.8f;   // how fast it returns to normal (per second)

    private Vector3 baseScale;
    private Vector3 currentScale;
    private bool isGrowing = false;

    private bool canGrow = false;

    void Awake()
    {
        baseScale = transform.localScale;
        currentScale = baseScale;
        canGrow = false;
    }

    void OnEnable()
    {
        Player.OnJumped += HandleJump;
    }

    void OnDisable()
    {
        Player.OnJumped -= HandleJump;
    }

    void HandleJump(Vector2 dir)
    {
        if (!canGrow) return;

        if(dir == Vector2.right) return;

        // Increase scale smoothly
        currentScale += Vector3.one * growAmount;

        // Clamp to prevent too large growth
        currentScale = Vector3.Min(currentScale, baseScale * maxScale);
    }

    public IEnumerator ScaleOut()
    {
        canGrow = false;

        float t = 0f;
        Vector3 startScale = transform.localScale;
        Vector3 targetScale = transform.localScale * 10;

        while (t < 1f)
        {
            t += Time.deltaTime; // Adjust speed as needed
            transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }

        transform.localScale = targetScale;
    }

    public IEnumerator ScaleIn()
    {
        float t = 0f;
        Vector3 startScale = baseScale * 10;
        Vector3 targetScale = baseScale;

        while (t < 1f)
        {
            t += Time.deltaTime; // Adjust speed as needed
            transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }

        transform.localScale = targetScale;

        canGrow = true;
    }

    void Update()
    {
        // Smoothly ease back to normal size over time
        currentScale = Vector3.Lerp(currentScale, baseScale, Time.deltaTime * shrinkSpeed);
        transform.localScale = currentScale;
    }
}
