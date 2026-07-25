using System;
using UnityEngine;

public class PaidAbilityUseManager : MonoBehaviour
{
    public static PaidAbilityUseManager Instance { get; private set; }

    public static event Action OnChanged;

    private int _usesPerLevel;
    private int _usesRemaining;
    private int _currencyCost;

    public bool IsActive => _usesPerLevel > 0;
    public int UsesRemaining => _usesRemaining;
    public int CurrencyCost => _currencyCost;

    public bool CanUsePaidAbility
    {
        get
        {
            if (!IsActive || _usesRemaining <= 0)
                return false;

            if (CurrencyManager.Instance == null)
                return false;

            return CurrencyManager.Instance.CurrentCurrency >= _currencyCost;
        }
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

    private void OnEnable()
    {
        RoundManager.OnRoundStart += HandleRoundStart;
    }

    private void OnDisable()
    {
        RoundManager.OnRoundStart -= HandleRoundStart;
    }

    public void AddPaidUsePerLevel(
        int amount,
        int currencyCost)
    {
        _usesPerLevel += Mathf.Max(0, amount);

        // For now, all uses share one cost.
        _currencyCost = Mathf.Max(0, currencyCost);

        OnChanged?.Invoke();
    }

    public bool TryConsumePaidUse()
    {
        if (!CanUsePaidAbility)
            return false;

        if (!CurrencyManager.Instance.TrySpendCurrency(_currencyCost))
            return false;

        _usesRemaining--;

        OnChanged?.Invoke();
        return true;
    }

    private void HandleRoundStart()
    {
        _usesRemaining = _usesPerLevel;
        OnChanged?.Invoke();
    }
}