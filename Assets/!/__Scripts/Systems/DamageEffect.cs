using UnityEngine;

public class DamageEffect : MonoBehaviour
{
    [Header("Damage")]
    public int damage = 1;
    public int abilityChargeDamage = 0;

    [Header("Death Info")]
    public string sourceName = "Unknown";

    [Header("Audio")]
    public bool playHitSound = true;
}