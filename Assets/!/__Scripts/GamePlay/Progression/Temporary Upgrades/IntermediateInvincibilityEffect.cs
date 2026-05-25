using UnityEngine;

[CreateAssetMenu(menuName = "Intermediate Effects/InvincibilityEffect")]
public class IntermediateInvincibilityEffect : UpgradeBase
{
    public float invincibleDuration = 3f;
    
    public override string GetDescription()
    {
        return descriptionTemplate
            .Replace("{INVINCIBLE_DURATION}", invincibleDuration.ToString("N0"));
    }

    public override void Apply()
    {
        Player.OnAbilityUsed += HandleAbilityUsed;
        GameStateManager.OnStateChanged += HandleStateChanged;
        //Player.Instance.AbilityCharge = Player.Instance.MaxAbilityCharge;
    }

    private void HandleStateChanged(GameState previousState, GameState newState)
    {
        if(newState == GameState.UpgradeSelection && previousState != newState)
        {
            Player.OnAbilityUsed -= HandleAbilityUsed;
            GameStateManager.OnStateChanged -= HandleStateChanged;
        }
    }

    private void HandleAbilityUsed()
    {
        Player.Instance.SetInvincible(invincibleDuration);
    }
}
