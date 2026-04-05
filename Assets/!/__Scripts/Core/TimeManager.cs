using UnityEngine;
using DG.Tweening;
using System;

public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance { get; private set; }

    public static event Action<float> OnTimeScaleChanged;

    [SerializeField] private float baseScale = 1f;
    [SerializeField] private float modifier = 1f;
    [SerializeField] private float impulse = 1f;

    private Tween impulseTween;
    private Tween baseTween;
    private Tween modifierTween;

    private float previousModifier = 1f;
    private float previousBase = 1f;
    private bool paused = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        ApplyCombinedScale();
    }

    // ------------------------------
    // Core Combiner
    // ------------------------------
    private void ApplyCombinedScale()
    {
        float scale = Mathf.Clamp(baseScale * modifier * impulse, 0.0f, 2f);
        Time.timeScale = scale;
        OnTimeScaleChanged?.Invoke(scale);
    }

    public float GetCurrentScale() => Time.timeScale;

    // ------------------------------
    // Controls
    // ------------------------------

    public void SetBaseScale(float newBase, float duration = 0.2f)
    {
        baseTween?.Kill();
        baseTween = DOTween.To(() => baseScale, x => baseScale = x, newBase, duration)
            .SetEase(Ease.InOutSine)
            .SetUpdate(true)
            .OnUpdate(ApplyCombinedScale);
    }

    public void SetModifier(float newModifier, float duration = 0.2f)
    {
        modifierTween?.Kill();
        modifierTween = DOTween.To(() => modifier, x => modifier = x, newModifier, duration)
            .SetEase(Ease.InOutSine)
            .SetUpdate(true)
            .OnUpdate(ApplyCombinedScale);
    }

    public void ResetAll(float duration = 0.3f)
    {
        SetBaseScale(1f, duration);
        SetModifier(1f, duration);
    }

    public void Pause()
    {
        if(paused) return;
        previousModifier = modifier;
        previousBase = baseScale;
        SetBaseScale(0f, duration: 0f);
        SetModifier(0f, duration: 0f);
        paused = true;
    }

    public void Resume()
    {
        SetBaseScale(previousBase, duration: 0f);
        SetModifier(previousModifier, duration: 0f);
        paused = false;
    }

    public void PlayImpulseSlow(
        float slowMultiplier,
        float inDuration,
        float holdDuration,
        float outDuration
    )
    {
        impulseTween?.Kill();

        impulseTween = DOTween.Sequence()
            .SetUpdate(true)

            .Append(DOTween.To(() => impulse, x => impulse = x, slowMultiplier, inDuration)
                .SetEase(Ease.OutSine)
                .OnUpdate(ApplyCombinedScale))

            .AppendInterval(holdDuration)

            .Append(DOTween.To(() => impulse, x => impulse = x, 1f, outDuration)
                .SetEase(Ease.InSine)
                .OnUpdate(ApplyCombinedScale));
    }
}
