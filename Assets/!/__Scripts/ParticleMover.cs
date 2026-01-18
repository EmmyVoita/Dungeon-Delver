using UnityEngine;
using DG.Tweening;

public class ParticleMover : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ParticleSystem particleSystemRef;

    [Header("Movement Settings")]
    [SerializeField] private float moveDuration = 0.6f;
    [SerializeField] private float moveDelay = 0.5f;       // ⏱ delay before it starts moving
    [SerializeField] private Ease moveEase = Ease.InOutQuad;
    [SerializeField] private bool destroyOnArrival = true;
    [SerializeField] private float destroyDelay = 0.5f;

    private Transform target;

    /// <summary>
    /// Initializes and starts the movement of this particle object.
    /// </summary>
    public void Initialize(Transform start, Transform end, float durationOverride = -1f)
    {
        transform.position = start.position;
        target = end;

        if (particleSystemRef == null)
            particleSystemRef = GetComponentInChildren<ParticleSystem>();

        // Start the particle effect immediately
        if (particleSystemRef != null)
            particleSystemRef.Play();

        float duration = (durationOverride > 0f) ? durationOverride : moveDuration;

        // Build the tween sequence
        Sequence seq = DOTween.Sequence();

        // Step 1: Optional delay before motion starts
        if (moveDelay > 0)
            seq.AppendInterval(moveDelay);

        // Step 2: Perform movement
        seq.Append(transform.DOMove(target.position, duration).SetEase(moveEase));

        // Step 3: Stop particle + destroy after completion
        seq.OnComplete(() =>
        {
            if (particleSystemRef != null)
                particleSystemRef.Stop();

            if (destroyOnArrival)
                Destroy(gameObject, destroyDelay);
        });

        seq.Play();
    }
}
