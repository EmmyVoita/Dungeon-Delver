using UnityEngine;

[CreateAssetMenu(menuName = "Run Modifiers/Unlocks/Shops Requirement")]
public class ShopsVisitedUnlockRequirement : RunModifierUnlockRequirement
{
    [SerializeField] private int shopsRequirement;

    public override string GetRequirementRatioText()
    {
        return $"{RunHistoryManager.Instance.TotalShopsVisited.ToString("N0")}/{shopsRequirement.ToString("N0")}";
    }

    public override string GetUnlockDescriptionText()
    {
        return $"Visit Shops";
    }

    public override bool IsUnlocked()
    {
        return RunHistoryManager.Instance.TotalShopsVisited >= shopsRequirement;
    }
}