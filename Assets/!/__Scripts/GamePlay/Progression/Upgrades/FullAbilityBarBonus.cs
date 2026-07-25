using UnityEngine;


[CreateAssetMenu(menuName = "Upgrades/Full Ability Bar Bonus")]
public class FullAbilityBarBonus : UpgradeBase, IActivatableUpgrade, IArrowScoreModifier
{
    public float arrowValueMultiplier = 1.1f;

    public override string GetDescription()
    {
        return descriptionTemplate
            .Replace("{ARROW_VALUE_MULTIPLIER}", (arrowValueMultiplier - 1).ToString("P0"));
    }

    public void Activate()
    {
        Player.OnAbilityChargeChanged += HandleChargeChanged;
    }

    public void Deactivate()
    {
        Player.OnAbilityChargeChanged -= HandleChargeChanged;
        //UpgradeManager.Instance.SetUpgradeActive(upgradeId, false);
    }

    private void HandleChargeChanged(int previousCharge, int attemptedDelta, int appliedDelta)
    {
        bool active = Player.Instance.FullAbilityCharge;
        //UpgradeManager.Instance.SetUpgradeActive(upgradeId, active);
    }

    public float ModifyArrowScore(float currentValue)
    {
        if (Player.Instance.FullAbilityCharge)
            return currentValue * arrowValueMultiplier;

        return currentValue;
    }
}
