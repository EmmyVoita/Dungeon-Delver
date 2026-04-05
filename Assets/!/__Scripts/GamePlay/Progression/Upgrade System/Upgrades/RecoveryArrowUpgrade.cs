using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

[CreateAssetMenu(menuName = "Upgrades/Recovery Arrow")]
public class RecoveryArrowUpgrade : UpgradeBase, IActivatableUpgrade
{
    [Header("Trigger Rules")]
    public int rearmCountRequired = 10;
    public int minCountRequired = 5;

    [Header("Reward")]
    public int recoveryArrowsGranted = 1;
    [Range(0,1)] public float recoveryPercentage = 0.5f;
    //public float goldenMultiplier = 10f;

    [Header("Feedback")]
    public SoundEffect activateSound;
    public SoundEffect successSound;
    public SoundEffect failiureSound;
    public GameObject bonusVFX;

    // ------------------------
    // Runtime State
    // ------------------------

    private bool isArmed = true;
    private int rearmHitCounter = 0;
    private int cachedComboCount = 0;
     private float rechargeProgress = 1f;

    // ------------------------
    // Lifecycle
    // ------------------------

    public override string GetDescription()
    {
        return descriptionTemplate
            .Replace("{REARM_COUNT_REQUIRED}", rearmCountRequired.ToString())
            .Replace("{MIN_COUNT_REQUIRED}", minCountRequired.ToString())
            .Replace("{RECOVERY_ARROWS_GRANTED}", recoveryArrowsGranted.ToString())
            .Replace("{RECOVERY_PERCENTAGE}", (recoveryPercentage * 100).ToString("F0") + "%");
    }

    public void Activate()
    {
        rearmHitCounter = 0;
        isArmed = true;

        ArrowBase.OnArrowResolved += HandleArrowResult;
        ComboManager.OnComboBreak += HandleComboBreak;
        AbilityCooldownForCritBonus.OnShouldReduceUpgradeCooldown += HandleReducingCooldown;
    }

    public void Deactivate()
    {
        ArrowBase.OnArrowResolved -= HandleArrowResult;
        ComboManager.OnComboBreak-= HandleComboBreak;
        AbilityCooldownForCritBonus.OnShouldReduceUpgradeCooldown -= HandleReducingCooldown;

        UpgradeManager.Instance.SetUpgradeActive(upgradeId, false);
    }


    private void HandleComboBreak(int comboCount, ComboBreakReason reason)
    {
        Debug.Log($"Recovery Arrow Combo Break: {comboCount} (armed: {isArmed})");
        if (!isArmed || comboCount < minCountRequired)
            return;

        if(reason != ComboBreakReason.Damage && reason != ComboBreakReason.ArrowMiss)
            return;

        cachedComboCount = comboCount;

        TriggerRecoveryArrow();
        Disarm();
    }   

    // ------------------------
    // Core Logic
    // ------------------------
    private void HandleArrowResult(ArrowResolvedData data)
    {
        if (data.goalType == Goal.GoalType.Miss)
        {
            return;
        }

        if(data.status.HasFlag(ArrowStatus.Recovery))
        {
            if(data.goalType == Goal.GoalType.Critical)
            {
                AudioHelpers.PlaySoundEffect(successSound, Camera.main.transform.position);
                int amount = (int)(cachedComboCount * recoveryPercentage);
                ComboManager.Instance.AddHit(amount);
                if(bonusVFX != null)
                {
                    Instantiate(
                        bonusVFX,
                        Player.Instance.transform.position,
                        Quaternion.identity
                    );
                }
                return;
            } 
            else
            {
                AudioHelpers.PlaySoundEffect(failiureSound, Camera.main.transform.position);
                return;
            }
        }

        HandleReducingCooldown();
    }

   
    public void HandleReducingCooldown()
    {
        if (isArmed)
            return;

        float deltaAmount = 1f / rearmCountRequired;

        rechargeProgress = Mathf.Clamp01(rechargeProgress + deltaAmount);

        UpgradeManager.OnUpgradeRechargeProgress?.Invoke(
            upgradeId,
            deltaAmount
        );

        if (rechargeProgress >= 1f)
            Rearm();
    }



    // Activation effects
    private void TriggerRecoveryArrow()
    {
        AudioHelpers.PlaySoundEffect(activateSound, Camera.main.transform.position);

        BuffHelpers.GetOrCreateRecoveryArrow(
            recoveryArrowsGranted
        );

        UpgradeManager.Instance.SetUpgradeActive(upgradeId, true);
    }

    // After we use the buff, we disarm until rearmed
    private void Disarm()
    {
        isArmed = false;
        rechargeProgress = 0f;
        UpgradeManager.OnUpgradeRechargeProgress?.Invoke(upgradeId, -1f);
    }


    private void Rearm()
    {
        isArmed = true;
        rearmHitCounter = 0;
        UpgradeManager.OnUpgradeRechargeProgress?.Invoke(upgradeId, 1f);

        UpgradeManager.Instance.SetUpgradeActive(upgradeId, false);
        Debug.Log("✨ Recovery arrow rearmed");
    }
}
