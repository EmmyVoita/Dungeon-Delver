using UnityEngine;
using System.Collections.Generic;

public class ArrowEffectManager : MonoBehaviour
{
    public static ArrowEffectManager Instance { get; private set; }

    private readonly List<IArrowEffect> activeEffects = new();

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnEnable()
    {
        GameStateManager.OnStateChanged += HandleGameStateChanged;
    }

    private void OnDisable()
    {
        GameStateManager.OnStateChanged -= HandleGameStateChanged;
    }

    private void HandleGameStateChanged(GameState prev, GameState current)
    {
        if (current != GameState.RoundActive)
            activeEffects.Clear();
    }

    public T GetEffect<T>() where T : class, IArrowEffect
    {
        foreach (var effect in activeEffects)
            if (effect is T typed)
                return typed;

        return null;
    }

    public void AddOrExtend<T>(T effect) where T : IArrowEffect
    {
        activeEffects.Add(effect);
    }

    public void ApplyEffectsToArrow(ArrowBase arrow)
    {
        for (int i = activeEffects.Count - 1; i >= 0; i--)
        {
            var effect = activeEffects[i];
            effect.ApplyToArrow(arrow);

            if (effect.IsExpired)
                activeEffects.RemoveAt(i);
        }
    }
}
