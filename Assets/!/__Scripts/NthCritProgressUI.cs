using UnityEngine;
using UnityEngine.UI;

public class NthCritProgressUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image fillImage;

    [Header("Target Upgrade")]
    [Tooltip("Upgrade ID this UI listens to")]
    [SerializeField] private string upgradeId;

    private void Awake()
    {
        if (fillImage != null)
            fillImage.fillAmount = 0f;
    }

    private void OnEnable()
    {
        NthCritBonus.OnNthCritProgress += HandleProgress;
    }

    private void OnDisable()
    {
        NthCritBonus.OnNthCritProgress -= HandleProgress;
    }

    private void HandleProgress(string sourceUpgradeId, int current, int required)
    {
        if (sourceUpgradeId != upgradeId)
            return;

        if (required <= 0)
        {
            fillImage.fillAmount = 0f;
            return;
        }

        float t = Mathf.Clamp01((float)current / required);
        fillImage.fillAmount = t;
    }
}
