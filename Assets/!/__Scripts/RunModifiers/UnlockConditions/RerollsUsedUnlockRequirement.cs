using UnityEngine;

[CreateAssetMenu(menuName = "Run Modifiers/Unlocks/Rerolls Used Requirement")]
public class RerollsUsedUnlockRequirement : RunModifierUnlockRequirement
{
    [SerializeField] private int rerollRequirement;

    public override string GetRequirementRatioText()
    {
        return $"{ScoreManager.Instance.TotalRerollsUsed().ToString("N0")}/{rerollRequirement.ToString("N0")}";
    }

    public override string GetUnlockDescriptionText()
    {
        return $"Use Rerolls";
    }

    public override bool IsUnlocked()
    {
        return ScoreManager.Instance.TotalRerollsUsed() >= rerollRequirement;
    }
}