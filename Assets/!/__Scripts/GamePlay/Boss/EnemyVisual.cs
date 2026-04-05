using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(SpriteRenderer))]
public class EnemyVisual : MonoBehaviour
{
    [Header("Idle Float Settings")]
    [SerializeField] private float floatHeight = 0.2f;
    [SerializeField] private float floatDuration = 1.5f;
    [SerializeField] private Ease floatEase = Ease.InOutSine;

    [Header("Hit Feedback Settings")]
    [SerializeField] private float shakeStrength = 0.1f;
    [SerializeField] private float shakeDuration = 0.25f;
    [SerializeField] private int shakeVibrato = 10;

    [Header("Attack Animation Settings")]
    [SerializeField] private float attackDistance = 0.3f;
    [SerializeField] private float attackDuration = 0.25f;
    [SerializeField] private Ease attackEaseOut = Ease.OutQuad;
    [SerializeField] private Ease attackEaseReturn = Ease.InSine;

    [Header("Sprite / Animation")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Animator animator;

    private Vector3 startPos;
    private Tween floatTween;
    private Tween shakeTween;
    private Tween attackTween;



    void Awake()
    {
        if (!spriteRenderer)
            spriteRenderer = GetComponent<SpriteRenderer>();
        startPos = transform.localPosition;
    }

    void OnEnable()
    {
        StartFloating();
        ArrowBase.OnArrowResolved += HandleArrowDeath;
        //ScreenDimmerManager.OnObstacleAppears += PlayAttackAnimation;
    }

    void OnDisable()
    {
        floatTween?.Kill();
        shakeTween?.Kill();
        attackTween?.Kill();
        ArrowBase.OnArrowResolved -= HandleArrowDeath;
        //ScreenDimmerManager.OnObstacleAppears -= PlayAttackAnimation;
    }

    private void StartFloating()
    {
        floatTween?.Kill();

        floatTween = transform.DOLocalMoveY(startPos.y + floatHeight, floatDuration)
            .SetEase(floatEase)
            .SetLoops(-1, LoopType.Yoyo);
    }

    private void HandleArrowDeath(ArrowResolvedData data)
    {
        // When arrow hits — shake and maybe flash
        PlayDamageShake();

        if (animator != null)
            animator.SetTrigger("Hit");
    }

    private void PlayDamageShake()
    {
        shakeTween?.Kill();

        shakeTween = transform.DOShakePosition(
            duration: shakeDuration,
            strength: new Vector3(shakeStrength, 0f, 0f),
            vibrato: shakeVibrato,
            randomness: 0,
            snapping: false,
            fadeOut: true
        ).SetEase(Ease.OutSine)
         .OnComplete(() => transform.localPosition = startPos);
    }

    // 🗡️ Attack lunge animation
    public void PlayAttackAnimation()
    {
        if (animator != null)
            animator.SetTrigger("Attack");

        attackTween?.Kill();

        // Stop floating temporarily
        floatTween?.Pause();

        // Move forward and back smoothly
        attackTween = DOTween.Sequence()
            .Append(transform.DOLocalMoveX(startPos.x + attackDistance, attackDuration * 0.5f).SetEase(attackEaseOut))
            .Append(transform.DOLocalMoveX(startPos.x, attackDuration * 0.5f).SetEase(attackEaseReturn))
            .OnComplete(() =>
            {
                floatTween?.Play(); // Resume float motion
            });
    }

    public void PlayHitAnimation()
    {
        if (animator != null)
            animator.SetTrigger("Hit");
    }

    public void SetSprite(Sprite newSprite)
    {
        if (animator != null)
        {
            Debug.LogWarning("⚠️ Trying to set sprite manually while Animator is active — ignoring.");
            return;
        }

        spriteRenderer.sprite = newSprite;
    }

    public void ResetPosition()
    {
        floatTween?.Kill();
        transform.localPosition = startPos;
        StartFloating();
    }
}
