using System.Collections.Generic;
using UnityEngine;

public class OrbitingTreasure : MonoBehaviour
{
    public enum OrbitCenter
    {
        WorldCenter,
        Player
    }

    [Header("References")]
    [SerializeField] private OrbitingRelic relicPrefab;
    [SerializeField] private OrbitCenter orbitCenter;

    [Header("Orbit Settings")]
    [SerializeField, Min(1)] private int maxRelicCount = 3;
    [SerializeField] private float orbitRadius = 3f;
    [SerializeField] private float orbitSpeed = 90f;

    private readonly List<OrbitingRelic> activeRelics = new();
    private OrbitingRelic[] _relicSlots;

    private float _orbitAngle;

    public int ActiveRelicCount => activeRelics.Count;
    public int MaxRelicCount => maxRelicCount;
    public bool IsFull => ActiveRelicCount >= maxRelicCount;

    private void Awake()
    {
        maxRelicCount = Mathf.Max(1, maxRelicCount);
        _relicSlots = new OrbitingRelic[maxRelicCount];
    }

    public bool AddRelic(int currencyReward)
    {
        if (relicPrefab == null)
        {
            Debug.LogWarning(
                $"{nameof(OrbitingTreasure)} has no relic prefab assigned."
            );

            return false;
        }

        int emptySlot = FindEmptySlot();

        if (emptySlot < 0)
            return false;

        Vector3 center = GetOrbitCenter();
        Vector2 spawnPosition = GetSlotPosition(emptySlot, center);

        OrbitingRelic relic = Instantiate(
            relicPrefab,
            spawnPosition,
            Quaternion.identity,
            transform
        );

        relic.Initialize(
            this,
            currencyReward,
            emptySlot
        );

        _relicSlots[emptySlot] = relic;
        activeRelics.Add(relic);

        return true;
    }

    public void NotifyRelicDestroyed(OrbitingRelic relic)
    {
        if (relic == null)
            return;

        int slotIndex = relic.SlotIndex;

        if (slotIndex >= 0 &&
            slotIndex < _relicSlots.Length &&
            _relicSlots[slotIndex] == relic)
        {
            _relicSlots[slotIndex] = null;
        }

        activeRelics.Remove(relic);
    }

    public void ClearRelics()
    {
        for (int i = activeRelics.Count - 1; i >= 0; i--)
        {
            if (activeRelics[i] != null)
                Destroy(activeRelics[i].gameObject);
        }

        activeRelics.Clear();

        for (int i = 0; i < _relicSlots.Length; i++)
            _relicSlots[i] = null;
    }

    private void Update()
    {
        if (activeRelics.Count == 0)
            return;

        _orbitAngle = Mathf.Repeat(
            _orbitAngle + orbitSpeed * Time.deltaTime,
            360f
        );

        UpdateRelicPositions();
    }

    private void UpdateRelicPositions()
    {
        Vector3 center = GetOrbitCenter();

        for (int slotIndex = 0;
             slotIndex < _relicSlots.Length;
             slotIndex++)
        {
            OrbitingRelic relic = _relicSlots[slotIndex];

            if (relic == null)
                continue;

            Vector2 targetPosition =
                GetSlotPosition(slotIndex, center);

            relic.SetOrbitPosition(targetPosition);
        }
    }

    private Vector2 GetSlotPosition(
        int slotIndex,
        Vector3 center)
    {
        float angleStep = 360f / maxRelicCount;

        float angle =
            _orbitAngle + slotIndex * angleStep;

        float radians =
            angle * Mathf.Deg2Rad;

        Vector2 offset = new Vector2(
            Mathf.Cos(radians),
            Mathf.Sin(radians)
        ) * orbitRadius;

        return (Vector2)center + offset;
    }

    private int FindEmptySlot()
    {
        for (int i = 0; i < _relicSlots.Length; i++)
        {
            if (_relicSlots[i] == null)
                return i;
        }

        return -1;
    }

    private Vector3 GetOrbitCenter()
    {
        if (orbitCenter == OrbitCenter.Player &&
            Player.Instance != null)
        {
            return Player.Instance.transform.position;
        }

        return Vector3.zero;
    }
}