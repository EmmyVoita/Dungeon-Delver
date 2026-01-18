using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System;

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
    private string nextSceneName;

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
    void Open(string nextSceneName)
    {
        this.nextSceneName = nextSceneName;
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
                SceneManager.LoadScene(nextSceneName);
                break;

            default:
                Debug.LogWarning($"⚠️ No action defined for option index {selectedIndex}");
                break;
        }
    }
}
