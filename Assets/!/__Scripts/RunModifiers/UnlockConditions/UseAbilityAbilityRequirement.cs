using UnityEngine;

[CreateAssetMenu(menuName = "Run Modifiers/Unlocks/Ability Use Requirement")]
public class UseAbilityUnlockRequirement : RunModifierUnlockRequirement
{
    [SerializeField] private int useRequirement;

    public override string GetRequirementRatioText()
    {
        return $"{ScoreManager.Instance.TotalAbilityUsed().ToString("N0")}/{useRequirement.ToString("N0")}";
    }

    public override string GetUnlockDescriptionText()
    {
        return $"Use Ability";
    }

    public override bool IsUnlocked()
    {
        return ScoreManager.Instance.TotalAbilityUsed() >= useRequirement;
    }
}