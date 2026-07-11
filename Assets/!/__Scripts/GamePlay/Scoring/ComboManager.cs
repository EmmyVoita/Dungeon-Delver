using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


public enum ComboBreakReason
{
    ArrowMiss,
    Damage,
    StateChange,
    RoundEnd,
}


public class ComboManager : MonoBehaviour
{
    public static ComboManager Instance { get; private set; }

    enum ComboBreakPriority
    {
        ArrowMiss = 0,
        Damage = 1,
        StateChange = 2,
        RoundEnd = 3
    }


    public static event Action<int, int> OnComboAccent;
    public static event Action<int> OnComboUpdated;
    public static event Action<int> AddComboScoreDisplay;
    public static event Action<int, ComboBreakReason> OnComboBreak;
    public static event Action<int, ComboBreakReason> OnComboBreakImmediate;
    public static event Action<int> OnCritStreakUpdated;


    [Header("References")]
    public ScoreTallyController tallyController;


    [Header("Score Settings")]
    public int baseValue = 100;
    public float growth = 10f;
    public float exponent = 1.1f;


    [Header("Audio")]
    [SerializeField] private SoundEffect comboTickSEff;
    [SerializeField] private SoundEffect comboBreakSEff;
    [Tooltip("Plays as a subtle layer at high combo milestones")]
    [SerializeField] private SoundEffect highComboAccentSEff;


    [Header("ScreenShake")]
    //[SerializeField] private ScreenShakeRequest ssRequest;

    [Header("Normal Combo Settings")]

    public float minGoodComboCount = 10;
    public int resetComboThreshold = 20;
    // ===================== High Combo Accent =====================

    [Header("High Combo Accent")]
    [Tooltip("Combo count where accent starts")]
    [SerializeField] private int highComboStart = 40;
    [Tooltip("Play accent every N combos after start")]
    [SerializeField] private int highComboStep = 10;


    [Header("Base Combo Sound Settings")]
    [Range(0f, 3f)] [SerializeField] private float basePitch = 1f;
    [Range(0f, 1f)] [SerializeField] private float pitchStep = 0.05f;
    [Range(0f, 5f)] [SerializeField] private float maxPitch = 2f;
    [Range(0f, 1f)] [SerializeField] private float baseVolume = 1f;
    [Range(0f, 1f)] [SerializeField] private float volumeStep = 0.02f;
    
    
    [Header("High Combo Accent Sound Settings")]
    [Range(0f, 1f)] [SerializeField] private float baseAccentVolume = 0.5f;
    [Range(0f, 1f)] [SerializeField] private float accentVolumeStep = 0.7f;
    [Range(0f, 3f)] [SerializeField] private float baseAccentPitch = 1f;
    [Range(0f, 1f)] [SerializeField] private float accentPitchStep = 0.05f;
    [Range(0f, 5f)] [SerializeField] private float maxAccentPitch = 2f;

    [Header("Combo Miss Ignore Settings")]
    [SerializeField] private SoundEffect ignoreMissSoundEffect;
    [SerializeField] private GameObject ignoreMissVFXPrefab;
    [SerializeField] private GameObject shieldPrefab;
    private int _ignoreNextMissCount = 0;


    private int comboCount;
      private int critsInARow = 0;

    private bool hasPendingBreak = false;
    private ComboBreakPriority pendingBreakPriority;
    private ComboBreakReason pendingBreakReason;
    private List<GameObject> _activeShieldObjects = null;
    private Coroutine ignoreNextMissCoroutine;
    private bool blockBreakThisFrame = false;
  


    public int CritsInARow => critsInARow;
    public int GetCurrentComboCount => comboCount;
    public int HighComboStep => highComboStep;
    public int HighComboStart => highComboStart;


    void Awake()
    {
        Instance = this;

        _activeShieldObjects = new();
    } 

    private void OnEnable()
    {
        ArrowBase.OnArrowResolved += HandleArrowResolved;
        Player.OnPreDamageTaken += HandleDamageTaken;
        GameStateManager.OnStateChanged += HandleStateChanged;
    }

    private void OnDisable()
    {
        ArrowBase.OnArrowResolved -= HandleArrowResolved;
        Player.OnPreDamageTaken -= HandleDamageTaken;
        GameStateManager.OnStateChanged -= HandleStateChanged;
    }

    public void PreventNextComboBreak(float duration = -1)
    {
        GameObject shieldObject = null;

        if (shieldPrefab != null)
        {
            shieldObject = Instantiate(
                shieldPrefab,
                Player.Instance.transform.position,
                Quaternion.identity
            );

            shieldObject.transform.localScale =
                Vector3.one * (1f + _ignoreNextMissCount * .2f);

            shieldObject.transform.SetParent(
                Player.Instance.transform
            );

            _activeShieldObjects.Add(shieldObject);
        }

        _ignoreNextMissCount++;

        if(duration != -1)
        {
            StartCoroutine(
                HandleIgnoreNextMiss(
                    duration,
                    shieldObject
                )
            );
        }
    }

    private IEnumerator HandleIgnoreNextMiss(
    float duration,
    GameObject shieldObject)
    {
        yield return new WaitForSeconds(duration);

        // Already consumed?
        if (!_activeShieldObjects.Contains(shieldObject))
            yield break;

        _activeShieldObjects.Remove(shieldObject);

        Destroy(shieldObject);

        _ignoreNextMissCount--;
    }


    // Event Handling for breaking the combo
    // ------------------------------------------------------------------------------------

    private void HandleStateChanged(GameState previousState, GameState newState)
    {
        if (newState == GameState.RoundActive && previousState != GameState.RoundActive)
        {
            RequestComboBreak(ComboBreakReason.StateChange, ComboBreakPriority.StateChange);
        }

        if( newState == GameState.RoundResultsTally)
        {
            _ignoreNextMissCount = 0;

            foreach(GameObject shield in _activeShieldObjects)
            {
                Destroy(shield);
            }

            RequestComboBreak(ComboBreakReason.RoundEnd, ComboBreakPriority.RoundEnd);
            critsInARow = 0;
            OnCritStreakUpdated?.Invoke(critsInARow);
        }
    }

    private void HandleDamageTaken(int damage) => RequestComboBreak(ComboBreakReason.Damage,ComboBreakPriority.Damage);
                                                                    
                                                                    
               // call BreakComboMiss at the end of th round for adding the remaining combo instead of just tallying it somehwere ourselves.                                                  


    private void HandleArrowResolved(ArrowResolvedData data)
    {
        switch (data.goalType)
        {
            case Goal.GoalType.Critical:
                AddCrit();
                break;

            case Goal.GoalType.Normal:
                AddHit();
                ResetCritStreak();
                break;

            case Goal.GoalType.Miss:
                ResetCritStreak();
                RequestComboBreak(ComboBreakReason.ArrowMiss, ComboBreakPriority.ArrowMiss);
                break;
        }
    }

    private void AddCrit()
    {
        comboCount++;
        critsInARow++;

        OnComboUpdated?.Invoke(comboCount);

        // Optional future event
        OnCritStreakUpdated?.Invoke(critsInARow);

        if (comboCount > minGoodComboCount)
        {
            PlayComboSound(comboCount);
            TryPlayHighComboAccent(comboCount);
        }
    }

    private void ResetCritStreak()
    {
        if (critsInARow == 0)
            return;

        critsInARow = 0;

        OnCritStreakUpdated?.Invoke(0);
    }




    // Core Combo Logic
    // ------------------------------------------------------------------------------------

    private void RequestComboBreak(
    ComboBreakReason reason,
    ComboBreakPriority priority)
    {
        Debug.Log($"Requesting combo break. Reason => {reason}. Priority => {priority}");

        // Shield: consume + block entire frame
        if (_ignoreNextMissCount > 0 && reason != ComboBreakReason.StateChange)
        {
         
            if(!blockBreakThisFrame)
            {
                AudioHelpers.PlaySoundEffect(ignoreMissSoundEffect, Camera.main.transform.position);

                if (ignoreMissVFXPrefab != null)
                {
                    Instantiate(
                        ignoreMissVFXPrefab,
                        Player.Instance.transform.position,
                        Quaternion.identity
                    );
                }
                
                if (_activeShieldObjects.Count > 0)
                {
                    //Debug.LogError($"Destroying Shield because shield count is => {_activeShieldObjects.Count} \n"+
                    //            $"Ignore miss count is => {_ignoreNextMissCount} \n");
                            
                    GameObject shield =
                        _activeShieldObjects[^1];

                    _activeShieldObjects.RemoveAt(
                        _activeShieldObjects.Count - 1
                    );

                    Destroy(shield);
                }

                _ignoreNextMissCount--;
            }

                 
            blockBreakThisFrame = true;

            // 🔥 IMPORTANT: still register this frame so it resolves/reset correctly
            if (!hasPendingBreak)
            {
                hasPendingBreak = true;
                pendingBreakPriority = priority;
                pendingBreakReason = reason;

                StartCoroutine(ResolveComboBreakEndOfFrame());
            }

            return;
        }

        Debug.Log($"RequestComboBreak(). blockBreakThisFrame => {blockBreakThisFrame}");

        // 🛡️ If already blocked this frame, just register and exit
        if (blockBreakThisFrame)
        {
            Debug.Log("🛡️ Combo break already blocked this frame.");

            if (!hasPendingBreak)
            {
                hasPendingBreak = true;
                pendingBreakPriority = priority;
                pendingBreakReason = reason;

                StartCoroutine(ResolveComboBreakEndOfFrame());
            }

            return;
        }

        // 🔔 Immediate feedback (only if not blocked)
        OnComboBreakImmediate?.Invoke(comboCount, reason);

        // 🧠 Register break request (batched per frame)
        if (!hasPendingBreak)
        {
            hasPendingBreak = true;
            pendingBreakPriority = priority;
            pendingBreakReason = reason;

            StartCoroutine(ResolveComboBreakEndOfFrame());
            return;
        }

        // Higher priority overrides (Damage > Miss, etc.)
        if (priority > pendingBreakPriority)
        {
            pendingBreakPriority = priority;
            pendingBreakReason = reason;
        }
    }

    private IEnumerator ResolveComboBreakEndOfFrame()
    {
        Debug.Log($"ResolveComboBreakEndOfFrame(). blockBreakThisFrame => {blockBreakThisFrame}");
        yield return null;

        if (!blockBreakThisFrame)
        {
            ExecuteComboBreak(pendingBreakReason);
        }

        hasPendingBreak = false;
        blockBreakThisFrame = false; // 🔥 reset here
    }

    private void ExecuteComboBreak(ComboBreakReason reason)
    {
        CacheCombo(
            playSound: reason == ComboBreakReason.Damage,
            animateAbilityCharge: reason == ComboBreakReason.RoundEnd 
        );

        OnComboBreak?.Invoke(comboCount, reason);
        ResetCombo();
    }

    private void CacheCombo(bool playSound, bool animateAbilityCharge)
    {
        //int comboScore = CalculateComboScore(comboCount);
        //ScoreManager.Instance.AddScore(comboScore, ScoreSource.Combo);
        Debug.Log($"CacheCombo. AnimateAbilityCharge => {animateAbilityCharge}");

        if(animateAbilityCharge)
        {
            tallyController.StartCoroutine(
                tallyController.StartRoundEndTally()
            );

        }
        else
        {
            tallyController.AnimateComboAdd();
        }
        

        if (comboCount > resetComboThreshold && playSound)
        {
            //TimeManager.Instance.AddTemporaryModifier(new TimeScaleModifier("ComboFreezeFrame",0), 0.5f);

            AudioHelpers.PlaySoundEffect(comboBreakSEff, transform.position);
        }

        AddComboScoreDisplay?.Invoke(comboCount * baseValue);
    }


    public void ResetCombo()
    {
        comboCount = 0;
        OnComboUpdated?.Invoke(0);

    }




    public void AddHit(int amount = 1)
    {
        comboCount += amount;
        OnComboUpdated?.Invoke(comboCount);

        if (comboCount > minGoodComboCount)
        {
            PlayComboSound(comboCount);
            TryPlayHighComboAccent(comboCount);
        }
    }


    // Calculate score based on combo count
    // ------------------------------------------------------------------------------------

    public static int CalculateComboScore(int comboCount)
    {
        if (comboCount <= 0) return 0;

        float score = Instance.baseValue + Instance.growth * Mathf.Pow(comboCount, Instance.exponent);

        return Mathf.RoundToInt(score);
    }


    // Audio
    // ------------------------------------------------------------------------------------


    private void TryPlayHighComboAccent(int combo)
    {
        //if (highComboAccentSound == null)
            //return;

        if (combo < highComboStart)
            return;

        int offset = combo - highComboStart;

        if (offset % highComboStep != 0)
            return;

        int accentIndex = offset / highComboStep; // 🔥 THIS IS THE KEY

        float pitch = Mathf.Min(
            baseAccentPitch + accentIndex * accentPitchStep,
            maxAccentPitch
        );

        float vol = Mathf.Clamp(
            baseAccentVolume + accentIndex * accentVolumeStep,
            0f,
            1f
        );

        AudioHelpers.PlaySoundEffect(highComboAccentSEff, transform.position, pitch, vol);

        OnComboAccent?.Invoke(combo, accentIndex);
    }


    private void PlayComboSound(int count)
    {
        //if (clip == null) return;

        float pitch = Mathf.Min(
            basePitch + ((count - minGoodComboCount) - 1) * pitchStep,
            maxPitch
        );

        float vol = Mathf.Clamp(
            baseVolume * (0.1f + ((count - minGoodComboCount) - 1) * volumeStep),
            0f,
            1f
        );

        AudioHelpers.PlaySoundEffect(comboTickSEff, transform.position, pitch, vol);
    }



}
