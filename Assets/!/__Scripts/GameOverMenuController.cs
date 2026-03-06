using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;
using DG.Tweening;

public class GameOverMenuController : MonoBehaviour
{
    [SerializeField] private RectTransform optionContainer;

    [Header("FadeIn Options")]
    [SerializeField] private bool fadeInOnGameOver;
    [SerializeField] private float fadeInDuration = 0.3f;
    [SerializeField] private float fadeInDelay = 1.0f;

    [Header("Input Options")]
    [SerializeField] private float inputDelay = 1.0f;

    [Header("Menu Options")]
    [SerializeField] private TextMeshProUGUI[] options;
    [SerializeField] private TMP_Text playAgainText;
    [SerializeField] private TMP_Text returnToMenuText;

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
        //UIManager.OnGameOverUI += ActivateMenu;
        GameStateManager.OnStateChanged += HandleStateChanged;
    }

    private void OnDisable()
    {
        //UIManager.OnGameOverUI -= ActivateMenu;
        GameStateManager.OnStateChanged -= HandleStateChanged;
    }

    private void Awake()
    {
        optionContainer.gameObject.SetActive(false);

        menuActive = false;
    }

    private void HandleStateChanged(GameState previousState, GameState newState)
    {
        if(newState == GameState.GameOver && previousState != newState && fadeInOnGameOver)
        {
            FadeInTextOptions();
        }
    }

    private void FadeInTextOptions()
    {
        playAgainText.alpha = 0f;
        returnToMenuText.alpha = 0f;

        optionContainer.gameObject.SetActive(true);

        Sequence seq = DOTween.Sequence();

        seq.AppendInterval(fadeInDelay);
        seq.Append(playAgainText.DOFade(1f, fadeInDuration));
        seq.Append(returnToMenuText.DOFade(1f, fadeInDuration));
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
        if (!menuActive) return;

        if (InputBindingManager.Instance.GetKeyDown(InputActionType.MoveDown))
        {
            selectedIndex = (selectedIndex + 1) % options.Length;
            AudioSettingsManager.PlayNavigateSound();
            UpdateVisuals();
        }
        else if (InputBindingManager.Instance.GetKeyDown(InputActionType.MoveUp))
        {
            selectedIndex = (selectedIndex - 1 + options.Length) % options.Length;
            AudioSettingsManager.PlayNavigateSound();
            UpdateVisuals();
        }

        if (InputBindingManager.Instance.GetKeyDown(InputActionType.Confirm))
        {
            AudioSettingsManager.PlaySelectSound();
            ConfirmSelection();
        }

        AnimateSelection();
    }

    private void UpdateVisuals()
    {
        playAgainText.color = (selectedIndex == 0) ? new Color(selectedColor.r,selectedColor.g,selectedColor.b,playAgainText.color.a) : 
                                                     new Color(normalColor.r,normalColor.g,normalColor.b,playAgainText.color.a);
        returnToMenuText.color = (selectedIndex == 1) ? new Color(selectedColor.r,selectedColor.g,selectedColor.b,playAgainText.color.a) : 
                                                        new Color(normalColor.r,normalColor.g,normalColor.b,playAgainText.color.a);
    }

    private void ConfirmSelection()
    {

        ScoreManager.Instance.SaveCurrentScore();
        switch (selectedIndex)
        {
            case 0:
                Debug.Log("Restarting game...");
                Time.timeScale = 1f; // ensure game unpauses
                SceneReturnHandler.ReturnToAbilitySelect = true;
                SceneManager.LoadScene("MainMenuScene");
                break;

            case 1:
                Debug.Log("Returning to main menu...");
                Time.timeScale = 1f;
                SceneReturnHandler.ReturnToAbilitySelect = false;
                SceneManager.LoadScene("MainMenuScene");
                break;
        }
    }
}
