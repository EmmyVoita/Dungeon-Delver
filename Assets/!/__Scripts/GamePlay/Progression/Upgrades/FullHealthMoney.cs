using UnityEngine;


[CreateAssetMenu(menuName = "Upgrades/Full Health Money")]
public class FullHealthMoney : UpgradeBase
{
    [SerializeField] private int currencyAmount = 10;
    [SerializeField] private bool infiniteDuration = true;


    private void OnDisable()
    {
        GameStateManager.OnStateChanged -= HandleStateChanged;
    }

    private void HandleStateChanged(GameState previousState, GameState newState)
    {
        if(newState == GameState.WorldMapView && !infiniteDuration)
            Deactivate();
    }

    public override string GetDescription()
    {
        return descriptionTemplate
            .Replace("{CURRENCY_AMOUNT}", currencyAmount.ToString());
    }

    public override void Apply()
    {
        Player.OnHeal -= HandleHeal;
        Player.OnHeal += HandleHeal;

        GameStateManager.OnStateChanged -= HandleStateChanged;

        if(!infiniteDuration)
            GameStateManager.OnStateChanged += HandleStateChanged;

        //UpgradeManager.Instance.AddUpgrade(this);
    }

    public void Deactivate()
    {
        Player.OnHeal -= HandleHeal;
    }

    private void HandleHeal(int amount, bool wasfullHealth)
    {
        if(wasfullHealth)
        {
            CurrencyManager.Instance.AddCurrency(currencyAmount * amount, popupPrefix: "Excess Heal");
        }
    }
}
