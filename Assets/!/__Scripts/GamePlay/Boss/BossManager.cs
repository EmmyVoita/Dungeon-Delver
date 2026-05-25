using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BossManager : MonoBehaviour
{
    public static BossManager Instance { get; private set; }

    //[SerializeField]private BossDefinition activeBoss;
    private Coroutine bossRoutine;
    [SerializeField] private bool _practiceBoss = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private bool _activeBoss;

    public bool IsBossActive => _activeBoss;

    // --------------------------------------------------
    // Entry / Exit
    // --------------------------------------------------

    private void OnEnable()
    {
        GameStateManager.OnStateChanged += HandleStateChanged;
    }

    private void OnDisable()
    {
        GameStateManager.OnStateChanged -= HandleStateChanged;
    }

    private void HandleStateChanged(GameState previousState, GameState newState)
    {
        if(newState == GameState.UpgradeSelection)
        {
            Debug.Log($"Setting active boss to false. Active boss was => {IsBossActive}");
            _activeBoss = false;
        }
    }

    private void Start()
    {
        _practiceBoss = GameSessionBootstrap.Config.Mode == GameMode.ObstaclePracticeBoss;
        if(_practiceBoss)
         _activeBoss = true;

    }

    public void StartBoss()
    {
        Debug.Log("StartingBoss");
        _activeBoss = true;
    }

    /*
    public void StopBoss()
    {
        if (bossRoutine != null)
            StopCoroutine(bossRoutine);

        bossRoutine = null;
        //activeBoss = null;

        BossContext.EndBoss();
    }
    */

    // --------------------------------------------------
    // Phase Flow
    // --------------------------------------------------

    /*
    private IEnumerator RunBoss(BossDefinition boss)
    {
        BossContext.StartBoss();

        yield return new WaitUntil(() => !BossContext.IsBossActive);

        //float bpm = ArrowSpawner.Instance.ActiveBPM;
        //float secondsPerBeat = 60f / bpm;

        //float elapsedBeats = 0f;

        // Track active effects
        //HashSet<BossEffect> active = new();

        //while (BossContext.IsBossActive)
        //{   
            /*
            elapsedBeats += Time.deltaTime / secondsPerBeat;

            foreach (var effect in boss.supportedEffects)
            {
                bool shouldBeActive =
                    elapsedBeats >= effect.startBeat &&
                    elapsedBeats < effect.startBeat + effect.durationBeats;

                bool isActive = active.Contains(effect);

                if (shouldBeActive && !isActive)
                {
                    BossContext.EnableEffect(effect.effectType);
                    active.Add(effect);
                }
                else if (!shouldBeActive && isActive)
                {
                    BossContext.DisableEffect(effect.effectType);
                    active.Remove(effect);
                }
            }
            */

            //yield return null;
        //}

        //BossContext.EndBoss();
    //}


}
