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
        if(RoundManager.Instance == null || ScoreManager.Instance == null)
        {
            Debug.LogError("Score Manager or Round Manager Instance can not be null when evaluating display stat for UI");
        }

        switch (data.statValueType)
        {
            case StatValueType.TotalScore:
                return StatValue.FromInt(
                    RoundManager.Instance.runStats.TotalScore
                );
            case StatValueType.Hits:
                return StatValue.FromRatio(
                    RoundManager.Instance.stats.Hit,
                    RoundManager.Instance.stats.Spawned
                );
            case StatValueType.RunHits:
                return StatValue.FromRatio(
                    RoundManager.Instance.runStats.TotalHit,
                    RoundManager.Instance.runStats.TotalSpawned
                );
            case StatValueType.RunTotalScore:
                return StatValue.FromInt(
                    ScoreManager.Instance.RunScoreTotal
                );
            case StatValueType.RunHighestCombo:
                return StatValue.FromInt(
                    RoundManager.Instance.runStats.HighestCombo
                );
            case StatValueType.RunDamageTaken:
                return StatValue.FromInt(
                    RoundManager.Instance.runStats.TotalDamageTaken
                );
            case StatValueType.CauseOfDeath:
                return StatValue.FromString(
                    Player.Instance.LastDamageSource
                );
            case StatValueType.LevelIndex:
                return StatValue.FromString(
                    RoundManager.Instance.LevelIndex
                );
            case StatValueType.RunCritRate:
                int critPercent = Mathf.RoundToInt(RoundManager.Instance.runStats.RunCritRate * 100f);
                return StatValue.FromPercent(
                    critPercent
                );
            case StatValueType.DamageTaken:
                return StatValue.FromInt(
                    RoundManager.Instance.stats.DamageTaken
                );
            case StatValueType.RoundScore:
                return StatValue.FromInt(
                    RoundManager.Instance.stats.Score
                );
            case StatValueType.Currency:
                return StatValue.FromInt(
                    CurrencyManager.Instance.CurrentCurrency
                );

            default:
                return StatValue.FromString("Unknown Stat Row Data");
        }
    }
}
