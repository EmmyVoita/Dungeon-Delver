using UnityEngine;

public class CurrencyDamageSave : IDamageSave
{
    public int Priority => 0;
    public bool RemoveAtLevelEnd => true;
    public bool PreventComboBreak => _preventComboBreak;
    public GameObject ShieldPrefab {get;}
    
    private bool _preventComboBreak;
    private int _currencyRequired;

    public CurrencyDamageSave(int currencyRequired, bool preventComboBreak, GameObject shieldPrefab)
    {
        _preventComboBreak = preventComboBreak;
        _currencyRequired = currencyRequired;
        ShieldPrefab = shieldPrefab;
    }

    public bool CanPreventDamage(int damage)
    {
        return damage > 0 && CurrencyManager.Instance.CurrentCurrency >= _currencyRequired;
    }

    public bool HasExpired()
    {
        return false;
    }

    public bool PreventDamage(int damage)
    {
        ComboManager.Instance._ignoreNextMissCount++;
        bool spent = CurrencyManager.Instance.TrySpendCurrency(_currencyRequired);

        if(spent)
            Player.Instance.AddHitBlock(1);

        return spent;
    }

    public IRuntimeModifier Clone()
    {
        return new CurrencyDamageSave(_currencyRequired, _preventComboBreak, ShieldPrefab);
    }

    public void OnDestroy()
    {
        
    }

     public void OnActivate()
    {
        
    }
}