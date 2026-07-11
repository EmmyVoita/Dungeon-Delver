using UnityEngine;

[CreateAssetMenu(menuName = "Intermediate Effects/Speed Challenge")]
public class IntermediateSpeedChallenge : UpgradeBase
{
    [SerializeField] private int currencyAmount = 3000;
    [SerializeField] private float modifier;
    private TimeScaleModifier _modifier;

    public override string GetDescription()
    {
        return descriptionTemplate
            .Replace("{SCORE_AMOUNT}", currencyAmount.ToString("N0"))
            .Replace("{TIME_SCALE}", (-(1-modifier)).ToString("P0"));
    }

    private void OnDisable()
    {
        GameStateManager.OnStateChanged -= HandleStateChanged;
    }

    private void HandleStateChanged(GameState previousState, GameState newState)
    {
        if(newState == GameState.RoundActive)
        {
            _modifier = new TimeScaleModifier("speedChallenge", modifier);
            TimeManager.Instance.AddModifier(_modifier);
        }
        else if(newState == GameState.WorldMapView)
        {
            TimeManager.Instance.RemoveModifier(_modifier.Id);
            GameStateManager.OnStateChanged -= HandleStateChanged;
        }
    }

    public override void Apply()
    {
        CurrencyManager.Instance.AddCurrency(currencyAmount);
        GameStateManager.OnStateChanged += HandleStateChanged;
    }
}
