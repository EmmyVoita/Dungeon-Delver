using System.Collections.Generic;
using UnityEngine;

public abstract class RuntimeModifierManager<TModifier> : MonoBehaviour
    where TModifier : class, IRuntimeModifier
{
    protected readonly List<TModifier> activeModifiers = new();
    protected readonly List<TModifier> renewingModifiers = new();
    protected readonly List<TModifier> permanentModifiers = new();

    private bool _initialized;

    protected virtual void OnDisable()
    {
        Unsubscribe();
        _initialized = false;
    }

    protected void Initialize()
    {
        if (_initialized)
            return;

        _initialized = true;

        GameStateManager.OnStateChanged += HandleStateChanged;
        Subscribe();
    }

    protected virtual void Subscribe()
    {
    }

    protected virtual void Unsubscribe()
    {
        GameStateManager.OnStateChanged -= HandleStateChanged;
    }

    private void HandleStateChanged(
        GameState previousState,
        GameState newState)
    {
        if (newState == GameStateManager.LevelStartState)
            RestoreRenewingModifiers();

        if (newState == GameStateManager.LevelEndState)
            RemoveTemporaryModifiers();

        OnGameStateChanged(previousState, newState);
    }

    protected virtual void OnGameStateChanged(
        GameState previousState,
        GameState newState)
    {
    }

    /// <summary>
    /// Adds a temporary modifier that is removed at the end of the level.
    /// </summary>
    public virtual void Register(TModifier modifier)
    {
        if (modifier == null)
            return;

        Initialize();
        AddActiveModifier(modifier);
    }

    /// <summary>
    /// Adds a modifier template that is cloned into the active list
    /// at the beginning of every level.
    /// </summary>
    public virtual void RegisterRenewing(TModifier modifier)
    {
        if (modifier == null)
            return;

        Initialize();

        renewingModifiers.Add(modifier);
    }

    /// <summary>
    /// Adds a modifier immediately and keeps it active between levels.
    /// </summary>
    public virtual void RegisterPermanent(TModifier modifier)
    {
        if (modifier == null)
            return;

        Initialize();

        permanentModifiers.Add(modifier);

        AddActiveModifier(modifier);
    }

    protected virtual void RestoreRenewingModifiers()
    {
        foreach (TModifier modifier in renewingModifiers)
        {
            TModifier clone = modifier.Clone() as TModifier;

            if (clone != null)
                AddActiveModifier(modifier);
        }

        SortActiveModifiers();
    }

    protected virtual void RemoveTemporaryModifiers()
    {
        foreach(TModifier modifier in activeModifiers)
        {
            modifier.OnDestroy();
        }

        activeModifiers.Clear();

        // Permanent modifiers use the same runtime instance,
        // so their internal state is preserved between levels.
        activeModifiers.AddRange(permanentModifiers);

        SortActiveModifiers();
    }

    protected void SortActiveModifiers()
    {
        activeModifiers.Sort(
            (a, b) => b.Priority.CompareTo(a.Priority)
        );
    }

    // Helper Method for adding an active modifier
    protected virtual void AddActiveModifier(TModifier modifier)
    {
        if (modifier == null)
            return;

        activeModifiers.Add(modifier);

        OnModifierActivated(modifier);
        
        modifier.OnActivate();

        SortActiveModifiers();
    }

    protected virtual void OnModifierActivated(TModifier modifier)
    {
        
    }

    public virtual void ClearAllModifiers()
    {
        activeModifiers.Clear();
        renewingModifiers.Clear();
        permanentModifiers.Clear();
    }
}