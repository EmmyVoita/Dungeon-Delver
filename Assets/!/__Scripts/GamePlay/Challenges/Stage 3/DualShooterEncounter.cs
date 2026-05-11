using UnityEngine;
using System.Collections;

public class DualShooterEncounter : ChallengeBase
{
    [SerializeField] private LaneDodgerConfig config;

    [Header("References")]
    [SerializeField] private GameObject leftShooterPrefab;
    [SerializeField] private GameObject rightShooterPrefab;

    [Header("Timing")]
    [SerializeField] private float encounterDuration = 10f;

    [Header("Positions")]
    [SerializeField] private float spawnX = 4.75f;
    
    private GameObject leftShooter;
    private GameObject rightShooter;

    void Start()
    {
        Begin();
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

        End();
    }

    public override void Begin(object config = null)
    {
        base.Begin(this.config);
        SpawnShooters();
        StartCoroutine(EncounterRoutine());
    }

    public override void End()
    {
        base.End();
        Destroy(gameObject);
    }
}