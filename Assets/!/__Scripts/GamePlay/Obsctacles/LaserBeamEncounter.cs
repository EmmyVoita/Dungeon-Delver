using UnityEngine;
using System.Collections;

public class LaserBeamEncounter : MonoBehaviour
{
    [SerializeField] private LaneDodgerConfig config;

    [Header("Spawn")]
    [SerializeField] private float spawnX = 8f;

    [Header("Tracking")]
    [SerializeField] private float followSmoothTime = 0.4f;
    [SerializeField] private float maxFollowSpeed = 10f;

    [Header("Attack")]
    [SerializeField] private int fireCount = 5;
    [SerializeField] private float onTargetThreshold = 0.05f;
    [SerializeField] private float windupDelay = 0.6f;
    [SerializeField] private float sweepSpeed = 6f;
    [SerializeField] private float waitAfterFireDelay = 0.5f;

    [Header("References")]
    [SerializeField] private GameObject laserObject;
    [SerializeField] private LaserBeamHeightPulse beam;

    [Header("References")]
    [SerializeField] private SoundEffect activateSound;
    [SerializeField] private AudioSource audio;

    private float velocityY;
    private float targetY;
    private bool doTrack;

    void Start()
    {
        laserObject.SetActive(false);
        StartCoroutine(PerformAttackSequence());
    }

    void Update()
    {
        TrackToTargetHeight();
    }

    // -------------------------------------
    // MAIN SEQUENCE
    // -------------------------------------
    IEnumerator PerformAttackSequence()
    {
        for (int i = 0; i < fireCount; i++)
        {
            //int startLane = Random.Range(0f, 1f) > 0.5f ? 0 : config.maxLanes;
            int startLane = i % 2 == 0 ? 0 : config.maxLanes;
            int endLane = GetOppositeInnerLane(startLane);

            float startY = GetLaneY(startLane);
            float endY = GetLaneY(endLane);

            // Move to start
            yield return MoveToY(startY);


            AudioHelpers.PlaySoundEffect(activateSound, transform.position);
            // WINDUP (telegraph)
            
            yield return new WaitForSeconds(windupDelay);

            laserObject.SetActive(true);
            audio.Play();

            beam.Play(2f);

            // SWEEP
            yield return SweepToY(endY);

            audio.Stop();

            // Turn off laser
            laserObject.SetActive(false);

            yield return new WaitForSeconds(waitAfterFireDelay);
        }

        Cleanup();
    }

    // -------------------------------------
    // MOVEMENT HELPERS
    // -------------------------------------

    IEnumerator MoveToY(float y)
    {
        targetY = y;
        doTrack = true;

        yield return new WaitUntil(() => Mathf.Abs(transform.position.y - targetY) < onTargetThreshold);

        doTrack = false;
    }

    IEnumerator SweepToY(float endY)
    {
        doTrack = false;

        while (Mathf.Abs(transform.position.y - endY) > onTargetThreshold)
        {
            float newY = Mathf.MoveTowards(
                transform.position.y,
                endY,
                sweepSpeed * Time.deltaTime
            );

            transform.position = new Vector3(spawnX, newY, 0f);

            yield return null;
        }
    }

    void TrackToTargetHeight()
    {
        if (!doTrack) return;

        float newY = Mathf.SmoothDamp(
            transform.position.y,
            targetY,
            ref velocityY,
            followSmoothTime,
            maxFollowSpeed
        );

        transform.position = new Vector3(spawnX, newY, 0f);
    }

    // -------------------------------------
    // UTIL
    // -------------------------------------

    int GetOppositeInnerLane(int startLane)
    {
        int lastLane = config.maxLanes;

        // If starting from bottom → go near top (second to last)
        if (startLane == 0)
        {
            return Mathf.Max(1, lastLane - 2); // ensure valid
        }

        // If starting from top → go near bottom
        if (startLane == lastLane)
        {
            return Mathf.Min(lastLane - 1, 2);
        }

        // Fallback (shouldn't really happen in your current setup)
        return Random.Range(1, lastLane - 1);
    }

    float GetLaneY(int lane)
    {
        float centerOffset = (config.maxLanes - 1) * 0.5f;
        return (lane - centerOffset) * config.laneSpacing;
    }

    void Cleanup()
    {
        Destroy(gameObject);
    }
}