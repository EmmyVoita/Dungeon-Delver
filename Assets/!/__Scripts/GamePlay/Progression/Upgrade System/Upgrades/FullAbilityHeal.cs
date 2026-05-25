using UnityEngine;


[CreateAssetMenu(menuName = "Upgrades/Full Ability Heal")]
public class FullAbilityHeal : UpgradeBase
{
    public int healAmount = 1;

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
        if(newState == GameState.UpgradeSelection)
        {
            Deactivate();
        }
    }

    public override string GetDescription()
    {
        return descriptionTemplate
            .Replace("{HEAL_VALUE}", healAmount.ToString());
    }

    public override void Apply()
    {
        Player.OnAbilityChargeChanged += HandleChargeChanged;
    }

    public void Deactivate()
    {
        Player.OnAbilityChargeChanged -= HandleChargeChanged;
    }

    private void HandleChargeChanged(int previousCharge, int attemptedDelta, int appliedDelta)
    {
        if(Player.Instance.AbilityCharge == Player.Instance.MaxAbilityCharge && appliedDelta > 0)
        {
            Player.Instance.HealPlayer(healAmount);
        }
    }
}
