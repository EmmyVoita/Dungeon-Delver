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

    [Header("Micro Shake")]
    public float shakeDuration = 0.15f;
    public float shakeStrength = 4f;
    public int shakeVibrato = 10;



    [Header("Idle Sway")]
    public float swayAngle = 6f;
    public float swayDuration = 2.5f;

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
    private Tween swayTween;
    private Tween jumpTween;
    private Tween scaleTween;
    private Tween shakeTween;
    private Sequence jumpSeq;


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
    // STATE CONTROL (IMPORTANT)
    // ----------------------------------------------------

    /// <summary>
    /// Re-applies hover / sway / jump behavior based on toggles.
    /// Call this whenever UI mode changes.
    /// </summary>
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

        if (enableSway && swayTween == null)
            StartIdleSway();
    }

    /// <summary>
    /// Stops all tweens and hard-resets transforms.
    /// </summary>
    public void StopAllBehaviors()
    {
        hoverTween?.Kill();
        swayTween?.Kill();
        jumpTween?.Kill();

        transform.localPosition = originalPos;
        transform.localRotation = originalRot;

        if (jumpTargetTransform != null)
            jumpTargetTransform.localPosition = jumpBaseLocalPos;

        hoverTween = null;
        swayTween = null;
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
        transform.localRotation = Quaternion.Euler(0, 0, -swayAngle);

        swayTween = transform.DOLocalRotate(
            new Vector3(0, 0, swayAngle),
            swayDuration
        )
        .SetEase(Ease.InOutSine)
        .SetLoops(-1, LoopType.Yoyo);
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
            // Safety snap-back
            jumpTargetTransform.localPosition = jumpBaseLocalPos;
        });
    }

}
