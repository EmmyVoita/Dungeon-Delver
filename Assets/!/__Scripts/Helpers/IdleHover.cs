using UnityEngine;
using DG.Tweening;
using System;

public class IdleHover : MonoBehaviour
{
    [Header("Behavior Toggles")]
    public bool enableHover = true;
    public bool enableSway = true;
    public bool enableScoreJump = true;
    public bool scaleOnJump = false;

    [Header("Idle Hover")]
    public float hoverAmplitude = 10f;
    public float hoverDuration = 2f;

    [Header("Idle Sway Axis")]
    public bool swayX = false;
    public bool swayY = false;
    public bool swayZ = true;

    [Header("Idle Sway Angles")]
    public float swayAngleX = 6f;
    public float swayAngleY = 6f;
    public float swayAngleZ = 6f;

    [Header("Idle Sway Durations")]
    public float swayDurationX = 2.5f;
    public float swayDurationY = 2.5f;
    public float swayDurationZ = 2.5f;

    [Header("Micro Shake")]
    public float shakeDuration = 0.15f;
    public float shakeStrength = 4f;
    public int shakeVibrato = 10;

    [Header("Jump When Score Changes")]
    public float jumpStrength = 20f;
    public float jumpDuration = 0.25f;
    public float jumpScaleAmount = 1.4f;
    public Transform jumpTargetTransform;

    // Baselines
    private Vector3 originalPos;
    private Quaternion originalRot;
    private Vector3 jumpBaseLocalPos;

    // Tweens
    private Tween hoverTween;
    private Sequence swaySeq;
    private Tween jumpTween;
    private Tween scaleTween;
    private Tween shakeTween;

    // ----------------------------------------------------
    // LIFECYCLE
    // ----------------------------------------------------

    private void Awake()
    {
        originalPos = transform.localPosition;
        originalRot = transform.localRotation;

        if (jumpTargetTransform != null)
            jumpBaseLocalPos = jumpTargetTransform.localPosition;
    }

    private void OnEnable()
    {
        if (enableScoreJump)
            ScoreManager.OnScoreUpdated += HandleScoreJump;
    }

    private void OnDisable()
    {
        ScoreManager.OnScoreUpdated -= HandleScoreJump;
    }

    private void Start()
    {
        ApplyState();
    }

    // ----------------------------------------------------
    // STATE CONTROL
    // ----------------------------------------------------

    public void ApplyState()
    {
        StopAllBehaviors();

        if (enableHover)
            StartIdleHover();

        if (enableSway)
            StartIdleSway();
    }

    public void UpdateState()
    {
        if (enableHover && hoverTween == null)
            StartIdleHover();

        if (enableSway && swaySeq == null)
            StartIdleSway();
    }

    public void StopAllBehaviors()
    {
        hoverTween?.Kill();
        swaySeq?.Kill();
        jumpTween?.Kill();
        scaleTween?.Kill();
        shakeTween?.Kill();

        transform.localPosition = originalPos;
        transform.localRotation = originalRot;

        if (jumpTargetTransform != null)
            jumpTargetTransform.localPosition = jumpBaseLocalPos;

        hoverTween = null;
        swaySeq = null;
    }

    // ----------------------------------------------------
    // IDLE HOVER
    // ----------------------------------------------------

    private void StartIdleHover()
    {
        hoverTween = transform.DOLocalMoveY(
            originalPos.y + hoverAmplitude,
            hoverDuration
        )
        .SetEase(Ease.InOutSine)
        .SetLoops(-1, LoopType.Yoyo);
    }

    // ----------------------------------------------------
    // IDLE SWAY
    // ----------------------------------------------------

    private void StartIdleSway()
    {
        swaySeq?.Kill();

        swaySeq = DOTween.Sequence();

        Vector3 startRot = originalRot.eulerAngles;
        Vector3 targetRot = startRot;

        if (swayX)
            targetRot.x += swayAngleX;

        if (swayY)
            targetRot.y += swayAngleY;

        if (swayZ)
            targetRot.z += swayAngleZ;

        float duration = Mathf.Max(
            swayDurationX,
            Mathf.Max(swayDurationY, swayDurationZ)
        );

        swaySeq.Append(
            transform.DOLocalRotate(
                targetRot,
                duration
            ).SetEase(Ease.InOutSine)
        );

        swaySeq.Append(
            transform.DOLocalRotate(
                startRot,
                duration
            ).SetEase(Ease.InOutSine)
        );

        swaySeq.SetLoops(-1);
    }

    // ----------------------------------------------------
    // SCORE JUMP
    // ----------------------------------------------------

    public void JumpImmediate(Action onComplete = null)
    {
        if (!enableScoreJump || jumpTargetTransform == null)
            return;

        DoScoreJump(onComplete);
    }

    private void HandleScoreJump(int newScore)
    {
        if (!enableScoreJump || jumpTargetTransform == null)
            return;

        DoScoreJump();
    }

    private void DoScoreJump(Action onComplete = null)
    {
        jumpTween?.Kill(false);
        scaleTween?.Kill(false);

        jumpTween = jumpTargetTransform.DOLocalMoveY(
            jumpBaseLocalPos.y + jumpStrength,
            jumpDuration * 0.5f
        )
        .SetEase(Ease.OutQuad)
        .OnComplete(() =>
            jumpTargetTransform.DOLocalMoveY(
                jumpBaseLocalPos.y,
                jumpDuration * 0.5f
            )
            .SetEase(Ease.InQuad)
            .OnComplete(() => onComplete?.Invoke())
        );

        if (scaleOnJump)
        {
            scaleTween = jumpTargetTransform.DOScale(
                jumpScaleAmount,
                jumpDuration * 0.5f
            )
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
                jumpTargetTransform.DOScale(
                    1f,
                    jumpDuration * 0.5f
                ).SetEase(Ease.InQuad)
            );
        }
    }

    // ----------------------------------------------------
    // SHAKE
    // ----------------------------------------------------

    public void ShakeJumpTarget()
    {
        if (jumpTargetTransform == null)
            return;

        shakeTween?.Kill();

        shakeTween = jumpTargetTransform.DOShakePosition(
            duration: shakeDuration,
            strength: new Vector3(shakeStrength, shakeStrength, 0f),
            vibrato: shakeVibrato,
            randomness: 90,
            snapping: false,
            fadeOut: true
        )
        .OnComplete(() =>
        {
            jumpTargetTransform.localPosition = jumpBaseLocalPos;
        });
    }
}