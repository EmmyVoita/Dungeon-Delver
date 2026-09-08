public interface IHealthChangeEffect : IRuntimeModifier
{
    bool CanTriggerEffect(int changeAmount);
    bool TriggerEffect(int changeAmount);
}