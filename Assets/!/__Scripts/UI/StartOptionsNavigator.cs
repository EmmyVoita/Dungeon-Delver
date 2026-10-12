using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using DG.Tweening;

public class StartOptionsNavigator : MonoBehaviour
{
    public static event Action OnStartSelected;
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
        StartOptionMouseHandler.OnStartOptionClicked -= HandleStartOptionClicked;
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

    private void OnDestroy()
    {
        rect.DOKill();
        dimCanvasGroup.DOKill();
    }

    void HandleInput()
    {
        if (InputBindingManager.Instance.GetKeyDown(InputActionType.MoveRight))
        {
            _selectedIndex = (_selectedIndex + 1) % options.Length;
            AudioHelpers.PlaySoundEffect(AudioLibrary.Instance.Database.navigateMajor, transform.position);
            UpdateVisuals();
        }
        else if (InputBindingManager.Instance.GetKeyDown(InputActionType.MoveLeft))
        {
            _selectedIndex = (_selectedIndex - 1 + options.Length) % options.Length;
            AudioHelpers.PlaySoundEffect(AudioLibrary.Instance.Database.navigateMajor, transform.position);
            UpdateVisuals();
        }

        if (InputBindingManager.Instance.GetKeyDown(InputActionType.Confirm))
        {
            AudioHelpers.PlaySoundEffect(AudioLibrary.Instance.Database.select, transform.position);
            ActivateOption();
        }
        else if (InputBindingManager.Instance.GetKeyDown(InputActionType.Back))
        {
            Debug.LogWarning("START OPTIONS BACK PRESSED");
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

        InputFocusManager.Claim(this);

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

        _canAcceptInput = false;
        _isOpen = false;

        InputFocusManager.Release(this);

        rect.DOAnchorPos(new Vector2(hiddenX, hiddenY),duration).SetEase(Ease.InBack);

        dimCanvasGroup.DOFade(0f, duration);
    }

    void CloseImmediate()
    {
        rect.DOKill();
        dimCanvasGroup.DOKill();

        rect.anchoredPosition = new Vector2(hiddenX, hiddenY);
        dimCanvasGroup.alpha = 0f;

        _isOpen = false;
        _canAcceptInput = false;

        InputFocusManager.Release(this);
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
                OnStartSelected?.Invoke();
                break;
                /*
                GameSceneLoader.PendingConfig = new GameSceneConfig(
                    _targetGameMode,
                    0,
                    null
                );

                SceneManager.LoadScene(_targetSceneName);
                break;
                */
        }
        
    }
}
