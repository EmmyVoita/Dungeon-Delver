public interface ICoinCollectEffect : IRuntimeModifier
{
    bool CanTriggerEffect(int incomingDamage);
    bool TriggerEffect(int incomingDamage);
    bool RemoveAtLevelEnd { get; }
}