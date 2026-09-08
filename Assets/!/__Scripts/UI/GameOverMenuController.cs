using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;
using DG.Tweening;
using UnityEngine.InputSystem;

public class GameOverMenuController : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private RectTransform rootContainer;
    [SerializeField] private RectTransform optionContainer;
    [SerializeField] private YourCardsInventory inventory;
    [SerializeField] private KeyPromptController keyPromptController;
    [SerializeField] private GameOverPagesController gameOverPagesController;
    

    [Header("FadeIn Options")]
    [SerializeField] private bool fadeInOnGameOver;
    [SerializeField] private float fadeInDuration = 0.3f;
    [SerializeField] private float fadeInDelay = 1.0f;

    [Header("Input Options")]
    [SerializeField] private float inputDelay = 1.0f;
    [SerializeField] private InputActionType inventoryKey = InputActionType.ViewUpgrades;

    [Header("Menu Options")]
    [SerializeField] private TextMeshProUGUI[] options;

    [Header("Visual Settings")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color selectedColor = Color.yellow;
    [SerializeField] private float selectedScale = 1.2f;
    [SerializeField] private float transitionSpeed = 8f;


    [Header("Dynamic")]
    [SerializeField] private int selectedIndex = 0; // 0 = Play Again, 1 = Return to Menu
    [SerializeField] private bool menuActive = false;

    

    private void OnEnable()
    {
        GameStateManager.OnStateChanged += HandleStateChanged;
        GameStatsUI.OnStatsTallyComplete += HandleTallyComplete;
        Inventory.OnInventoryStateChanged += HandleInventoryStateChanged;
    }

    private void OnDisable()
    {
        GameStateManager.OnStateChanged -= HandleStateChanged;
        GameStatsUI.OnStatsTallyComplete -= HandleTallyComplete;
        Inventory.OnInventoryStateChanged -= HandleInventoryStateChanged;
    }

    private void HandleInventoryStateChanged(bool openState)
    {
        canvasGroup.DOFade(openState ? 0 : 1, 0.1f);
    }

    private void HandleTallyComplete()
    {
        if(GameStateManager.Instance.CurrentState == GameState.GameOverTally)
            GameStateManager.Instance.SetState(GameState.GameOverResults);
    }

    private void Awake()
    {
        //rootContainer.gameObject.SetActive(false);
        optionContainer.gameObject.SetActive(false);
        canvasGroup.alpha = 1;

        menuActive = false;
    }

    private void Start()
    {
        rootContainer.gameObject.SetActive(true);
    }

    private void HandleStateChanged(GameState previousState, GameState newState)
    {
        if(newState == GameState.GameOverResults && previousState != newState && fadeInOnGameOver)
        {
            rootContainer.gameObject.SetActive(true);
            FadeInTextOptions();
        }
    }

    private void FadeInTextOptions()
    {
        
        for(int i = 0; i < options.Length; i++)
        {
            options[i].alpha = 0;
        }

        optionContainer.gameObject.SetActive(true);

        Sequence seq = DOTween.Sequence();

        seq.AppendInterval(fadeInDelay);

        for(int i = 0; i < options.Length; i++)
        {
            seq.Append(options[i].DOFade(1f, fadeInDuration));
        }
        
        seq.AppendInterval(inputDelay);
        seq.AppendCallback(() =>
        {
            
            menuActive = true;
            UpdateVisuals();
        });
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
    

    private void Update()
    {
        if (!menuActive || InputFocusManager.CurrentOwner != null) 
            return;

        AnimateSelection();

        if(HandleConfirmInput())
            return;

        if(HandleInventoryInput())
            return;

        HandleNaviagationInput();
    }

    
    // Update Loop Input
    // --------------------------------------------------------------------------------------------

    private bool HandleInventoryInput()
    {
        if(!InputBindingManager.Instance.GetKeyDown(inventoryKey))
            return false;

        inventory.SetVisibilityData(gameOverPagesController, GameOverPagesController.GameOverPage.Results);

        if(inventory.TryOpen(keyPromptController))
            gameOverPagesController.ShowInventory();
        
        return true;
    }

    private bool HandleConfirmInput()
    {
        if (!InputBindingManager.Instance.GetKeyDown(InputActionType.Confirm))
            return false;
        
        AudioHelpers.PlaySoundEffect(AudioLibrary.Instance.Database.select, transform.position);
        
        switch (selectedIndex)
        {
            case 0:
                SceneReturnHandler.ReturnToAbilitySelect = false;
                RunLifecycle.RestartRun();
                StartCoroutine(LoadSceneDelay(SceneNames.ArrowGameScene));
                break;
            case 1:
                SceneReturnHandler.ReturnToAbilitySelect = false;
                RunLifecycle.AbadonRun();
                StartCoroutine(LoadSceneDelay(SceneNames.MainMenu));
                break;
            case 2:
                inventory.SetVisibilityData(gameOverPagesController, GameOverPagesController.GameOverPage.Results);

                if(inventory.TryOpen(keyPromptController))
                    gameOverPagesController.ShowInventory();

                break;
        }

        //rootContainer.gameObject.SetActive(false);

        return true;
    }

    private void HandleNaviagationInput()
    {
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
    }


    private void UpdateVisuals()
    {
        for(int i = 0; i < options.Length; i++)
        {
            options[i].color = selectedIndex == i ? selectedColor : normalColor;
        }
    }

    private IEnumerator LoadSceneDelay(string sceneName)
    {
        yield return new WaitForSeconds(1.0f);
        SceneManager.LoadScene(sceneName);
    }
}
