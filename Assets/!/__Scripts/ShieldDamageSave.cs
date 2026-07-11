public class ShieldDamageSave : IDamageSave
{
    public int Priority => 100;
    public bool RemoveAtLevelEnd => true;
    
    private bool _preventComboBreak;

    public ShieldDamageSave(bool preventComboBreak)
    {
        _preventComboBreak = preventComboBreak;
    }

    public bool CanPreventDamage(int damage)
    {
        return damage > 0;
    }

    public bool PreventDamage(int damage)
    {
        Player.Instance.AddHitBlock(1);
        return true;
    }

    public IDamageSave Clone()
    {
        return new ShieldDamageSave(_preventComboBreak);
    }
}