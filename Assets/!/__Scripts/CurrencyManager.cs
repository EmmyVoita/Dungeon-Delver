using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.VFX;

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance { get; private set; }

    public static event Action<int> OnCurrencyChanged;
    public static event Action<int> OnCurrencyAdded;
    public static event Action<int> OnCurrencySpent;

    [Header("VFX")]
    [SerializeField] private CurrencyVFXController coinEffect;
    [SerializeField] private CurrencyVFXController coinReleaseEffect;
    [SerializeField] private float minDuration = 0.25f;
    [SerializeField] private float maxDuration = 1.0f;
    [SerializeField] private int maxAmount = 200;

    [Header("Bonuses")]
    [SerializeField] private int tookNoDamage = 1000;
    [SerializeField] private int tookNoDamageRerolls = 1;
    [SerializeField] private int perfectRound = 1000;
    [SerializeField] private List<ComboRewardTier> comboRewards;
    [SerializeField] private SoundEffect rewardSound;
    [SerializeField] private List<SoundEffect> currencyRewardSound;


    [SerializeField] private TextPopupObject rerollPopupPrefab;
    [SerializeField] private TextPopupObject popupPrefab;
    [SerializeField] private float bonusesInterval = 2.0f;
    [SerializeField] private Transform popupTargetSpawnPos;

    [SerializeField] private float resetVFXTime = 1f;
    private float _lastVFXTime;

    public int CurrentCurrency { get; private set; }
    public int PreviousCurrency { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        _lastVFXTime = float.MinValue;
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.K))
        {
            AddCurrency(1000, silent: false);
        }
    }

    public void ResetCurrency()
    {
        CurrentCurrency = 0;
        OnCurrencyChanged?.Invoke(CurrentCurrency);
    }

    public void AddCurrency(int amount, string popupPrefix = null, string popupSuffix = null, bool silent = false)
    {
        if (amount <= 0)
            return;

        PreviousCurrency = CurrentCurrency;

        CurrentCurrency += amount;

        OnCurrencyAdded?.Invoke(amount);
        OnCurrencyChanged?.Invoke(CurrentCurrency);

        Debug.Log($"ADDED CURRENCY => {amount}");

        if(silent) return;

        //float playDuration = Mathf.Lerp(minDuration, maxDuration,(float)amount/maxAmount);

        // Reset to default visual effect animation when the time between last played exceeds the threshold
        if(Time.time > _lastVFXTime + resetVFXTime)
        {
            //Reset
            coinEffect.Play(amount);
            _lastVFXTime = Time.time;
        }
        else
        {
            coinEffect.PlayAccent();
            _lastVFXTime = Time.time;
        }
        

        if (popupPrefab != null)
        {
            TextPopupObject popup = Instantiate(popupPrefab, popupTargetSpawnPos.position, Quaternion.identity);
            popup.Initialize(amount, popupPrefix, popupSuffix);
        }

        AudioHelpers.PlaySoundEffect(currencyRewardSound.GetRandom(), transform.position);
    }

    public void AddShopRerolls(int amount, string popupPrefix = null, string popupSuffix = null)
    {
        RunStateManager.Instance.GrantShopReroll(amount);

        if (popupPrefab != null)
        {
            TextPopupObject popup = Instantiate(rerollPopupPrefab, popupTargetSpawnPos.position, Quaternion.identity);
            popup.Initialize(tookNoDamageRerolls, popupPrefix, popupSuffix);
        }

        AudioHelpers.PlaySoundEffect(rewardSound, transform.position);
    }

    public bool TrySpendCurrency(int amount)
    {
        if (amount <= 0)
            return true;

        if (CurrentCurrency < amount)
            return false;

        PreviousCurrency = CurrentCurrency;

        CurrentCurrency -= amount;

        coinReleaseEffect.Play(amount);

        OnCurrencySpent?.Invoke(amount);
        OnCurrencyChanged?.Invoke(CurrentCurrency);

        return true;
    }


    public IEnumerator EndOfRoundSequence()
    {
        AddCurrency(RoundManager.Instance.CurrentLevelReward, "Level Reward");

        yield return new WaitForSeconds(bonusesInterval);


        if (popupPrefab != null)
        {
            int reward = 0;
            int comboRequirement = 0;

            foreach(var tier in comboRewards)
            {
                if(RoundManager.Instance.stats.HighestCombo >= tier.comboRequirement)
                {
                    reward = tier.currencyReward;
                    comboRequirement = tier.comboRequirement;
                }
            }

            if (reward > 0)
            {
                AddCurrency(reward, $"Combo Bonus x{comboRequirement}");
                yield return new WaitForSeconds(bonusesInterval);
            }
        }
      

        if(RoundManager.Instance.stats.PlayerTookNoDamage)
        {
            AddCurrency(tookNoDamage, "Took No Damage");
            
            yield return new WaitForSeconds(bonusesInterval);

            AddShopRerolls(tookNoDamageRerolls, "Took No Damage", "Rerolls");
            
            yield return new WaitForSeconds(bonusesInterval);
        }


        if(RoundManager.Instance.stats.PerfectRound)
        {
            AddCurrency(perfectRound, "Perfect Round");

            yield return new WaitForSeconds(bonusesInterval);
        }
    }
}