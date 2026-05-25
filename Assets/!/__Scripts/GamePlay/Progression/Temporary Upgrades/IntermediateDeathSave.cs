using UnityEngine;

[CreateAssetMenu(menuName = "Intermediate Effects/Death Save")]
public class IntermediateDeathSave : UpgradeBase
{
    public int immediateHealAmount = 4;

    [SerializeField] private ScreenShakeRequest screenShakeData;
    [SerializeField] private SoundEffect procSoundEffect;
    [SerializeField] private GameObject procEffect;

    
    private void HandleStateChanged(GameState previousState, GameState newState)
    {
        if(newState == GameState.UpgradeSelection && previousState != newState)
        {
            //Debug.Log("HEALING PLAYER");
            //Player.Instance.HealPlayer(finishHealAmount);
            GameStateManager.OnStateChanged -= HandleStateChanged;
        }
    }

    public override string GetDescription()
    {
        return descriptionTemplate
            .Replace("{HEAL_AMOUNT}", immediateHealAmount.ToString("N0"));
    }

    public override void Apply()
    {
        GameStateManager.OnStateChanged += HandleStateChanged;
        Player.OnPreDamageTaken += HandleDamageTaken;
    }

    private void  HandleDamageTaken(int damage)
    {
        if(Player.Instance.Health - damage <= 0)
        {
            AudioHelpers.PlaySoundEffect(procSoundEffect, Player.Instance.transform.position);

            /*
            ScreenShakeRequest ssRequest = new ScreenShakeRequest(duration: 1.0f,
                                                                magnitude: 0.1f,
                                                                direction: Vector2.up,
                                                                directional: true,
                                                                unscaled: true);
            */
            
            ScreenShakeManager.Instance.Shake(screenShakeData);

            Instantiate(procEffect, Player.Instance.transform.position,Quaternion.identity);

            Player.Instance.HealPlayer(immediateHealAmount);
            Player.OnPreDamageTaken -= HandleDamageTaken;
        }
    }
}
