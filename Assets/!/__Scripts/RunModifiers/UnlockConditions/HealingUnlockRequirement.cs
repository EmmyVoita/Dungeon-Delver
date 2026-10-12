using UnityEngine;

[CreateAssetMenu(menuName = "Run Modifiers/Unlocks/Healing Requirement")]
public class HealingUnlockRequirement : RunModifierUnlockRequirement
{
    [SerializeField] private int healingRequirement;

    public override string GetRequirementRatioText()
    {
        return $"{RunHistoryManager.Instance.TotalHealing.ToString("N0")}/{healingRequirement.ToString("N0")}";
    }

    public override string GetUnlockDescriptionText()
    {
        return $"Heal";
    }

    public override bool IsUnlocked()
    {
        return RunHistoryManager.Instance.TotalHealing >= healingRequirement;
    }
}