using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BossManager : MonoBehaviour
{
    public static BossManager Instance { get; private set; }

    [SerializeField]private BossDefinition activeBoss;
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



    public bool IsBossActive => activeBoss != null || _practiceBoss;

    // --------------------------------------------------
    // Entry / Exit
    // --------------------------------------------------

    private void Start()
    {
        //if(GameSceneLoader.PendingConfig == null) return;
        _practiceBoss = GameSessionBootstrap.Config.Mode == GameMode.ObstaclePracticeBoss;
    }

    public void StartBoss(BossDefinition bossDef)
    {
        if (bossDef == null)
        {
            Debug.LogError("❌ StartBoss called with null BossDefinition");
            return;
        }

        StopBoss();

        activeBoss = bossDef;

        // Spawn visuals if any
        if (bossDef.bossVisualPrefab != null)
            Instantiate(bossDef.bossVisualPrefab);

        BossContext.StartBoss();
        bossRoutine = StartCoroutine(RunBoss(bossDef));

        Debug.Log($"👹 Boss started: {bossDef.bossName}");
    }

    public void StopBoss()
    {
        if (bossRoutine != null)
            StopCoroutine(bossRoutine);

        bossRoutine = null;
        activeBoss = null;

        BossContext.EndBoss();
    }

    // --------------------------------------------------
    // Phase Flow
    // --------------------------------------------------

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

        BossContext.EndBoss();
    }


}
