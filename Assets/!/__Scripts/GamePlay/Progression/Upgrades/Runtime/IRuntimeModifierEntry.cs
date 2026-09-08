using System;

public class RuntimeModifierEntry<TModifier>
    where TModifier : class, IRuntimeModifier
{
    public TModifier Modifier { get; }
    public UpgradeBase SourceUpgrade { get; }

    // True for a one-level purchase whose card should disappear.
    // False for renewing and permanent upgrades.
    public bool RemoveCardWhenExpired { get; }

    public RuntimeModifierEntry(
        TModifier modifier,
        UpgradeBase sourceUpgrade,
        bool removeCardWhenExpired)
    {
        Modifier = modifier;
        SourceUpgrade = sourceUpgrade;
        RemoveCardWhenExpired = removeCardWhenExpired;
    }
}