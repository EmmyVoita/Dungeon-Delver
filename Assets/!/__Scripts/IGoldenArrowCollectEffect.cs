public interface ICoinCollectEffect
{
    bool CanTriggerEffect(int incomingDamage);
    bool TriggerEffect(int incomingDamage);
    int Priority { get; }
    bool RemoveAtLevelEnd { get; }
    ICoinCollectEffect Clone();
}