using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class CritWindowVisualController : MonoBehaviour
{
    public enum CritWindowVisualState
    {
        Normal,
        Increased,
        Decreased
    }

    [System.Serializable]
    public struct VisualStateData
    {
        public CritWindowVisualState state;
        public Sprite sprite;
        public Color tint;
    }

    [Header("References")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Visual States")]
    [SerializeField] private List<VisualStateData> visualStates;

    [Header("Crit Window Settings")]
    [Tooltip("Base crit window value (before upgrades)")]
    [SerializeField] private float baseCritWindow = 1f;

    [Tooltip("How much difference is needed before visuals change")]
    [SerializeField] private float visualThreshold = 0.05f;

    private Dictionary<CritWindowVisualState, VisualStateData> lookup;
    private CritWindowVisualState currentState = CritWindowVisualState.Normal;
    [SerializeField] private float delta;
    [SerializeField] private float modified;

    private void Awake()
    {
        lookup = new Dictionary<CritWindowVisualState, VisualStateData>();
        foreach (var v in visualStates)
            lookup[v.state] = v;

        ApplyState(CritWindowVisualState.Normal);
    }

    private void OnEnable()
    {
        UpgradeManager.OnScoreContextChanged += HandleScoreContextChanged;

         // 🔑 Pull current state immediately
        //ResolveFromUpgradeManager();
    }



    private void OnDisable()
    {
        UpgradeManager.OnScoreContextChanged -= HandleScoreContextChanged;
    }

    private void Start()
    {
        StartCoroutine(Setup());
    }

    private IEnumerator Setup()
    {
        // wait a frame to ensure UpgradeManager is initialized
        yield return new WaitForSeconds(0.1f);
        ResolveFromUpgradeManager();
    }   


    private void HandleScoreContextChanged(LiveScoreState _)
    {
        ResolveFromUpgradeManager();
    }


    private void ResolveFromUpgradeManager()
    {
        if (UpgradeManager.Instance == null)
            return;

        modified = UpgradeManager.Instance.ModifyCritWindow(baseCritWindow);

        delta = modified - baseCritWindow;

        CritWindowVisualState newState = CritWindowVisualState.Normal;

        if (delta >= visualThreshold)
            newState = CritWindowVisualState.Increased;
        else if (delta <= -visualThreshold)
            newState = CritWindowVisualState.Decreased;

        if (newState != currentState)
            ApplyState(newState);
    }

    private void ApplyState(CritWindowVisualState state)
    {
        currentState = state;

        if (!lookup.TryGetValue(state, out var data))
        {
            Debug.LogWarning($"No visual data for crit window state {state}");
            return;
        }

        spriteRenderer.sprite = data.sprite;
        spriteRenderer.color = data.tint;
    }
}
