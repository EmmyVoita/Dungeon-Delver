using UnityEngine;
using System.Collections;

public class DoorWallMoverSpawner : MonoBehaviour
{
    [Header("Instruction Message Settings")]
    public GameObject instructionCanvasPrefab;
    public float messageDuration = 2.0f;   
    public string displayMessage = "REPEAT"; 

    public BackgroundPulseOnJump backgroundPulse;
    [Header("Prefab Options (Random Selection)")]
    public GameObject[] wallPrefabs;

    [Header("Movement Settings")]
    public Vector2 fireDirection = Vector2.left;
    public float moveSpeed = 6f;
    public float lifeDuration = 4f;
    public float OpenCloseBaseDuration = 0.6f;
    public float OpenCloseRandomOffset = 0.3f;

    [Header("Spawn Settings")]
    public bool autoSpawnOnStart = true;
    public Vector3 spawnOffset = Vector3.zero;

    [Header("Multi Spawn Settings")]
    public int spawnCount = 4;          // how many to spawn back to back
    public float spawnInterval = 0.5f;  // seconds between each spawn

     private int lastPrefabIndex = -1;   // 🔹 remember last used prefab

    void Start()
    {
        ObstacleManager.Instance.RegisterObstacle(gameObject);

        if (autoSpawnOnStart)
        {
            StartCoroutine(SpawnSequence());
        }
    }

    private IEnumerator ShowInstructionMessage(string message = null)
    {
        bool finished = false;

        var canvas = Instantiate(instructionCanvasPrefab);
        canvas.GetComponent<InstructionCanvas>()
            .ShowMessage(message ?? displayMessage, messageDuration,() => finished = true);

        // Wait here until canvas calls callback
        while (!finished)
            yield return null;
    }

    IEnumerator SpawnSequence()
    {
        yield return StartCoroutine(backgroundPulse.ScaleIn());
        StartCoroutine(ShowInstructionMessage("JUMP"));

        for (int i = 0; i < spawnCount; i++)
        {
            SpawnRandomWall();
            yield return new WaitForSeconds(spawnInterval);
        }

        yield return new WaitForSeconds(1f);
        yield return StartCoroutine(backgroundPulse.ScaleOut());
        ObstacleManager.Instance.UnregisterObstacle(gameObject);
        Destroy(gameObject);
    }

    public void SpawnRandomWall()
    {
        if (wallPrefabs == null || wallPrefabs.Length == 0)
        {
            Debug.LogWarning("No wall prefabs assigned!");
            return;
        }

        // 🔹 Pick a random index, but never the same as last time
        int index;
        do
        {
            index = Random.Range(0, wallPrefabs.Length);
        }
        while (index == lastPrefabIndex && wallPrefabs.Length > 1);

        lastPrefabIndex = index;  // update last used

        GameObject prefab = wallPrefabs[index];

        GameObject obj = Instantiate(
            prefab,
            transform.position + spawnOffset,
            Quaternion.identity
        );

        DoorWall wall = obj.GetComponent<DoorWall>();
        if (wall != null)
        {
            wall.Init(fireDirection, moveSpeed, lifeDuration, OpenCloseBaseDuration, OpenCloseRandomOffset);
        }
        else
        {
            Debug.LogError("Prefab missing MovingWall component: " + prefab.name);
        }
    }
}
