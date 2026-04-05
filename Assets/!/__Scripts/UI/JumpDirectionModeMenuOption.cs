using TMPro;
using UnityEngine;

public class JumpDirectionModeMenuOption : PracticeMenuOption
{
    [SerializeField] private int selectedIndex = 0;
    [SerializeField] private int optionCount = 2; // 4-way, 8-way. Expandable later.

    public TextMeshProUGUI[] modeTexts; // Assign 4-way, 8-way in Inspector
    public Color selectedColor = Color.yellow;
    public Color defaultColor = Color.white;
    public Color leaveColor = Color.gray;

    public override void HandleDirectionalInput(Vector2 input)
    {
      // Move to next/previous based on input
        if (input == Vector2.down)
            OnNavigateToOption?.Invoke(input);

        else if (input == Vector2.right) // go to settings panel
        {
            AudioSettingsManager.PlaySelectSound();
            selectedIndex = (selectedIndex + 1) % optionCount;  // wrap forward
        }
        else if (input == Vector2.left)
        {
            AudioSettingsManager.PlaySelectSound();
            selectedIndex = (selectedIndex - 1 + optionCount) % optionCount; //
        }
            
        ApplyMode();
        UpdateVisuals();
    }

    private void ApplyMode()
    {
        //GameSceneLoader.PendingConfig.DirectionMode = (selectedIndex == 0) ? JumpDirectionMode.FourDirectional : JumpDirectionMode.EightDirectional;
    }

    public override void OnEnter()
    {
        UpdateVisuals();

        Debug.Log("Entering JumpDirectionModeMenuOption");
    }

    public override void OnExit()
    {
        Debug.Log("Exiting JumpDirectionModeMenuOption");
        UpdateVisuals(isExiting: true); // 🔹 force exit visuals
    }

    private void UpdateVisuals(bool isExiting = false)
    {
        bool isCurrent = ObstacleLabMenuNavigator.Instance.CurrentOption == this;

        // If we are explicitly exiting, treat as "not current"
        if (isExiting)
            isCurrent = false;

        for (int i = 0; i < modeTexts.Length; i++)
        {
            if (isCurrent)
            {
                modeTexts[i].color = (i == selectedIndex) ? selectedColor : defaultColor;
                modeTexts[i].transform.localScale = (i == selectedIndex) ? Vector3.one * 1.2f : Vector3.one;
            }
            else
            {
                // 🔹 Exit / non-current visuals
                // You probably want leaveColor here instead of Color.white
                modeTexts[i].color = leaveColor;
                modeTexts[i].transform.localScale = Vector3.one;
            }
        }
    }

}
