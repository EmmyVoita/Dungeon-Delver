public interface IDeathSave
{
    bool CanPreventDeath(int incomingDamage);
    bool PreventDeath(int incomingDamage);
    int Priority { get; }
    bool RemoveAtLevelEnd { get; }
    IDeathSave Clone();
}