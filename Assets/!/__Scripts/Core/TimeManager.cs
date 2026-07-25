using UnityEngine;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance { get; private set; }

    public static event Action<float> OnTimeScaleChanged;

    private bool _paused;

    private readonly List<TimeScaleModifier> modifiers = new();
    private readonly HashSet<string> levelModifierIds = new();

    public float GetCurrentScale()
    {
        return CalculateFinalScale();
    }

    private void Awake()
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

    private void OnEnable()
    {
        SceneManager.sceneLoaded += HandleSceneLoaded;
        GameStateManager.OnStateChanged += HandleStateChanged;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
        GameStateManager.OnStateChanged -= HandleStateChanged;
    }

    private void HandleSceneLoaded(Scene scene,LoadSceneMode mode)
    {
        _paused = false;
        ClearAllModifiers();
    }

    private void HandleStateChanged(GameState previousState,GameState newState)
    {
        if (newState == GameStateManager.LevelEndState)
            ClearLevelModifiers();
    }

    private void ApplyCombinedScale()
    {
        float scale = CalculateFinalScale();

        Time.timeScale = scale;
        OnTimeScaleChanged?.Invoke(scale);
    }

    private float CalculateFinalScale()
    {
        if (_paused)
            return 0f;

        float result = 1f;

        foreach (TimeScaleModifier mod in modifiers)
        {
            if (mod == null || !mod.IsActive)
                continue;

            result *= mod.Value;
        }

        return Mathf.Max(result, 0f);
    }

    public void AddModifier(TimeScaleModifier mod)
    {
        if (mod == null)
            return;

        mod.OnChanged += ApplyCombinedScale;
        modifiers.Add(mod);

        ApplyCombinedScale();
    }

    public void AddLevelModifier(TimeScaleModifier mod)
    {
        if (mod == null)
            return;

        AddModifier(mod);
        levelModifierIds.Add(mod.Id);
    }

    public void AddTemporaryModifier(TimeScaleModifier mod,float duration)
    {
        if (mod == null)
            return;

        StartCoroutine(
            ApplyForDuration(mod, duration)
        );
    }

    public void RemoveModifier(string id)
    {
        if (string.IsNullOrEmpty(id))
            return;

        for (int i = modifiers.Count - 1; i >= 0; i--)
        {
            TimeScaleModifier mod = modifiers[i];

            if (mod == null || mod.Id != id)
                continue;

            mod.OnChanged -= ApplyCombinedScale;
            modifiers.RemoveAt(i);
        }

        levelModifierIds.Remove(id);

        ApplyCombinedScale();
    }

    private void ClearLevelModifiers()
    {
        if (levelModifierIds.Count == 0)
            return;

        string[] idsToRemove =
            new string[levelModifierIds.Count];

        levelModifierIds.CopyTo(idsToRemove);

        foreach (string id in idsToRemove)
            RemoveModifier(id);
    }

    private void ClearAllModifiers()
    {
        foreach (TimeScaleModifier mod in modifiers)
        {
            if (mod != null)
                mod.OnChanged -= ApplyCombinedScale;
        }

        modifiers.Clear();
        levelModifierIds.Clear();

        ApplyCombinedScale();
    }

    public TimeScaleModifier AddTweenedModifier(
        string id,
        float startValue,
        float targetValue,
        float duration,
        Ease ease)
    {
        var mod = new TimeScaleModifier(
            id,
            startValue
        );

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

    private IEnumerator ApplyForDuration(TimeScaleModifier mod,float duration)
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