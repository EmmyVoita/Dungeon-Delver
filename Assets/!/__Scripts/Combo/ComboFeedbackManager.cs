using System.Collections.Generic;
using UnityEngine;

public class ComboFeedbackManager : MonoBehaviour
{
    [SerializeReference] private List<ComboEffect> comboEffects;
    [SerializeReference] private List<ComboEffect> comboBreakEffects;

    private void OnEnable()
    {
        ComboManager.OnComboUpdated += HandleComboUpdated;
        ComboManager.OnComboBreak += HandleComboBreak;
    }

    private void OnDisable()
    {
        ComboManager.OnComboUpdated -= HandleComboUpdated;
        ComboManager.OnComboBreak -= HandleComboBreak;
    }

    private void Awake()
    {
        foreach(var comboEffect in comboEffects)
        {
            comboEffect.Initialize();
        }
    }

    private void HandleComboUpdated(int newCombo)
    {
        if(newCombo == 0)
            return;
            
        PullComboEffects(comboEffects, newCombo);
    }

    private void HandleComboBreak(int combo, ComboBreakReason reason)
    {
        if(reason != ComboBreakReason.RoundEnd)
            PullComboEffects(comboBreakEffects, combo);
    }

    private void PullComboEffects(List<ComboEffect> effects, int newCombo)
    {
        foreach(var comboEffect in effects)
        {
            bool shouldExecute = comboEffect.ShouldTrigger(newCombo);
            if(shouldExecute)
                comboEffect.Execute(newCombo);
        }
    }
}