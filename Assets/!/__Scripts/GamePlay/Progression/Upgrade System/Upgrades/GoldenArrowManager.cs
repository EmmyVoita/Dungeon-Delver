using System;
using System.Collections.Generic;
using UnityEngine;

public class GoldenArrowManager : MonoBehaviour
{
    public static GoldenArrowManager Instance;

    public static event Action<int, int> OnCreateGoldenArrow;
    public static event Action<bool> OnGoldenArrowResolved;

    [Header("Settings")]
    [SerializeField] private int goldenArrowWorth = 10;


    [Header("Feedback")]
    [SerializeField] private SoundEffect activateSound;


    private int _comboRequired = int.MaxValue;
    private int _arrowsGranted = 0;
    private bool _initialized;


    private int _arrowsPerCrit = 1;
    private int _maxArrowStack = 0;
    private int _stacksLeft = 0;

    private readonly List<IGoldenArrowWorthModifier> _worthModifiers = new();


    private void OnDisable()
    {
        ArrowBase.OnArrowResolved -= HandleArrowResolved;
        ComboManager.OnComboUpdated -= HandleComboUpdated;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Initialize()
    {
        if (_initialized)
            return;

        _initialized = true;

        ComboManager.OnComboUpdated += HandleComboUpdated;
        ArrowBase.OnArrowResolved += HandleArrowResolved;
        BuffHelpers.OnGoldenArrowSessionStarted += AddArrows;
    }

    public void AddGoldenArrowUpgrade(GoldenArrowUpgrade upgrade)
    {
        Initialize();

        _arrowsGranted += upgrade.GoldenArrowsGranted;

        _comboRequired = Mathf.Min(_comboRequired, upgrade.ComboRequired);

        Debug.Log($"Adding Golden Arrow Upgrade \n"+
                  $"GoldenArrowsGranted => {upgrade.GoldenArrowsGranted} \n" +
                  $"ComboRequired => {upgrade.ComboRequired} \n");
    }

    public void AddGoldenExtension(ExtendGoldenOnCritUpgrade upgrade)
    {
        Initialize();

        _maxArrowStack += upgrade.maxStack;
        _arrowsPerCrit = upgrade.arrowsPerCrit; 

        _stacksLeft = 0;
    }



    private void AddArrows()
    {
        _stacksLeft = _maxArrowStack;
    }

    private void HandleArrowResolved(ArrowResolvedData data)
    {
        if (data.goalType != Goal.GoalType.Critical)
            return;

        if (!data.status.HasFlag(ArrowStatus.Golden))
            return;

  
        CurrencyManager.Instance.AddCurrency(GetGoldenArrowWorth(), "Golden");

        if(_stacksLeft > 0)
        {
            BuffHelpers.GetOrCreateGoldenEffect(_arrowsPerCrit);
            _stacksLeft--;
        }
    }

    private void HandleComboUpdated(int comboCount)
    {
        if(comboCount > 0 &&
        comboCount % _comboRequired == 0)
        {
            TriggerGoldenBuff();
        }
    }   

    private void TriggerGoldenBuff()
    {
        BuffHelpers.OnGoldenArrowSessionStarted?.Invoke();

        AudioHelpers.PlaySoundEffect(activateSound, Camera.main.transform.position);

        BuffHelpers.GetOrCreateGoldenEffect(_arrowsGranted);
    }


    public void AddWorthModifier(IGoldenArrowWorthModifier modifier)
    {
        if(modifier == null || _worthModifiers.Contains(modifier))
            return;

        _worthModifiers.Add(modifier);
    }

    public void RemoveWorthModifier(IGoldenArrowWorthModifier modifier)
    {
        if(modifier == null || !_worthModifiers.Contains(modifier))
            return;
            
        _worthModifiers.Remove(modifier);
    }

    private int GetGoldenArrowWorth()
    {
        int worth = goldenArrowWorth;

        foreach(IGoldenArrowWorthModifier modifier in _worthModifiers)
        {
            worth = modifier.ModifyGoldenArrowWorth(worth);
        }

        return worth;
    }
}