  using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using DG.Tweening;
using System.Collections.Generic;

public class GameSpeedOption : MonoBehaviour//BaseSettingOption, IDragHandler, IPointerDownHandler
{

    [Header("Input")]
    [SerializeField] private InputActionType moveSliderUpKey = InputActionType.MoveRight;
    [SerializeField] private InputActionType moveSliderDownKey = InputActionType.MoveLeft;

    [Header("References")]
    public AudioControl audioControl;
    public AudioClip adjustSound;
    public TextMeshProUGUI label;
    public Image fillBar;
    public Transform tickParent; // parent with 10 tick mark children
    public RectTransform sliderBounds;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TextMeshProUGUI timeScaleText;

    [SerializeField] private RectTransform scoreTextRect;
    [SerializeField] private RectTransform currencyTextRect;

    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI currencyText;

    [SerializeField] private PlayPagesController playPagesController;
    [SerializeField] private UIPanelNudge panelNudge;

    [Header("Key Prompts")]
    [SerializeField] private List<KeyPrompt> keyPrompts;
    [SerializeField] private KeyPromptController keyPromptController;


    [Header("Value Change Animation")]
    [SerializeField] private float popScale = 1.15f;
    [SerializeField] private float popRotation = 6f;
    [SerializeField] private float popDuration = 0.12f;
    [SerializeField] private float settleDuration = 0.16f;


    [Header("Settings")]
    public int tickCount = 10;
    [SerializeField] private float timescaleTickValue = 0.05f;
    [SerializeField] private float currencyTickValue = 0.05f;
    [SerializeField] private int currencyTicksPerStep = 2;

    [SerializeField] private float scoreMultTickValue = 0.05f;
    [SerializeField] private int scoreTicksPerStep = 1;
    
    public float fillRange = 220f; 
    public RectTransform fillTransform;

    [Header("Colors")]
    public Color normalFillColor = Color.white;
    public Color selectedFillColor = Color.yellow;

    private bool isSelected = false;
    private int _currentValue = 5;
    private float _timeScaleMod = 1;
    private float _currencyMod = 1;
    private float _scoreMod = 1;
    private bool _isOpen;

    public float TimeScaleMultiplier => _timeScaleMod;
    public float ScoreMultiplier => _scoreMod;
    public float CurrencyMultiplier => _currencyMod;

    void OnDestroy()
    {
        Cleanup();
    }

    void Start()
    {
        SetValue(0);
        UpdateVisual();
    }

    public void Open()
    {
        InputFocusManager.Claim(this);

        _isOpen = true;
        //_currentValue = 0;
        canvasGroup.alpha = 1f;

        keyPromptController.PushPrompts(keyPrompts);
        
        UpdateVisual();
    }

    public void Close()
    {
        //canvasGroup.alpha = 0f;
        playPagesController.ShowAbilitySelect();

        keyPromptController.PopPrompts();

        Cleanup();
    }

    void Cleanup()
    {
        InputFocusManager.Release(this);
        _isOpen = false;
    }

    void Update()
    {
        if(!InputFocusManager.HasFocus(this) || !_isOpen || playPagesController.IsTransitioning)
            return;

        if(InputBindingManager.Instance.GetKeyDown( moveSliderUpKey))
        {
            AdjustValue(1);
            panelNudge.NudgeRight();
        }
        else if(InputBindingManager.Instance.GetKeyDown( moveSliderDownKey))
        {
            AdjustValue(-1);
            panelNudge.NudgeLeft();
        }

        if(InputBindingManager.Instance.GetKeyDown(InputActionType.Back))
        {
            Debug.LogWarning("GAMESPEED OPTION BACK PRESSED");
            InputBindingManager.Instance.BlockUntilRelease(InputActionType.Back);
            AudioHelpers.PlaySoundEffect(AudioLibrary.Instance.Database.back, Camera.main.transform.position);
            Close();
        }
    }


    public void SetValue(int step)
    {
        if(step == _currentValue) return;
        _currentValue = step;

        _timeScaleMod = 1.0f + _currentValue * timescaleTickValue;
        int currencySteps = _currentValue / currencyTicksPerStep;
        int scoreSteps = _currentValue / scoreTicksPerStep;

        _currencyMod = 1.0f + currencySteps * currencyTickValue;
        _scoreMod = 1.0f + scoreSteps * scoreMultTickValue;

        _timeScaleMod = 1.0f + _currentValue * timescaleTickValue;

        SoundEffect adjustSound = AudioLibrary.Instance.Database.navigate;
        AudioHelpers.PlaySoundEffect(adjustSound,Camera.main.transform.position);

        UpdateVisual();
        PlayOnValueChangedAnimations();
    }

    public void AdjustValue(int direction)
    {
        _currentValue = Mathf.Clamp(_currentValue + direction, 0, tickCount);

        _timeScaleMod = 1.0f + _currentValue * timescaleTickValue;
        int currencySteps = _currentValue / currencyTicksPerStep;
        int scoreSteps = _currentValue / scoreTicksPerStep;

        _currencyMod = 1.0f + currencySteps * currencyTickValue;
        _scoreMod = 1.0f + scoreSteps * scoreMultTickValue;

        _timeScaleMod = 1.0f + _currentValue * timescaleTickValue;

        SoundEffect adjustSound = AudioLibrary.Instance.Database.navigate;
        AudioHelpers.PlaySoundEffect(adjustSound,Camera.main.transform.position);
        UpdateVisual();
        PlayOnValueChangedAnimations();
    }

   

    public void SetSelected(bool selected)
    {
        isSelected = selected;
        UpdateVisual();
    }

    public void UpdateVisual()
    {
        float fillPercent = (float)_currentValue / tickCount;
        fillPercent = Mathf.Clamp01(fillPercent);

        float xPos = Mathf.Lerp(-fillRange / 2f, fillRange / 2f, fillPercent);
        Vector2 pos = fillTransform.anchoredPosition;
        pos.x = xPos;
        fillTransform.anchoredPosition = pos;

        if (fillBar != null)
            fillBar.color = isSelected ? selectedFillColor : normalFillColor;

        if(timeScaleText)
            timeScaleText.text = $"x{_timeScaleMod:F2}";

        if(currencyText)
            currencyText.text = $"x{_currencyMod:F2}";

        if(scoreText)
            scoreText.text = $"x{_scoreMod:F2}";
    }

    public void ApplyModifiers()
    {
        
        //TimeManager.Instance.AddRunModifier(new TimeScaleModifier($"GameSpeedModifier_{_timeScaleMod}", _timeScaleMod));
        //ScoreManager.Instance.AddScoreModifier(new ScoreScaleModifier($"GameSpeedModifer_Score_{_scoreMod}", _scoreMod));
        //CurrencyManager.Instance.AddCurrencyModifier(new CurrencyScaleModifier($"GameSpeedModifer_Currency_{_currencyMod}", _currencyMod));
    }

    private void PlayOnValueChangedAnimations()
    {
        PlayTextPop(scoreTextRect);
        PlayTextPop(currencyTextRect);
    }

    private void PlayTextPop(RectTransform rect)
    {
        if (rect == null)
            return;
        
        float rotationDirection = Random.value > 0.5f ? -1 : 1;

        rect.DOKill();

        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;

        Sequence sequence = DOTween.Sequence();

        sequence.Append(
            rect.DOScale(popScale, popDuration)
                .SetEase(Ease.OutBack)
        );

        sequence.Join(
            rect.DORotate(
                new Vector3(0f, 0f, popRotation * rotationDirection),
                popDuration
            )
            .SetEase(Ease.OutQuad)
        );

        sequence.Append(
            rect.DOScale(1f, settleDuration)
                .SetEase(Ease.OutBack)
        );

        sequence.Join(
            rect.DORotate(
                Vector3.zero,
                settleDuration
            )
            .SetEase(Ease.OutBack)
        );

        sequence.SetUpdate(true);
    }
}
