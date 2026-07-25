
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(menuName = "Ability/Card", fileName = "NewAbilityCard")]
public class AbilityData : ScriptableObject
{
    [Header("Identity")]
    public AbilityType abilityType;

    [Header("Gameplay")]
    public int baseCost;
    public int scoreRequirement;

    [Tooltip("Used only by abilities that have a duration.")]
    [Min(0f)]
    public float baseDuration;

    [Header("Display")]
    public string abilityName;
    [TextArea] public string description;
    public IconData iconData;
    public Sprite cardBackground;
    public float iconScale = 1f;

    [Header("Audio")]
    public SoundEffect activationSound;
    public SoundEffect deactivateSound;

    [Header("Runtime")]
    public AbilityBase abilityPrefab;
}