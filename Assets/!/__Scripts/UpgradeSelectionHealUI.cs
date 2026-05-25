using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class UpgradeSelectionHealUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private List<GameState> showStates;
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private int healAmount = 1;

    private void OnEnable()
    {
        GameStateManager.OnStateChanged += HandleStateChanged;
    }

    private void OnDisable()
    {
        GameStateManager.OnStateChanged -= HandleStateChanged;
    }

    private void HandleStateChanged(GameState previousState, GameState newState)
    {
        if(showStates.Contains(newState) && Player.Instance.Health < Player.Instance.MaxHealth)
        {
            text.text = $"Skip [<color=#{UIColors.ToHex(UIColors.Yellow)}>{InputBindingManager.Instance.GetKeyName(InputActionType.Jump)}</color>] and Heal [<color=#{UIColors.ToHex(UIColors.Green)}>{healAmount}</color>]";
        }
        else if(showStates.Contains(newState))
        {
            text.text = $"Skip [{InputBindingManager.Instance.GetKeyName(InputActionType.Jump)}] and Heal [0]";
        }
    }

}