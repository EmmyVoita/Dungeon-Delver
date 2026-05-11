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

    [Header("Randomization")]
    public float randomStartDelayMax = 0.5f;
    public float durationVariance = 0.3f;
    public float amplitudeVariance = 2f;

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

        if (enableSway && swayTween == null)
            StartIdleSway();
    }

    public void StopAllBehaviors()
    {
        hoverTween?.Kill();
        swayTween?.Kill();
        jumpTween?.Kill();
        scaleTween?.Kill();
        shakeTween?.Kill();

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
        float randomDelay = UnityEngine.Random.Range(0f, randomStartDelayMax);

        float duration = hoverDuration + UnityEngine.Random.Range(-durationVariance, durationVariance);
        float amplitude = hoverAmplitude + UnityEngine.Random.Range(-amplitudeVariance, amplitudeVariance);

        hoverTween = transform.DOLocalMoveY(
            originalPos.y + amplitude,
            duration
        )
        .SetEase(Ease.InOutSine)
        .SetLoops(-1, LoopType.Yoyo)
        .SetDelay(randomDelay);
    }

    // ----------------------------------------------------
    // IDLE SWAY
    // ----------------------------------------------------

    private void StartIdleSway()
    {
        swayTween?.Kill();

        float randomDelay = UnityEngine.Random.Range(0f, randomStartDelayMax);

        Vector3 baseRot = originalRot.eulerAngles;

        Vector3 posRot = baseRot;
        Vector3 negRot = baseRot;

        // Randomize angle slightly
        float angleX = swayAngleX + UnityEngine.Random.Range(-2f, 2f);
        float angleY = swayAngleY + UnityEngine.Random.Range(-2f, 2f);
        float angleZ = swayAngleZ + UnityEngine.Random.Range(-2f, 2f);

        if (swayX)
        {
            posRot.x += angleX;
            negRot.x -= angleX;
        }

        if (swayY)
        {
            posRot.y += angleY;
            negRot.y -= angleY;
        }

        if (swayZ)
        {
            posRot.z += angleZ;
            negRot.z -= angleZ;
        }

        float duration = Mathf.Max(
            swayDurationX,
            Mathf.Max(swayDurationY, swayDurationZ)
        );

        duration += UnityEngine.Random.Range(-durationVariance, durationVariance);

        // Random starting side (feels better than always starting negative)
        bool startPositive = UnityEngine.Random.value > 0.5f;
        transform.localRotation = Quaternion.Euler(startPositive ? posRot : negRot);

        // Direct tween (no Sequence needed)
        swayTween = transform
            .DOLocalRotate(startPositive ? negRot : posRot, duration)
            .SetEase(Ease.InOutSine)
            .SetDelay(randomDelay)
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

    private void OnDestroy()
    {
        hoverTween?.Kill();
        swayTween?.Kill();
        jumpTween?.Kill();
        scaleTween?.Kill();
        shakeTween?.Kill();
    }
}