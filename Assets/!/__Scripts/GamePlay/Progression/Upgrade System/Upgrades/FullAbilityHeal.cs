using UnityEngine;


[CreateAssetMenu(menuName = "Upgrades/Full Ability Heal")]
public class FullAbilityHeal : UpgradeBase, IAbilityCostModifier
{
    [SerializeField] private int healAmount = 1;
    [SerializeField] private float abilityCostMult = 1.5f;
    [SerializeField] private bool infiniteDuration = true;



    private void OnDisable()
    {
        GameStateManager.OnStateChanged -= HandleStateChanged;
        Player.OnAbilityChargeChanged -= HandleChargeChanged;
    }

    private void HandleStateChanged(GameState previousState, GameState newState)
    {
        if(newState == GameState.WorldMapView && !infiniteDuration)
            Deactivate();
    }

    public override string GetDescription()
    {
        return descriptionTemplate
            .Replace("{HEAL_VALUE}", healAmount.ToString())
            .Replace("{ABILITY_COST_MULT}", (abilityCostMult-1).ToString("P0"));
    }

    public override void Apply()
    {
        Player.OnAbilityChargeChanged -= HandleChargeChanged;
        Player.OnAbilityChargeChanged += HandleChargeChanged;

        GameStateManager.OnStateChanged -= HandleStateChanged;

        if(!infiniteDuration)
            GameStateManager.OnStateChanged += HandleStateChanged;

        FullAbilityBarBonusIndicator.RequestStartAnimateOutline();

        UpgradeManager.Instance.AddUpgrade(this);
    }

    public void Deactivate()
    {
        Player.OnAbilityChargeChanged -= HandleChargeChanged;
        FullAbilityBarBonusIndicator.RequestStopAnimateOutline();
    }

    private void HandleChargeChanged(int previousCharge, int attemptedDelta, int appliedDelta)
    {
        if(Player.Instance.AbilityCharge == Player.Instance.MaxAbilityCharge && appliedDelta > 0)
        {
            Player.Instance.HealPlayer(healAmount);
        }
    }

    public float ModifyCost(float baseCost)
    {
        return baseCost * abilityCostMult;
    }
}
