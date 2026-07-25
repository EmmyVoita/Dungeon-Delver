using System;
using UnityEngine;


[CreateAssetMenu(menuName = "Upgrades/Ability Cooldown For Crit Bonus")]
public class AbilityCooldownForCritBonus 
    : UpgradeBase, IActivatableUpgrade, ICritBaseOverride
{
    public static event Action OnShouldReduceUpgradeCooldown;

    public void Activate()
    {
        ArrowBase.OnArrowResolved += HandleArrowResult;
    }

    public void Deactivate()
    {
        ArrowBase.OnArrowResolved -= HandleArrowResult;
    }

    private void HandleArrowResult(ArrowResolvedData data)
    {
        if (data.goalType == Goal.GoalType.Critical)
        {
            // ONE-SHOT SIGNAL ONLY
            OnShouldReduceUpgradeCooldown?.Invoke();
        }
    }

    // Crits are treated as base = 1 instead of 2
    public float ModifyCritBase(float currentValue)
    {
        return 1f;
    }
}
