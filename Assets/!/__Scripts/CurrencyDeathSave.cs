using UnityEngine;

public class CurrencyDeathSave : IDeathSave
{
    public int Priority => 100;
    public bool RemoveAtLevelEnd => _removeAtEndLevel;

    private int _healAmount = 1;
    private int _cost;
    private bool _removeAtEndLevel;



    public CurrencyDeathSave(int healAmount, int cost, bool removeAtEndLevel)
    {
        _healAmount = healAmount;
        _removeAtEndLevel = removeAtEndLevel;
        _cost = cost;
    }

    public bool CanPreventDeath(int damage)
    {
        return CurrencyManager.Instance.CurrentCurrency >= _cost;
    }

    public bool PreventDeath(int damage)
    {
        bool spent = CurrencyManager.Instance.TrySpendCurrency(_cost);

        if(spent)
            Player.Instance.HealPlayer(_healAmount);
    
        return spent;
    }

    public IDeathSave Clone()
    {
        return new CurrencyDeathSave(_healAmount,_cost, _removeAtEndLevel);
    }
}