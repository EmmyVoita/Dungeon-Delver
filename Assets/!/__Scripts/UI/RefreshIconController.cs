using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.Rendering;

public class RefreshIconController : MonoBehaviour
{
    public Transform containerTransform;
    public TextMeshProUGUI refreshCountText;
    public TextMeshProUGUI buttonText;
    public Image refreshIconImage;
    public IdleHover idleHover;

    public Color activeColor = Color.white;
    public Color inactiveColor = Color.gray;
    public Color inactiveButtonColor = Color.clear;

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

    private void Awake()
    {
        buttonText.color = inactiveButtonColor;
        HideUI();
    }

    private void HandleGameStateChanged(GameState prev, GameState current)
    {
        if (current == GameState.UpgradeSelection)
        {
            containerTransform.gameObject.SetActive(true);
            RefreshUI();
        }
        else
        {
            HideUI();
        }
    }

    private void RefreshUI()
    {
        buttonText.text = $"[{InputBindingManager.Instance.GetKey(InputActionType.Interact)}]";
        int rerolls = RunStateManager.Instance != null ? RunStateManager.Instance.ShopRerollsRemaining : 0;
        refreshCountText.text = rerolls.ToString();
        HandleIconState(rerolls > 0);
    }

    private void HideUI()
    {
        containerTransform.gameObject.SetActive(false);
        buttonText.color = inactiveButtonColor;
    }

    private void HandleIconState(bool isActive)
    {
        refreshIconImage.color = isActive ? activeColor : inactiveColor;
        refreshCountText.color = isActive ? activeColor : inactiveColor;
        if(isActive)
        {
            idleHover.enableHover = true;
            idleHover.UpdateState();
            buttonText.DOColor(activeColor, 0.2f)
            .SetDelay(0.25f);
            
        }
        else
        {
            idleHover.enableHover = false;
            idleHover.UpdateState();
            buttonText.DOColor(inactiveButtonColor, 0.2f);
        }
    }
}