using UnityEngine;

public class ApplyGoalScale : MonoBehaviour
{
    [SerializeField] private Transform goalContainer;
    void Update()
    {
        goalContainer.localScale = Vector3.one * UpgradeManager.Instance.ModifyGoalSize(1.0f);
    }
}