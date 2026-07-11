using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class AudioSettingOption : BaseSettingOption, IDragHandler, IPointerDownHandler
{
    [Header("References")]
    public AudioControl audioControl;
    public AudioClip adjustSound;
    public TextMeshProUGUI label;
    public Image fillBar;
    public Transform tickParent; // parent with 10 tick mark children
    public RectTransform sliderBounds;

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

    public void OnDrag(PointerEventData eventData)
    {
        Debug.Log("OnDrag");
        UpdateFromMouse(eventData);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("OnPointerDown");
        UpdateFromMouse(eventData);
    }

    override public void OnPointerClick(PointerEventData eventData)
    {
        
    }


    void UpdateFromMouse(PointerEventData eventData)
    {
        // Get mouse drag position
        Vector2 position = eventData.position;

        // convert to 0-1 range
        // We need to know the range of slider position on the screen

        Vector2 localPoint;
        
        // converts
        //Screen Space
        //↓
        //Rect local space

         RectTransformUtility.ScreenPointToLocalPointInRectangle(
            sliderBounds,
            position,
            eventData.pressEventCamera,
            out localPoint
        );

        Rect rect = sliderBounds.rect;

        float normalized = Mathf.InverseLerp(rect.xMin, rect.xMax, localPoint.x);

        // Snap to increments
        int step =
            Mathf.RoundToInt(
                normalized * tickCount
            );

        SetValue(step);
    }

    public void SetValue(int step)
    {
        if(step == currentValue) return;
        currentValue = step;

        float fillPercent = (float)currentValue / tickCount;
        AudioSettingsManager.Instance.SetVolume(audioControl, fillPercent); // raw channel value only

        SoundEffect adjustSound = AudioLibrary.Instance.Database.navigate;
        AudioHelpers.PlaySoundEffect(adjustSound,Camera.main.transform.position);

        //AudioHelpers.PlayMyClipAtPoint(adjustSound, AudioChannel.UI, Camera.main.transform.position, 0.5f);
        UpdateVisual();
    }

    public override void AdjustValue(int direction)
    {
        currentValue = Mathf.Clamp(currentValue + direction, 0, tickCount);

        float fillPercent = (float)currentValue / tickCount;
        AudioSettingsManager.Instance.SetVolume(audioControl, fillPercent); // raw channel value only

        //AudioHelpers.PlayMyClipAtPoint(adjustSound, AudioChannel.UI, Camera.main.transform.position, 0.5f);

        SoundEffect adjustSound = AudioLibrary.Instance.Database.navigate;
        AudioHelpers.PlaySoundEffect(adjustSound,Camera.main.transform.position);
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
        return audioControl switch
        {
            AudioControl.Master => AudioSettingsManager.Instance.masterVolume,
            AudioControl.Music => AudioSettingsManager.Instance.musicVolume,
            AudioControl.SFX => AudioSettingsManager.Instance.sfxVolume,
            AudioControl.UI => AudioSettingsManager.Instance.uiVolume,
            AudioControl.Ambience => AudioSettingsManager.Instance.ambienceVolume,
            _ => 1f
        };
    }
}
