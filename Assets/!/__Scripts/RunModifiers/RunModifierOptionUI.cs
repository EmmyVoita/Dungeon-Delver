using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RunModifierOptionUI : RunModifierMenuOptionBase
{
    public static event Action<RunModifierOptionUI> OnClicked;
    [Header("Main")]
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI valueText;

    [SerializeField] private RectTransform nameContainerRect;
    [SerializeField] private RectTransform rootRect;
    [SerializeField] private LayoutElement outlineLayoutElement;
    [SerializeField] private RectTransform outlineRect;
    [SerializeField] private Image iconImage;
    [SerializeField] private GameObject lockUI;

    [Header("Panel")]
    [SerializeField] private Image panelImage;
    [SerializeField] private Material baseMaterial;
    [SerializeField] private Color challengeColorA;
    [SerializeField] private Color challengeColorB;
    [SerializeField] private string colorAPropertyName = "_ColorA";
    [SerializeField] private string colorBPropertyName = "_ColorB";


    [Header("Value Arrows")]
    [SerializeField] private RectTransform leftArrowRect;
    [SerializeField] private Image leftArrowImage;

    [SerializeField] private RectTransform rightArrowRect;
    [SerializeField] private Image rightArrowImage;

    [SerializeField] private Color enabledArrowColor = Color.white;
    [SerializeField] private Color disabledArrowColor = new Color(1f, 1f, 1f, 0.3f);

    [Header("Value Feedback")]
    [SerializeField] private float arrowPunchScale = 0.2f;
    [SerializeField] private float arrowPunchDuration = 0.15f;

    [SerializeField] private float valuePunchScale = 0.08f;
    [SerializeField] private float valuePunchDuration = 0.15f;

    [SerializeField] private int punchVibrato = 6;
    [SerializeField] private float punchElasticity = 0.7f;

    [Header("Hover")]
    
    [SerializeField] private float hoverScale = 1.05f;
    [SerializeField] private float hoverDuration = 0.15f;
    [SerializeField] private Ease hoverEase = Ease.OutCubic;

    [Header("Punch Scale")]
    [SerializeField] private float punchScale = 0.1f;

    [Header("Outline Image")]
    [SerializeField] private float outlineYoyoScale = 1.025f;
    [SerializeField] private float extraXPadding = 0.0f;
    [SerializeField] private float extraYPadding = 0.0f;

    [Header("Audio")]
    [SerializeField] private SoundEffect maximumSound;
    [SerializeField] private SoundEffect minimumSound;

    
    private RunModifierDefinition _definition;
    private float _currentValue;
    private Material runTimeMaterial;
    private float _maxAllowedValue;


    private Color baseColorA;
    private Color baseColorB;

    public RunModifierDefinition Definition => _definition;
    public float CurrentValue => _currentValue;

    private void OnEnable()
    {
        RunModifierManager.OnValueChanged += HandleValueChanged;
    }

    private void OnDisable()
    {
        RunModifierManager.OnValueChanged -= HandleValueChanged;
    }

    private void HandleValueChanged(RunModifierSelection selection)
    {
        RecalculateMaxValue();
        RefreshValueUI();
    }

    public void Initialize(RunModifierDefinition definition)
    {
        outlineRect.DOKill();
        outlineRect.localScale = Vector3.one;
        outlineRect.gameObject.SetActive(false);

        _definition = definition;

        nameText.text = definition.displayName.ToUpper();
        _currentValue = definition.minValue;

        iconImage.sprite = definition.icon;

        nameText.ForceMeshUpdate();

        LayoutRebuilder.ForceRebuildLayoutImmediate(nameContainerRect);

        outlineLayoutElement.preferredWidth = nameContainerRect.rect.width + extraXPadding;
        outlineLayoutElement.preferredHeight = nameContainerRect.rect.height + extraYPadding;

        _maxAllowedValue = _definition.maxValue;

        RefreshValue(true);

        RefreshValueUI();

        SetSelected(false, false);

        if (panelImage != null && baseMaterial != null)
        {
            runTimeMaterial = Instantiate(baseMaterial);
            panelImage.material = runTimeMaterial;

            baseColorA = runTimeMaterial.GetColor(colorAPropertyName);
            baseColorB = runTimeMaterial.GetColor(colorBPropertyName);
        }

       bool challenge = _definition.category == RunModifierCategory.Challenge;

        runTimeMaterial.SetColor(colorAPropertyName, challenge ? challengeColorA : baseColorA);
        runTimeMaterial.SetColor(colorBPropertyName, challenge ? challengeColorB : baseColorB);

        lockUI.SetActive(!_definition.IsUnlocked);
    }

    public void ChangeValueFromMouse(int direction)
    {
        ChangeValue(direction);
        OnClicked?.Invoke(this);
    }

    public void SetSelected(bool selected, bool animate = true)
    {
        outlineLayoutElement.preferredWidth = nameContainerRect.rect.width + extraXPadding;
        outlineLayoutElement.preferredHeight = nameContainerRect.rect.height + extraYPadding;

        if (outlineRect)
            outlineRect.gameObject.SetActive(selected);

        if (!rootRect)
            return;

        if (outlineRect != null)
        {
            outlineRect.DOKill();
            outlineRect.localScale = Vector3.one;
            outlineRect.gameObject.SetActive(selected);
        }

        rootRect.DOKill();

        float targetScale = selected ? hoverScale : 1f;

        if (!animate)
        {
            rootRect.localScale = Vector3.one * targetScale;
            return;
        }

        
        
        // Only yoyo while selected.
        if (selected && outlineRect != null)
        {
            outlineRect
                .DOScale(
                    Vector3.one * outlineYoyoScale,
                    0.5f
                )
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetUpdate(true);
        }
        

        rootRect.DOScale(targetScale, hoverDuration).SetEase(hoverEase).SetUpdate(true);
    }

    public override void OnEnter()
    {
        SetSelected(true);
    }

    public override void OnExit()
    {
        SetSelected(false);
    }

    public override void HandleDirectionalInput(Vector2 input)
    {
        if (input == Vector2.up || input == Vector2.down)
        {
            OnNavigateToOption?.Invoke(input);
            return;
        }

        if (input == Vector2.left)
        {
            ChangeValue(-1);
            return;
        }

        if (input == Vector2.right)
        {
            ChangeValue(1);
        }
    }

    private void RefreshValue(bool silent = false)
    {
        if (_definition == null)
            return;

        _currentValue = RunModifierManager.Instance.GetValue(Definition.type);

        RefreshValueUI();

        if(!silent)
            AudioHelpers.PlaySoundEffect(AudioLibrary.Instance.Database.navigate, Camera.main.transform.position);
    }

    private void ChangeValue(int direction)
    {
        if(!_definition.IsUnlocked)
            return;

        if (_definition == null)
            return;

        float previousValue = _currentValue;

        RecalculateMaxValue();

        float targetValue = previousValue + (_definition.step * direction);
   

        _currentValue = Mathf.Clamp(targetValue,_definition.minValue,_maxAllowedValue);

        // Already at the limit.
        if (Mathf.Approximately(previousValue, targetValue))
        {
            PunchArrow(direction, false);
            return;
        }

        //_currentValue = targetValue;

        RunModifierManager.Instance.SetValue(Definition,_currentValue);

        RefreshValueUI();

        bool reachedMax =!Mathf.Approximately(previousValue, _maxAllowedValue) &&
                         Mathf.Approximately(_currentValue, _maxAllowedValue);

        bool reachedMin = !Mathf.Approximately(previousValue, _definition.minValue) &&
                           Mathf.Approximately(_currentValue, _definition.minValue);    

        //bool shouldBeGolden = reachedMax && _definition.category == RunModifierCategory.Challenge;


        if (reachedMax)
        {
            AudioHelpers.PlaySoundEffect(maximumSound,Camera.main.transform.position);

            rootRect.DOPunchScale(Vector3.one * punchScale, 
                                    duration: 0.2f,
                                    vibrato: 10,
                                    elasticity: 1).SetUpdate(true);
        }
        else if(reachedMin)
        {
            AudioHelpers.PlaySoundEffect(minimumSound,Camera.main.transform.position);
        }
        else
        {
            AudioHelpers.PlaySoundEffect(AudioLibrary.Instance.Database.navigate,Camera.main.transform.position);
        }

        PunchArrow(direction, true);
        PunchValue();
    }

    private void RecalculateMaxValue()
    {
        if(_definition == null)
            return;

        if(_definition.category != RunModifierCategory.Assist)
            return;

        _maxAllowedValue = _definition.maxValue;

        // we want to clamp the value that the player can move to the right
        // by the definitions max value as well as the remaining points
        _maxAllowedValue = Mathf.Min(_definition.maxValue, 
                                     _currentValue + RunModifierMenuNavigator.Instance.PointsRemaining * _definition.step);

    }

    private void RefreshValueUI()
    {
        if(!_definition.IsUnlocked)
        {
            leftArrowImage.color = disabledArrowColor;
            rightArrowImage.color = disabledArrowColor;
            valueText.color = disabledArrowColor;
            valueText.text = FormatValue(_currentValue);
            return;
        }

        if (_definition == null)
            return;

        if (valueText != null)
        {
            valueText.text = FormatValue(_currentValue);
        }

        bool canGoLeft =_currentValue > _definition.minValue;
        bool canGoRight =_currentValue < _maxAllowedValue;

        if (leftArrowImage != null)
        {
            leftArrowImage.color = canGoLeft? enabledArrowColor: disabledArrowColor;
        }

        if (rightArrowImage != null)
        {
            rightArrowImage.color = canGoRight? enabledArrowColor: disabledArrowColor;
        }
    }

    private string FormatValue(float value)
    {
        // If you have suffixes like "%" in the definition.
        string suffix = _definition.valueSuffix;

        if (Mathf.Approximately(value, Mathf.Round(value)))
            return $"{Mathf.RoundToInt(value)}{suffix}";

        return $"{value:0.##}{suffix}";
    }

    private void PunchArrow(int direction, bool successful)
    {
        RectTransform arrow =
            direction < 0
                ? leftArrowRect
                : rightArrowRect;

        if (arrow == null)
            return;

        arrow.DOKill();
        arrow.localScale = Vector3.one;

        float strength = successful
            ? arrowPunchScale
            : arrowPunchScale * 0.5f;

        arrow.DOPunchScale(
            Vector3.one * strength,
            arrowPunchDuration,
            punchVibrato,
            punchElasticity
        )
        .SetUpdate(true);
    }

    private void PunchValue()
    {
        if (valueText == null)
            return;

        RectTransform valueRect =
            valueText.rectTransform;

        valueRect.DOKill();
        valueRect.localScale = Vector3.one;

        valueRect.DOPunchScale(
            Vector3.one * valuePunchScale,
            valuePunchDuration,
            punchVibrato,
            punchElasticity
        )
        .SetUpdate(true);
    }

    private void OnDestroy()
    {
        rootRect?.DOKill();
        leftArrowRect?.DOKill();
        rightArrowRect?.DOKill();

        if (valueText != null)
            valueText.rectTransform.DOKill();
    }
}