
using System.Collections.Generic;
using UnityEngine;



[System.Serializable]
public struct ArrowStatusScoreRule
{
    public ArrowStatus status;
    public float scoreMultiplier;
}


public class ScoreRules : MonoBehaviour
{
    public static ScoreRules Instance;

    [Header("Base Values")]
    public int baseArrowScore = 100;
    public int goldenCritArrowScoreMult = 5;
    public float critMultiplier = 2f;

    [Header("Status Multipliers")]
    public List<ArrowStatusScoreRule> statusRules;
    public float NormalArrowTotalModifier => GetNormalArrowTotalModifier();
    public float CritArrowTotalModifier => GetCritArrowTotalModifier();
    public float ComboTotalModifier => GetComboTotalModifier();

    private void Awake()
    {
        Instance = this;
    }

    public int CalculateScore(
        ArrowBase arrow,
        Goal.GoalType goalType)
    {
        float score = baseArrowScore;

        switch(goalType)
        {
            case Goal.GoalType.Normal:
                float normalWorth = 1.0f;//UpgradeManager.Instance.ModifyNormalHitValue(1.0f);
                score = score * normalWorth;
                break;
            case Goal.GoalType.Critical:
                float critBaseMultiplier = critMultiplier;//UpgradeManager.Instance.ModifyCritBase(critMultiplier);
                float critWorth = critBaseMultiplier;//UpgradeManager.Instance.ModifyCritHitValue(critBaseMultiplier);
                score = score * critWorth;
                break;
            default:
                return 0; 
        }

        //score = UpgradeManager.Instance.ModifyArrowScore(score);
        //score = UpgradeManager.Instance.ModifyGlobalScoreMultiplier(score);
            
        // ---- status modifiers ----
        ArrowStatus status = arrow.GetStatus();

        score *= GetStatusMultiplier(status);

        return Mathf.RoundToInt(score);
    }

    private float GetStatusMultiplier(ArrowStatus status)
    {
        float multiplier = 1f;

        foreach (var rule in statusRules)
        {
            if (status.HasFlag(rule.status))
                multiplier *= rule.scoreMultiplier;
        }

        /*
        // 🔑 allow upgrades to modify the result
        foreach (var mod in UpgradeManager.Instance.StatusScoreModifiers)
        {
            multiplier = mod.ModifyStatusMultiplier(status, multiplier);
        }
        */

        return multiplier;
    }


    private float GetNormalArrowTotalModifier()
    {
        float normalWorth = 1.0f;//UpgradeManager.Instance.ModifyNormalHitValue(1.0f);
        //normalWorth = UpgradeManager.Instance.ModifyArrowScore(normalWorth);
        //normalWorth = UpgradeManager.Instance.ModifyGlobalScoreMultiplier(normalWorth);
        
        return normalWorth;
    }

    private float GetCritArrowTotalModifier()
    {
        float critWorth = 1.0f; //UpgradeManager.Instance.ModifyCritHitValue(1.0f);
        //critWorth = UpgradeManager.Instance.ModifyArrowScore(critWorth);
        //critWorth = UpgradeManager.Instance.ModifyGlobalScoreMultiplier(critWorth);
        
        return critWorth;
    }

    private float GetComboTotalModifier()
    {
        float comboWorth = 1.0f;//UpgradeManager.Instance.ModifyComboScoreMultiplier(1.0f);
        //comboWorth = UpgradeManager.Instance.ModifyGlobalScoreMultiplier(comboWorth);
        
        return comboWorth;
    }
}

