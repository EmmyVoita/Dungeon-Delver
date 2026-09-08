using UnityEngine;

public class PassiveAbilityChargeEffect : IPassiveEffect
{
    private float _chargeRate;
    private bool _isActive;
    private float _currentCharge;

    public int Priority => 1;

    public PassiveAbilityChargeEffect(float chargeRate)
    {
        _chargeRate = chargeRate;
        _isActive = false;
    }

    public void OnActivate()
    {
        //throw new System.NotImplementedException();
    }

    public void OnDestroy()
    {
        //throw new System.NotImplementedException();
    }

    public void Update()
    {
        if(_isActive)
        {
            _currentCharge += _chargeRate * Time.deltaTime;

            int currentChargeInt = Mathf.FloorToInt(_currentCharge);

            if(currentChargeInt >= 1)
            {
                Player.Instance.AbilityCharge += currentChargeInt;
                _currentCharge = 0;
            }
        }
    }

    public bool ShouldStart()
    {
        return true;
    }

    public void Start()
    {
        _isActive = true;
    }

    public void Stop()
    {
        _isActive = false;
    }

    public IRuntimeModifier Clone()
    {
        return new PassiveAbilityChargeEffect(_chargeRate);
    }
}