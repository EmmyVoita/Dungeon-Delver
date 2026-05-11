using UnityEngine;
using UnityEngine.UIElements;

public class ApplyGoalScale : MonoBehaviour
{
    [SerializeField] private Transform goalContainer;
    void Update()
    {
        float scale = UpgradeManager.Instance != null ? UpgradeManager.Instance.ModifyGoalSize(1.0f) : 1f;
        goalContainer.localScale = Vector3.one * scale;
    }
}