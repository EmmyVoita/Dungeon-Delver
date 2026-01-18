using UnityEngine;

[CreateAssetMenu(menuName = "Upgrades/High Combo Risk Conversion")]
public class HighComboRiskUpgrade :
    UpgradeBase,
    IActivatableUpgrade,
    IArrowScoreModifier,
    IDamageModifier
{
    // =====================
    // CONFIG
    // =====================

    [Header("Interval Scaling")]
    [Tooltip("Every X combo grants a new tier")]
    public int comboInterval = 3;

    [Tooltip("Maximum number of tiers (prevents infinite scaling)")]
    public int maxIntervals = 5;

    [Header("Reward")]
    [Tooltip("Score bonus per tier (0.25 = +25%)")]
    public float scoreBonusPerInterval = 0.25f;

    [Header("Risk")]
    [Tooltip("Extra damage taken per tier when combo breaks")]
    public int damagePerInterval = 1;

    // =====================
    // STATE
    // =====================

    private int pendingExtraDamage = 0;

    // =====================
    // LIFECYCLE
    // =====================

    public override string GetDescription()
    {
        return descriptionTemplate
            .Replace("{SCORE_BONUS_PER_INTERVAL}", (scoreBonusPerInterval).ToString("P0"))
            .Replace("{COMBO_INTERVAL}", comboInterval.ToString())
            .Replace("{MAX_INTERVALS}", maxIntervals.ToString())
            .Replace("{DAMAGE_PER_INTERVAL}", damagePerInterval.ToString());
    }

    public void Activate()
    {
        ComboManager.OnComboBreakImmediate += HandleComboBroken;
        ComboManager.OnComboUpdated += HandleComboUpdated;
    }

    public void Deactivate()
    {
        ComboManager.OnComboBreakImmediate -= HandleComboBroken;
        ComboManager.OnComboUpdated -= HandleComboUpdated;
    }

    // =====================
    // SCORE MODIFIER
    // =====================

    public float ModifyArrowScore(float current)
    {
        int tier = GetTier(ComboManager.Instance.GetCurrentComboCount);

        if (tier <= 0)
            return current;

        float multiplier = 1f + tier * scoreBonusPerInterval;
        return current * multiplier;
    }

    // =====================
    // DAMAGE MODIFIER
    // =====================

    private void HandleComboBroken(int comboAtBreak, ComboBreakReason reason)
    {
        int tier = GetTier(comboAtBreak);
        pendingExtraDamage = tier * damagePerInterval;

#if UNITY_EDITOR
        Debug.Log($"High Combo Risk: Combo {comboAtBreak}, Tier {tier}, Pending Damage +{pendingExtraDamage}");
#endif
    }

    public int ModifyDamageTaken(int current)
    {
        if (pendingExtraDamage <= 0)
            return current;

        int result = current + pendingExtraDamage;
        pendingExtraDamage = 0; // consume risk

#if UNITY_EDITOR
        Debug.Log($"High Combo Risk: Damage modified {current} → {result}");
#endif

        return result;
    }

    // =====================
    // VISUAL / UI STATE
    // =====================

    private void HandleComboUpdated(int combo)
    {
        UpgradeManager.Instance.SetUpgradeActive(
            upgradeId,
            GetTier(combo) > 0
        );
    }

    // =====================
    // CORE LOGIC
    // =====================

    private int GetTier(int combo)
    {
        if (comboInterval <= 0)
            return 0;

        int tier = combo / comboInterval;
        return Mathf.Clamp(tier, 0, maxIntervals);
    }
}
