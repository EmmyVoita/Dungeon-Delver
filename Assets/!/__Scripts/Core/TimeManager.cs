using UnityEngine;
using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Collections;



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

    public void AddTemporaryModifier(TimeScaleModifier mod, float duration)
    {
        StartCoroutine(ApplyForDuration(mod,duration));
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

    private IEnumerator ApplyForDuration(TimeScaleModifier mod, float duration)
    {
        AddModifier(mod);

        yield return new WaitForSecondsRealtime(duration);

        RemoveModifier(mod.Id);
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
}
