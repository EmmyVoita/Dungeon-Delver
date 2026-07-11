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
        UpgradeCardManager.OnCardPurchased += HandleCardPurchased;
    }

    private void OnDisable()
    {
        GameStateManager.OnStateChanged -= HandleStateChanged;
        UpgradeCardManager.OnCardPurchased -= HandleCardPurchased;
    }

    private void HandleStateChanged(GameState previousState, GameState newState)
    {
        RefreshText();
    }

    private void HandleCardPurchased(UpgradeOption upgrade)
    {
        Debug.Log("HandleCardPurchased");
        RefreshText();
    }

    private void RefreshText()
    {
        //text.text = $"Continue";
        /*
        bool canHeal =
            showStates.Contains(GameStateManager.Instance.CurrentState) &&
            Player.Instance.Health < Player.Instance.MaxHealth &&
            UpgradeCardManager.Instance.PurchasedCardsCount == 0;
        */
        /*
        if(canHeal)
        {
            text.text =
                $"Continue. Heal: [<color=#{UIColors.ToHex(UIColors.Green)}>{healAmount}</color>]";
        }
        else
        {
            text.text =
                $"Continue";
        }
        */
        
    }

}