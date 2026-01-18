using System.Collections;
using UnityEngine;

public class HealOnFullAbilityBar : UpgradeEffectBase
{
    public int healthAmount = 1;
    [SerializeField] private AudioClip itemActivationSound;

    void OnEnable()
    {
        Player.OnAbilityFilled += HandleAbilityFilled;
    }

    void OnDisable()
    {
        Player.OnAbilityFilled -= HandleAbilityFilled;
    }

    private void HandleAbilityFilled()
    {
        Debug.Log("💫 Heal On Full Ability Bar Activated!");
        Player.Instance.HealPlayer(healthAmount);
    }
    

    public override void Apply(Player player)
    {
        //RoundManager.Instance.ApplyTempBPMBonus(bpmBonus);
    }
}
