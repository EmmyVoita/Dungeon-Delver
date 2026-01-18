using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameOverMenuController : MonoBehaviour
{
    [Header("Menu Options")]
    [SerializeField] private TMP_Text playAgainText;
    [SerializeField] private TMP_Text returnToMenuText;

    [Header("Visual Settings")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color selectedColor = Color.yellow;

    private int selectedIndex = 0; // 0 = Play Again, 1 = Return to Menu
    private bool menuActive = false;

    private void OnEnable()
    {
        //Invoke(nameof(ActivateMenu), 1.0f);
        UIManager.OnGameOverUI += ActivateMenu;
    }

    private void OnDisable()
    {
        UIManager.OnGameOverUI -= ActivateMenu;
    }

    private void Awake()
    {
        menuActive = false;
    }

    private void ActivateMenu()
    {
        menuActive = true;
        UpdateSelection();
    }

    private void Update()
    {
        if (!menuActive) return;

        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            selectedIndex = Mathf.Max(0, selectedIndex - 1);
            UpdateSelection();
        }
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            selectedIndex = Mathf.Min(1, selectedIndex + 1);
            UpdateSelection();
        }

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
        {
            ConfirmSelection();
        }
    }

    private void UpdateSelection()
    {
        playAgainText.color = (selectedIndex == 0) ? selectedColor : normalColor;
        returnToMenuText.color = (selectedIndex == 1) ? selectedColor : normalColor;
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
