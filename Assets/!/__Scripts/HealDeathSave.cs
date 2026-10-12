public class HealDeathSave : IDeathSave
{
    public int Priority => 100;
    public bool RemoveAtLevelEnd => true;
    
    private int _healAmount = 1;

    public HealDeathSave(int healAmount)
    {
        _healAmount = healAmount;
    }

    public bool CanPreventDeath(int damage)
    {
        return true;
    }

    public bool PreventDeath(int damage)
    {
        Player.Instance.HealPlayer(_healAmount);
        return true;
    }

    public IRuntimeModifier Clone()
    {
        return new HealDeathSave(_healAmount);
    }

    public void OnActivate()
    {
        
    }

    public void Destroy()
    {
        
    }
}