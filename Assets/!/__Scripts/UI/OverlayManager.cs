using UnityEngine;
using System;

public enum OverlayState
{
    None,
    Pause
}

public class OverlayManager : MonoBehaviour
{
    public static OverlayManager Instance { get; private set; }

    public static event Action<OverlayState, OverlayState> OnOverlayChanged;

    public OverlayState CurrentOverlay { get; private set; } =
        OverlayState.None;

    public bool IsPaused => CurrentOverlay == OverlayState.Pause;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void ShowOverlay(OverlayState newOverlay)
    {
        if (CurrentOverlay == newOverlay)
            return;

        OverlayState previous = CurrentOverlay;
        CurrentOverlay = newOverlay;

        OnOverlayChanged?.Invoke(previous, CurrentOverlay);
    }

    public void CloseOverlay()
    {
        ShowOverlay(OverlayState.None);
    }
}