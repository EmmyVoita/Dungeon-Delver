using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Database/Gamble Reel Reward Database")]
public class GambleRewardDatabase : ScriptableObject
{
    [SerializeField] private List<GambleReward> rewardPool;
    public List<GambleReward> RewardPool => rewardPool;
}