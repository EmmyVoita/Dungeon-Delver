using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class CritStreakText : MonoBehaviour
{
    private TextMeshProUGUI text;

    [Header("Display")]
    [SerializeField] private string prefix = "CRITS: ";
    [SerializeField] private bool hideWhenZero = true;

    private void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();
    }

    private void OnEnable()
    {
        ComboManager.OnCritStreakUpdated += UpdateText;

        // Sync immediately so UI is never stale
        if (ComboManager.Instance != null)
            UpdateText(ComboManager.Instance.CritsInARow);
    }

    private void OnDisable()
    {
        ComboManager.OnCritStreakUpdated -= UpdateText;
    }

    private void UpdateText(int critCount)
    {
        if (hideWhenZero && critCount == 0)
        {
            text.enabled = false;
            return;
        }

        text.enabled = true;
        text.text = $"{prefix}{critCount}";
    }
}
