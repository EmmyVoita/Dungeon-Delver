using UnityEngine;

[CreateAssetMenu(menuName = "Run Modifiers/Unlocks/Total Cards Requirement")]
public class TotalCardsUnlockRequirement : RunModifierUnlockRequirement
{
    [SerializeField] private int cardRequirement;

    public override string GetRequirementRatioText()
    {
        return $"{ScoreManager.Instance.TotalCardsGained().ToString("N0")}/{cardRequirement.ToString("N0")}";
    }

    public override string GetUnlockDescriptionText()
    {
        return $"Obtain Cards"; 
    }

    public override bool IsUnlocked()
    {
        return ScoreManager.Instance.TotalCardsGained() >= cardRequirement;
    }
}