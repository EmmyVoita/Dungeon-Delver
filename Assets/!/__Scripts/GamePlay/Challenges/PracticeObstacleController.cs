using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class PracticeObstacleController : MonoBehaviour
{
    [Header("Test")]
    [SerializeField] private ChallengeTestMode testMode = ChallengeTestMode.Off;
    [SerializeField] private GameObject defaultChallenge;
    [SerializeField] private BossDefinition testBossDefintion;



    [Header("Settings")]
    public float respawnDelay = 1.0f;
    public float firstSpawnDelay = 3f;
    private bool spawnLoopActive = false;

    public bool TestOn => testMode == ChallengeTestMode.RepeatOnStart;

    private void OnEnable()
    {
        ObstacleManager.OnObstacleCleared += HandleObstacleCleared;
        GameStateManager.OnStateChanged += HandleStateChanged;
    }

    private void OnDisable()
    {
        ObstacleManager.OnObstacleCleared -= HandleObstacleCleared;
        GameStateManager.OnStateChanged -= HandleStateChanged;
    }

    private void Start()
    {
        if(testMode == ChallengeTestMode.RepeatOnStart)
            GameStateManager.Instance.SetState(GameState.Practice);

        if (testBossDefintion != null)
        {
            BossManager.Instance.StartBoss();//testBossDefintion);
        }
    }

    private void HandleStateChanged(GameState previousState, GameState newState)
    {
        if(newState == GameState.Practice)
        {
            // 2️⃣ After delay, remove simulated obstacle and spawn real one
            StartCoroutine(InitialSpawnRoutine());

            //Player.Instance.UseEightDirections = GameSessionBootstrap.Config.DirectionMode == JumpDirectionMode.EightDirectional;  
        }
    }


    void Update()
    {
        if (GameStateManager.Instance.CurrentState == GameState.Practice && InputBindingManager.Instance.GetKeyHeld(InputActionType.Back))
        {
            SceneManager.LoadScene(SceneNames.MainMenu);
        }
    }

    private IEnumerator InitialSpawnRoutine()
    {
        yield return new WaitForSeconds(firstSpawnDelay);

        SpawnNewObstacle();
    }

    void SpawnNewObstacle()
    {
        if (GameSessionBootstrap.Config == null) 
            return;

        bool sessionValid = GameSessionBootstrap.Config.PracticeObstacle != null && GameSessionBootstrap.Config.PracticeObstacle.prefab != null;

        GameObject prefab = sessionValid ? GameSessionBootstrap.Config.PracticeObstacle.prefab : defaultChallenge;

        Instantiate(
            prefab,
            Vector3.zero,
            Quaternion.identity
        );
    }

    private void HandleObstacleCleared()
    {
        if(GameStateManager.Instance.CurrentState != GameState.Practice) return;

        if (!spawnLoopActive)
            StartCoroutine(SpawnAfterDelay(respawnDelay));
    }

    private IEnumerator SpawnAfterDelay(float delay)
    {
        spawnLoopActive = true;
        yield return new WaitForSeconds(delay);
        SpawnNewObstacle();
        spawnLoopActive = false;
    }
}
