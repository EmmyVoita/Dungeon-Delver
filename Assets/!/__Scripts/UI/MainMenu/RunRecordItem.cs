using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class RunRecordItem : MonoBehaviour, IPointerClickHandler
{
    public static event Action<RunRecordItem> OnClicked;

    [Header("References")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI rankText;
    [SerializeField] private TextMeshProUGUI accuracyText;
    [SerializeField] private TextMeshProUGUI critAccuracyText;
    [SerializeField] private RectTransform hoverRect;
    

    [Header("Formatting")]
    [SerializeField] private string rankPrefix = "#";
    [SerializeField] private string accuracySuffixText;

    private RunRecord _runRecord;

    public RunRecord Record => _runRecord;
    public AbilityType AbilityType => _runRecord.abilityUsed;


    public void Initialize(RunRecord runRecord, int rank)
    {
        Sprite icon = AbilityIconDatabase.Instance?.GetSmallIcon(runRecord.abilityUsed);

        if(icon != null)
            iconImage.sprite = icon;
        else
            Debug.LogWarning($"When requesting ability icon for => {runRecord.abilityUsed}, the database returned null");

        _runRecord = runRecord;
        rankText.text = $"{rankPrefix}{rank}";
        scoreText.text = runRecord.score.ToString("N0");
        accuracyText.text = FormatPercentageDown(runRecord.accuracy) + accuracySuffixText;
        critAccuracyText.text = FormatPercentageDown(runRecord.critAccuracy);
    }

    public void SetHovered(bool isHovered)
    {
        if(hoverRect == null)
            return;

        hoverRect.gameObject.SetActive(isHovered);
    }

    private string FormatPercentageDown(float value)
    {
        // Convert 0-1 to percentage
        float percentage = value * 100f;

        // Truncate to 1 decimal place instead of rounding
        percentage = Mathf.Floor(percentage * 10f) / 10f;

        return $"{percentage:F1}%";
    }

    void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
    {
        if (InputModeManager.Instance.CurrentMode
            != InputModeManager.InputMode.Mouse)
            return;

        OnClicked?.Invoke(this);
    }
}