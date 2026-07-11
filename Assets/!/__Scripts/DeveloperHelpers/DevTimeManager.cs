using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

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

    private bool _isVisible;
    [SerializeField] private float _currentScale = 1f;

    void OnEnable()
    {
        SceneManager.sceneLoaded += HandleSceneLoaded;
        
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;

        TimeManager.Instance?.RemoveModifier("DevTimeManager");

        DevPanelFocusManager.ClearFocus(this);
    }


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

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
       StartCoroutine(WaitForPeriod());
    }

    private IEnumerator WaitForPeriod()
    {
        yield return null;
        SetTimeScale(_currentScale);
    }

    // ─────────────────────────────────────────────
    // TOGGLE / FOCUS
    // ─────────────────────────────────────────────

    private void HandleToggle()
    {
        if (!Input.GetKeyDown(toggleKey))
            return;

        if (_isVisible)
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
        _isVisible = visible;

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
        SetTimeScale(_currentScale + delta);
    }

    private void SetTimeScale(float value)
    {
        
        _currentScale = Mathf.Clamp(value, minScale, maxScale);

        Debug.Log($"Updateing time scale modifier DevTimeManager => {_currentScale}");

        TimeManager.Instance?.RemoveModifier("DevTimeManager");
        var mod = new TimeScaleModifier("DevTimeManager", _currentScale);
        TimeManager.Instance?.AddModifier(mod);

        UpdateUI();
    }

    private void UpdateUI()
    {
        if (timeScaleText != null)
            timeScaleText.text = $"TimeScale: {_currentScale:F2}x";
    }
}
