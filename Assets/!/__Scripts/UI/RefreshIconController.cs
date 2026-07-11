using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.Rendering;

public class RefreshIconController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI refreshCountText;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private PromptLayoutController prompt;

    private void OnEnable()
    {
        GameStateManager.OnStateChanged += HandleGameStateChanged;
        RunStateManager.OnRefreshRerollsChanged += RefreshUI;
    }

    private void OnDisable()
    {
        GameStateManager.OnStateChanged -= HandleGameStateChanged;
        RunStateManager.OnRefreshRerollsChanged -= RefreshUI;
    }

    private void HandleGameStateChanged(GameState prev, GameState current)
    {
        if (current == GameState.UpgradeSelection)
        {
            RefreshUI();
        }
    }

    private void RefreshUI()
    {
        int rerolls = RunStateManager.Instance != null
            ? RunStateManager.Instance.ShopRerollsRemaining
            : 0;

        canvasGroup.alpha = rerolls > 0 ? 1 : 0;

        refreshCountText.text = rerolls > 0
            ? $"<color=#{UIColors.ToHex(UIColors.Yellow)}>Rerolls Left: {rerolls}</color>"
            : "";

        refreshCountText.ForceMeshUpdate();

        LayoutRebuilder.ForceRebuildLayoutImmediate(
            refreshCountText.rectTransform
        );

        LayoutRebuilder.ForceRebuildLayoutImmediate(
            refreshCountText.transform.parent as RectTransform
        );

        prompt.Refresh();
    }
}