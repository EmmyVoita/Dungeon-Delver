using UnityEngine;

public class CoinCollectZap : ICoinCollectEffect
{
    public int Priority => 0;
    public bool RemoveAtLevelEnd => false;
    public bool PreventComboBreak => _preventComboBreak;

    private bool _preventComboBreak;
    private int _currencyRequired;
    private float _zapRadius; 
    private float _zapChance; 

    public CoinCollectZap(int currencyRequired, bool preventComboBreak, float zapRadius = 8f, float zapChance = 0.5f)
    {
        _preventComboBreak = preventComboBreak;
        _currencyRequired = currencyRequired;
        _zapRadius = zapRadius;
        _zapChance = zapChance;
    }

    public bool CanTriggerEffect(int amount)
    {
        return amount > 0 &&
               CoinZapManager.Instance != null &&
               CoinZapManager.Instance.HasTarget(_zapRadius);
    }

    public bool TriggerEffect(int amount)                  
    {
        if(CurrencyManager.Instance.CurrentCurrency >= _currencyRequired)
        {
            CoinZapManager.Instance.QueueZap(_zapRadius, _preventComboBreak, true);
        }
        else if(Random.value >= _zapChance)
        {
            CoinZapManager.Instance.QueueZap(_zapRadius, _preventComboBreak, false);
        }

        return true;
    }

    public ICoinCollectEffect Clone()
    {
        return new CoinCollectZap(_currencyRequired, _preventComboBreak, _zapRadius, _zapChance);
    }
}