using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using Unity.VisualScripting;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine.Assertions.Must;

public class PauseMenuNavigator : BaseMenu
{
    public static event Action<bool> OnPaused;
    public static event Action OnResume;
    public static event Action OnQuit;
    public static event Action OnRestart;

    [Header("References")]
    [SerializeField] private SettingsMenuNavigator settingsMenu;
    [SerializeField] private GameObject pauseMenuRoot;
    [SerializeField] private GameObject settingsMenuRoot;

    [Header("Building UI")]
    [SerializeField] private List<PauseMenuOption> menuOptions;
    [SerializeField] private GameObject menuOptionPrefab;
    [SerializeField] private RectTransform menuOptionContainer;

    
    [Header("Background")]
    [SerializeField] private Material pauseBackgroundMaterial;
    [SerializeField] private string transtionPropertyName = "_Transition";
    [SerializeField] private float transitionInTargetValue = 1f;
    [SerializeField] private float transitionOutTargetValue = 1f;
    [SerializeField] private float transitionDuration = 0.5f;


    [Header("Audio")]
    [SerializeField] private SoundEffect openSound;
    [SerializeField] private SoundEffect closeSound;


    private List<PauseMenuOptionUI> menuOptionUIs = new();

    //public List<GameState> ignorePauseStates;

    [Header("UI Options")]
    public CanvasGroup pauseMenuGroup;
    public Color selectedColor = Color.yellow;
    public Color defaultColor = Color.white;
    public float selectedScale = 1.2f;
    public float transitionSpeed = 8f;
    private bool waitForConfirmRelease = false;


  

    [Header("Rows")]
    [SerializeField] private float rowDelay = 0.2f;
    [SerializeField] private float textFadeInDuration = 0.3f;

    private int _selectedIndex = 0;

    private Coroutine resumeRoutine;

    private void OnEnable()
    {
    }

    private void OnDisable()
    {
        Cleanup();
    }

    private void OnDestroy()
    {
        Cleanup();
    }

    private void Cleanup()
    {
        pauseBackgroundMaterial.DOKill();
        
        if(pauseBackgroundMaterial)
            pauseBackgroundMaterial.SetFloat(transtionPropertyName, transitionOutTargetValue);

        if(pauseMenuGroup)
            pauseMenuGroup.alpha = 0f;
        
        isActive = false;

        InputFocusManager.Release(this);
    }

    protected override void Awake()
    {
        if(pauseMenuGroup)
            pauseMenuGroup.alpha = 0f;

        _selectedIndex = -1;

        //pauseMenuRoot.SetActive(false);
        //settingsMenuRoot.SetActive(false);

        base.Awake();
    }


    // -------------------------------------------------------
    // MENU OPEN LOGIC
    // -------------------------------------------------------

    public override void OnOpen()
    {
        InputFocusManager.Claim(this);

        OnPaused?.Invoke(true);

        CountdownUI.Instance.KillActiveCountdown();

        waitForConfirmRelease = true;

        //pauseMenuRoot.SetActive(true);
        //settingsMenuRoot.SetActive(true);

        if (resumeRoutine != null)
        {
            StopCoroutine(resumeRoutine);
            resumeRoutine = null;
        }

        TimeManager.Instance.Pause();
        MusicManager.Instance.PauseMusic();

        AudioHelpers.PlaySoundEffect(openSound, transform.position);

        BuildMenuOptions();

        if (!pauseBackgroundMaterial.HasProperty(transtionPropertyName))
        {
            Debug.LogError(
                "Cannot find pause menu background transition property value"
            );
        }


        if(MenuManager.Instance.PreviousMenuState == MenuState.None)
        {
            pauseBackgroundMaterial.DOKill();

            pauseBackgroundMaterial.SetFloat(transtionPropertyName,transitionOutTargetValue);

            pauseBackgroundMaterial.DOFloat(transitionInTargetValue,transtionPropertyName,transitionDuration)
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    base.OnOpen();
                    pauseMenuGroup.alpha = 1f;
                });
        }
        else
        {
            base.OnOpen();
            pauseMenuGroup.alpha = 1f;
        }

        //InputModeManager.Instance.EnableMouseInput();
    }

    private void BuildMenuOptions()
    {
        foreach (PauseMenuOptionUI option in menuOptionUIs)
        {
            if (option != null)
                Destroy(option.gameObject);
        }

        menuOptionUIs.Clear();

        //foreach (PauseMenuOption menuOption in menuOptions)
        for(int i = 0; i < menuOptions.Count; i++)
        {
            GameObject menuUI = Instantiate(menuOptionPrefab, menuOptionContainer);

            PauseMenuOptionUI optionUI = menuUI.GetComponent<PauseMenuOptionUI>();
            PauseMenuOptionClickHandler clickHandler = menuUI.GetComponent<PauseMenuOptionClickHandler>();

            if (optionUI == null || clickHandler == null)
            {
                Destroy(menuUI);
                continue;
            }

            optionUI.Initialize(menuOptions[i]);
            clickHandler.Initialize(this, i);

            menuOptionUIs.Add(optionUI);
        }

        if (menuOptionUIs.Count > 0)
        {
            _selectedIndex = 0;
            UpdateVisuals();
        }
    }

    public override void OnClose()
    {
        //pauseMenuRoot.SetActive(false);
        //settingsMenuRoot.SetActive(false);
        InputFocusManager.Release(this);
        _selectedIndex = -1;
        UpdateVisuals();
        isActive = false;
        base.OnClose();
    }

    private void AnimateClose(Action onComplete)
    {
        AudioHelpers.PlaySoundEffect(closeSound, transform.position);
        pauseBackgroundMaterial.DOFloat(transitionOutTargetValue, transtionPropertyName, transitionDuration)
        .SetUpdate(true)
        .OnComplete(() =>
        {
            onComplete?.Invoke();
        });
    }

    // -------------------------------------------------------
    // INPUT + ANIMATION
    // -------------------------------------------------------

    void Update()
    {
        bool backPressed = InputBindingManager.Instance.GetKeyDown(InputActionType.Back);
        bool pauseAllowed = GameStateEffectManager.PauseAllowed;
        bool isTransitioning =TransitionManager.Instance.IsPlayingTransition;

        bool shouldOpenPause =backPressed && pauseAllowed && !isActive && !isTransitioning;


        if (shouldOpenPause)
        {
            MenuManager.Instance.OpenMenu(MenuState.Pause);
            return;
        }

        if (!CanProcessInput)
            return;
        
        if(!InputFocusManager.HasFocus(this))
            return;

        if(HandleSelectInput())
            return;

        HandleNavigation();
    }


    // Input Handling
    //-----------------------------------------------------------------------------------------

    private bool HandleSelectInput()
    {
        if (!InputBindingManager.Instance.GetKeyDown(InputActionType.Confirm))
            return false;

        AudioHelpers.PlaySoundEffect(AudioLibrary.Instance.Database.select, transform.position);
        ActivateOption();
        return true;
    }

    private void HandleNavigation()
    {
        if (InputBindingManager.Instance.GetKeyDown(InputActionType.MoveDown))
        {
            _selectedIndex = (_selectedIndex + 1) % menuOptions.Count;
            AudioHelpers.PlaySoundEffect(AudioLibrary.Instance.Database.navigateMajor, transform.position);
            UpdateVisuals();
        }
        else if (InputBindingManager.Instance.GetKeyDown(InputActionType.MoveUp))
        {
            _selectedIndex = (_selectedIndex - 1 + menuOptions.Count) % menuOptions.Count;
            AudioHelpers.PlaySoundEffect(AudioLibrary.Instance.Database.navigateMajor, transform.position);
            UpdateVisuals();
        }
    }

    private void UpdateVisuals()
    {
        for (int i = 0; i < menuOptionUIs.Count; i++)
        {
            menuOptionUIs[i].SetSelected(i == _selectedIndex);
        }
    }

    public void SetIndex(int index)
    {
        _selectedIndex = index;
        AudioHelpers.PlaySoundEffect(AudioLibrary.Instance.Database.select, transform.position);
        ActivateOption();
    }



    // -------------------------------------------------------
    // SELECTION
    // -------------------------------------------------------



    void ActivateOption()
    {
        PauseMenuOption menuOption = menuOptionUIs[_selectedIndex].MenuOption;

        switch (menuOption)
        {
            case PauseMenuOption.Resume:
                HandleResume();
                break;
            case PauseMenuOption.Restart:
                StartCoroutine(RestartSequence());
                break;
            case PauseMenuOption.Settings:
                MenuManager.Instance.RequestMenuTransition(MenuState.Settings);
                //settingsCanvasGroup.alpha = 1;
                //settingsMenu.OnOpen();
                break;
            case PauseMenuOption.Quit:
                StartCoroutine(QuitSequence());
                break;
            default:
                break;
        }
    }

    private void HandleResume()
    {
        pauseMenuGroup.alpha = 0;

        OnPaused?.Invoke(false);

        AnimateClose(() =>
        {
            MenuManager.Instance.CloseCurrentMenu();
        });

        if (GameStateManager.Instance.CurrentState == GameState.RoundActive)
        {
            OverlayManager.Instance.CloseOverlay();

            CountdownUI.Instance.KillActiveCountdown(() =>
            {
                CountdownUI.Instance.BeginCountdown(() =>
                {
                    resumeRoutine = StartCoroutine(ResumeSequence());
                });
            });
        }
        else
        {
            resumeRoutine = StartCoroutine(ResumeSequence());
        }
    }

    private IEnumerator ResumeSequence()
    {
        InputBindingManager.Instance.BlockUntilRelease(InputActionType.Confirm);

        yield return new WaitForSecondsRealtime(0.1f);

        OverlayManager.Instance.CloseOverlay();

        yield return null;

        OnResume?.Invoke();

        TimeManager.Instance.Resume();
        MusicManager.Instance.ResumeMusic();

        resumeRoutine = null;
    }


    private IEnumerator QuitSequence()
    {
        SceneReturnHandler.ReturnToAbilitySelect = false;
        OnQuit?.Invoke();
        
    
        yield return new WaitForSecondsRealtime(0.5f);

        RunLifecycle.AbadonRun();
        SceneManager.LoadScene(SceneNames.MainMenu);
        yield return null;
    }

    private IEnumerator RestartSequence()
    {
        SceneReturnHandler.ReturnToAbilitySelect = false;

        

        yield return new WaitForSecondsRealtime(0.5f);

        RunLifecycle.RestartRun();
        SceneManager.LoadScene(SceneNames.ArrowGameScene);
        yield return null;
    }
}
