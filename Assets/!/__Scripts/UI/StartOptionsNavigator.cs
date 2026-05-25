using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using DG.Tweening;

public class StartOptionsNavigator : MonoBehaviour
{
    [Header("Core")]
    [SerializeField] private int startIndex = 1;
    [SerializeField] private RectTransform rect;
    [SerializeField] private CanvasGroup dimCanvasGroup;

    [Header("Slide Animation")]
    [SerializeField] private float hiddenX = -300f;
    [SerializeField] private float hiddenY = -300f;
    [SerializeField] private float shownY = 0f;
    [SerializeField] private float shownX = 0f;
    [SerializeField] private float duration = 0.4f;
    [SerializeField] private Ease ease = Ease.OutBack;

    public GameObject startOptionsUI;

    [Header("Options")]
    [SerializeField] private TextMeshProUGUI[] options;
    [SerializeField] private Color selectedColor = Color.yellow;
    [SerializeField] private Color defaultColor = Color.white;
    [SerializeField] private float selectedScale = 1.2f;
    [SerializeField] private float transitionSpeed = 8f;

    private int _selectedIndex = 0;
    private bool _isOpen = false;
    private bool _canAcceptInput = false;

    private GameMode _targetGameMode;
    private string _targetSceneName;

    private void OnEnable()
    {
        StartOptionMouseHandler.OnStartOptionClicked += HandleStartOptionClicked;
    }

    private void OnDisable()
    {
        StartOptionMouseHandler.OnStartOptionClicked += HandleStartOptionClicked;
    }

    private void HandleStartOptionClicked(int index)
    {
        _selectedIndex = index;
        ActivateOption();
    }

    void Start()
    {
        CloseImmediate();

        rect.anchoredPosition = new Vector2(hiddenX, hiddenY);

        UpdateVisuals();
    }

    void Update()
    {
        if (!_isOpen || !_canAcceptInput) return;

        HandleInput();
        AnimateSelection();
    }

    void HandleInput()
    {
        if (InputBindingManager.Instance.GetKeyDown(InputActionType.MoveRight))
        {
            _selectedIndex = (_selectedIndex + 1) % options.Length;
            AudioHelpers.PlaySoundEffect(AudioLibrary.Instance.Database.navigate, transform.position);
            UpdateVisuals();
        }
        else if (InputBindingManager.Instance.GetKeyDown(InputActionType.MoveLeft))
        {
            _selectedIndex = (_selectedIndex - 1 + options.Length) % options.Length;
            AudioHelpers.PlaySoundEffect(AudioLibrary.Instance.Database.navigate, transform.position);
            UpdateVisuals();
        }

        if (InputBindingManager.Instance.GetKeyDown(InputActionType.Confirm))
        {
            AudioHelpers.PlaySoundEffect(AudioLibrary.Instance.Database.select, transform.position);
            ActivateOption();
        }
        else if (InputBindingManager.Instance.GetKeyDown(InputActionType.Back))
        {
            AudioHelpers.PlaySoundEffect(AudioLibrary.Instance.Database.back, transform.position);
            CancelAndClose();
        }
    }

    void UpdateVisuals()
    {
        for (int i = 0; i < options.Length; i++)
        {
            options[i].color = (i == _selectedIndex) ? selectedColor : defaultColor;
        }
    }

    void AnimateSelection()
    {
        for (int i = 0; i < options.Length; i++)
        {
            float targetScale = (i == _selectedIndex) ? selectedScale : 1f;

            options[i].transform.localScale = Vector3.Lerp(
                options[i].transform.localScale,
                Vector3.one * targetScale,
                Time.deltaTime * transitionSpeed
            );
        }
    }

    public void Open(GameMode targetGameMode, string targetScene)
    {
        _targetGameMode = targetGameMode;
        _targetSceneName = targetScene;

        //startOptionsUI.SetActive(true);

        rect.DOKill();
        dimCanvasGroup.DOKill();

        rect.anchoredPosition = new Vector2(hiddenX, hiddenY);

        rect.DOAnchorPosY(shownY, duration).SetEase(ease);
        rect.DOAnchorPosX(shownX, duration).SetEase(ease);

        dimCanvasGroup.alpha = 0f;
        dimCanvasGroup.DOFade(1, duration);

        _isOpen = true;
        _selectedIndex = startIndex;

        UpdateVisuals();

        MenuManager.Instance.LockActiveMenuInput(true);
        StartCoroutine(EnableInputAfterDelay(duration));
    }

    IEnumerator EnableInputAfterDelay(float delay)
    {
        _canAcceptInput = false;
        yield return new WaitForSeconds(delay);
        _canAcceptInput = true;
    }

    void Close()
    {
        rect.DOKill();
        dimCanvasGroup.DOKill();

        rect.DOAnchorPosY(hiddenY, duration)
            .SetEase(Ease.InBack);

        rect.DOAnchorPosX(hiddenX, duration)
            .SetEase(Ease.InBack);


        dimCanvasGroup.DOFade(0f, duration);

        _isOpen = false;

        MenuManager.Instance.LockActiveMenuInput(false, 0.25f);
    }

    void CloseImmediate()
    {
        //startOptionsUI.SetActive(false);
        dimCanvasGroup.alpha = 0f;
        _isOpen = false;
    }

    void CancelAndClose()
    {
        Close();
        _selectedIndex = 0;
    }

    void ActivateOption()
    {
        switch (_selectedIndex)
        {
            case 0:
                CancelAndClose();
                break;

            case 1:
                Close();

                GameSceneLoader.PendingConfig = new GameSceneConfig(
                    _targetGameMode,
                    0,
                    null
                );

                SceneManager.LoadScene(_targetSceneName);
                break;
        }
    }
}

/*using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using DG.Tweening;

public class StartOptionsNavigator : MonoBehaviour
{
    [Header("Core Transforms")]
    [SerializeField] private RectTransform rect;
    [SerializeField] private RectTransform cartridgeRect;
    [SerializeField] private RectTransform containerRect;
    [SerializeField] private CanvasGroup DimCanvasGroup;

    [Header("Menu Slide")]
    [SerializeField] private float hiddenY = -300f;
    [SerializeField] private float shownY = 0f;
    [SerializeField] private float slideDuration = 0.4f;
    [SerializeField] private Ease slideEase = Ease.OutBack;

    [Header("Cartridge Animation")]
    [SerializeField] private float cartridgeHiddenY = 400f;
    [SerializeField] private float cartridgeShownY = 0f;
    [SerializeField] private float cartridgeSlideDuration = 0.35f;
    [SerializeField] private Ease cartridgeSlideEase = Ease.OutBack;

    [Header("Impact Bounce")]
    [Range(0,1)] [SerializeField] private float bounceTimePercent = 0.5f;
    [SerializeField] private float bounceAmount = 18f;
    [SerializeField] private float bounceDuration = 0.25f;

    [Header("Audio")]
    [SerializeField] private SoundEffect clickSoundEffect;

    public static Action<StartMenuWindows> OnReturnFromStartOptions;

    public GameObject startOptionsUI;

    [Header("UI Options")]
    public TextMeshProUGUI[] options;
    public Color selectedColor = Color.yellow;
    public Color defaultColor = Color.white;
    public float selectedScale = 1.2f;
    public float transitionSpeed = 8f;

    private int _selectedIndex = 0;
    private bool _isOpen = false;
    private bool _canAcceptInput = false;

    private GameMode _targetGameMode;
    private string _targetSceneName;

    private float containerBaseY;

    void Start()
    {
        CloseImmediate();

        if (options.Length == 0)
        {
            Debug.LogError("No options assigned.");
            return;
        }

        containerBaseY = containerRect.anchoredPosition.y;

        rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, hiddenY);
        cartridgeRect.anchoredPosition = new Vector2(cartridgeRect.anchoredPosition.x, cartridgeHiddenY);

        UpdateVisuals();
    }

    void Update()
    {
        if (!_isOpen || !_canAcceptInput) return;

        HandleInput();
        AnimateSelection();
    }

    void HandleInput()
    {
        if (InputBindingManager.Instance.GetKeyDown(InputActionType.MoveDown))
        {
            _selectedIndex = (_selectedIndex + 1) % options.Length;
            AudioSettingsManager.PlayNavigateSound();
            UpdateVisuals();
        }
        else if (InputBindingManager.Instance.GetKeyDown(InputActionType.MoveUp))
        {
            _selectedIndex = (_selectedIndex - 1 + options.Length) % options.Length;
            AudioSettingsManager.PlayNavigateSound();
            UpdateVisuals();
        }

        if (InputBindingManager.Instance.GetKeyDown(InputActionType.Confirm))
        {
            AudioSettingsManager.PlaySelectSound();
            ActivateOption();
        }
        else if (InputBindingManager.Instance.GetKeyDown(InputActionType.Back))
        {
            AudioSettingsManager.PlayBackSound();
            CancelAndClose();
        }
    }

    void UpdateVisuals()
    {
        for (int i = 0; i < options.Length; i++)
        {
            options[i].color = (i == _selectedIndex) ? selectedColor : defaultColor;
        }
    }

    void AnimateSelection()
    {
        for (int i = 0; i < options.Length; i++)
        {
            float targetScale = (i == _selectedIndex) ? selectedScale : 1f;

            options[i].transform.localScale = Vector3.Lerp(
                options[i].transform.localScale,
                Vector3.one * targetScale,
                Time.deltaTime * transitionSpeed
            );
        }
    }

    public void Open(GameMode targetGameMode, string targetScene)
    {
        _targetGameMode = targetGameMode;
        _targetSceneName = targetScene;

        startOptionsUI.SetActive(true);

        rect.DOKill();
        DimCanvasGroup.DOKill();

        rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, hiddenY);
        rect.DOAnchorPosY(shownY, slideDuration).SetEase(slideEase);

        DimCanvasGroup.alpha = 0f;
        DimCanvasGroup.DOFade(1f, slideDuration);

        _isOpen = true;
        _selectedIndex = 0;

        UpdateVisuals();

        MenuManager.Instance.LockActiveMenuInput(true);
        StartCoroutine(EnableInputAfterDelay(slideDuration));
    }

    IEnumerator EnableInputAfterDelay(float delay)
    {
        _canAcceptInput = false;
        yield return new WaitForSeconds(delay);
        _canAcceptInput = true;
    }

    void Close()
    {
        rect.DOKill();
        DimCanvasGroup.DOKill();
        containerRect.DOKill();
        cartridgeRect.DOKill();

        rect.DOAnchorPosY(hiddenY, slideDuration)
            .SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                startOptionsUI.SetActive(false);
            });

        DimCanvasGroup.DOFade(0f, slideDuration);

        cartridgeRect.anchoredPosition = new Vector2(cartridgeRect.anchoredPosition.x, cartridgeHiddenY);

        _isOpen = false;
        MenuManager.Instance.LockActiveMenuInput(false, 0.25f);
    }

    void CloseImmediate()
    {
        startOptionsUI.SetActive(false);
        DimCanvasGroup.alpha = 0f;
        _isOpen = false;
    }

    void CancelAndClose()
    {
        Close();
        _selectedIndex = 0;
    }

    void AnimateCartridge()
    {
        _canAcceptInput = false;

        cartridgeRect.DOKill();
        containerRect.DOKill();

        float baseY = containerBaseY;

        cartridgeRect.anchoredPosition = new Vector2(
            cartridgeRect.anchoredPosition.x,
            cartridgeHiddenY
        );

        Sequence seq = DOTween.Sequence();

        // 1. Cartridge drop
        seq.Append(
            cartridgeRect.DOAnchorPosY(
                cartridgeShownY,
                cartridgeSlideDuration
            ).SetEase(cartridgeSlideEase)
        );

        // 2. Bounce starts BEFORE impact
        float bounceStartTime = cartridgeSlideDuration * bounceTimePercent;

        seq.InsertCallback(bounceStartTime, () =>
        {
            AudioHelpers.PlaySoundEffect(clickSoundEffect, transform.position);
        });

        seq.Insert(bounceStartTime,
            containerRect.DOAnchorPosY(
                baseY - bounceAmount,
                bounceDuration * 0.35f
            ).SetEase(Ease.InQuad)
        );

        // 3. Continue bounce chain
        seq.Insert(bounceStartTime + bounceDuration * 0.35f,
            containerRect.DOAnchorPosY(
                baseY + bounceAmount * 0.2f,
                bounceDuration * 0.45f
            ).SetEase(Ease.OutQuad)
        );

        seq.Insert(bounceStartTime + bounceDuration * 0.8f,
            containerRect.DOAnchorPosY(
                baseY,
                bounceDuration * 0.3f
            ).SetEase(Ease.OutQuad)
        );

        
        // 🔹 4. Slight pause (feels good)
        seq.AppendInterval(2.0f);

        // 🔹 5. Load scene
        seq.AppendCallback(() =>
        {   
             CancelAndClose();
            
            GameSceneLoader.PendingConfig = new GameSceneConfig(
                _targetGameMode,
                0,
                null
            );

            SceneManager.LoadScene(_targetSceneName);
            
        });
        
    }

    void ActivateOption()
    {
        switch (_selectedIndex)
        {
            case 0:
                CancelAndClose();
                break;

            case 1:
                AnimateCartridge();
                break;

            default:
                Debug.LogWarning("No action for index " + _selectedIndex);
                break;
        }
    }
}*/