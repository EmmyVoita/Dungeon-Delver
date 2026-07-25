using System;
using UnityEngine;

[Serializable]
public class RewardDefinition
{
    [SerializeField] private RewardType rewardType;
    [SerializeField] private int amount;
    [SerializeField] private Sprite icon;
    [SerializeField] private SoundEffect winSound;

    [SerializeField] private float rollWeight;

    public RewardType Type => rewardType;
    public int Amount => amount;
    public Sprite Icon => icon;
    public SoundEffect WinSound => winSound;
    public float Weight => rollWeight;


    public void Apply()
    {
        AudioHelpers.PlaySoundEffect(winSound, Camera.main.transform.position);

        switch (rewardType)
        {
            case RewardType.Health:
                Player.Instance.HealPlayer(amount);
                break;

            case RewardType.Currency:
                CurrencyManager.Instance.AddCurrency(amount);
                break;

            case RewardType.RerollCharge:
                RunStateManager.Instance.GrantShopReroll(amount);
                break;

            case RewardType.AbilityCharge:
                Player.Instance.AbilityCharge += amount;
                break;

            default:
                Debug.LogWarning(
                    $"No reward behavior exists for {rewardType}."
                );
                break;
        }
    }
}