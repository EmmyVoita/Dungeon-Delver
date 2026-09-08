using UnityEngine;

[CreateAssetMenu(menuName = "Run Modifiers/Unlocks/High Combo Requirement")]
public class HighComboUnlockRequirement : RunModifierUnlockRequirement
{
    [SerializeField] private int comboRequirement;

    public override string GetRequirementRatioText()
    {
        return $"{ScoreManager.Instance.HighestCombo().ToString("N0")}/{comboRequirement.ToString("N0")}";
    }

    public override string GetUnlockDescriptionText()
    {
        return $"Reach Combo";
    }

    public override bool IsUnlocked()
    {
        return ScoreManager.Instance.HighestCombo() >= comboRequirement;
    }
}