using UnityEngine;
using TMPro;

public class DevTimeManager : MonoBehaviour, IDevPanel
{
    [Header("UI")]
    [SerializeField] private GameObject panel;
    [SerializeField] private TextMeshProUGUI timeScaleText;

    [Header("Toggle")]
    [SerializeField] private KeyCode toggleKey = KeyCode.F2;

    [Header("Time Scale Settings")]
    [SerializeField] private float step = 0.25f;
    [SerializeField] private float minScale = 0.25f;
    [SerializeField] private float maxScale = 3.0f;

    private bool isVisible;
    private float currentScale = 1f;

    void Awake()
    {
        SetVisible(false);
        SetTimeScale(1f);
    }

    void Update()
    {
        HandleToggle();

        // 🔒 Only the focused panel reacts
        if (DevPanelFocusManager.HasFocus(this))
            HandleTimeScaleInput();
    }

    // ─────────────────────────────────────────────
    // TOGGLE / FOCUS
    // ─────────────────────────────────────────────

    private void HandleToggle()
    {
        if (!Input.GetKeyDown(toggleKey))
            return;

        if (isVisible)
        {
            SetVisible(false);
            DevPanelFocusManager.ClearFocus(this);
        }
        else
        {
            SetVisible(true);
            DevPanelFocusManager.RequestFocus(this);
        }
    }

    private void SetVisible(bool visible)
    {
        isVisible = visible;

        if (panel != null)
            panel.SetActive(visible);

        UpdateUI();
    }

    public void OnFocusGained()
    {
        // Optional: highlight panel, glow, sound, etc.
    }

    public void OnFocusLost()
    {
        SetVisible(false);
    }

    // ─────────────────────────────────────────────
    // INPUT
    // ─────────────────────────────────────────────

    private void HandleTimeScaleInput()
    {
        if (Input.GetKeyDown(KeyCode.Equals) || Input.GetKeyDown(KeyCode.KeypadPlus))
            AdjustTimeScale(+step);

        if (Input.GetKeyDown(KeyCode.Minus) || Input.GetKeyDown(KeyCode.KeypadMinus))
            AdjustTimeScale(-step);

        if (Input.GetKeyDown(KeyCode.Alpha0))
            SetTimeScale(1f);
    }

    // ─────────────────────────────────────────────
    // TIME CONTROL
    // ─────────────────────────────────────────────

    private void AdjustTimeScale(float delta)
    {
        SetTimeScale(currentScale + delta);
    }

    private void SetTimeScale(float value)
    {
        currentScale = Mathf.Clamp(value, minScale, maxScale);

        TimeManager.Instance?.RemoveModifier("DevTimeManager");
        var mod = new TimeScaleModifier("DevTimeManager", currentScale);
        TimeManager.Instance?.AddModifier(mod);

        UpdateUI();
    }

    private void UpdateUI()
    {
        if (timeScaleText != null)
            timeScaleText.text = $"TimeScale: {currentScale:F2}x";
    }

    private void OnDisable()
    {
        TimeManager.Instance?.RemoveModifier("DevTimeManager");

        DevPanelFocusManager.ClearFocus(this);
    }
}
