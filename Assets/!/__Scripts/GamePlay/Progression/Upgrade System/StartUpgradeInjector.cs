using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class StartupUpgradeInjector : MonoBehaviour
{
    [Header("Debug / Testing")]
    public bool enableOnStart = true;

    [Header("Upgrades to Grant on Spawn")]
    public List<UpgradeCard> startingUpgradeCards;
    public List<UpgradeBase> startingIntermediateEffects;

    private void Start()
    {
        StartCoroutine(StartSetup());
    }

    private IEnumerator StartSetup()
    {
        if (!enableOnStart)
            yield break;

        // Wait ONE frame so everything initializes & subscribes
        yield return null;

        ApplyStartingUpgrades();

        // Force a final recompute AFTER all modifiers are registered
        //UpgradeManager.Instance.RecomputeScoreContext();
    }


    private void ApplyStartingUpgrades()
    {
        var cardManager = UpgradeCardManager.Instance;

        if (cardManager == null)
        {
            Debug.LogWarning("StartupUpgradeInjector: No UpgradeCardManager found.");
            return;
        }

        // --- Normal upgrade cards (with UI) ---
        foreach (var card in startingUpgradeCards)
        {
            if (card == null || card.upgrade == null)
                continue;

            Debug.Log($"[StartupUpgradeInjector] Granting upgrade + UI: {card.name}");

            //cardManager.GrantUpgradeWithUI(card);
        }

        // --- Intermediate effects (usually no UI icons) ---
        foreach (var effect in startingIntermediateEffects)
        {
            if (effect == null)
                continue;

            Debug.Log($"[StartupUpgradeInjector] Applying intermediate effect: {effect.name}");
            effect.Apply();
        }

        
    }

    void Update()
    {
    
    }
}
