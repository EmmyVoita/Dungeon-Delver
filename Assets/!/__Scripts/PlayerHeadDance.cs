using UnityEngine;
using DG.Tweening;

public class PlayerHeadDance : MonoBehaviour
{
    [Header("Dance Settings")]
    public float tiltAmount = 8f;        // degrees left/right
    public float bobAmount = 0.05f;      // optional vertical bob
    public float danceDuration = 0.12f;  // how fast the motion is

    [Header("Easing")]
    public Ease leanEase = Ease.OutQuad;
    public Ease returnEase = Ease.InQuad;

    [Header("Direction")]
    public bool randomizeDirection = true;

    private Vector3 startLocalPos;
    private Quaternion startLocalRot;

    private Sequence danceSequence;

    private int alternatingSign = 1; // +1 or -1, flips each hit

    void Awake()
    {
        startLocalPos = transform.localPosition;
        startLocalRot = transform.localRotation;
    }

    void OnEnable()
    {
        // 🔔 Hook into your existing event
        ArrowBase.OnArrowResolved += OnArrowResolved;
    }

    void OnDisable()
    {
        ArrowBase.OnArrowResolved -= OnArrowResolved;
    }

    void OnArrowResolved(ArrowResolvedData data)
    {
        PlayDance();
    }

    void PlayDance()
    {
        // Kill previous dance if still playing
        danceSequence?.Kill();

        float dir;

        if (randomizeDirection)
        {
            dir = Random.value < 0.5f ? -1f : 1f;
        }
        else
        {
            dir = alternatingSign;
            alternatingSign *= -1; // flip for next time
        }

        float targetTilt = tiltAmount * dir;
        float targetBob = bobAmount;

        danceSequence = DOTween.Sequence();

        // Lean + bob
        danceSequence.Append(
            transform.DOLocalRotate(
                new Vector3(0f, 0f, targetTilt),
                danceDuration
            ).SetEase(leanEase)
        );

        danceSequence.Join(
            transform.DOLocalMoveY(
                startLocalPos.y + targetBob,
                danceDuration
            ).SetEase(leanEase)
        );

        // Return to neutral
        danceSequence.Append(
            transform.DOLocalRotate(
                startLocalRot.eulerAngles,
                danceDuration * 1.1f
            ).SetEase(returnEase)
        );

        danceSequence.Join(
            transform.DOLocalMoveY(
                startLocalPos.y,
                danceDuration * 1.1f
            ).SetEase(returnEase)
        );
    }
}
