using UnityEngine;

[CreateAssetMenu(menuName = "Run Modifiers/Unlocks/Total Arrows Requirement")]
public class TotalArrowsUnlockRequirement : RunModifierUnlockRequirement
{
    [SerializeField] private int arrowCountRequirement;

    public override string GetRequirementRatioText()
    {
        return $"{RunHistoryManager.Instance.TotalArrowsCaught.ToString("N0")}/{arrowCountRequirement.ToString("N0")}";
    }

    public override string GetUnlockDescriptionText()
    {
        return $"Catch Arrows";
    }

    public override bool IsUnlocked()
    {
        return RunHistoryManager.Instance.TotalArrowsCaught >= arrowCountRequirement;
    }
}