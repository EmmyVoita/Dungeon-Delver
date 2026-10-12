using UnityEngine;

[CreateAssetMenu(menuName = "Run Modifiers/Unlocks/Crit Count Requirement")]
public class CritCountUnlockRequirement : RunModifierUnlockRequirement
{
    [SerializeField] private int critCountRequirement;

    public override string GetRequirementRatioText()
    {
        return $"{RunHistoryManager.Instance.TotalCrits.ToString("N0")}/{critCountRequirement.ToString("N0")}";
    }

    public override string GetUnlockDescriptionText()
    {
        return $"Land Crits";
    }

    public override bool IsUnlocked()
    {
        return RunHistoryManager.Instance.TotalCrits >= critCountRequirement;
    }
}