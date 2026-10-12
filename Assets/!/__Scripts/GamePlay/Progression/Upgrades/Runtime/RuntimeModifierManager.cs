using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class RuntimeModifierManager<TModifier> : MonoBehaviour
    where TModifier : class, IRuntimeModifier
{
    protected readonly List<RuntimeModifierEntry<TModifier>> activeModifiers = new();
    protected readonly List<RuntimeModifierEntry<TModifier>> renewingModifiers = new();
    protected readonly List<RuntimeModifierEntry<TModifier>> permanentModifiers = new();

    /// <summary>
    /// Fired when a one-level modifier expires and its inventory card
    /// should be removed.
    /// </summary>
    public static event Action<UpgradeBase> OnUpgradeExpired;

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
        {
            RestoreRenewingModifiers();
        }

        if (newState == GameStateManager.LevelEndState)
        {
            RemoveTemporaryModifiers();
        }

        OnGameStateChanged(previousState, newState);

        if(newState == GameState.DeathSequence)
        {
            ClearAllModifiers();
        }
    }

    protected virtual void OnGameStateChanged(
        GameState previousState,
        GameState newState)
    {
    }

    /// <summary>
    /// Adds a temporary modifier that is removed at level end.
    /// Its inventory card is also removed when it expires.
    /// </summary>
    public virtual void Register(
        TModifier modifier,
        UpgradeBase sourceUpgrade)
    {
        if (modifier == null)
            return;

        Initialize();

        RuntimeModifierEntry<TModifier> entry =
            new RuntimeModifierEntry<TModifier>(
                modifier,
                sourceUpgrade,
                removeCardWhenExpired: true
            );

        AddActiveModifier(entry);
    }

    /// <summary>
    /// Adds a modifier template that creates a fresh active modifier
    /// at the beginning of each level.
    /// Its inventory card remains between levels.
    /// </summary>
    public virtual void RegisterRenewing(
        TModifier modifier,
        UpgradeBase sourceUpgrade)
    {
        if (modifier == null)
            return;

        Initialize();

        RuntimeModifierEntry<TModifier> entry =
            new RuntimeModifierEntry<TModifier>(
                modifier,
                sourceUpgrade,
                removeCardWhenExpired: false
            );

        renewingModifiers.Add(entry);
    }

    /// <summary>
    /// Adds a modifier immediately and keeps it active between levels.
    /// Its inventory card remains.
    /// </summary>
    public virtual void RegisterPermanent(
        TModifier modifier,
        UpgradeBase sourceUpgrade = null)
    {
        if (modifier == null)
            return;

        Initialize();

        RuntimeModifierEntry<TModifier> entry =
            new RuntimeModifierEntry<TModifier>(
                modifier,
                sourceUpgrade,
                removeCardWhenExpired: false
            );

        permanentModifiers.Add(entry);
        AddActiveModifier(entry);
    }

    protected virtual void RestoreRenewingModifiers()
    {
        foreach (RuntimeModifierEntry<TModifier> template in renewingModifiers)
        {
            TModifier clone = template.Modifier.Clone() as TModifier;

            if (clone == null)
                continue;

            RuntimeModifierEntry<TModifier> activeEntry =
                new RuntimeModifierEntry<TModifier>(
                    clone,
                    template.SourceUpgrade,
                    removeCardWhenExpired: false
                );

            AddActiveModifier(activeEntry);
        }
    }

    protected virtual void RemoveTemporaryModifiers()
    {
        for (int i = activeModifiers.Count - 1; i >= 0; i--)
        {
            RuntimeModifierEntry<TModifier> entry = activeModifiers[i];

            // Permanent modifiers stay active and keep their runtime state.
            if (permanentModifiers.Contains(entry))
                continue;

            entry.Modifier.Destroy();
            activeModifiers.RemoveAt(i);

            // Renewing modifier instances end each level, but their
            // inventory cards remain because they return next level.
            if (entry.RemoveCardWhenExpired &&
                entry.SourceUpgrade != null)
            {
                ShopManager.Instance.RemoveUpgrade(entry.SourceUpgrade);
                //OnUpgradeExpired?.Invoke(entry.SourceUpgrade);
            }
        }

        SortActiveModifiers();
    }

    protected void SortActiveModifiers()
    {
        activeModifiers.Sort(
            (a, b) =>
                b.Modifier.Priority.CompareTo(a.Modifier.Priority)
        );
    }

    protected virtual void AddActiveModifier(
    RuntimeModifierEntry<TModifier> entry)
    {
        if (entry == null || entry.Modifier == null)
            return;

        activeModifiers.Add(entry);

        OnModifierActivated(entry.Modifier);

        entry.Modifier.OnActivate();

        SortActiveModifiers();
    }

    protected virtual void OnModifierActivated(TModifier modifier)
    {
    }

    protected void ExpireModifier(RuntimeModifierEntry<TModifier> entry)
    {
        if (entry == null)
            return;

        entry.Modifier.Destroy();
        activeModifiers.Remove(entry);

        if (entry.RemoveCardWhenExpired &&
            entry.SourceUpgrade != null)
        {
            ShopManager.Instance.RemoveUpgrade(entry.SourceUpgrade);
        }
    }

    protected virtual void OnDestroy()
    {
        Unsubscribe();

        DestroyAllModifiers();
    }

    protected void DestroyAllModifiers()
    {
        foreach (var entry in activeModifiers)
        {
            entry.Modifier?.Destroy();
        }

        activeModifiers.Clear();

        // If these contain SEPARATE modifier instances,
        // destroy those too.
        permanentModifiers.Clear();
        renewingModifiers.Clear();
        activeModifiers.Clear();
    }


    public virtual void ClearAllModifiers()
    {
        foreach (RuntimeModifierEntry<TModifier> entry in activeModifiers)
        {
            entry.Modifier.Destroy();
        }

        activeModifiers.Clear();
        renewingModifiers.Clear();
        permanentModifiers.Clear();
    }
}