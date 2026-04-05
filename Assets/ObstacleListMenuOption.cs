using UnityEngine;
using TMPro;

public class ObstacleListMenuOption : PracticeMenuOption
{
    private ObstacleTypeDefinition obstacle;
    private TextMeshProUGUI text;
    //public Color exitColor = Color.white;

    public void Setup(ObstacleTypeDefinition def)
    {
        obstacle = def;
        text = GetComponent<TextMeshProUGUI>();
        text.color = obstacle.textColor;
    }

    public override void OnEnter()
    {
        base.OnEnter();
        text.color = Color.yellow;
        text.transform.localScale = Vector3.one * 1.2f;
    }

    public override void OnExit()
    {
        base.OnExit();
        text.color = obstacle.textColor;
        text.transform.localScale = Vector3.one;
    }

    public override void HandleDirectionalInput(Vector2 input)
    {
        // Move to next/previous based on input
        if (input == Vector2.up || input == Vector2.down)
            OnNavigateToOption?.Invoke(input);
    }

    public override void OnConfirm()
    {
        GameSceneLoader.PendingConfig = new GameSceneConfig(
            GameMode.ObstaclePractice,
            0,
            ObstacleLabMenuNavigator.Instance.CurrentObstacle,
            JumpDirectionMode.FourDirectional
        );
    }
}
