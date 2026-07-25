using UnityEngine;


[CreateAssetMenu(menuName = "Upgrades/Ability Overflow To Score")]
public class AbilityOverflowToScore : UpgradeBase, IActivatableUpgrade
{
    public int scorePerOverflow = 500;
    public SoundEffect bonusSound;

    public override string GetDescription()
    {
        return descriptionTemplate
            .Replace("{SCORE_PER_OVERFLOW}", scorePerOverflow.ToString("N0"));
    }

    public void Activate()
    {
        Player.OnAbilityChargeChanged += OnChargeChanged;
    }

    public void Deactivate()
    {
        Player.OnAbilityChargeChanged -= OnChargeChanged;
        //UpgradeManager.Instance.SetUpgradeActive(upgradeId, false);
    }

    private void OnChargeChanged(int previousCharge, int attemptedDelta, int appliedDelta)
    {
        int max = Player.Instance.MaxAbilityCharge;

        if (previousCharge == max && attemptedDelta > 0)
        {
            int overflow = attemptedDelta;
            int score = overflow * scorePerOverflow;
            int finalAddedScore = ScoreManager.Instance.AddScore(score, ScoreSource.Bonus);
            
            ScoreEvents.OnScorePopupRequested?.Invoke(
                finalAddedScore,
                ScorePopupKind.AbilityOverflow
            );
            //ScoreDisplayView.Instance.SpawnScorePopup(score, ScoreDisplayView.Instance.abilityOverflowPopup);

            AudioHelpers.PlaySoundEffect(bonusSound, Camera.main.transform.position);

            //UpgradeManager.Instance.SetUpgradeActive(upgradeId, true);
        }
        else
        {
            //UpgradeManager.Instance.SetUpgradeActive(upgradeId, false);
        }
    }
}
