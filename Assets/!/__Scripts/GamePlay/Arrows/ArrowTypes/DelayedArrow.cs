using UnityEngine;
using System.Collections;

public class DelayedArrow : ArrowBase
{
    [Header("Delay Settings")]
    [Tooltip("Distance from player where the arrow will stop")]
    public float stopDistance = 2f;

    [Tooltip("How many beats the arrow will pause")]
    public float delayBeats = 2f;

    [Header("Optional")]
    public bool useManualStopDistance = true;

    private bool hasStopped = false;
    private Coroutine delayRoutine;

    void Update()
    {
        if (hasStopped) return;

        float distanceToCenter = Vector2.Distance(transform.position, Vector2.zero);

        if (distanceToCenter <= stopDistance)
        {
            StartDelay();
        }
    }

    private void StartDelay()
    {
        hasStopped = true;

        // Stop movement
        rb.linearVelocity = Vector2.zero;

        if (delayRoutine != null)
            StopCoroutine(delayRoutine);

        delayRoutine = StartCoroutine(DelayThenResume());
    }

    private IEnumerator DelayThenResume()
    {
        // --- BPM timing ---
        float bpm = (ArrowSpawner.Instance != null) ? ArrowSpawner.Instance.ActiveBPM : 120f;
        float secondsPerBeat = 60f / bpm;
        float delayDuration = delayBeats * secondsPerBeat;

        if (delayDuration <= 0f)
            delayDuration = 0.1f;

        yield return new WaitForSeconds(delayDuration);

        // Resume movement toward player
        rb.linearVelocity = -direction * speed;
    }
}