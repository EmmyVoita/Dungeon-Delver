using System;
using UnityEngine;

public class RecoveryArrowManager : MonoBehaviour
{
    public static RecoveryArrowManager Instance;

    public static event Action<int, int> OnCreateRecoveryArrow;
    public static event Action<bool> OnRecoveryArrowResolved;

    [Header("Feedback")]
    public SoundEffect activateSound;
    public SoundEffect successSound;
    public SoundEffect failiureSound;



    private int _minCombo = int.MaxValue;
    private float _recoveryPercent = 0;
    private int _charges = 0;
    private int _maxCharges = 0;
    private int _cachedComboCount = 0;
    private bool _initialized;

    private int RecoveryAmount => (int)(_cachedComboCount * _recoveryPercent);


    private void OnDisable()
    {
        ArrowBase.OnArrowResolved -= HandleArrowResolved;
        ComboManager.OnComboBreak -= HandleComboBreak;
        GameStateManager.OnStateChanged -= HandleStateChanged;
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

        ComboManager.OnComboBreak += HandleComboBreak;
        ArrowBase.OnArrowResolved += HandleArrowResolved;
        GameStateManager.OnStateChanged += HandleStateChanged;
    }


    // Reset our charges when the round starts.
    private void HandleStateChanged(GameState previousState, GameState newState)
    {
        if(newState == GameStateManager.LevelStartState)
        {
            _charges = _maxCharges;
        }
    }


    public void AddRecoveryArrow(RecoveryArrowUpgrade upgrade)
    {
        Initialize();

        _maxCharges += upgrade.RecoveryArrowsGranted;

        _minCombo = Mathf.Min(_minCombo,upgrade.MinComboRequired);

        _recoveryPercent = Mathf.Max(_recoveryPercent,upgrade.RecoveryPercentage);

        Debug.Log($"Adding recovery arrow upgrade \n"+
                  $"RecoveryArrowsGranted => {upgrade.RecoveryArrowsGranted} \n" +
                  $"MinComboRequired => {upgrade.MinComboRequired} \n" +
                  $"RecoveryPercentage => {upgrade.RecoveryPercentage} \n\n" +
                  $"New Max Charges => {_maxCharges}");
    }

    private void HandleArrowResolved(ArrowResolvedData data)
    {
        if(data.status.HasFlag(ArrowStatus.Recovery))
        {
            if(data.goalType == Goal.GoalType.Critical)
            {
                OnRecoveryArrowResolved?.Invoke(true);
                AudioHelpers.PlaySoundEffect(successSound, Camera.main.transform.position);
                ComboManager.Instance.AddHit(RecoveryAmount);
                return;
            } 
            else
            {
                OnRecoveryArrowResolved?.Invoke(false);
                AudioHelpers.PlaySoundEffect(failiureSound, Camera.main.transform.position);
                return;
            }
        }
    }

    private void HandleComboBreak(int comboCount, ComboBreakReason reason)
    {
        if (comboCount < _minCombo || _charges <= 0)
            return;

        if(reason != ComboBreakReason.Damage && reason != ComboBreakReason.ArrowMiss)
            return;

        _cachedComboCount = comboCount;
        TriggerRecoveryArrow();
        _charges--;
    }   

    private void TriggerRecoveryArrow()
    {
        AudioHelpers.PlaySoundEffect(activateSound, Camera.main.transform.position);
        BuffHelpers.GetOrCreateRecoveryArrow(1);
        OnCreateRecoveryArrow?.Invoke(_charges, RecoveryAmount);
    }
}