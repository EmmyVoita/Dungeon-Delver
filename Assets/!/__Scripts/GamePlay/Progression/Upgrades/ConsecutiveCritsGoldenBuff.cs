using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

[CreateAssetMenu(menuName = "Upgrades/Consecutive Crits Golden Buff")]
public class ConsecutiveCritsGoldenBuff : UpgradeBase, IActivatableUpgrade
{
    [Header("Trigger Rules")]
    public int critsInARowRequired = 5;
    public int rearmCountRequired = 10;

    [Header("Reward")]
    public int goldenArrowsGranted = 3;
    //public float goldenMultiplier = 10f;

    [Header("Feedback")]
    public SoundEffect bonusSound;
    public GameObject bonusVFX;

    // ------------------------
    // Runtime State
    // ------------------------
    private int critCounter = 0;
    private bool isArmed = true;
    private int rearmHitCounter = 0;
    private float rechargeProgress = 0f;

    // ------------------------
    // Lifecycle
    // ------------------------

    public override string GetDescription()
    {
        return descriptionTemplate
            .Replace("{CRITS_IN_A_ROW}", critsInARowRequired.ToString())
            .Replace("{REARM_COUNT}", rearmCountRequired.ToString())
            .Replace("{GOLDEN_GRANTED}", goldenArrowsGranted.ToString());
    }


    public void Activate()
    {
        critCounter = 0;
        rearmHitCounter = 0;
        isArmed = true;

        ArrowBase.OnArrowResolved += HandleArrowResult;
        AbilityCooldownForCritBonus.OnShouldReduceUpgradeCooldown += HandleReducingCooldown;
    }

    public void Deactivate()
    {
        ArrowBase.OnArrowResolved -= HandleArrowResult;
        AbilityCooldownForCritBonus.OnShouldReduceUpgradeCooldown -= HandleReducingCooldown;
        //UpgradeManager.Instance.SetUpgradeActive(upgradeId, false);
    }

    // ------------------------
    // Core Logic
    // ------------------------
    private void HandleArrowResult(ArrowResolvedData data)
    {
        if(data.goalType == Goal.GoalType.Miss) return;
        
        // If we hit a crit, increment crit counter, otherwise reset it. This covers both non-crits and misses.
        if (data.goalType == Goal.GoalType.Critical)
        {
            critCounter++;

            // Trigger only if armed
            if (isArmed && critCounter >= critsInARowRequired)
            {
                TriggerGoldenBuff();
                Disarm();
                return;
            }
        }
        else
        {
            critCounter = 0;
        }

        HandleReducingCooldown();
    }

    public void HandleReducingCooldown()
    {
        // If already armed, do nothing
        if (isArmed)
            return;

        // This is logic for the ui
        float deltaAmount = 1f / rearmCountRequired;

        rechargeProgress = Mathf.Clamp01(rechargeProgress + deltaAmount);

        //UpgradeManager.OnUpgradeRechargeProgress?.Invoke(
         //   upgradeId,
           // deltaAmount
        //);

        // we also care about the recharge progress reaching full
        if (rechargeProgress >= 1f)
            Rearm();
    }


    // Activation effects
    private void TriggerGoldenBuff()
    {
        BuffHelpers.OnGoldenArrowSessionStarted?.Invoke();

        AudioHelpers.PlaySoundEffect(bonusSound, Camera.main.transform.position);

        BuffHelpers.GetOrCreateGoldenEffect(
            goldenArrowsGranted
        );

        if(bonusVFX != null)
        {
            Instantiate(
                bonusVFX,
                Player.Instance.transform.position,
                Quaternion.identity
            );
        }

        //UpgradeManager.Instance.SetUpgradeActive(upgradeId, true);
    }

    // After we use the buff, we disarm until rearmed
    private void Disarm()
    {
        // we reset all of our counters
        isArmed = false;
        rechargeProgress = 0f;
        critCounter = 0;
        //UpgradeManager.OnUpgradeRechargeProgress?.Invoke(upgradeId, -1f);
    }

    private void Rearm()
    {
        isArmed = true;
        rearmHitCounter = 0;
        //UpgradeManager.OnUpgradeRechargeProgress?.Invoke(upgradeId, 1f);

        //UpgradeManager.Instance.SetUpgradeActive(upgradeId, false);
        Debug.Log("✨ Recovery arrow rearmed");
    }
}
