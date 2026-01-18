using UnityEngine;


[CreateAssetMenu(menuName = "Upgrades/Score For Crit Window")]
public class ScoreForCritWindowCost : UpgradeBase, ICritWindowModifier, ICritHitValueModifier, INormalHitValueModifier
{
    public float critWindowMultiplier = 0.8f;
    public float CritHitValueMultiplier = 1.5f;
    public float NormalHitValueMultiplier = 0.5f;

    public override string GetDescription()
    {
        return descriptionTemplate
            .Replace("{CRIT_WINDOW_MULTIPLIER}", (critWindowMultiplier - 1).ToString("P0"))
            .Replace("{CRIT_HIT_VALUE_MULTIPLIER}", (CritHitValueMultiplier - 1).ToString("P0"))
            .Replace("{NORMAL_HIT_VALUE_MULTIPLIER}", (NormalHitValueMultiplier - 1).ToString("P0"));
    }

    public float ModifyCritWindow(float current)
        => current * critWindowMultiplier;

    public float ModifyCritHitValue(float currentValue)
        => currentValue * CritHitValueMultiplier;

    public float ModifyNormalHitValue(float currentValue)
        => currentValue * NormalHitValueMultiplier;
}