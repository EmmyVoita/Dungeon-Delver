using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using DG.Tweening;

public class AudioArrowSettingOption : BaseSettingOption, IDragHandler, IPointerDownHandler
{
    [Header("References")]
    public TextMeshProUGUI nameText;
    public RectTransform nameRect;

    [Header("Colors")]
    public Color normalFillColor = Color.white;
    public Color selectedFillColor = Color.yellow;

    private bool isSelected = false;
    private int _currentAudioIndex = 0;

    void Start()
    {
        // AudioSettingsMAanager sets the ArrowHitSoundData in the audiolibarary 
        // to the saved one, so we set the visuals
        UpdateVisual(AudioLibrary.Instance.ArrowHitSoundData, false);

        _currentAudioIndex = AudioLibrary.Instance.ArrowHitSoundIndex;
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
        /*
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
        */
    }

    
    public void SetValue(int step)
    {
        
    }
    

    public override void AdjustValue(int direction)
    {
        int audioOptionCount = AudioLibrary.Instance.Database.arrowSounds.Count;

        _currentAudioIndex = (_currentAudioIndex + direction + audioOptionCount) % audioOptionCount;

        AudioSettingsManager.Instance.UpdateSelectedArrowAudio(_currentAudioIndex);

        ArrowHitSoundData audioData = AudioLibrary.Instance.ArrowHitSoundData;

        AudioHelpers.PlayMyClipAtPoint(
            audioData.arrowHitSound,
            AudioChannel.SFX,
            Camera.main.transform.position,
            AudioSettingsManager.GetVolume(AudioChannel.SFX),
            audioData.normalHitPitch
        );

        UpdateVisual(audioData);
    }
    
    public void SetSelected(bool selected)
    {
        isSelected = selected;
        //UpdateVisual();
    }

    public void UpdateVisual(ArrowHitSoundData audioData, bool animate = true)
    {
        if (nameText)
            nameText.text = audioData.name;

        if (nameRect && animate)
        {
            nameRect.DOKill();

            nameRect.localScale = Vector3.one;

            nameRect.DOPunchScale(
                Vector3.one * 0.2f,
                0.18f,
                vibrato: 5,
                elasticity: 0.7f
            ).SetUpdate(true);
        }
    }
}
