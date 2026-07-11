public interface IChallengeReward
{
    int Priority { get; }
    float AppearancePercentage { get; }
    int MaxUses {get; }
    int UsesRemaining {get; }
    bool ShouldGrantReward(int damageTaken);
    bool GrantReward(int damageTaken);
    IChallengeReward Clone();
}