using UnityEngine;

[CreateAssetMenu(menuName = "Intermediate Effects/Prevent Next Combo Break")]
public class IntermediatePreventComboBreak : UpgradeBase
{
    [SerializeField] private int hitBlockCharges = 1;
    private void OnDisable()
    {
        //GameStateManager.OnStateChanged -= HandleStateChanged;
    }

    /*
    private void HandleStateChanged(GameState previousState, GameState newState)
    {
        if(newState == GameState.RoundActive)
        {
            ComboManager.Instance.PreventNextComboBreak();
            GameStateManager.OnStateChanged -= HandleStateChanged;
            Player.Instance.AddHitBlock(hitBlockCharges);
        }
    }
    */

    public override void Apply()
    {
        DamageSaveManager.Instance.Register(new ShieldDamageSave(true));
        ComboManager.Instance.PreventNextComboBreak();
    }
}
