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


    [Header("Popup")]
    [SerializeField] private float popupBatchWindow = 0.15f;
    [SerializeField] private SoundEffect popupSound;
    [SerializeField] private RewardStampText stampText;

    private int _pendingPopupAmount;
    private Coroutine _popupBatchRoutine;
    private bool _startingCurrencyInitalized = false;

    private readonly List<CurrencyScaleModifier> modifiers = new();


    private float _lastVFXTime;
    private int _totalCurrency;

    public int CurrentCurrency { get; private set; }
    public int PreviousCurrency { get; private set; }

    public int TotalCurrency => _totalCurrency;

    
      private void OnEnable()
    {
        RunLifecycle.OnRunAbandoned += HandleRunAbandoned;
        GameStateManager.OnStateChanged += HandleStateChanged;
    }

    private void OnDisable()
    {
        RunLifecycle.OnRunAbandoned -= HandleRunAbandoned;
        GameStateManager.OnStateChanged -= HandleStateChanged;
    }

    private void HandleStateChanged(GameState previousState, GameState newState)
    {
        if(newState == GameStateManager.LevelStartState && !_startingCurrencyInitalized)
        {
            if(GameSessionBootstrap.Config.StartingCurrency > 0)
            {
                AddCurrency(GameSessionBootstrap.Config.StartingCurrency,"Run Modifier");
                _startingCurrencyInitalized = true;
            }
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

        _lastVFXTime = float.MinValue;
        _totalCurrency = 0;
        _startingCurrencyInitalized = false;
    }


    private void Update()
    {
        #if UNITY_EDITOR
            if(Input.GetKeyDown(KeyCode.K))
            {
                AddCurrency(1000, silent: false);
            }
        #endif
    }

    public void ResetCurrency()
    {
        CurrentCurrency = 0;
        OnCurrencyChanged?.Invoke(CurrentCurrency);
    }


    private void QueueCurrencyPopup(int amount)
    {
        _pendingPopupAmount += amount;

        if(_popupBatchRoutine != null)
            StopCoroutine(_popupBatchRoutine);

        _popupBatchRoutine = StartCoroutine(DisplayPopupAfterWindow(_pendingPopupAmount));
    }

    private IEnumerator DisplayPopupAfterWindow(int amount)
    {
        yield return new WaitForSeconds(popupBatchWindow);

        if (popupPrefab != null)
        {
            TextPopupObject popup = Instantiate(popupPrefab, popupTargetSpawnPos.position, Quaternion.identity);
            popup.Initialize(_pendingPopupAmount);

            AudioHelpers.PlaySoundEffect(popupSound, popup.transform.position);
        }

        _pendingPopupAmount = 0;
        _popupBatchRoutine = null;

  
    }

    public void AddShopRerolls(int amount, string popupPrefix = null, string popupSuffix = null)
    {
        RunStateManager.Instance.GrantShopReroll(amount);

        if (popupPrefab != null)
        {
            TextPopupObject popup = Instantiate(rerollPopupPrefab, popupTargetSpawnPos.position, Quaternion.identity);
            popup.Initialize(tookNoDamageRerolls, popupPrefix, popupSuffix);
        }

        //AudioHelpers.PlaySoundEffect(rewardSound, transform.position);
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
        //AddShopRerolls(RoundManager.Instance.CurrentLevelRerollReward, "Rerolls");
        stampText.Play($"Level Cleared!");

        yield return new WaitForSeconds(bonusesInterval);


        if (popupPrefab != null)
        {
            int reward = 0;
            int comboRequirement = 0;

            foreach(var tier in comboRewards)
            {
                if(RunHistoryManager.Instance.RoundStatsTracker.HighestCombo >= tier.comboRequirement)
                {
                    reward = tier.currencyReward;
                    comboRequirement = tier.comboRequirement;
                }
            }

            if (reward > 0)
            {
                stampText.Play($"Combo x{comboRequirement}");
                AddCurrency(reward, $"Combo x{comboRequirement}");
                yield return new WaitForSeconds(bonusesInterval);
            }
        }
      

        if(RunHistoryManager.Instance.RoundStatsTracker.PlayerTookNoDamage)
        {
            AddCurrency(tookNoDamage, "No Damage");
            stampText.Play($"No Damage!");
            
            yield return new WaitForSeconds(bonusesInterval);

            PlayRerollPopup(tookNoDamageRerolls);

            AddShopRerolls(tookNoDamageRerolls, "No Damage!", "Rerolls");
            
            yield return new WaitForSeconds(bonusesInterval);
        }


        if(RunHistoryManager.Instance.RoundStatsTracker.PerfectRound)
        {
            AddCurrency(perfectRound, "Perfect Round");
            stampText.Play($"Never Missed!");

            yield return new WaitForSeconds(bonusesInterval);
        }

        int runModifierRerolls = GameSessionBootstrap.Config != null ? GameSessionBootstrap.Config.RerollsToGrant : 0;

        if(runModifierRerolls > 0)
        {
            stampText.Play($"Run Modifier!");
            PlayRerollPopup(runModifierRerolls);
            AddShopRerolls(runModifierRerolls, "Run Modifier!", "Rerolls");
            yield return new WaitForSeconds(bonusesInterval);
        }
            

        stampText.Hide();
    }

    public void PlayRerollPopup(int amount)
    {
        if (popupPrefab != null)
        {
            TextPopupObject popup = Instantiate(popupPrefab, popupTargetSpawnPos.position, Quaternion.identity);
            popup.Initialize(amount,suffix:" REROLLS");

            AudioHelpers.PlaySoundEffect(popupSound, popup.transform.position);
        }
    }

    // Currency Calculation
    //---------------------------------------------------------------------------------------------
    
    public void AddCurrency(int amount, string popupPrefix = null, string popupSuffix = null, bool silent = false)
    {
        if (amount <= 0)
            return;

        int adjustedAmount = Mathf.FloorToInt(amount * CalculateFinalScale());

        PreviousCurrency = CurrentCurrency;
        CurrentCurrency += adjustedAmount;

        OnCurrencyAdded?.Invoke(adjustedAmount);
        OnCurrencyChanged?.Invoke(CurrentCurrency);

        // Reset to default visual effect animation when the time between last played exceeds the threshold
        if(Time.time > _lastVFXTime + resetVFXTime)
        {
            //Reset
            coinEffect.Play(adjustedAmount);
            _lastVFXTime = Time.time;
        }
        else
        {
            //coinEffect.PlayAccent();
            _lastVFXTime = Time.time;
        }

        QueueCurrencyPopup(adjustedAmount);

        AudioHelpers.PlaySoundEffect(currencyRewardSound.GetRandom(), transform.position);

        _totalCurrency += adjustedAmount;
    }

    public CurrencyScaleModifier AddTemporaryCurrencyModifier(float scale,float duration)
    {
        CurrencyScaleModifier modifier = new CurrencyScaleModifier(scale);

        modifiers.Add(modifier);

        StartCoroutine(RemoveCurrencyModifierAfterDuration(modifier,duration));

        return modifier;
    }
    private IEnumerator RemoveCurrencyModifierAfterDuration(CurrencyScaleModifier modifier,float duration)
    {
        yield return new WaitForSeconds(duration);

        modifier.Disable();
        modifiers.Remove(modifier);
    }

    public void AddCurrencyModifier(CurrencyScaleModifier mod)
    {
        if (mod == null)
            return;

        modifiers.Add(mod);
    }

    private void ClearModifiers()
    {
        modifiers.Clear();
    }

    private float CalculateFinalScale()
    {
        float result = 1f;

        foreach (CurrencyScaleModifier mod in modifiers)
        {
            if (mod == null || !mod.IsActive)
                continue;

            result *= mod.Value;
        }

        return Mathf.Max(result,0f);
    }

    private void HandleRunAbandoned()
    {
        ClearModifiers();
    }
}