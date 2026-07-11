public interface IDamageSave
{
    bool CanPreventDamage(int incomingDamage);
    bool PreventDamage(int incomingDamage);
    int Priority { get; }
    bool RemoveAtLevelEnd { get; }
    IDamageSave Clone();
}