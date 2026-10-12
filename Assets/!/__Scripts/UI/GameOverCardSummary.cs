using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Jobs;

public class GameOverCardSummary : MonoBehaviour
{
    [SerializeField] private GameState showState = GameState.GameOverResults;

    [SerializeField] private GameObject ItemPrefab;
    [SerializeField] private RectTransform parentTransform;

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
            BuildSummary();
        }
    }

    private void BuildSummary()
    {
        foreach(var kvp in ShopManager.Instance.AllPurchasedCards)
        {
            
            var upgrade = kvp.Key;
            int count = kvp.Value;

            Sprite icon = upgrade.CenterIcon;

            GameObject item = Instantiate(ItemPrefab,transform.position, Quaternion.identity, parentTransform);
            if (item.TryGetComponent(out GameOverUpgradeIcon upgradeIcon))
                upgradeIcon.Initialize(icon,count);
        }
    }
}
