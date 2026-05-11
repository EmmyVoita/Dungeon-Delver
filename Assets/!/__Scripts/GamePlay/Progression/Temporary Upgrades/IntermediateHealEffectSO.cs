using UnityEngine;

[CreateAssetMenu(menuName = "Intermediate Effects/Heal")]
public class IntermediateHealEffectSO : UpgradeBase
{
    public int immediateHealAmount = 1;
    public int finishHealAmount = 2;


    private void HandleStateChanged(GameState previousState, GameState newState)
    {
        if(newState == GameState.UpgradeSelection && previousState != newState)
        {
            Debug.Log("HEALING PLAYER");
            Player.Instance.HealPlayer(finishHealAmount);
            GameStateManager.OnStateChanged -= HandleStateChanged;
        }
    }

    public override string GetDescription()
    {
        return descriptionTemplate
            .Replace("{HEAL_AMOUNT}", immediateHealAmount.ToString("N0"))
            .Replace("{FINISH_HEAL_AMOUNT}", finishHealAmount.ToString("N0"));
    }

    public override void Apply()
    {
        Player.Instance.HealPlayer(immediateHealAmount);
        GameStateManager.OnStateChanged += HandleStateChanged;
    }
}
