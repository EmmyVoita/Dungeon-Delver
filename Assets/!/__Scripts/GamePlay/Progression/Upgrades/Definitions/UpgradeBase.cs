using System.Collections.Generic;
using UnityEngine;

public abstract class UpgradeBase : ScriptableObject
{
    private string LASTS_THIS_NEXT_LEVEL =>
        $"\n\n<color=#{UIColors.ToHex(UIColors.Orange)}>" +
        $"Lasts this next level only.</color>";

    [Header("Identity")]
    [SerializeField] private string upgradeId;

    [Header("Card Type")]
    [SerializeField] private UpgradeCategory category;

    [Header("Shop Offering")]
    [Tooltip("The player must own every upgrade in this list.")]
    [SerializeField] private List<UpgradeRequirement> requiredUpgrades = new();

    [Tooltip("A value that describes how likely the card is to appear in the shop.")]
    [Min(0)]
    [SerializeField] private float baseOfferWeight = 1f;

    [Header("Settings")]
    [SerializeField] private int _cost;
    [SerializeField] private int _maxStacks;
    [SerializeField] private UpgradeDuration duration = UpgradeDuration.Permanent;

    [Header("Card Decoration")]
    public Sprite cardDecoration;
    public Material cardDecorationMaterial;

    [Header("Card UI")]
    public Sprite centerIcon;
    public Material iconMaterial;

    [Header("Display UI")]
    public string displayName;

    [TextArea]
    public string descriptionTemplate;

    [TextArea]
    public string detailsTemplate;

    public string UpgradeID => upgradeId;
    public int MaxStacks => _maxStacks;
    public int Cost => _cost;
    public UpgradeCategory Category => category;
    public UpgradeDuration Duration => duration;
    public virtual string DisplayName => displayName;

    #if UNITY_EDITOR
        private void OnValidate()
        {
            if (string.IsNullOrEmpty(upgradeId))
            {
                upgradeId = System.Guid.NewGuid().ToString();
                UnityEditor.EditorUtility.SetDirty(this);
            }
        }
    #endif

    public virtual void Apply()
    {
    }

    public virtual bool CanAppear()
    {
        if (ShopManager.Instance == null)
            return false;

        int ownedCount =
            ShopManager.Instance.GetPurchasedCardCount(this);

        if (ownedCount >= MaxStacks)
            return false;

        return ShopManager.Instance.HasCards(requiredUpgrades);
    }

    public virtual float GetOfferWeight()
    {
        return CanAppear() ? baseOfferWeight : 0.0f;
    }

    public virtual string GetFormattedDescription()
    {
        return FormatText(descriptionTemplate);
    }

    public string GetDescription()
    {
        string description = GetFormattedDescription();

        if (duration == UpgradeDuration.ThisLevel)
            description += LASTS_THIS_NEXT_LEVEL;

        return description;
    }

    public virtual string GetDetails()
    {
        return FormatText(detailsTemplate);
    }

    public Sprite GetCategoryIcon(
        UpgradeCategoryDatabase database)
    {
        if (database == null)
            return null;

        return database.GetIcon(Category);
    }

    protected virtual void AddReplacements(Dictionary<string, string> replacements)
    {
        replacements["{MAX_STACKS}"] =
            $"<color=#{UIColors.ToHex(UIColors.Yellow)}>" +
            $"{MaxStacks:N0}</color>";

        replacements["{COST}"] =
            $"<color=#{UIColors.ToHex(UIColors.Yellow)}>" +
            $"${Cost:N0}</color>";

        replacements["{CATEGORY}"] =
            $"<color=#{UIColors.ToHex(UIColors.Purple)}>" +
            Category.ToString();

        replacements["{GOLDEN_ARROWS}"] = 
            $"<color=#{UIColors.ToHex(UIColors.Purple)}>Golden Arrows</color>";

        replacements["{EMPOWERED_ARROW}"] = 
            $"<color=#{UIColors.ToHex(UIColors.Purple)}>Empowered Arrows</color>";
    }

    protected string FormatText(string text)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;

        Dictionary<string, string> replacements = new();

        AddReplacements(replacements);

        foreach (KeyValuePair<string, string> replacement in replacements)
        {
            text = text.Replace(
                replacement.Key,
                replacement.Value
            );
        }

        return text;
    }

    public string GetUnavailableDescription()
    {
        if(!ShopManager.Instance)
            return "UnavailableDescription() shop instance null";

        int ownedCount = ShopManager.Instance.GetPurchasedCardCount(this);

        string description = ownedCount >= MaxStacks ? $"At max stacks [<color=#{UIColors.ToHex(UIColors.Orange)}>{MaxStacks}</color>]" : 
                                                        "This card requires the following cards:";

        foreach (UpgradeRequirement requirement in requiredUpgrades)
        {
            description += $"\n <color=#{UIColors.ToHex(UIColors.Orange)}>{requirement.upgrade.displayName}</color>";
        }

        return description;
    }

    public void OnSelected()
    {
        Debug.Log($"Upgrade Option Affect Aplied => {displayName}");
        Apply();
    }
}