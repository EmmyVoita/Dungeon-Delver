using UnityEngine;
using DG.Tweening;
using TMPro;

public class BounceOnConfirm : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textElement;

    [Header("Input")]
    [SerializeField] private InputActionType actionType = InputActionType.Confirm;

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
    [SerializeField] private bool visibleOnAwake = true;

    private RectTransform rect;
    private Vector2 originalPos;

    private Sequence bounceSequence;
    private bool _pressed;
    private Sequence _hideSequence;
    private bool _visible;

    

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        originalPos = rect.anchoredPosition;

       

        _pressed = false;

        _visible = visibleOnAwake ? true : false;

        BuildSequence();
    }

    private void Start()
    {
        string key = InputBindingManager.Instance.GetBoundKey(actionType).ToString();
        textElement.text = $"[<color=yellow>{key}</color>]";
    }

    public void Show()
    {
        canvasGroup.DOFade(1,0.5f).OnComplete(() => _visible = true);
    }

    private void BuildSequence()
    {
        bounceSequence = DOTween.Sequence();

        // --- POSITION ---
        bounceSequence.Append(
            rect.DOAnchorPosY(originalPos.y + bounceHeight, moveDuration)
                .SetEase(Ease.OutCubic)
        );

        bounceSequence.Append(
            rect.DOAnchorPosY(originalPos.y, moveDuration)
                .SetEase(Ease.InCubic)
        );

        // --- SCALE PUNCH ---
        bounceSequence.Join(
            rect.DOPunchScale(
                new Vector3(scaleAmount, -scaleAmount * 0.6f, 0f),
                moveDuration * 2f,
                6,
                0.7f
            )
        );

        // --- ROTATION PUNCH ---
        bounceSequence.Join(
            rect.DOPunchRotation(
                new Vector3(0f, 0f, rotationAmount),
                moveDuration * 2f,
                6,
                0.6f
            )
        );

        bounceSequence.SetAutoKill(false);
        bounceSequence.Pause();
    }

    private void Update()
    {   
        if(!_visible) 
            return;

        if(hideOnPress && _pressed)
            return;

        if (InputBindingManager.Instance.GetKeyDown(actionType))
        {
            TriggerBounce();
            _pressed = true;

            if(hideOnPress)
                Hide();
        }
    }

    private void Hide()
    {
        _hideSequence = DOTween.Sequence();

        _hideSequence.Insert(hideDelay,canvasGroup.DOFade(0,fadeDuration)).OnComplete(() => _visible = false);
    }

    public void TriggerBounce()
    {
        // Clean restart without snapping
        bounceSequence.Restart(true);
    }
}