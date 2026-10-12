using UnityEngine;

public class RerollOnComboEffect : IPassiveEffect
{
    public int Priority => 1;

    private int _comboRequired;
    private int _rerollAmount;
    private float _rerollChance;
    private float _zapChance;
    private Gradient _lightningGradient;

    public RerollOnComboEffect(int comboRequired, int rerollAmount, float rerollChance, float zapChance, Gradient lightningGradient)
    {
        _comboRequired = comboRequired;
        _rerollAmount = rerollAmount;
        _rerollChance = rerollChance;
        _zapChance = zapChance;
        _lightningGradient = lightningGradient;
    }

    public IRuntimeModifier Clone()
    {
        return new RerollOnComboEffect(_comboRequired, _rerollAmount,_rerollChance, _zapChance,_lightningGradient);
    }

    private void OnDestroy()
    {
        Destroy();
    }

    public void OnActivate()
    {
        //throw new System.NotImplementedException();
    }

    public void Destroy()
    {
        ComboManager.OnComboUpdated -= HandleComboChanged;
    }

    public bool ShouldStart()
    {
        return true;
    }

    public void Start()
    {
        ComboManager.OnComboUpdated -= HandleComboChanged;
        ComboManager.OnComboUpdated += HandleComboChanged;
    }

    public void Stop()
    {
        ComboManager.OnComboUpdated -= HandleComboChanged;
    }

    public void Update()
    {
    }

    private void HandleComboChanged(int comboCount)
    {
        if(comboCount > 0 && comboCount % _comboRequired == 0)
        {
            bool shouldGrant = Random.value > Mathf.Max(1 - _rerollChance, 0);

            if(shouldGrant)
            {
                CurrencyManager.Instance.AddShopRerolls(_rerollAmount, "Combo!", "Rerolls");
                CurrencyManager.Instance.PlayRerollPopup(_rerollAmount);
                CoinZapManager.Instance.ZapAllArrows(_lightningGradient);
            }
        }
    }
}