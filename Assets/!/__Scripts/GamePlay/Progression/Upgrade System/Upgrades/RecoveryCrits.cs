using UnityEngine;


[CreateAssetMenu(menuName = "Upgrades/Recovery Crits")]
public class RecoveryCrits : UpgradeBase, IActivatableUpgrade
{
    public int abilityChargeBonus = 3;
    public SoundEffect bonusSound;
    public int streakRequired = 3;
    private bool isActive = false;
    private int currentStreak = 0;

    public override string GetDescription()
    {
        return descriptionTemplate
            .Replace("{ABILITY_CHARGE_BONUS}", abilityChargeBonus.ToString())
            .Replace("{STREAK_REQUIRED}", streakRequired.ToString());
    }

    public void Activate()
    {
        Player.OnDamageTaken += HandlePlayerDamaged;
        ArrowBase.OnArrowResolved += HandleArrowResult;
        GameStateManager.OnStateChanged += HandleGameStateChanged;
    }

    public void Deactivate()
    {
        Player.OnDamageTaken -= HandlePlayerDamaged;
        ArrowBase.OnArrowResolved -= HandleArrowResult;
        GameStateManager.OnStateChanged -= HandleGameStateChanged;
        UpgradeManager.Instance.SetUpgradeActive(upgradeId, false);
    }

    private void HandleGameStateChanged(GameState previousGameState, GameState newState)
    {
        if(newState != GameState.RoundActive)
        {
            isActive = false;
            currentStreak = 0;
        }
    }

    private void HandlePlayerDamaged(int damage)
    {
        currentStreak = 0;
        isActive = true;
    }


    private void HandleArrowResult(ArrowResolvedData data)
    {
        if(!isActive) return;

        if (data.goalType == Goal.GoalType.Critical)
        {
            currentStreak++;
        }
        else
        {
            isActive = false;
        }

        if(currentStreak >= streakRequired)
        {
            UpgradeManager.Instance.SetUpgradeActive(upgradeId, true);

            AudioHelpers.PlaySoundEffect(bonusSound, Camera.main.transform.position);

            UpgradeManager.Instance.SetUpgradeActive(upgradeId, true);
            Player.Instance.AbilityCharge += abilityChargeBonus;

            isActive = false;
        }
        else
        {
            UpgradeManager.Instance.SetUpgradeActive(upgradeId, false);
        }
    }


}