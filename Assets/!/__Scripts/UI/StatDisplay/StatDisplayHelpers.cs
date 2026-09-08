using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public static class StatDisplayHelpers
{
    public static void SetupStatRow(TextMeshProUGUI displayText, StatRowData rowData, GameObject rowObj)
    {
        displayText.font = rowData.font;
        displayText.fontSize = rowData.fontSize;
        displayText.alignment = rowData.alignment; 
        displayText.color = rowData.textColor;
        
        RectTransform rowRT = rowObj.GetComponent<RectTransform>();

        rowRT.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Vertical,
            rowData.rowHeight
        );
    }


    public static string FormatStatValue(StatValue stat, string prefix)
    {
        return stat.type switch
        {
            StatDisplayType.Int     => $"{prefix}{stat.value.ToString("N0")}",
            StatDisplayType.Ratio   => $"{prefix}{stat.value.ToString("N0")}/{stat.total.ToString("N0")}",
            StatDisplayType.String  => $"{prefix}{stat.text}",
            StatDisplayType.Percent => $"{prefix}{stat.value.ToString("N0")}%",
            _ => "Unknown Stat Display Type"
        };
    }

    public static StatValue ResolveStatValue(StatRowData data)
    {
        if(RoundManager.Instance == null || 
           ScoreManager.Instance == null || 
           Player.Instance == null || 
           CurrencyManager.Instance == null ||
           ScoreManager.Instance.RoundStatsTracker == null ||
           ScoreManager.Instance.RunStatsTracker  == null
           )
        {
            Debug.LogError("Score Manager or Round Manager Instance can not be null when evaluating display stat for UI");
        }


        switch (data.statValueType)
        {
            case StatValueType.TotalScore:
                return StatValue.FromInt(
                    ScoreManager.Instance.RunStatsTracker.TotalScore
                );
            case StatValueType.Hits:
                return StatValue.FromRatio(
                    ScoreManager.Instance.RoundStatsTracker.Hit,
                   ScoreManager.Instance.RoundStatsTracker.Spawned
                );
            case StatValueType.RunHits:
                return StatValue.FromRatio(
                    ScoreManager.Instance.RunStatsTracker.TotalHit,
                    ScoreManager.Instance.RunStatsTracker.TotalSpawned
                );
            case StatValueType.RunTotalScore:
                {
                    Debug.Log($"Entered RunTotalScore case. Data: {data.statValueType}");
                    Debug.Log($"ScoreManager.Instance: {ScoreManager.Instance}");
                    Debug.Log("About to read RunScoreTotal");

                    int runScoreTotal = ScoreManager.Instance.RunScoreTotal;

                    Debug.Log($"Successfully read RunScoreTotal: {runScoreTotal}");

                    return StatValue.FromInt(runScoreTotal);
                }
            case StatValueType.RunHighestCombo:
                return StatValue.FromInt(
                    ScoreManager.Instance.RunStatsTracker.HighestCombo
                );
            case StatValueType.RunDamageTaken:
                return StatValue.FromInt(
                    ScoreManager.Instance.RunStatsTracker.TotalDamageTaken
                );
            case StatValueType.CauseOfDeath:
                {
                    return RoundManager.Instance.GameComplete ? StatValue.FromString("You Win!") : 
                                                                StatValue.FromString($"Killed by {Player.Instance.LastDamageSource}");
                }
            case StatValueType.LevelIndex:
                return StatValue.FromString(
                    RoundManager.Instance.LevelIndex
                );
            case StatValueType.RunCritRate:
                int critPercent = Mathf.RoundToInt(ScoreManager.Instance.RunStatsTracker.RunCritRate * 100f);
                return StatValue.FromPercent(
                    critPercent
                );
            case StatValueType.DamageTaken:
                return StatValue.FromInt(
                    ScoreManager.Instance.RoundStatsTracker.DamageTaken
                );
            case StatValueType.RoundScore:
                return StatValue.FromInt(
                    ScoreManager.Instance.RoundStatsTracker.Score
                );
            case StatValueType.Currency:
                return StatValue.FromInt(
                    CurrencyManager.Instance.CurrentCurrency
                );

            default:
                return StatValue.FromString("Unknown Stat Row Data");
        }
    }

    public static StatValue ResolveRunRecordStatValue(StatRowData data, RunRecord runRecord)
    {
        switch (data.statValueType)
        {
            case StatValueType.RunHits:
                return StatValue.FromRatio(runRecord.totalHit, runRecord.totalSpawned);
            case StatValueType.RunTotalScore:
                return StatValue.FromInt(runRecord.score);
            case StatValueType.RunHighestCombo:
                return StatValue.FromInt(runRecord.highestCombo);
            case StatValueType.RunDamageTaken:
                return StatValue.FromInt(runRecord.damageTaken);
            case StatValueType.CauseOfDeath:
                return StatValue.FromString(runRecord.causeOfDeath);
                    /*
                    return RoundManager.Instance.GameComplete ? StatValue.FromString("You Win!") : 
                                                                StatValue.FromString($"Killed by {Player.Instance.LastDamageSource}");
                    */
            case StatValueType.LevelIndex:
                return StatValue.FromString(runRecord.levelIndex);
            case StatValueType.RunCritRate:
                int critPercent = Mathf.RoundToInt(runRecord.critAccuracy * 100f);
                return StatValue.FromPercent(critPercent);
            case StatValueType.Currency:
                return StatValue.FromInt(runRecord.totalCurrency);
            default:
                return StatValue.FromString("Unknown Stat Row Data");
        }
    }
}
