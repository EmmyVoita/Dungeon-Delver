using UnityEngine;

public class CoinCollectZap : ICoinCollectEffect
{
    public int Priority => 0;
    public bool RemoveAtLevelEnd => false;
    public bool PreventComboBreak => _preventComboBreak;

    private bool _preventComboBreak;
    private int _currencyRequired;
    private float _zapRadius; 
    private float _baseZapChance; 
    private float _enhancedZapChance;
    private Gradient _gradient;
    private Gradient _empoweredGradient;

    public CoinCollectZap(Gradient gradient,
                          Gradient empoweredGradient,
                          int currencyRequired,     
                          bool preventComboBreak, 
                          float zapRadius = 8f, 
                          float zapChance = 0.25f,
                          float enhancedZapChance = 0.5f)
    {
        _gradient = gradient;
        _empoweredGradient = empoweredGradient;
        _preventComboBreak = preventComboBreak;
        _currencyRequired = currencyRequired;
        _zapRadius = zapRadius;
        _baseZapChance = zapChance;
        _enhancedZapChance = enhancedZapChance;
    }

    public bool CanTriggerEffect(int amount)
    {
        return amount > 0 &&
               CoinZapManager.Instance != null &&
               CoinZapManager.Instance.HasTarget(_zapRadius);
    }

    public bool TriggerEffect(int amount)                  
    {
        float zapChance = CurrencyManager.Instance.CurrentCurrency >= _currencyRequired ? _enhancedZapChance : _baseZapChance;

        if(Random.value < zapChance)
        {
            CoinZapManager.Instance.QueueZap(_zapRadius, _preventComboBreak, false, _gradient);
        }

        return true;
    }

    public IRuntimeModifier Clone()
    {
        return new CoinCollectZap(
            _gradient, 
            _empoweredGradient, 
            _currencyRequired, 
            _preventComboBreak, 
            _zapRadius, 
            _baseZapChance,
            _enhancedZapChance
        );
    }

    public void Destroy()
    {
        
    }

    public void OnActivate()
    {
        
    }
}