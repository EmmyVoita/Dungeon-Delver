using UnityEngine;

[CreateAssetMenu(menuName = "Data / Level Data")]
public class LevelDataObject : ScriptableObject
{
    [SerializeField] private TextAsset _levelFile;

    [Header("Shop Rewards")]
    [SerializeField] private int _baseCurrencyReward = 100;
    [SerializeField] private int _baseRerollReward = 1;

    public int BaseCurrencyReward => _baseCurrencyReward;
    public int BaseRerollReward => _baseCurrencyReward;
    public TextAsset LevelFile => _levelFile;
}