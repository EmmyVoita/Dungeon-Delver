using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;
using UnityEditor;

public class ObstacleManager : MonoBehaviour
{
    public enum TestMode
    {
        Off,
        RepeatOnStart
    }

    public static ObstacleManager Instance { get; private set; }

    public static event Action OnFirstObstacleAppeared;
    public static event Action OnAllObstaclesCleared;
    public static event Action OnObstacleCleared;
    public static Action<GameObject> OnObstacleSpawned;

    private readonly HashSet<GameObject> activeObstacles = new();


    // ========================================================
    //  Main Manager Setup
    // ========================================================
  
    public bool AnyActive => activeObstacles.Count > 0;
    public int ActiveCount => activeObstacles.Count;
    public IEnumerable<GameObject> ActiveObstacles => activeObstacles;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnDestroy()
    {
        activeObstacles.Clear();
    }


    // ========================================================
    //  Register / Unregister
    // ========================================================
    public void RegisterObstacle(GameObject obstacle)
    {
        bool wasEmpty = activeObstacles.Count == 0;

        activeObstacles.Add(obstacle);
        Debug.Log($"Obstacle registered. Count = {activeObstacles.Count}");

        if (wasEmpty)
            OnFirstObstacleAppeared?.Invoke();
    }

    public void UnregisterObstacle(GameObject obstacle, float delay = 0f)
    {
        if (delay > 0f)
        {
            StartCoroutine(UnregisterDelayed(obstacle, delay));
            return;
        }

        if (activeObstacles.Remove(obstacle))
        {
            Debug.Log($"Obstacle unregistered. Count = {activeObstacles.Count}");
            OnObstacleCleared?.Invoke();

            if (activeObstacles.Count == 0)
                OnAllObstaclesCleared?.Invoke();
        }
    }

    private IEnumerator UnregisterDelayed(GameObject obstacle, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (activeObstacles.Remove(obstacle))
        {
            Debug.Log($"Obstacle unregistered (delayed). Count = {activeObstacles.Count}");
            OnObstacleCleared?.Invoke();

            if (activeObstacles.Count == 0)
                OnAllObstaclesCleared?.Invoke();
        }
    }


    // ======================================================================
    // == DEV FEATURES =======================================================
    // ======================================================================


    [Header("Dev Testing")]
    public TestMode testMode = TestMode.Off;
    [SerializeField] private bool simulateObstacle = false;
    private GameObject _debugObstacle;

    [SerializeField] private GameObject _spawnObstacle;
    [SerializeField] private BossDefinition bossDefinition;

    private void Start()
    {
        switch(testMode)
        {
            case TestMode.Off:
            break;
            case TestMode.RepeatOnStart:
            GameStateManager.Instance.SetState(GameState.Practice);
            StartAutoLoopNow();
            break;
        }
    }


    // ------------------------------------------------------
    // Context Menu: Toggle Simulated Obstacle
    // ------------------------------------------------------
    [ContextMenu("Toggle Simulated Obstacle")]
    public void ToggleSimulatedObstacle()
    {
        if (_debugObstacle == null)
        {
            _debugObstacle = new GameObject("DebugObstacle_SIMULATED");
            RegisterObstacle(_debugObstacle);
            Debug.Log("⚠️ Simulated obstacle added (DEV).");
        }
        else
        {
            UnregisterObstacle(_debugObstacle);
            DestroyImmediate(_debugObstacle);
            _debugObstacle = null;
            Debug.Log("🟢 Simulated obstacle removed (DEV).");
        }
    }


    // ------------------------------------------------------
    // Context Menu: Spawn Test Obstacle Once
    // ------------------------------------------------------
    [ContextMenu("Spawn Test Obstacle")]
    public void SpawnTestObstacle()
    {
        StartCoroutine(SpawnTestObstacleCoroutine());
    }

    private IEnumerator SpawnTestObstacleCoroutine()
    {
        yield return new WaitForSeconds(2f);
        GameObject obj = Instantiate(_spawnObstacle, Vector3.zero, Quaternion.identity);
        OnObstacleSpawned?.Invoke(obj);  
    }


    // ======================================================================
    // == AUTO LOOP DEV MODE
    // ======================================================================
    [Header("Dev Auto-Loop")]
    [SerializeField] private bool autoLoopObstacle = false;
    [SerializeField] private float autoLoopDelay = 1f;


    private void OnEnable()
    {
        OnAllObstaclesCleared += HandleAutoLoop;
    }

    private void OnDisable()
    {
        OnAllObstaclesCleared -= HandleAutoLoop;
    }

    private void HandleAutoLoop()
    {
        if (!autoLoopObstacle || _spawnObstacle == null)
            return;

        Debug.Log("🔁 Auto-looping obstacle (DEV mode enabled)");
        StartCoroutine(SpawnLoopCoroutine());
    }

    private IEnumerator SpawnLoopCoroutine()
    {
        yield return new WaitForSeconds(autoLoopDelay);

        GameObject obj = Instantiate(_spawnObstacle, Vector3.zero, Quaternion.identity);
        RegisterObstacle(obj);
        OnObstacleSpawned?.Invoke(obj);
    }


    // ------------------------------------------------------
    // Context Menu: Toggle Auto-Loop Mode
    // ------------------------------------------------------
    [ContextMenu("Toggle Auto-Loop Mode")]
    private void ToggleAutoLoop()
    {
        autoLoopObstacle = !autoLoopObstacle;
        Debug.Log("Auto-loop mode: " + (autoLoopObstacle ? "ENABLED" : "DISABLED"));
    }


    // ------------------------------------------------------
    // Context Menu: Start Auto Loop Immediately
    // ------------------------------------------------------
    [ContextMenu("Start Auto-Loop Now")]
    private void StartAutoLoopNow()
    {
        autoLoopObstacle = true;

        if (bossDefinition != null)
        {
            BossManager.Instance.StartBoss(bossDefinition);
        }

        Debug.Log("Auto-loop ENABLED. Spawning first obstacle…");

        GameObject obj = Instantiate(_spawnObstacle, Vector3.zero, Quaternion.identity);
        RegisterObstacle(obj);
    }


}
