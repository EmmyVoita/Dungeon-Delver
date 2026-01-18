using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingOption : BaseSettingOption
{
    [Header("References")]
    public AudioChannel audioChannel;
    public AudioClip adjustSound;
    public TextMeshProUGUI label;
    public Image fillBar;
    public Transform tickParent; // parent with 10 tick mark children

    [Header("Settings")]
    public int tickCount = 10;
    public int currentValue = 5;
    public float fillRange = 220f; 
    public RectTransform fillTransform;

    [Header("Colors")]
    public Color normalFillColor = Color.white;
    public Color selectedFillColor = Color.yellow;

    private bool isSelected = false;

    void Start()
    {
        // ✅ Initialize current value from saved audio settings
        float volume = GetChannelRawVolume();
        currentValue = Mathf.RoundToInt(volume * tickCount);
        UpdateVisual();
    }

    public override void AdjustValue(int direction)
    {
        currentValue = Mathf.Clamp(currentValue + direction, 0, tickCount);

        float fillPercent = (float)currentValue / tickCount;
        AudioSettingsManager.Instance.SetVolume(audioChannel, fillPercent); // raw channel value only

        AudioHelpers.PlayMyClipAtPoint(adjustSound, AudioChannel.UI, Camera.main.transform.position, 0.5f);
        UpdateVisual();
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;
        UpdateVisual();
    }

    public void UpdateVisual()
    {
        float fillPercent = (float)currentValue / tickCount;
        fillPercent = Mathf.Clamp01(fillPercent);

        float xPos = Mathf.Lerp(-fillRange / 2f, fillRange / 2f, fillPercent);
        Vector2 pos = fillTransform.anchoredPosition;
        pos.x = xPos;
        fillTransform.anchoredPosition = pos;

        // 🟡 Change color if selected
        if (fillBar != null)
            fillBar.color = isSelected ? selectedFillColor : normalFillColor;
    }

    private float GetChannelRawVolume()
    {
        // Helper: get the raw volume (not multiplied by master)
        return audioChannel switch
        {
            AudioChannel.Master => AudioSettingsManager.Instance.masterVolume,
            AudioChannel.Music => AudioSettingsManager.Instance.musicVolume,
            AudioChannel.SFX => AudioSettingsManager.Instance.sfxVolume,
            AudioChannel.UI => AudioSettingsManager.Instance.uiVolume,
            AudioChannel.Ambience => AudioSettingsManager.Instance.ambienceVolume,
            _ => 1f
        };
    }
}
