public interface IDeathSave : IRuntimeModifier
{
    bool CanPreventDeath(int incomingDamage);
    bool PreventDeath(int incomingDamage);
    bool RemoveAtLevelEnd { get; }
}