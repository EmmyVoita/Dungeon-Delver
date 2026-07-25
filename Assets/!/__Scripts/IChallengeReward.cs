public interface IChallengeReward : IRuntimeModifier
{
    float AppearancePercentage { get; }
    int MaxUses {get; }
    int UsesRemaining {get; }
    bool ShouldGrantReward(int damageTaken);
    bool GrantReward(int damageTaken);
}