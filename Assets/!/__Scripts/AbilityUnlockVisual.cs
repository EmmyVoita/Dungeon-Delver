using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class AbilityUnlockVisual : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform lockTransform;
    [SerializeField] private CanvasGroup lockCanvasGroup;
    [Header("Effects")]
    [SerializeField] private SoundEffect unlockSound;
    [SerializeField] private SoundEffect popSound;

    [Header("Animation")]
    [SerializeField] private float anticipationScale = 0.92f;
    [SerializeField] private float anticipationDuration = 0.06f;

    [SerializeField] private float shakeDuration = 0.25f;
    [SerializeField] private float shakeStrength = 12f;
    [SerializeField] private int shakeVibrato = 25;

    [SerializeField] private float popHeight = 120f;
    [SerializeField] private float popDuration = 0.35f;
    [SerializeField] private float popRotation = 25f;

    [SerializeField] private float cardPunchScale = 0.12f;
    [SerializeField] private float glowFlashDuration = 0.2f;

    private Vector2 _lockStartPos;
    private Vector3 _lockStartScale;
    private Quaternion _lockStartRot;

    private void Awake()
    {
        _lockStartPos = lockTransform.anchoredPosition;
        _lockStartScale = lockTransform.localScale;
        _lockStartRot = lockTransform.localRotation;
    }

    [ContextMenu("Play Unlock Animation")]
    public void TestPlay()
    {
        StartCoroutine(PlayUnlockAnimation());
    }

    public void ShowLocked()
    {
        lockCanvasGroup.alpha = 1f;
    }

    public void ShowUnlocked()
    {
        lockCanvasGroup.alpha = 0f;
    }

    public IEnumerator PlayUnlockAnimation()
    {
        lockTransform.DOKill();
        transform.DOKill();

        // -----------------------------------------
        // Reset
        // -----------------------------------------

        lockTransform.gameObject.SetActive(true);

        lockTransform.anchoredPosition = _lockStartPos;
        lockTransform.localScale = _lockStartScale;
        lockTransform.localRotation = _lockStartRot;

        if (lockCanvasGroup != null)
            lockCanvasGroup.alpha = 1f;

        // -----------------------------------------
        // Anticipation squash
        // -----------------------------------------

        yield return lockTransform
            .DOScale(anticipationScale, anticipationDuration)
            .SetEase(Ease.OutQuad)
            .WaitForCompletion();

        // -----------------------------------------
        // Re-expand + shake
        // -----------------------------------------

        Sequence shakeSeq = DOTween.Sequence();

        shakeSeq.Append(
            lockTransform
                .DOScale(1.1f, 0.08f)
                .SetEase(Ease.OutBack)
        );

        shakeSeq.Join(
            lockTransform.DOShakeAnchorPos(
                shakeDuration,
                shakeStrength,
                shakeVibrato,
                randomness: 90,
                snapping: false,
                fadeOut: true
            )
        );

        AudioHelpers.PlaySoundEffect(unlockSound, transform.position);

        yield return shakeSeq.WaitForCompletion();

        // -----------------------------------------
        // POP OFF
        // -----------------------------------------

        Sequence popSeq = DOTween.Sequence();

        popSeq.Append(
            lockTransform
                .DOAnchorPosY(
                    _lockStartPos.y + popHeight,
                    popDuration
                )
                .SetEase(Ease.OutQuad)
        );

        popSeq.Join(
            lockTransform
                .DORotate(
                    new Vector3(0, 0, Random.Range(-popRotation, popRotation)),
                    popDuration
                )
                .SetEase(Ease.OutQuad)
        );

        popSeq.Join(
            lockTransform
                .DOScale(1.35f, popDuration * 0.5f)
                .SetEase(Ease.OutBack)
        );

        if (lockCanvasGroup != null)
        {
            popSeq.Join(
                lockCanvasGroup
                    .DOFade(0f, popDuration * 0.8f)
            );
        }


        AudioHelpers.PlaySoundEffect(popSound, transform.position);

        // -----------------------------------------
        // Card bounce
        // -----------------------------------------

        transform.DOPunchScale(
            Vector3.one * cardPunchScale,
            0.35f,
            8,
            0.7f
        );

        yield return popSeq.WaitForCompletion();

        // -----------------------------------------
        // Finalize
        // -----------------------------------------

        lockTransform.gameObject.SetActive(false);
    }
}