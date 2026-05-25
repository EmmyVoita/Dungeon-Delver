using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.Rendering;

public class RefreshIconController : MonoBehaviour
{
    public TextMeshProUGUI refreshCountText;
    public TextMeshProUGUI buttonText;

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
        int rerolls = RunStateManager.Instance != null ? RunStateManager.Instance.ShopRerollsRemaining : 0;
        buttonText.text = rerolls > 0 ? $"[<color=#{UIColors.ToHex(UIColors.Yellow)}>{InputBindingManager.Instance.GetKeyName(InputActionType.Interact)}</color>]" :
                                        $"[{InputBindingManager.Instance.GetKeyName(InputActionType.Interact)}]";
                
        refreshCountText.text = rerolls > 0 ? $"[<color=#{UIColors.ToHex(UIColors.Green)}>{rerolls}</color>]" : $"[{rerolls}]";
    }
}