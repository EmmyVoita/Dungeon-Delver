using UnityEngine;

[CreateAssetMenu(menuName = "Run Modifiers/Unlocks/High Combo Requirement")]
public class HighComboUnlockRequirement : RunModifierUnlockRequirement
{
    [SerializeField] private int comboRequirement;

    public override string GetRequirementRatioText()
    {
        return $"{RunHistoryManager.Instance.HighestCombo.ToString("N0")}/{comboRequirement.ToString("N0")}";
    }

    public override string GetUnlockDescriptionText()
    {
        return $"Reach Combo";
    }

    public override bool IsUnlocked()
    {
        return RunHistoryManager.Instance.HighestCombo >= comboRequirement;
    }
}