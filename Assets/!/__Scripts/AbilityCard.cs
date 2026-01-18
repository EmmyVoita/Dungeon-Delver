using UnityEngine;

[CreateAssetMenu(menuName = "Ability/Card", fileName = "NewAbilityCard")]
public class AbilityCard : ScriptableObject
{
    public int scoreRequirement = 0;
    public string abilityName;
    [TextArea] public string description;
    public Sprite icon;
    public AbilityType abilityType; // same enum as before (SlowTime, Shield, Projectile)

    [Header("Visual Settings")]
    public Material cardMaterial; // 👈 assign different materials here
}
