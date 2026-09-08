using UnityEngine;

public class FullAbilityGoldenWorthMod : IGoldenArrowWorthModifier
{
    private float _val;
    private bool _addValue;

    public FullAbilityGoldenWorthMod(float val, bool addValue)
    {
        _val = val;
        _addValue = addValue;
    }

    public int ModifyGoldenArrowWorth(int baseWorth)
    {   
        if(Player.Instance.FullAbilityCharge)
        {
            if(_addValue)
                return Mathf.FloorToInt(baseWorth + _val);
            else
                return Mathf.FloorToInt(baseWorth * _val);
        }
            
        else
            return baseWorth;
    }
}