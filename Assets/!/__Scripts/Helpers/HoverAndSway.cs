using UnityEngine;
using DG.Tweening;
using System;

public class HoverAndSway : MonoBehaviour
{
    [Header("Behavior Toggles")]
    [SerializeField] private  bool enableHover = true;
    [SerializeField] private  bool enableSway = true;


    [Header("Idle Hover")]
    [SerializeField] private float hoverAmplitude = 10f;
    [SerializeField] private float hoverDuration = 2f;


    [Header("Idle Sway Axis")]
    [SerializeField] private  bool swayX = false;
    [SerializeField] private  bool swayY = false;
    [SerializeField] private  bool swayZ = true;


    [Header("Idle Sway Angles")]
    [SerializeField] private  float swayAngleX = 6f;
    [SerializeField] private  float swayAngleY = 6f;
    [SerializeField] private  float swayAngleZ = 6f;


    [Header("Idle Sway Durations")]
    [SerializeField] private  float swayDurationX = 2.5f;
    [SerializeField] private  float swayDurationY = 2.5f;
    [SerializeField] private  float swayDurationZ = 2.5f;


    [Header("Randomization")]
    [SerializeField] private  float randomStartDelayMax = 0.0f;
    [SerializeField] private  float durationVariance = 0.0f;
    [SerializeField] private  float amplitudeVariance = 0.0f;


    // Baselines
    private Vector3 _originalPos;
    private Quaternion _originalRot;

    // Tweens
    private Tween _hoverTween;
    private Tween _swayTween;
    private Tween _shakeTween;

    // ----------------------------------------------------
    // LIFECYCLE
    // ----------------------------------------------------

    private void Awake()
    {
        _originalPos = transform.localPosition;
        _originalRot = transform.localRotation;

        /*
        if (jumpTargetTransform != null)
            jumpBaseLocalPos = jumpTargetTransform.localPosition;
        */
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
        if (enableHover && _hoverTween == null)
            StartIdleHover();

        if (enableSway && _swayTween == null)
            StartIdleSway();
    }

    public void StopAllBehaviors()
    {
        _hoverTween?.Kill();
        _swayTween?.Kill();
        _shakeTween?.Kill();

        transform.localPosition = _originalPos;
        transform.localRotation = _originalRot;


        _hoverTween = null;
        _swayTween = null;
    }

    // ----------------------------------------------------
    // IDLE HOVER
    // ----------------------------------------------------

    private void StartIdleHover()
    {
        float randomDelay = UnityEngine.Random.Range(0f, randomStartDelayMax);

        float duration = hoverDuration + UnityEngine.Random.Range(-durationVariance, durationVariance);
        float amplitude = hoverAmplitude + UnityEngine.Random.Range(-amplitudeVariance, amplitudeVariance);

        _hoverTween = transform.DOLocalMoveY(
            _originalPos.y + amplitude,
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
        _swayTween?.Kill();

        float randomDelay = UnityEngine.Random.Range(0f, randomStartDelayMax);

        Vector3 baseRot = _originalRot.eulerAngles;

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
        _swayTween = transform
            .DOLocalRotate(startPositive ? negRot : posRot, duration)
            .SetEase(Ease.InOutSine)
            .SetDelay(randomDelay)
            .SetLoops(-1, LoopType.Yoyo);
    }


    // ----------------------------------------------------
    // SHAKE
    // ----------------------------------------------------
    
    /*
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
    */

    private void OnDestroy()
    {
        _hoverTween?.Kill();
        _swayTween?.Kill();
        _shakeTween?.Kill();
    }
}