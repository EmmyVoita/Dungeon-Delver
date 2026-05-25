using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using Unity.VisualScripting;
using System.Collections.Generic;

public class PauseMenuNavigator : BaseMenu
{

    public static Action OnResume;
    public static Action OnQuit;
    public static Action OnRestart;

    
    //public List<GameState> ignorePauseStates;

    [Header("UI Options")]
    public RectTransform pauseMenuUI;
    public TextMeshProUGUI[] options;
    public Color selectedColor = Color.yellow;
    public Color defaultColor = Color.white;
    public float selectedScale = 1.2f;
    public float transitionSpeed = 8f;
    private bool waitForConfirmRelease = false;


    [SerializeField] private int selectedIndex = 0;

    private void OnEnable()
    {
    }

    private void OnDisable()
    {
    }

    void Awake()
    {
        //MenuManager.Instance.RegisterMenu(this);
        //MenuManager.Instance.OpenMenu(StartMenuWindows.MainMenu);
        pauseMenuUI.gameObject.SetActive(false);
        lockInput = true;
    }

    void Start()
    {
        /*
        if (SceneReturnHandler.ReturnToAbilitySelect)
        {
           MenuManager.Instance.TransitionToMenu(StartMenuWindows.PlayMenu, 0.2f);
        }
        */
    }

    // -------------------------------------------------------
    // MENU OPEN LOGIC
    // -------------------------------------------------------

    public override void OnOpen()
    {
        base.OnOpen();

        waitForConfirmRelease = true;

        selectedIndex = 0;

        if (options == null || options.Length == 0)
        {
            Debug.LogError("MainMenuNavigator: 'options' is empty or not assigned.");
            return;
        }

        foreach (var opt in options)
        {
            if (opt == null)
            {
                Debug.LogError("MainMenuNavigator: Null option found in array.");
                continue;
            }
            opt.gameObject.SetActive(true);
        }

        UpdateVisuals();
    }

    public override void OnClose()
    {
        foreach (var option in options)
            option.gameObject.SetActive(false);
            
        base.OnClose();
    }

    // -------------------------------------------------------
    // INPUT + ANIMATION
    // -------------------------------------------------------

    void Update()
    {
        if(InputBindingManager.Instance.GetKeyDown(InputActionType.Back) 
           && !pauseMenuUI.gameObject.activeSelf
           && GameStateEffectManager.PauseAllowed
           && !TransitionManager.Instance.IsPlayingTransition)
        {
            OnOpen();
            pauseMenuUI.gameObject.SetActive(true);

            waitForConfirmRelease = true;

            TimeManager.Instance.Pause();
            MusicManager.Instance.PauseMusic();
            lockInput = false;

            //GameStateManager.Instance.SetState(GameState.Paused);

            OverlayManager.Instance.ShowOverlay(
                OverlayState.Pause
            );
            
           
            /*
           
            */
        }

    
        if (lockInput) return;

        HandleInput();
        AnimateSelection();
    }

    void HandleInput()
    {
        if (waitForConfirmRelease)
        {
            if (!InputBindingManager.Instance.GetKeyInput(InputActionType.Confirm))
            {
                waitForConfirmRelease = false;
            }
            return;
        }

        if (InputBindingManager.Instance.GetKeyDown(InputActionType.MoveDown))
        {
            selectedIndex = (selectedIndex + 1) % options.Length;
            AudioHelpers.PlaySoundEffect(AudioLibrary.Instance.Database.navigate, transform.position);
            UpdateVisuals();
        }
        else if (InputBindingManager.Instance.GetKeyDown(InputActionType.MoveUp))
        {
            selectedIndex = (selectedIndex - 1 + options.Length) % options.Length;
            AudioHelpers.PlaySoundEffect(AudioLibrary.Instance.Database.navigate, transform.position);
            UpdateVisuals();
        }

        if (InputBindingManager.Instance.GetKeyDown(InputActionType.Confirm))
        {
            AudioHelpers.PlaySoundEffect(AudioLibrary.Instance.Database.select, transform.position);
            ActivateOption();
        }
    }

    public void SetIndex(int index)
    {
        selectedIndex = index;
        AudioHelpers.PlaySoundEffect(AudioLibrary.Instance.Database.select, transform.position);
        ActivateOption();
    }

    void UpdateVisuals()
    {
        for (int i = 0; i < options.Length; i++)
        {
            options[i].color = (i == selectedIndex) ? selectedColor : defaultColor;
        }
    }

    void AnimateSelection()
    {
        float dt = Time.unscaledDeltaTime;

        for (int i = 0; i < options.Length; i++)
        {
            float targetScale = (i == selectedIndex) ? selectedScale : 1f;

            options[i].transform.localScale = Vector3.Lerp(
                options[i].transform.localScale,
                Vector3.one * targetScale,
                dt * transitionSpeed
            );
        }
    }


    // -------------------------------------------------------
    // SELECTION
    // -------------------------------------------------------



    void ActivateOption()
    {
        string optionName = options[selectedIndex].text.ToLower();

        switch (selectedIndex)
        {
            case 0:
                pauseMenuUI.gameObject.SetActive(false);
                lockInput = true;
                
                if (GameStateManager.Instance.CurrentState == GameState.RoundActive)
                {
                    OverlayManager.Instance.CloseOverlay();

                    CountdownUI.Instance.KillActiveCountdown(() =>
                    {
                        CountdownUI.Instance.BeginCountdown(() =>
                        {
                            StartCoroutine(ResumeSequence());
                        });
                    });
                }
                else
                {
                    StartCoroutine(ResumeSequence());
                }

                break;
            case 1:
                StartCoroutine(RestartSequence());
                break;
            case 2:
                StartCoroutine(QuitSequence());
                break;
            default:
                Debug.LogError("No action assigned to " + optionName);
                break;
        }
    }

    private IEnumerator ResumeSequence()
    {
        InputBindingManager.Instance.BlockConfirmUntilRelease();

        yield return new WaitForSecondsRealtime(0.1f);

        OverlayManager.Instance.CloseOverlay();

        yield return null;

        TimeManager.Instance.Resume();
    }


    private IEnumerator QuitSequence()
    {
        SceneReturnHandler.ReturnToAbilitySelect = false;

        yield return new WaitForSecondsRealtime(0.5f);
        SceneManager.LoadScene(SceneNames.MainMenu);
        yield return null;
    }

    private IEnumerator RestartSequence()
    {
        SceneReturnHandler.ReturnToAbilitySelect = false;

        yield return new WaitForSecondsRealtime(0.5f);
        SceneManager.LoadScene(SceneNames.ArrowGameScene);
        yield return null;
    }
}
