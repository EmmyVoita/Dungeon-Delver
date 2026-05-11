using UnityEngine;
using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;



public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance { get; private set; }

    public static event Action<float> OnTimeScaleChanged;
    

    private bool _paused = false;
    private List<TimeScaleModifier> modifiers = new List<TimeScaleModifier>();
    public float GetCurrentScale() => CalculateFinalScale();

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

    void OnEnable()
    {
        SceneManager.sceneLoaded += HandleSceneLoaded;
        
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Resume();
        modifiers.Clear();              
        ApplyCombinedScale();  
    }

    // ------------------------------
    // Core Combiner
    // ------------------------------
    private void ApplyCombinedScale()
    {
        float scale = CalculateFinalScale();
        Time.timeScale = scale;
        OnTimeScaleChanged?.Invoke(scale);
    }

    private float CalculateFinalScale()
    {
        if(_paused) return 0;

        if (modifiers.Count == 0)
            return 1f;

        float result = 1f;

        foreach (var mod in modifiers)
        {
            if (!mod.IsActive) continue;
            result *= mod.Value;
        }

        return Mathf.Max(result, 0f);
    }

    public void AddModifier(TimeScaleModifier mod)
    {
        mod.OnChanged += ApplyCombinedScale; // 🔥 THIS is the missing link
        modifiers.Add(mod);
        ApplyCombinedScale();
    }

    public void RemoveModifier(string id)
    {
        foreach (var mod in modifiers)
        {
            if (mod.Id == id)
            {
                mod.OnChanged -= ApplyCombinedScale; // cleanup
            }
        }

        modifiers.RemoveAll(m => m.Id == id);
        ApplyCombinedScale();
    }

    public TimeScaleModifier AddTweenedModifier(
        string id,
        float startValue,
        float targetValue,
        float duration,
        Ease ease
    )
    {
        var mod = new TimeScaleModifier(id, startValue);
        AddModifier(mod);

        DOTween.To(
            () => mod.Value,
            x => mod.SetValue(x),
            targetValue,
            duration
        )
        .SetEase(ease)
        .SetUpdate(true);

        return mod;
    }

    public void Pause()
    {
        _paused = true;
        ApplyCombinedScale();
    }

    public void Resume()
    {
        _paused = false;
        ApplyCombinedScale();
    }



  

    // ------------------------------
    // Controls
    // ------------------------------

    /*
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
    */

    /*
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
    */
    /*
    public void PlayImpulseSlow(TimeSlowImpulseData data)
    {
        impulseTween?.Kill();

        impulseTween = DOTween.Sequence()
            .SetUpdate(true)

            .Append(DOTween.To(() => impulse, x => impulse = x, data.slowMultiplier, data.inDuration)
                .SetEase(Ease.OutSine)
                .OnUpdate(ApplyCombinedScale))

            .AppendInterval(data.holdDuration)

            .Append(DOTween.To(() => impulse, x => impulse = x, 1f, data.outDuration)
                .SetEase(Ease.InSine)
                .OnUpdate(ApplyCombinedScale));
    }
    */
}
