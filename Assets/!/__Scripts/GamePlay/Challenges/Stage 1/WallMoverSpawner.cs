using UnityEngine;
using System.Collections;

public class WallMoverSpawner : ChallengeBase
{
    [Header("Audio")]
    public SoundEffect spawnBeep;
    public float audioDelay = 1.0f;
    public float beepPitchStart = 1f;
    public float beepPitchStep = 0.1f;
    private float currentPitch;


    [Header("Movement Settings")]
    public float baseMoveSpeed = 6f;
    public float speedMultiplier = 1f;
    public float speedVariation = 0.3f;
    public float lifeDuration = 4f;
    public float unRegisterDelay = 0.5f;
    public bool randomlyFlipWalls = true;

    [Header("Prefab Options")]
    public GameObject[] wallPrefabs;

    [Header("Spawn Directions")] 
    public Vector2[] fireDirections;  // 🔹 Array of directions (set in Inspector)

    [Header("Spawn Settings")]
    public bool autoSpawnOnStart = true;
    public float spawnOffset = 10;
    public int spawnCount = 4;
    public float spawnInterval = 0.5f;

    private int lastPrefabIndex = -1;
    private int lastDirectionIndex = -1;


    void Start()
    {
        currentPitch = beepPitchStart;

        Begin();
    }

    IEnumerator SpawnSequence()
    {
        for (int i = 0; i < spawnCount; i++)
        {
            SpawnRandomWall();
            yield return new WaitForSeconds(spawnInterval);
        }

        yield return new WaitForSeconds(unRegisterDelay);
        End();
    }

    private IEnumerator AudioWithDelay()
    {
        yield return new WaitForSeconds(audioDelay);
        AudioHelpers.PlaySoundEffect(spawnBeep, Camera.main.transform.position, currentPitch);
    }

    public void SpawnRandomWall()
    {
        if (wallPrefabs == null || wallPrefabs.Length == 0)
        {
            Debug.LogWarning("No wall prefabs assigned!");
            return;
        }

        StartCoroutine(AudioWithDelay());

        currentPitch += beepPitchStep; // increase pitch for next beep

   

        
        // 🔹 Randomly pick a prefab (avoid repeats)
        int prefabIndex;
        do { prefabIndex = Random.Range(0, wallPrefabs.Length); }
        while (prefabIndex == lastPrefabIndex && wallPrefabs.Length > 1);

        lastPrefabIndex = prefabIndex;

        // 🔹 Pick a direction (avoid repeats)
        int dirIndex;
        if (fireDirections.Length > 0)
        {
            do { dirIndex = Random.Range(0, fireDirections.Length); }
            while (dirIndex == lastDirectionIndex && fireDirections.Length > 1);

            lastDirectionIndex = dirIndex;
        }
        else
        {
            dirIndex = -1; // fallback
        }

        Vector2 chosenDirection = (dirIndex >= 0) ? fireDirections[dirIndex] : Vector2.left;

        bool flipDirection = randomlyFlipWalls ? Random.value > 0.5f : false;

        // 🔹 Spawn object at offset in chosen direction
        GameObject obj = Instantiate(
            wallPrefabs[prefabIndex],
            transform.position + spawnOffset * (Vector3)chosenDirection.normalized,
            Quaternion.identity
        );

        

        // 🔹 Initialize movement
        MovingWall wall = obj.GetComponent<MovingWall>();
        if (wall != null)
        {
            wall.Init(chosenDirection, baseMoveSpeed, lifeDuration, speedMultiplier, speedVariation);
            if (flipDirection)
            {
                obj.transform.Rotate(0f, 0f, 180f);
            }
        }
        else
        {
            Debug.LogError("Prefab missing MovingWall component: " + wallPrefabs[prefabIndex].name);
        }
    }

    public override void Begin(object config = null)
    {
        base.Begin();
        if (autoSpawnOnStart)
            StartCoroutine(SpawnSequence());
    }

    public override void End()
    {
        base.End();
        Destroy(gameObject);
    }

}
