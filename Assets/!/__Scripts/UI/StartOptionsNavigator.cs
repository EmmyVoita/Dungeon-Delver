using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System;

 public enum SelectOption
{
    Tutorial,
    MainGame
}

public class StartOptionsNavigator : MonoBehaviour
{
   

    public static Action<StartMenuWindows> OnReturnFromStartOptions;

    public GameObject startOptionsUI;
    [Header("UI Options")]
    public TextMeshProUGUI[] options;
    public Color selectedColor = Color.yellow;
    public Color defaultColor = Color.white;
    public float selectedScale = 1.2f;
    public float transitionSpeed = 8f;

    private int selectedIndex = 0;
    private bool isOpen = false;
    private SelectOption selectOption;

    private void OnEnable()
    {
        MainMenuNavigator.OnTutorialSelected += Open;
        AbilitySelectManager.OnAbilitySelected += Open;
    }

    private void OnDisable()
    {
        MainMenuNavigator.OnTutorialSelected -= Open;
        AbilitySelectManager.OnAbilitySelected -= Open;
    }

    void Start()
    {
        Close();

        if (options.Length == 0)
        {
            Debug.LogError("⚠️ No options assigned to StartOptionsNavigator.");
            return;
        }

        selectedIndex = 1;
        UpdateVisuals();
    }

    void Update()
    {
        if (!isOpen) return;

        HandleInput();
        AnimateSelection();
    }

    void HandleInput()
    {
        if (InputBindingManager.Instance.GetKeyDown(InputActionType.MoveRight))
        {
            selectedIndex = (selectedIndex + 1) % options.Length;
            AudioSettingsManager.PlayNavigateSound();
            UpdateVisuals();
        }
        else if (InputBindingManager.Instance.GetKeyDown(InputActionType.MoveLeft))
        {
            selectedIndex = (selectedIndex - 1 + options.Length) % options.Length;
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
            bool isSelected = (i == selectedIndex);
            options[i].color = isSelected ? selectedColor : defaultColor;
        }
    }

    void AnimateSelection()
    {
        for (int i = 0; i < options.Length; i++)
        {
            float targetScale = (i == selectedIndex) ? selectedScale : 1f;
            options[i].transform.localScale = Vector3.Lerp(
                options[i].transform.localScale,
                Vector3.one * targetScale,
                Time.deltaTime * transitionSpeed
            );
        }
    }

    // 🔹 Called from TutorialSelected / AbilitySelected
    void Open(SelectOption selectOption)
    {
        this.selectOption = selectOption;
        startOptionsUI.SetActive(true);
        isOpen = true;
        selectedIndex = 1;
        UpdateVisuals();

        // 🔒 Lock underlying input
        MenuManager.Instance.LockActiveMenuInput(true);
    }

    void Close()
    {
        startOptionsUI.SetActive(false);
        isOpen = false;

        // 🔓 Unlock underlying input
        MenuManager.Instance.LockActiveMenuInput(false, 0.25f);
    }

    void CancelAndClose()
    {
        Close();
        AudioSettingsManager.PlayNavigateSound();
        selectedIndex = 0;
    }

    void ActivateOption()
    {
        switch (selectedIndex)
        {
            case 0:
                // "Cancel" / "Exit" Option
                CancelAndClose();
                break;

            case 1:
                // "Confirm" Option
                Close(); // unlock + hide banner
                

                switch(selectOption)
                {
                    case SelectOption.Tutorial:
                    GameSceneLoader.PendingConfig = new GameSceneConfig(
                        GameMode.Tutorial,
                        0,
                        null,
                        JumpDirectionMode.FourDirectional);

                    SceneManager.LoadScene(SceneNames.TutorialScene);
                    break;

                    case SelectOption.MainGame:
                    GameSceneLoader.PendingConfig = new GameSceneConfig(
                        GameMode.StandardRun,
                        0,
                        null,
                        JumpDirectionMode.FourDirectional);

                    SceneManager.LoadScene(SceneNames.ArrowGameScene);
                    break;
                    default:
                    break;
                }

            
                break;

            default:
                Debug.LogWarning($"⚠️ No action defined for option index {selectedIndex}");
                break;
        }
    }
}
