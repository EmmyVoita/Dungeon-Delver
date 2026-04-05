using UnityEngine;
using UnityEngine.UI;

public class FullAbilityBarBonusIndicator : MonoBehaviour
{
    [Header("Upgrade")]
    [Tooltip("Must match UpgradeBase.upgradeId")]
    [SerializeField] private string upgradeId;

    [Header("UI")]
    //[SerializeField] private GameObject indicatorObject;
    // or use Image if you prefer:
    [SerializeField] private Image indicatorImage;

    private void Awake()
    {
        //if (indicatorObject != null)
        //    indicatorObject.SetActive(false);

        indicatorImage.enabled = false;
    }

    private void OnEnable()
    {
        UpgradeManager.OnUpgradeStateChanged += HandleUpgradeStateChanged;
    }

    private void OnDisable()
    {
        UpgradeManager.OnUpgradeStateChanged -= HandleUpgradeStateChanged;
    }

    private void HandleUpgradeStateChanged(string changedUpgradeId, bool active)
    {
        if (changedUpgradeId != upgradeId)
            return;

        //if (indicatorObject != null)
        //    indicatorObject.SetActive(active);

        // If using Image instead:
        indicatorImage.enabled = active;
    }
}
