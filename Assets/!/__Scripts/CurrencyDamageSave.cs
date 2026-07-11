public class CurrencyDamageSave : IDamageSave
{
    public int Priority => 0;
    public bool RemoveAtLevelEnd => true;
    public bool PreventComboBreak => _preventComboBreak;
    
    private bool _preventComboBreak;
    private int _currencyRequired;

    public CurrencyDamageSave(int currencyRequired, bool preventComboBreak)
    {
        _preventComboBreak = preventComboBreak;
        _currencyRequired = currencyRequired;
    }

    public bool CanPreventDamage(int damage)
    {
        return damage > 0 && CurrencyManager.Instance.CurrentCurrency >= _currencyRequired;
    }

    public bool PreventDamage(int damage)
    {
        bool spent = CurrencyManager.Instance.TrySpendCurrency(_currencyRequired);

        if(spent)
            Player.Instance.AddHitBlock(1);

        return spent;
    }

    public IDamageSave Clone()
    {
        return new CurrencyDamageSave(_currencyRequired, _preventComboBreak);
    }
}