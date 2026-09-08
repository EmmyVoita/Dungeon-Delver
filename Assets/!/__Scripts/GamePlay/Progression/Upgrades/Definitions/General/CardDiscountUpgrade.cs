using UnityEngine;

[CreateAssetMenu( menuName = "Upgrades/Card Discount")]
public class CardDiscountUpgrade : UpgradeBase
{
    [Header("Upgrade Settings")]
    [SerializeField] private UpgradeCategory targetCategory;
    [SerializeField, Range(0f, 1f)] private float costMultiplier = 0.5f;

    public override void Apply()
    {
        if (UpgradeCostManager.Instance == null)
        {
            Debug.LogError("UpgradeCostManager.Instance == null when trying to add card discount upgrade");
            return;
        }
            

        UpgradeCostManager.Instance.RegisterPermanent(new UpgradeCategoryCostModifier(targetCategory,costMultiplier),this);
    }

    public override string GetFormattedDescription()
    {
        return descriptionTemplate
        .Replace("{COST_MULTIPLIER}", $"<color=#{UIColors.ToHex(UIColors.Green)}>{costMultiplier.ToString("P0")}</color>")
        .Replace("{CATEGORY}", $"<color=#{UIColors.ToHex(UIColors.Purple)}>{targetCategory} Cards</color>");
    }
}