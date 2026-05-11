using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;
using UnityEditor;

public class ObstacleManager : MonoBehaviour
{
  
    public static ObstacleManager Instance { get; private set; }

    public static event Action OnFirstObstacleAppeared;
    public static event Action OnAllObstaclesCleared;
    public static event Action OnObstacleCleared;
    public static Action<GameObject> OnObstacleSpawned;

    private  List<ChallengeBase> activeObstacles = new();


    // ========================================================
    //  Main Manager Setup
    // ========================================================
  
    public bool AnyActive => ActiveCount > 0;
    public int ActiveCount => activeObstacles.Count;
    public IEnumerable<ChallengeBase> ActiveObstacles => activeObstacles;
    [SerializeField] private PracticeObstacleController practiceController;
 
    private bool _playerStateDirty = false;
    private bool _resolvingPlayerState = false;

    public bool TestOn => practiceController.TestOn;


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


        if (Instance == this)
            Instance = null;

    }

    private void MarkPlayerStateDirty()
    {
        _playerStateDirty = true;

        if(!_resolvingPlayerState)
            StartCoroutine(ResolvePlayerStateEndOfFrame());
    }

    private IEnumerator ResolvePlayerStateEndOfFrame()
    {
        _resolvingPlayerState = true;

        yield return null;

        if(_playerStateDirty)
        {
            _playerStateDirty = false;
            UpdatePlayerStateImmediate();
        }

        _resolvingPlayerState = false;
    }

    // ========================================================
    //  Register / Unregister
    // ========================================================
    public void RegisterObstacle(GameObject obj)
    {
        var challenge = obj.GetComponent<ChallengeBase>();
        if (challenge == null) return;

        bool wasEmpty = activeObstacles.Count == 0;

        activeObstacles.Add(challenge);

        if (wasEmpty)
            OnFirstObstacleAppeared?.Invoke();

        MarkPlayerStateDirty();
    }

    public void UnregisterObstacle(GameObject obj, float delay = 0f)
    {
        var challenge = obj.GetComponent<ChallengeBase>();

        if (delay > 0f)
        {
            StartCoroutine(UnregisterDelayed(challenge, delay));
            return;
        }

        if (activeObstacles.Remove(challenge))
        {
            OnObstacleCleared?.Invoke();

            MarkPlayerStateDirty();

            if (activeObstacles.Count == 0)
                OnAllObstaclesCleared?.Invoke();
        }
    }

    private void UpdatePlayerStateImmediate()
    {
        if (activeObstacles.Count == 0)
        {
            Player.Instance.SetPlayerControlState(Player.PlayerControlState.Normal);
            return;
        }

        ChallengeBase best = null;

        foreach (var obstacle in activeObstacles)
        {
            if (obstacle == null) continue;

            if (best == null || 
                obstacle.Priority > best.Priority ||
                (obstacle.Priority == best.Priority && 
                activeObstacles.IndexOf(obstacle) > activeObstacles.IndexOf(best)))
            {
                best = obstacle;
            }
        }

        if (best == null)
        {
            Player.Instance.SetPlayerControlState(Player.PlayerControlState.Normal);
            return;
        }

        Player.Instance.SetPlayerControlState(best.ControlState,best.Config);
    }

    private IEnumerator UnregisterDelayed(ChallengeBase obstacle, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (activeObstacles.Remove(obstacle))
        {
            OnObstacleCleared?.Invoke();

            if (activeObstacles.Count == 0)
                OnAllObstaclesCleared?.Invoke();

            MarkPlayerStateDirty();
        }
    }

    public void ForceClearAll()
    {
        var snapshot = new List<ChallengeBase>(activeObstacles);

        foreach (var o in snapshot)
        {
            if (o != null)
                o.End();
        }

        activeObstacles.Clear();
        MarkPlayerStateDirty();
    }

    


    // ======================================================================
    // == DEV FEATURES =======================================================
    // ======================================================================

    /*
    [Header("Dev Testing")]

    

    [SerializeField] private GameObject _spawnObstacle;
    [SerializeField] private BossDefinition bossDefinition;

    private GameObject _debugObstacle;

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

        

        Debug.Log("Auto-loop ENABLED. Spawning first obstacle…");

        GameObject obj = Instantiate(_spawnObstacle, Vector3.zero, Quaternion.identity);
        RegisterObstacle(obj);
    }
    */

}
