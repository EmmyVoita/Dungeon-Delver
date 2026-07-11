using UnityEngine;

public class CurrencyDeathSave : IDeathSave
{
    public int Priority => 100;
    public bool RemoveAtLevelEnd => _removeAtEndLevel;

    private int _healAmount = 1;
    private bool _removeAtEndLevel;



    public CurrencyDeathSave(int healAmount, bool removeAtEndLevel)
    {
        _healAmount = healAmount;
        _removeAtEndLevel = removeAtEndLevel;
    }

    public bool CanPreventDeath(int damage)
    {
        return CurrencyManager.Instance.CurrentCurrency >= 500;
    }

    public bool PreventDeath(int damage)
    {
        bool spent = CurrencyManager.Instance.TrySpendCurrency(500);

        if(spent)
            Player.Instance.HealPlayer(_healAmount);
    
        return spent;
    }

    public IDeathSave Clone()
    {
        return new CurrencyDeathSave(_healAmount,_removeAtEndLevel);
    }
}