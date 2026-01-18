using UnityEngine;


[CreateAssetMenu(menuName = "Upgrades/Nth Crit Bonus")]
public class NthCritBonus : UpgradeBase, IActivatableUpgrade
{
    public int critsRequired = 5;
    public int bonusScore = 250;

    private int critCounter;

    public SoundEffect bonusSound;

    public override string GetDescription()
    {
        return descriptionTemplate
            .Replace("{CRITS_REQUIRED}", critsRequired.ToString())
            .Replace("{BONUS_SCORE}", bonusScore.ToString());
    }

    public void Activate()
    {
        critCounter = 0;
        ArrowBase.OnArrowResolved += HandleCrit;
        ComboManager.OnComboBreak += ResetCounter;
    }

    public void Deactivate()
    {
        ArrowBase.OnArrowResolved -= HandleCrit;
        ComboManager.OnComboBreak -= ResetCounter;
        UpgradeManager.Instance.SetUpgradeActive(upgradeId, false);
    }

    private void HandleCrit(ArrowResolvedData data)
    {
        if(data.goalType != Goal.GoalType.Critical)
            return;
        
        critCounter++;
        Debug.Log($"Crit counter: {critCounter}/{critsRequired}");

        if (critCounter >= critsRequired)
        {
            AudioHelpers.PlaySoundEffect(bonusSound, Camera.main.transform.position);
            
            int finalAddedScore = ScoreManager.Instance.AddScore(bonusScore, ScoreSource.Bonus);

            ScoreEvents.OnScorePopupRequested?.Invoke(
                finalAddedScore,
                ScorePopupKind.AbilityOverflow
            );

            critCounter = 0;

            UpgradeManager.Instance.SetUpgradeActive(upgradeId, true);
        }
        else
        {
            UpgradeManager.Instance.SetUpgradeActive(upgradeId, false);
        }
    }

    private void ResetCounter(int comboBreakAt, ComboBreakReason reason)
    {
        critCounter = 0;
        UpgradeManager.Instance.SetUpgradeActive(upgradeId, false);
    }
}
