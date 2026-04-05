using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class PracticeObstacleController : MonoBehaviour
{
    public float respawnDelay = 1.0f;
    public float firstSpawnDelay = 3f;
    private bool spawnLoopActive = false;

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

    private void HandleStateChanged(GameState previousState, GameState newState)
    {
        if(newState == GameState.Practice && previousState != newState)
        {
            GameStateManager.Instance.SetState(GameState.Practice);

            // 2️⃣ After delay, remove simulated obstacle and spawn real one
            StartCoroutine(InitialSpawnRoutine());

            Player.Instance.UseEightDirections = GameSessionBootstrap.Config.DirectionMode == JumpDirectionMode.EightDirectional;  
        }
    }


    void Update()
    {
        if (GameStateManager.Instance.CurrentState == GameState.Practice && InputBindingManager.Instance.GetKeyInput(InputActionType.Back))
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
        if (GameSessionBootstrap.Config == null || GameSessionBootstrap.Config.PracticeObstacle.prefab == null) 
            return;

        Instantiate(
            GameSessionBootstrap.Config.PracticeObstacle.prefab,
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
