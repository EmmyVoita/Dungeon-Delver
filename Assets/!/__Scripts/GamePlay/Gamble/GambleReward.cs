using UnityEngine;

[System.Serializable]
public class GambleReward
{
    public string displayName;
    public Sprite icon;

    [Min(0f)]
    public float weight = 1f;

    public GambleRewardType type;

    public int amount = 1;
    public Color backgroundColor = Color.white;
}

