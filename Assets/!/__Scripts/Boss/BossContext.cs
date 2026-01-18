using System;
using System.Collections.Generic;

public static class BossContext
{
    public static bool IsBossActive { get; private set; }

    public static BossEffectType ActiveEffects { get; private set; } = BossEffectType.None;

    // Fired when a specific effect is enabled / disabled
    public static event Action<BossEffectType> OnEffectEnabled;
    public static event Action<BossEffectType> OnEffectDisabled;

    // Fired when *any* change happens (useful for UI/debug)
    public static event Action<BossEffectType> OnEffectsChanged;

    // Reference counting so overlapping timelines don’t fight
    private static readonly Dictionary<BossEffectType, int> effectCounts = new();

    // --------------------------------------------------
    // Effect Control
    // --------------------------------------------------

    public static void EnableEffect(BossEffectType effect)
    {
        if (!IsBossActive)
            return;

        if (!effectCounts.ContainsKey(effect))
            effectCounts[effect] = 0;

        effectCounts[effect]++;

        // Only trigger enable once
        if ((ActiveEffects & effect) == 0)
        {
            ActiveEffects |= effect;
            OnEffectEnabled?.Invoke(effect);
            OnEffectsChanged?.Invoke(ActiveEffects);
        }
    }

    public static void DisableEffect(BossEffectType effect)
    {
        if (!IsBossActive)
            return;

        if (!effectCounts.ContainsKey(effect))
            return;

        effectCounts[effect]--;

        if (effectCounts[effect] <= 0)
        {
            effectCounts.Remove(effect);

            if ((ActiveEffects & effect) != 0)
            {
                ActiveEffects &= ~effect;
                OnEffectDisabled?.Invoke(effect);
                OnEffectsChanged?.Invoke(ActiveEffects);
            }
        }
    }

    public static bool HasEffect(BossEffectType effect)
    {
        return (ActiveEffects & effect) != 0;
    }

    // --------------------------------------------------
    // Boss Lifecycle
    // --------------------------------------------------

    public static void StartBoss()
    {
        IsBossActive = true;
        ClearAllEffects();
    }

    public static void EndBoss()
    {
        ClearAllEffects();
        IsBossActive = false;
    }

    // --------------------------------------------------
    // Internal Helpers
    // --------------------------------------------------

    private static void ClearAllEffects()
    {
        foreach (BossEffectType effect in Enum.GetValues(typeof(BossEffectType)))
        {
            if (effect == BossEffectType.None)
                continue;

            if ((ActiveEffects & effect) != 0)
            {
                OnEffectDisabled?.Invoke(effect);
            }
        }

        effectCounts.Clear();
        ActiveEffects = BossEffectType.None;
        OnEffectsChanged?.Invoke(ActiveEffects);
    }

}
