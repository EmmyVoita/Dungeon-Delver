using System.Collections.Generic;
using UnityEngine;

public class ChargeMeterManager : MonoBehaviour
{
    public static ChargeMeterManager Instance { get; private set; }

    [SerializeField] private Transform meterContainer;
    [SerializeField] private ChargeMeterUI meterPrefab;

    private readonly Dictionary<int, ChargeMeterHandle> _meters = new();
    private int _nextId;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public ChargeMeterHandle CreateMeter(
        string displayName,
        Sprite icon,
        int maxCharge)
    {
        int id = _nextId++;

        ChargeMeterUI view = Instantiate(
            meterPrefab,
            meterContainer
        );

        ChargeMeterHandle handle = new ChargeMeterHandle(
            id,
            maxCharge,
            view,
            RemoveMeter
        );

        view.Initialize(displayName, icon,maxCharge);
        view.SetCharge(0, maxCharge);

        _meters.Add(id, handle);

        return handle;
    }

    public void RemoveMeter(int id)
    {
        if (!_meters.TryGetValue(id, out ChargeMeterHandle handle))
            return;

        _meters.Remove(id);

        if (handle.View != null)
            Destroy(handle.View.gameObject);
    }
}