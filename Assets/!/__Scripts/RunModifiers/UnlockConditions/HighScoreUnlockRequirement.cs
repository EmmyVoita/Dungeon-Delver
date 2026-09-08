using UnityEngine;

[CreateAssetMenu(menuName = "Run Modifiers/Unlocks/Score Requirement")]
public class HighScoreUnlockRequirement : RunModifierUnlockRequirement
{
    [SerializeField] private int scoreRequirement;

    public override string GetRequirementRatioText()
    {
        return $"{ScoreManager.Instance.HighScore().ToString("N0")}/{scoreRequirement.ToString("N0")}";
    }

    public override string GetUnlockDescriptionText()
    {
        return $"Reach Score";
    }

    public override bool IsUnlocked()
    {
        return ScoreManager.Instance.HighScore() >= scoreRequirement;
    }
}