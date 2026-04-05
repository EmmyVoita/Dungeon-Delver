using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

public struct LiveScoreState
{
    public float NormalArrowTotalModifier;
    public float CritArrowTotalModifier;
    public float ComboTotalModifier;

    public LiveScoreState(float normal, float crit, float combo)
    {
        NormalArrowTotalModifier = normal;
        CritArrowTotalModifier = crit;
        ComboTotalModifier = combo;
    }
}

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance;

    // -------------------------
    // EVENTS
    // -------------------------
    public static event Action<string, bool> OnUpgradeStateChanged;
    public static Action<string, float> OnUpgradeRechargeProgress;
    public static event Action<LiveScoreState> OnScoreContextChanged;

    // -------------------------
    // PERMANENT UPGRADES
    // -------------------------
    private readonly List<UpgradeBase> activeUpgrades = new();

    // -------------------------
    // TEMPORARY (ROUND-SCOPED) MODIFIERS
    // -------------------------
    private readonly List<object> temporaryModifiers = new();

    // -------------------------
    // MODIFIER BUCKETS
    // -------------------------
    private readonly List<ICritWindowModifier> critWindowMods = new();
    private readonly List<IArrowStatusScoreModifier> statusScoreMods = new();
    private readonly List<IAbilityCostModifier> abilityCostMods = new();
    private readonly List<ICritHitValueModifier> critValueMods = new();
    private readonly List<INormalHitValueModifier> normalValueMods = new();
    private readonly List<IComboScoreMultiplier> comboScoreMods = new();
    private readonly List<IDamageModifier> damageMods = new();
    private readonly List<IActivatableUpgrade> activatables = new();
    private readonly List<ICritBaseOverride> critBaseOverrides = new();
    private readonly List<IGlobalScoreMultiplier> globalScoreMultipliers = new();
    private readonly List<IArrowScoreModifier> arrowScoreMods = new();
    private readonly List<IGoalSizeModifier> goalSizeMods = new();

    private LiveScoreState lastState;

    public IReadOnlyList<IArrowStatusScoreModifier> StatusScoreModifiers
    => statusScoreMods;




    // -------------------------
    // UNITY LIFECYCLE
    // -------------------------
    private void Awake()
    {
        Instance = this;
        RecomputeScoreContext();
    }

    private void OnEnable()
    {
        GameStateManager.OnStateChanged += HandleStateChanged;
    }

    private void OnDisable()
    {
        GameStateManager.OnStateChanged -= HandleStateChanged;
    }

    // -------------------------
    // PERMANENT UPGRADE API
    // -------------------------
    public void AddUpgrade(UpgradeBase upgrade)
    {
        if (upgrade == null)
            return;

        Debug.Log($"➕ Adding Upgrade: {upgrade.name}");

        activeUpgrades.Add(upgrade);
        RegisterModifier(upgrade);

        // Default to inactive until something enables it
        OnUpgradeStateChanged?.Invoke(upgrade.upgradeId, false);

        RecomputeScoreContext();
    }

    public void RemoveUpgrade(UpgradeBase upgrade)
    {
        if (upgrade == null)
            return;

        OnUpgradeStateChanged?.Invoke(upgrade.upgradeId, false); // 🔥 ADD THIS

        UnregisterModifier(upgrade);
        activeUpgrades.Remove(upgrade);

        RecomputeScoreContext();
    }

    public void ClearUpgrades()
    {
        Debug.Log("🧹 Clearing Upgrades");

        if (activeUpgrades.Count == 0)
            return;

        for (int i = activeUpgrades.Count - 1; i >= 0; i--)
        {
            RemoveUpgrade(activeUpgrades[i]);
        }

        RecomputeScoreContext();
    }

    // -------------------------
    // TEMPORARY MODIFIER API
    // -------------------------

    private void HandleStateChanged(GameState previousState, GameState newState)
    {
        if (newState == GameState.UpgradeSelection && (temporaryModifiers.Count > 0 || activeUpgrades.Count > 0))
        {
            ClearTemporaryModifiers();
            ClearUpgrades();
        }
    }

    public void AddTemporaryModifier(object modifier)
    {
        if (modifier == null)
            return;

        RegisterModifier(modifier);
        temporaryModifiers.Add(modifier);

        RecomputeScoreContext();
    }

    public void RemoveTemporaryModifier(object modifier)
    {
        if (modifier == null) return;

        UnregisterModifier(modifier);
        temporaryModifiers.Remove(modifier);

        RecomputeScoreContext();
    }

    public void ClearTemporaryModifiers()
    {
        Debug.Log("🧹 Clearing Temporary Modifiers");
        if (temporaryModifiers.Count == 0)
            return;

        foreach (var mod in temporaryModifiers)
            UnregisterModifier(mod);

        temporaryModifiers.Clear();
        RecomputeScoreContext();
    }

    // -------------------------
    // UPGRADE ACTIVE STATE (UI / FEEDBACK)
    // -------------------------
    public void SetUpgradeActive(string upgradeId, bool active)
    {
        OnUpgradeStateChanged?.Invoke(upgradeId, active);
        RecomputeScoreContext();
    }

    // -------------------------
    // REGISTRATION CORE
    // -------------------------
    private void RegisterModifier(object modifier)
    {
        if (modifier is ICritWindowModifier cwm) critWindowMods.Add(cwm);
        if (modifier is IAbilityCostModifier acm) abilityCostMods.Add(acm);
        if (modifier is ICritHitValueModifier chv) critValueMods.Add(chv);
        if (modifier is INormalHitValueModifier nhv) normalValueMods.Add(nhv);
        if (modifier is IComboScoreMultiplier csm) comboScoreMods.Add(csm);
        if (modifier is IDamageModifier dm) damageMods.Add(dm);
        if (modifier is ICritBaseOverride cbo) critBaseOverrides.Add(cbo);
        if (modifier is IGlobalScoreMultiplier gsm) globalScoreMultipliers.Add(gsm);
        if (modifier is IArrowScoreModifier asm) arrowScoreMods.Add(asm);
        if (modifier is IArrowStatusScoreModifier ssm)  statusScoreMods.Add(ssm);
        if (modifier is IGoalSizeModifier gszm)  goalSizeMods.Add(gszm);

         if (modifier is ICritWindowModifier cw_)
        {
            Debug.Log($"✅ Registered ICritWindowModifier: {modifier}");
        }


        if (modifier is IActivatableUpgrade act)
        {
            activatables.Add(act);
            act.Activate();
        }
    }

    private void UnregisterModifier(object modifier)
    {
        if (modifier is ICritWindowModifier cwm) critWindowMods.Remove(cwm);
        if (modifier is IAbilityCostModifier acm) abilityCostMods.Remove(acm);
        if (modifier is ICritHitValueModifier chv) critValueMods.Remove(chv);
        if (modifier is INormalHitValueModifier nhv) normalValueMods.Remove(nhv);
        if (modifier is IComboScoreMultiplier csm) comboScoreMods.Remove(csm);
        if (modifier is IDamageModifier dm) damageMods.Remove(dm);
        if (modifier is ICritBaseOverride cbo) critBaseOverrides.Remove(cbo);
        if (modifier is IGlobalScoreMultiplier gsm) globalScoreMultipliers.Remove(gsm);
        if (modifier is IArrowScoreModifier asm) arrowScoreMods.Remove(asm);
        if (modifier is IArrowStatusScoreModifier ssm)  statusScoreMods.Remove(ssm);
        if (modifier is IGoalSizeModifier gszm)  goalSizeMods.Remove(gszm);
   


        if (modifier is IActivatableUpgrade act)
        {
            act.Deactivate();
            activatables.Remove(act);
        }
    }

    // -------------------------
    // SCORE CONTEXT
    // -------------------------
    public void RecomputeScoreContext()
    {
        var newState = BuildLiveScoreState();

        if (!newState.Equals(lastState))
        {
            lastState = newState;
            OnScoreContextChanged?.Invoke(newState);
        }
    }

    private LiveScoreState BuildLiveScoreState()
    {
        float normal = ModifyNormalHitValue(1f);
        normal = ModifyArrowScore(normal);
        normal = ModifyGlobalScoreMultiplier(normal);

        float crit = ModifyCritHitValue(1f);
        crit = ModifyArrowScore(crit);
        crit = ModifyGlobalScoreMultiplier(crit);

        float combo = ModifyComboScoreMultiplier(1f);
        combo = ModifyGlobalScoreMultiplier(combo);

        return new LiveScoreState(normal, crit, combo);
    }

    // -------------------------
    // MODIFIER AGGREGATION
    // -------------------------
    public float ModifyCritBase(float baseValue)
    {
        float value = baseValue;
        foreach (var mod in critBaseOverrides)
            value = mod.ModifyCritBase(value);
        return value;
    }

    public float ModifyCritWindow(float baseValue)
    {
        float value = baseValue;
        foreach (var mod in critWindowMods)
            value = mod.ModifyCritWindow(value);
        return value;
    }

    public float ModifyAbilityCost(float baseCost)
    {
        float cost = baseCost;
        foreach (var mod in abilityCostMods)
            cost = mod.ModifyCost(cost);
        return cost;
    }

    public float ModifyCritHitValue(float baseValue)
    {
        float value = baseValue;
        foreach (var mod in critValueMods)
            value = mod.ModifyCritHitValue(value);
        return value;
    }

    public float ModifyNormalHitValue(float baseValue)
    {
        float value = baseValue;
        foreach (var mod in normalValueMods)
            value = mod.ModifyNormalHitValue(value);
        return value;
    }

    public float ModifyComboScoreMultiplier(float baseValue)
    {
        float value = baseValue;
        foreach (var mod in comboScoreMods)
            value = mod.ModifyComboScoreMultiplier(value);
        return value;
    }

    public int ModifyDamageTaken(int baseDamage)
    {
        int damage = baseDamage;
        foreach (var mod in damageMods)
            damage = mod.ModifyDamageTaken(damage);
        return damage;
    }

    public float ModifyGlobalScoreMultiplier(float baseMultiplier)
    {
        float multiplier = baseMultiplier;
        foreach (var mod in globalScoreMultipliers)
            multiplier = mod.ModifyGlobalScore(multiplier);
        return multiplier;
    }

    public float ModifyArrowScore(float baseScore)
    {
        float value = baseScore;
        foreach (var mod in arrowScoreMods)
            value = mod.ModifyArrowScore(value);
        return value;
    }

    public float ModifyGoalSize(float baseSize)
    {
        float value = baseSize;
        foreach (var mod in goalSizeMods)
            value = mod.ModifyGoalSize(value);
        return value;
    }
}
