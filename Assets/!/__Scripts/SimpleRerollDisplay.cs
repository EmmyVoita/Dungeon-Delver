using NUnit.Framework.Interfaces;
using TMPro;
using UnityEngine;

public class SimpleRerollDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI countText;

    private void OnEnable()
    {
        RunStateManager.OnRefreshRerollsChanged += HandleRerollsChanged;

        HandleRerollsChanged();
    }

    private void OnDisable()
    {
        RunStateManager.OnRefreshRerollsChanged -= HandleRerollsChanged;
    }
    
    private void HandleRerollsChanged()
    {
        countText.text = RunStateManager.Instance.ShopRerollsRemaining.ToString();
    }
}