using UnityEngine;
using System.Collections;

public class DualShooterEncounter : MonoBehaviour
{
    [SerializeField] private LaneDodgerConfig config;

    [Header("References")]
    [SerializeField] private GameObject leftShooterPrefab;
    [SerializeField] private GameObject rightShooterPrefab;

    [Header("Timing")]
    [SerializeField] private float encounterDuration = 10f;

    [Header("Positions")]
    [SerializeField] private float spawnX = 4.75f;

    private bool registered = false;

    private GameObject leftShooter;
    private GameObject rightShooter;

    void Start()
    {
        // ✅ Register ONCE
        ObstacleManager.Instance.RegisterObstacle(gameObject);
        registered = true;

        // ✅ Set control state ONCE
        Player.Instance.SetPlayerControlState(Player.PlayerControlState.LaneDodger, config);

        SpawnShooters();

        StartCoroutine(EncounterRoutine());
    }

    void SpawnShooters()
    {
        // LEFT
        Vector3 leftPos = new Vector3(-spawnX, 0f, 0f);
        leftShooter = Instantiate(leftShooterPrefab, leftPos, Quaternion.identity, transform);

        // RIGHT
        Vector3 rightPos = new Vector3(spawnX, 0f, 0f);
        rightShooter = Instantiate(rightShooterPrefab, rightPos, Quaternion.identity, transform);
    }

    IEnumerator EncounterRoutine()
    {
        yield return new WaitForSeconds(encounterDuration);

        Cleanup();
    }

    void Cleanup()
    {
        if (registered && ObstacleManager.Instance != null)
        {
            ObstacleManager.Instance.UnregisterObstacle(gameObject);

            Player.Instance.SetPlayerControlState(Player.PlayerControlState.Normal);

            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        if (registered && ObstacleManager.Instance != null)
        {
            ObstacleManager.Instance.UnregisterObstacle(gameObject);
        }
    }
}