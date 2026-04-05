

using UnityEngine;
using TMPro;

public class FrameRateManager : MonoBehaviour, IDevPanel
{
    [Header("UI")]
    [SerializeField] private GameObject statsPanel; // parent panel
    [SerializeField] private TextMeshProUGUI avgFPSText;
    [SerializeField] private TextMeshProUGUI worstFPSText;
    [SerializeField] private TextMeshProUGUI bestFPSText;
    [SerializeField] private TextMeshProUGUI maxFPSText;

    [Header("Update Settings")]
    [SerializeField] private float updateInterval = 0.5f;

    [Header("Toggle")]
    [SerializeField] private KeyCode toggleKey = KeyCode.F;
    [SerializeField] private bool startVisible = false;

    [Header("FPS Cap Settings")]
    [SerializeField] private int[] fpsSteps = { 15, 30, 45, 60, 90, 120, 240 };
    [SerializeField] private int defaultFPSIndex = 5; // 60 FPS

    private float timeAccumulator;
    private int frameCount;
    private float bestFPS = float.MinValue;
    private float worstFPS = float.MaxValue;
    private float updateTimer;
    private bool isVisible;

    private int fpsIndex;

    void Awake()
    {
        // Disable vsync so Application.targetFrameRate actually works
        QualitySettings.vSyncCount = 0;

        fpsIndex = Mathf.Clamp(defaultFPSIndex, 0, fpsSteps.Length);
        ApplyFPSCap();

        isVisible = startVisible;
        if (statsPanel != null)
            statsPanel.SetActive(isVisible);
    }

    void Update()
    {
        HandleToggle();

        if (DevPanelFocusManager.HasFocus(this))
            HandleFPSCapInput();

        TrackFPS();
    }

    private void HandleToggle()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            if (isVisible)
            {
                isVisible = false;
                statsPanel.SetActive(false);
                DevPanelFocusManager.ClearFocus(this);
            }
            else
            {
                isVisible = true;
                statsPanel.SetActive(true);
                DevPanelFocusManager.RequestFocus(this);
                ResetStats();
            }
        }
    }

    public void OnFocusGained() { }
    public void OnFocusLost()
    {
        isVisible = false;
        statsPanel.SetActive(false);
    }
    // ─────────────────────────────────────────────
    // INPUT
    // ─────────────────────────────────────────────


    private void HandleFPSCapInput()
    {
        if (!isVisible) return;

        if (Input.GetKeyDown(KeyCode.Equals) || Input.GetKeyDown(KeyCode.KeypadPlus))
            StepFPS(1);

        if (Input.GetKeyDown(KeyCode.Minus) || Input.GetKeyDown(KeyCode.KeypadMinus))
            StepFPS(-1);
    }

    // ─────────────────────────────────────────────
    // FPS CAP
    // ─────────────────────────────────────────────

    private void StepFPS(int dir)
    {
        fpsIndex += dir;

        // Allow one extra index for Unlimited
        fpsIndex = Mathf.Clamp(fpsIndex, 0, fpsSteps.Length);

        ApplyFPSCap();
    }

    private void ApplyFPSCap()
    {
        // Unlimited state
        if (fpsIndex >= fpsSteps.Length)
        {
            Application.targetFrameRate = -1;

            if (maxFPSText != null)
                maxFPSText.text = "MaxFPS: Unlimited";
        }
        else
        {
            int cap = fpsSteps[fpsIndex];
            Application.targetFrameRate = cap;

            if (maxFPSText != null)
                maxFPSText.text = $"MaxFPS: {cap}";
        }
    }


    // ─────────────────────────────────────────────
    // FPS TRACKING
    // ─────────────────────────────────────────────

    private void TrackFPS()
    {
        float dt = Time.unscaledDeltaTime;
        if (dt <= 0f) return;

        float fps = 1f / dt;

        timeAccumulator += dt;
        frameCount++;

        bestFPS = Mathf.Max(bestFPS, fps);
        worstFPS = Mathf.Min(worstFPS, fps);

        updateTimer += dt;
        if (updateTimer >= updateInterval)
        {
            if (isVisible)
                UpdateUI();

            updateTimer = 0f;
        }
    }

    private void UpdateUI()
    {
        float avgFPS = frameCount > 0 ? frameCount / timeAccumulator : 0f;

        if (avgFPSText != null)
            avgFPSText.text = $"AvgFPS: {avgFPS:F1}";

        if (worstFPSText != null)
            worstFPSText.text = $"WorstFPS: {worstFPS:F1}";

        if (bestFPSText != null)
            bestFPSText.text = $"BestFPS: {bestFPS:F1}";
    }

    // ─────────────────────────────────────────────
    // PUBLIC UTIL
    // ─────────────────────────────────────────────

    public void ResetStats()
    {
        timeAccumulator = 0f;
        frameCount = 0;
        bestFPS = float.MinValue;
        worstFPS = float.MaxValue;
        updateTimer = 0f;
    }
}
