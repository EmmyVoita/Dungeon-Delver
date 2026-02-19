using UnityEngine;
using System;
using System.Collections;


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

    [Header("Combo Score Settings")]
    public float baseValue = 100;
    public float growth = 10f;
    public float exponent = 1.1f;

    [Header("Normal Combo Settings")]
    public Color comboFlashColor = Color.red;
    public AudioClip comboSound;
    public AudioClip resetComboSound;
    public float minGoodComboCount = 10;
    public int resetComboThreshold = 20;
    public int comboScoreBaseAmount = 100;
    public float comboBreakScreenShakeDuration = 0.4f;
    public float comboBreakScreenShakeMagnitude = 0.05f;

    // ===================== High Combo Accent =====================

    [Header("High Combo Accent")]
    [Tooltip("Plays as a subtle layer at high combo milestones")]
    public AudioClip highComboAccentSound;

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
    private bool ignoreNextMiss = false;


    private int comboCount;
      private int critsInARow = 0;

    private bool hasPendingBreak = false;
    private ComboBreakPriority pendingBreakPriority;
    private ComboBreakReason pendingBreakReason;
    private GameObject activeShieldObject = null;
  


    public int CritsInARow => critsInARow;
    public int GetCurrentComboCount => comboCount;
    public int HighComboStep => highComboStep;
    public int HighComboStart => highComboStart;


    void Awake() => Instance = this;

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

    public void PreventNextComboBreak()
    {
        ignoreNextMiss = true;
        if (shieldPrefab != null && activeShieldObject == null)
        {
            activeShieldObject = Instantiate(
                shieldPrefab,
                Player.Instance.transform.position,
                Quaternion.identity
            );
            activeShieldObject.transform.SetParent(Player.Instance.transform);
        }
    }


    // Event Handling for breaking the combo
    // ------------------------------------------------------------------------------------

    private void HandleStateChanged(GameState previousState, GameState newState)
    {
        if (newState == GameState.RoundActive && previousState != GameState.RoundActive)
        {
            RequestComboBreak(ComboBreakReason.StateChange, ComboBreakPriority.StateChange);
        }

        if( newState == GameState.RoundEnd)
        {
            ignoreNextMiss = false;
            Destroy(activeShieldObject);
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
            PlayComboSound(comboSound, comboCount);
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
        if (comboCount <= 0)
            return;

        // Immediate logical break
        OnComboBreakImmediate?.Invoke(comboCount, reason);

        // 🛡️ Ignore next miss protection
        if (ignoreNextMiss && reason != ComboBreakReason.StateChange && activeShieldObject != null)
        {
            ignoreNextMiss = false; // consume protection
            Debug.Log("🛡️ Combo miss ignored.");
            AudioHelpers.PlaySoundEffect(ignoreMissSoundEffect, Camera.main.transform.position);

            if (ignoreMissVFXPrefab != null)
            {
                Instantiate(
                    ignoreMissVFXPrefab,
                    Player.Instance.transform.position,
                    Quaternion.identity
                );
            }

            Destroy(activeShieldObject);
            return;
        }

        // First request this frame
        if (!hasPendingBreak)
        {
            hasPendingBreak = true;
            pendingBreakPriority = priority;
            pendingBreakReason = reason;

            // Resolve at end of frame
            StartCoroutine(ResolveComboBreakEndOfFrame());
            return;
        }

        // Higher-priority request overrides
        if (priority > pendingBreakPriority)
        {
            pendingBreakPriority = priority;
            pendingBreakReason = reason;
        }
    }

    private IEnumerator ResolveComboBreakEndOfFrame()
    {
        yield return null; // wait one frame

        ExecuteComboBreak(pendingBreakReason);

        hasPendingBreak = false;
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
        

        if (comboCount > resetComboThreshold && resetComboSound != null && playSound)
        {
            AudioHelpers.PlayMyClipAtPoint(
                resetComboSound,
                AudioChannel.SFX,
                Camera.main.transform.position
            );

            ScreenShake.Instance.Shake(
                comboBreakScreenShakeDuration,
                comboBreakScreenShakeMagnitude
            );
        }

        AddComboScoreDisplay?.Invoke(comboCount * comboScoreBaseAmount);
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
            PlayComboSound(comboSound, comboCount);
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
        if (highComboAccentSound == null)
            return;

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

        AudioHelpers.PlayMyClipAtPoint(
            highComboAccentSound,
            AudioChannel.SFX,
            Camera.main.transform.position,
            vol,
            pitch
        );

        OnComboAccent?.Invoke(combo, accentIndex);
    }


    private void PlayComboSound(AudioClip clip, int count)
    {
        if (clip == null) return;

        float pitch = Mathf.Min(
            basePitch + ((count - minGoodComboCount) - 1) * pitchStep,
            maxPitch
        );

        float vol = Mathf.Clamp(
            baseVolume * (0.1f + ((count - minGoodComboCount) - 1) * volumeStep),
            0f,
            1f
        );

        AudioHelpers.PlayMyClipAtPoint(
            clip,
            AudioChannel.SFX,
            Camera.main.transform.position,
            vol,
            pitch
        );
    }



}
