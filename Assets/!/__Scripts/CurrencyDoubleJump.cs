using Unity.VisualScripting;
using UnityEngine;

public class CurrencyDoubleJump : IJumpEffect
{
    public int Priority => 0;

    private int _currencyRequired;

    private JumpData _lastJumpData;


    public CurrencyDoubleJump(int currencyRequired)
    {
        _currencyRequired = currencyRequired;
    }

    public bool CanTriggerEffect(JumpData data)
    {
        if (!data.IsExtraJump)
        {
            _lastJumpData = data;
            return false;
        }
            

        if(_lastJumpData.IsExtraJump)
            return false;

        return CurrencyManager.Instance.CurrentCurrency >= _currencyRequired;
    }

    public bool TriggerEffect(JumpData data)                  
    {
        if(!CurrencyManager.Instance.TrySpendCurrency(_currencyRequired))
            return false;

        // We pass the previous jump direction in for when we call an extra jump
        // in for the jump direction
        Player.Instance.PerformJump(data.Direction);

        _lastJumpData = data;

        return true;
    }

    public IRuntimeModifier Clone()
    {
        return new CurrencyDoubleJump(_currencyRequired);
    }

    public void OnDestroy()
    {
        
    }

    public void OnActivate()
    {
        
    }
}