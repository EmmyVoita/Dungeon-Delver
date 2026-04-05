using UnityEngine;
using System;
using System.Collections;

public class OrbitingTimer : MonoBehaviour
{
    [Header("References")]
    public SpriteRenderer orbSprite;

    [Header("Orbit Settings")]
    public Transform orbitCenter;
    public float orbitRadius = 1.5f;
    public bool clockwise = true;
    public float angleOffset = 90f;   // Where orbit starts (90° = top)

    [Header("Facing Settings")]
    public bool faceCenter = true;   // Always face toward center
    public float faceOffset = 90f;   // Rotation offset for sprite

    [Header("Movement Type")]
    public bool useTickMovement = false;  // 🔹 True = ticking mode
    public float ticksPerSecond = 2f;     // 🔹 How many ticks per second
    public float tickLerpSpeed = 5f;      // 🔹 Smoothness between ticks

    [Header("Timer Settings")]
    public float duration = 5f;
    public bool hideOnEnd = true;

    [Header("Color Settings")]
    public Color startColor = Color.green;
    public Color endColor = Color.red;

    private float timeRemaining;
    private bool isActive = false;
    private Action onTimerEnd;

    private float angle = 0f;
    private bool isFinishingLoop = false; // 🔹 Finishing return-to-start animation

    private float firstTickAngle => angleOffset % 360f;

    void Awake()
    {
        if (orbSprite != null)
            orbSprite.enabled = false;
    }

    void Update()
    {
        if (!isActive || orbitCenter == null) return;

        // Not counting down anymore if in final "lap finish" mode
        if (!isFinishingLoop)
            timeRemaining -= Time.unscaledDeltaTime;

        float t = Mathf.Clamp01(timeRemaining / duration);
        float normalizedTime = 1f - t;

        // 🔁 Smooth continuous movement
        if (!useTickMovement)
        {
            angle = normalizedTime * 360f * (clockwise ? -1f : 1f);
        }
        else
        {
            // ⏱ Ticking movement
            float totalTicks = -1 + duration * ticksPerSecond;
            float tickIndex = Mathf.Floor((duration - timeRemaining) * ticksPerSecond);

            if (isFinishingLoop)
            {
                // Smoothly rotate toward starting position
                angle = Mathf.LerpAngle(angle, firstTickAngle, Time.unscaledDeltaTime * tickLerpSpeed);

                if (Mathf.Abs(Mathf.DeltaAngle(angle, firstTickAngle)) < 2f)
                {
                    angle = firstTickAngle;
                    FinishTimer(); // Only destroy/reset here
                    return;
                }
            }
            else
            {
                angle = (tickIndex / totalTicks) * 360f * (clockwise ? -1f : 1f) + firstTickAngle;
            }
        }

        // 🌀 Calculate position
        float rad = angle * Mathf.Deg2Rad;
        Vector3 targetPos =
            orbitCenter.position + new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0) * orbitRadius;

        transform.position =
            useTickMovement
            ? Vector3.Lerp(transform.position, targetPos, Time.unscaledDeltaTime * tickLerpSpeed)
            : targetPos;

        // 🎯 Face center
        if (faceCenter)
            FaceTowardCenter();

        // 🌈 Color change
        orbSprite.color = Color.Lerp(endColor, startColor, t);

        // 🚨 Timer expired → Start final travel to origin
        if (timeRemaining <= 0f && !isFinishingLoop)
        {
            isFinishingLoop = true;
            onTimerEnd?.Invoke(); // Inform system, but don't destroy yet\
            Hide();
        }
    }

    private void FaceTowardCenter()
    {
        Vector3 dir = orbitCenter.position - transform.position;
        float angleToCenter = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angleToCenter + faceOffset, Vector3.forward);
    }

    public void Show(float seconds, Action onEnd = null, float? customRadius = null)
    {
        duration = seconds;
        timeRemaining = seconds;
        isActive = true;
        orbSprite.enabled = true;
        isFinishingLoop = false;

        if (customRadius.HasValue)
            orbitRadius = customRadius.Value;

        angle = firstTickAngle; // Start at correct angled position
        onTimerEnd = onEnd;
    }

    public void Hide()
    {
        Debug.Log("Hiding OrbitingTimer");
        isActive = false;
        orbSprite.enabled = false;

        if (hideOnEnd)
            Destroy(gameObject);
    }

    private void FinishTimer()
    {
        isActive = false;
        orbSprite.enabled = false;

        if (hideOnEnd)
            Destroy(gameObject);
    }
}
