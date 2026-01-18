using System.Collections.Generic;
using UnityEngine;

public class ArrowManager : MonoBehaviour
{
    public static ArrowManager Instance { get; private set; }

    private List<ArrowBase> activeArrows = new List<ArrowBase>();

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    public void RegisterArrow(ArrowBase arrow)
    {
        if (!activeArrows.Contains(arrow))
            activeArrows.Add(arrow);
    }

    public void UnregisterArrow(ArrowBase arrow)
    {
        activeArrows.Remove(arrow);
    }

    public ArrowBase GetRandomArrow()
    {
        if (activeArrows.Count == 0) return null;
        return activeArrows[Random.Range(0, activeArrows.Count)];
    }

    public int ActiveCount => activeArrows.Count;
}
