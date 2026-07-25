using DG.Tweening;
using TMPro;
using UnityEngine;

public class HealOptionUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI healOptionText;
    [SerializeField] private GameState showState = GameState.UpgradeSelection;

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
        if(newState == showState)
        {
            if (Player.Instance.Health < Player.Instance.MaxHealth)
            {
                healOptionText.text = $"Skip [<color=#FFD700>{InputBindingManager.Instance.GetBoundKey(InputActionType.Jump)}</color>] and Heal for 1 health";
            }
            else
            {
                healOptionText.text = "";
            }
        }
    }
}

