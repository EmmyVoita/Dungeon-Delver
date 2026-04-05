using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpikeLaneObstacle : MonoBehaviour
{
    [SerializeField] private LaneDodgerConfig config;

    [Header("Prefabs")]
    [SerializeField] private GameObject spikePrefab;
    [SerializeField] private GameObject rewardPrefab; // optional

    [Header("Timing")]
    [SerializeField] private float startWindupTime = 1.0f;
    [SerializeField] private float minWindupTime = 0.3f;
    [SerializeField] private float windupDecreasePerCycle = 0.1f;
    [SerializeField] private float activeTime = 0.4f;
    [SerializeField] private int cycles = 6;

    [Header("Lane Rules")]
    [SerializeField] private int minOpenLanes = 1;
    [SerializeField] private int maxOpenLanes = 2;

    [Header("Special Lane")]
    [SerializeField] private bool enableSpecialLane = true;

    [Header("Audio")]
    [SerializeField] private SoundEffect windupSound;
    [SerializeField] private int windupCount;
    [SerializeField] private float pitchStep = 0.1f;
    [SerializeField] private SoundEffect attackSound;

    private List<SpikeLane> spikes = new List<SpikeLane>();
    private bool registered = false;

    void Start()
    {
        ObstacleManager.Instance.RegisterObstacle(gameObject);
        registered = true;

        Player.Instance.SetPlayerControlState(Player.PlayerControlState.LaneDodger, config);

        SpawnSpikes();
        StartCoroutine(ObstacleRoutine());
    }

    void SpawnSpikes()
    {
        for (int i = 0; i < config.maxLanes; i++)
        {
            float y = GetLaneY(i);

            GameObject obj = Instantiate(spikePrefab, new Vector3(0, y, 0), Quaternion.identity, transform);

            SpikeLane spike = obj.GetComponent<SpikeLane>();
            spike.SetLaneIndex(i);

            int direction = Random.Range(0f,1f) > 0.5f ? -1 : 1;
            spike.SetDirection(direction);

            spikes.Add(spike);
        }
    }

    IEnumerator ObstacleRoutine()
    {
        float currentWindup = startWindupTime;

        for (int cycle = 0; cycle < cycles; cycle++)
        {
            // 1. Pick open lanes
            List<int> openLanes = PickOpenLanes();

            // 2. Optional special lane
            int specialLane = -1;
            if (enableSpecialLane)
            {
                specialLane = openLanes[Random.Range(0, openLanes.Count)];
                SpawnReward(specialLane);
            }

            // 3. Telegraph
            foreach (var spike in spikes)
            {
                bool isOpen = openLanes.Contains(spike.LaneIndex);
                spike.SetTelegraph(isOpen, spike.LaneIndex == specialLane);
            }

            for(int i = 0; i < windupCount; i++)
            {
                float pitchMult = 1.0f + pitchStep * i;
                AudioHelpers.PlaySoundEffect(windupSound,transform.position,pitchMult);
                yield return new WaitForSeconds(currentWindup / (float) windupCount);
            }

            //yield return new WaitForSeconds(currentWindup);

            // 4. Activate (close spikes)
            foreach (var spike in spikes)
            {
                bool isOpen = openLanes.Contains(spike.LaneIndex);
                spike.SetActiveState(!isOpen);
            }

            AudioHelpers.PlaySoundEffect(attackSound,transform.position);

            // 5. Damage check
            CheckPlayerDamage(openLanes);

           

            yield return new WaitForSeconds(activeTime);

            // 6. Reset
            foreach (var spike in spikes)
            {
                spike.ResetState();
            }

            // 7. Decrease windup
            currentWindup = Mathf.Max(minWindupTime, currentWindup - windupDecreasePerCycle);
        }

        Cleanup();
    }

    List<int> PickOpenLanes()
    {
        int openCount = Random.Range(minOpenLanes, maxOpenLanes + 1);

        List<int> lanes = new List<int>();
        List<int> all = new List<int>();

        for (int i = 0; i < config.maxLanes; i++)
            all.Add(i);

        for (int i = 0; i < openCount; i++)
        {
            int index = Random.Range(0, all.Count);
            lanes.Add(all[index]);
            all.RemoveAt(index);
        }

        return lanes;
    }

    void SpawnReward(int lane)
    {
        if (rewardPrefab == null) return;

        float y = GetLaneY(lane);
        Instantiate(rewardPrefab, new Vector3(0, y, 0), Quaternion.identity, transform);
    }

    void CheckPlayerDamage(List<int> openLanes)
    {
        int playerLane = Player.Instance.CurrentLane;

        if (!openLanes.Contains(playerLane))
        {
            Player.Instance.DamageSelf(1);
        }
    }

    float GetLaneY(int lane)
    {
        float centerOffset = (config.maxLanes - 1) * 0.5f;
        return (lane - centerOffset) * config.laneSpacing;
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