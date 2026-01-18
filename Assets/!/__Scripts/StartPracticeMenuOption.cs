
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartPracticeMenuOption : PracticeMenuOption
{
    private TextMeshProUGUI text;

    public override void OnEnter()
    {
        base.OnEnter();
        Debug.Log("Entered StartPracticeMenuOption");
        text = GetComponent<TextMeshProUGUI>();
        text.color = Color.yellow;
        text.transform.localScale = Vector3.one * 1.2f;
    }

    public override void OnExit()
    {
        base.OnExit();
        text = GetComponent<TextMeshProUGUI>();
        text.color = Color.white;
        text.transform.localScale = Vector3.one;
    }

    public override void HandleDirectionalInput(Vector2 input)
    {
        Debug.Log("HandleDirectionalInput called in StartPracticeMenuOption");
        // Move to next/previous based on input
        if (input == Vector2.up)
            OnNavigateToOption?.Invoke(input);

        else if (input == Vector2.right) // go to settings panel
            OnNavigateToOption?.Invoke(Vector2.right);
    }
    public override void OnConfirm()
    {
        AudioSettingsManager.PlaySelectSound();
        ObstaclePracticeSession.SelectedObstacle = ObstacleLabMenuNavigator.Instance.CurrentObstacle; 
        SceneManager.LoadScene(SceneNames.ObstaclePractice);
    }
}