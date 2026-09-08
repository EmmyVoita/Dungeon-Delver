using UnityEngine;
using DG.Tweening;
using TMPro;

public class InputPopup : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textElement;

    [Header("Position Bounce")]
    [SerializeField] private float bounceHeight = 28f;
    [SerializeField] private float moveDuration = 0.12f;

    [Header("Scale Punch")]
    [SerializeField] private float scaleAmount = 0.15f;

    [Header("Rotation Jiggle")]
    [SerializeField] private float rotationAmount = 8f;

    [Header("Visibility")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private bool hideOnPress = false;
    [SerializeField] private float hideDelay = 1.0f;
    [SerializeField] private float fadeDuration = 1.0f;

    private RectTransform _rect;
    private Vector2 _originalPos;

    private Sequence _bounceSequence;
    private bool _pressed;
    private Sequence _hideSequence;

    private InputActionType _inputAction;
    private bool _isActive = false;
    private bool _hideOnPress = false;


    private void Awake()
    {
        _rect = GetComponent<RectTransform>();
        _originalPos = _rect.anchoredPosition;
        _pressed = false;
        Hide();
    }

    private void Update()
    {   
        if(!_isActive) 
            return;

        if(_hideOnPress && _pressed)
            return;
            

        if (InputBindingManager.Instance.GetKeyDown(_inputAction))
        {
            _bounceSequence.Restart(true);
            _pressed = true;

            if(_hideOnPress)
                Hide();
        }
    }

    public void Initialize(InputActionType input,
                           bool hideOnPress, 
                           string prefixText, 
                           string suffixText, 
                           Color textColor)
    {
        _inputAction = input;
        _hideOnPress = hideOnPress;

        string key = InputBindingManager.Instance.GetBoundKey(input).ToString();
        string hexColor = ColorUtility.ToHtmlStringRGB(textColor);

        textElement.text = $"{prefixText}<color=#{hexColor}>{key}</color>{suffixText}";

        canvasGroup.DOFade(1,0.5f).OnComplete(() => _isActive = true);

        BounceSequence();
    }

    public void Hide()
    {
        textElement.text = "";

        if(canvasGroup)
            canvasGroup.alpha = 0;
        
        _pressed = false;

        _hideSequence = DOTween.Sequence();
        _hideSequence.Insert(hideDelay,canvasGroup.DOFade(0,fadeDuration)).OnComplete(() => _isActive = false);
    }

    private void BounceSequence()
    {
        _bounceSequence = DOTween.Sequence();

        // --- POSITION ---
        _bounceSequence.Append(
            _rect.DOAnchorPosY(_originalPos.y + bounceHeight, moveDuration)
                .SetEase(Ease.OutCubic)
        );

        _bounceSequence.Append(
            _rect.DOAnchorPosY(_originalPos.y, moveDuration)
                .SetEase(Ease.InCubic)
        );

        // --- SCALE PUNCH ---
        _bounceSequence.Join(
            _rect.DOPunchScale(
                new Vector3(scaleAmount, -scaleAmount * 0.6f, 0f),
                moveDuration * 2f,
                6,
                0.7f
            )
        );

        // --- ROTATION PUNCH ---
        _bounceSequence.Join(
            _rect.DOPunchRotation(
                new Vector3(0f, 0f, rotationAmount),
                moveDuration * 2f,
                6,
                0.6f
            )
        );

        _bounceSequence.SetAutoKill(false);
        _bounceSequence.Pause();
    }
}