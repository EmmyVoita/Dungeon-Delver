using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;


public class StatModifierManager : MonoBehaviour
{
    public static StatModifierManager Instance;


    public static event Action<string, bool> OnUpgradeStateChanged;
    public static Action<string, float> OnUpgradeRechargeProgress;
    private readonly List<object> temporaryModifiers = new();


    private readonly List<IAbilityCostModifier> abilityCostMods = new();
    private readonly List<IDamageModifier> damageMods = new();


    private void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        GameStateManager.OnStateChanged += HandleStateChanged;
    }

    private void OnDisable()
    {
        GameStateManager.OnStateChanged -= HandleStateChanged;
    }
 
    private void HandleStateChanged(GameState previousState, GameState newState)
    {
        if (newState == GameStateManager.LevelEndState && (temporaryModifiers.Count > 0))
        {
            ClearTemporaryModifiers();
        }
    }

    public void AddTemporaryModifier(object modifier)
    {
        if (modifier == null)
            return;

        RegisterModifier(modifier);
        temporaryModifiers.Add(modifier);
    }

    public void RemoveTemporaryModifier(object modifier)
    {
        if (modifier == null) return;

        UnregisterModifier(modifier);
        temporaryModifiers.Remove(modifier);
    }

    public void ClearTemporaryModifiers()
    {
        if (temporaryModifiers.Count == 0)
            return;

        foreach (var mod in temporaryModifiers)
            UnregisterModifier(mod);

        temporaryModifiers.Clear();
    }


    private void RegisterModifier(object modifier)
    {
        if (modifier is IAbilityCostModifier acm) abilityCostMods.Add(acm);
        if (modifier is IDamageModifier dm) damageMods.Add(dm);
    }

    private void UnregisterModifier(object modifier)
    {
        if (modifier is IAbilityCostModifier acm) abilityCostMods.Remove(acm);
        if (modifier is IDamageModifier dm) damageMods.Remove(dm);
    }


    public float ModifyAbilityCost(float baseCost)
    {
        float cost = baseCost;
        foreach (var mod in abilityCostMods)
            cost = mod.ModifyCost(cost);
        return cost;
    }

    public int ModifyDamageTaken(int baseDamage)
    {
        int damage = baseDamage;
        foreach (var mod in damageMods)
            damage = mod.ModifyDamageTaken(damage);
        return damage;
    }
}
