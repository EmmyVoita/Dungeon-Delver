using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Boss/Boss Definition")]
public class BossDefinition : ScriptableObject
{
    [Header("Identity")]
    public string bossName;
    public Sprite bossPortrait;
    public GameObject bossVisualPrefab;

    //[Header("Supported Effects")]
    //public List<BossEffect> supportedEffects;
}

