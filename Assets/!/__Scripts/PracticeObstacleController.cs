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
        // 1️⃣ Create a simulated temporary obstacle
        simulatedObstacle = new GameObject("DebugObstacle_SIMULATED");
        ObstacleManager.Instance.RegisterObstacle(simulatedObstacle);

        // 2️⃣ After delay, remove simulated obstacle and spawn real one
        StartCoroutine(InitialSpawnRoutine());

        Player.Instance.UseEightDirections = ObstaclePracticeSession.DirectionMode == JumpDirectionMode.EightWay;  
    }

    void Update()
    {
        if (InputBindingManager.Instance.GetKeyInput(InputActionType.Back))
        {
            SceneManager.LoadScene(SceneNames.MainMenu);
        }
    }

    private IEnumerator InitialSpawnRoutine()
    {
        yield return new WaitForSeconds(firstSpawnDelay);

        // 3️⃣ Remove simulated obstacle to allow real spawns
        //ObstacleManager.Instance.UnregisterObstacle(simulatedObstacle);
        //Destroy(simulatedObstacle);

        // 4️⃣ Spawn first real obstacle
        SpawnNewObstacle();
    }

    void SpawnNewObstacle()
    {
        if (ObstaclePracticeSession.SelectedObstacle == null)
        {
            Debug.LogError("🚨 No obstacle selected for practice!");
            return;
        }

        Instantiate(
            ObstaclePracticeSession.SelectedObstacle.obstaclePrefab,
            Vector3.zero,
            Quaternion.identity
        );

        Debug.Log($"🧪 Spawned obstacle: {ObstaclePracticeSession.SelectedObstacle.displayName}");
    }

    private void HandleObstacleCleared()
    {
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
