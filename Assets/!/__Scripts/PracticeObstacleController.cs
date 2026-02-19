using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class PracticeObstacleController : MonoBehaviour
{
    public float respawnDelay = 1.0f;
    public float firstSpawnDelay = 3f;

    private bool spawnLoopActive = false;
    private GameObject simulatedObstacle;

    private void OnEnable()
    {
        ObstacleManager.OnObstacleCleared += HandleObstacleCleared;
    }

    private void OnDisable()
    {
        ObstacleManager.OnObstacleCleared -= HandleObstacleCleared;
    }

    void Start()
    {
        if(GameSceneLoader.PendingConfig != null && GameSceneLoader.PendingConfig.Mode == GameMode.ObstaclePractice)
        {
            // 1️⃣ Create a simulated temporary obstacle
            simulatedObstacle = new GameObject("DebugObstacle_SIMULATED");
            ObstacleManager.Instance.RegisterObstacle(simulatedObstacle);

            // 2️⃣ After delay, remove simulated obstacle and spawn real one
            StartCoroutine(InitialSpawnRoutine());

            Player.Instance.UseEightDirections = GameSceneLoader.PendingConfig.DirectionMode == JumpDirectionMode.EightDirectional;  
        }
    }

    void Update()
    {
        /*
        if (InputBindingManager.Instance.GetKeyInput(InputActionType.Back))
        {
            SceneManager.LoadScene(SceneNames.MainMenu);
        }
        */
    }

    private IEnumerator InitialSpawnRoutine()
    {
        yield return new WaitForSeconds(firstSpawnDelay);

        SpawnNewObstacle();
    }

    void SpawnNewObstacle()
    {
        if (GameSceneLoader.PendingConfig.PracticeObstacle == null)
        {
            Debug.LogError("🚨 No obstacle selected for practice!");
            return;
        }

        Instantiate(
            GameSceneLoader.PendingConfig.PracticeObstacle.obstaclePrefab,
            Vector3.zero,
            Quaternion.identity
        );
    }

    private void HandleObstacleCleared()
    {
        if(GameSceneLoader.PendingConfig == null) return;
        if(GameSceneLoader.PendingConfig.Mode != GameMode.ObstaclePractice) return;

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
