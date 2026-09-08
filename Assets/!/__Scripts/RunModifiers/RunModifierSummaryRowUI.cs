using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RunModifierSummaryRowUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI valueText;
    [SerializeField] private Image iconImage;

    public void Setup(RunModifierDefinition definition,string value)
    {
        nameText.text = definition.summaryName;
        valueText.text = value;

        if (iconImage != null)
        {
            iconImage.sprite = definition.summaryIcon;
            iconImage.enabled = definition.summaryIcon != null;
        }
    }

    public void SetValue(string value)
    {
        valueText.text = value;
    }
}