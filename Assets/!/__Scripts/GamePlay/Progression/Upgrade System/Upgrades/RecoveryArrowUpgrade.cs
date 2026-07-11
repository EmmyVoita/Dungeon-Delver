using System;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

[CreateAssetMenu(menuName = "Upgrades/Recovery Arrow")]
public class RecoveryArrowUpgrade : UpgradeBase
{
   

    [Header("Trigger Rules")]
    [SerializeField] private int minComboRequired = 5;

    [Header("Reward")]
    [SerializeField] private int recoveryArrowsGranted = 1;
    [Range(0,1)] [SerializeField] private float recoveryPercentage = 0.5f;

  
    public int MinComboRequired => minComboRequired;
    public int RecoveryArrowsGranted => recoveryArrowsGranted;
    public float RecoveryPercentage => recoveryPercentage;
    
    public override void Apply()
    {
        Debug.Log("Recovery arrow apply!");
        RecoveryArrowManager.Instance.AddRecoveryArrow(this);
    }

    public override string GetDescription()
    {
        return descriptionTemplate
            .Replace("{MIN_COUNT_REQUIRED}", minComboRequired.ToString())
            .Replace("{RECOVERY_ARROWS_GRANTED}", recoveryArrowsGranted.ToString())
            .Replace("{RECOVERY_PERCENTAGE}", (recoveryPercentage * 100).ToString("F0") + "%");
    }
}
