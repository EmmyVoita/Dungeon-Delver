
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
    public int baseCost = 0;
    public int scoreRequirement = 0;

    [Header("Display")]
    public string abilityName;
    [TextArea] public string description;
    public IconData iconData;
    public Sprite cardBackground;
    public float iconScale = 1.0f;


    [Header("Runtime")]
    public GameObject abilityPrefab; // ✅ perfectly fine here
}
