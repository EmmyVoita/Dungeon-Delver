using UnityEngine;
using System.Collections;
using Unity.VisualScripting;

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
    private float stopTime;
private float resumeTime;
private Vector2 stopPos;



    public override void Init(Vector2 direction, float speed, float spawnTime, float arrivalTime, Vector3 startPos, Vector3 endPos)
    {
        base.Init(direction, speed, spawnTime, arrivalTime, startPos, endPos);

        float totalDistance = Vector2.Distance(startPos, endPos);

        float clampedStopDistance = Mathf.Min(totalDistance - stopDistance, totalDistance);
        float stopRatio = clampedStopDistance / totalDistance;

        stopTime = Mathf.Lerp(spawnTime, arrivalTime, stopRatio);

        float secondsPerBeat = 60f / ArrowSpawner.Instance.ActiveBPM;
        float delayDuration = delayBeats * secondsPerBeat;

        resumeTime = stopTime + delayDuration;
        this.arrivalTime = delayDuration + arrivalTime;

        // ✅ Correct stop position
        stopPos = Vector2.Lerp(startPos, endPos, stopRatio);
    }

    protected override void Update()
    {
        if (_isDead) return;

        float elapsed = (float)MusicManager.Instance.ScaledElapsedTime;

        Vector2 targetPos = Vector2.zero;

        if (elapsed < stopTime)
        {
            float t = Mathf.InverseLerp(spawnTime, stopTime, elapsed);
            targetPos = Vector2.Lerp(startPos, stopPos, t);
        }
        else if (elapsed < resumeTime)
        {
            Debug.Log($"Stop Time => {stopTime}, Resume Time => {resumeTime}, Elapsed => {elapsed}");
            targetPos = stopPos;
        }
        else if(elapsed <= arrivalTime)
        {
            float t = Mathf.InverseLerp(resumeTime, arrivalTime, elapsed);
            targetPos = Vector2.Lerp(stopPos, endPos, t);
        }
        else
        {
            // AFTER goal → continue to center
            float extraTime = elapsed - arrivalTime;

            float postTravelDuration = 0.2f; // tweak this

            float t = Mathf.Clamp01(extraTime / postTravelDuration);

            targetPos = Vector2.Lerp(endPos, Vector2.zero, t);
        }

        SmoothTranslate(targetPos);
    }
}