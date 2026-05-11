using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class RunRecordItem : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI rankText;
    [SerializeField] private TextMeshProUGUI accuracyText;
    [SerializeField] private TextMeshProUGUI critAccuracyText;

    [Header("Formatting")]
    [SerializeField] private string rankPrefix = "#";

    public void Initialize(RunRecord runRecord, int rank)
    {
        Sprite icon = AbilityIconDatabase.Instance?.GetIcon(runRecord.abilityUsed);

        if(icon != null)
            iconImage.sprite = icon;
        else
            Debug.LogWarning($"When requesting ability icon for => {runRecord.abilityUsed}, the database returned null");

        rankText.text = $"{rankPrefix}{rank}";
        scoreText.text = runRecord.score.ToString("N0");
        accuracyText.text = runRecord.accuracy.ToString("P0");
        critAccuracyText.text = runRecord.critAccuracy.ToString("P0");
    }
}