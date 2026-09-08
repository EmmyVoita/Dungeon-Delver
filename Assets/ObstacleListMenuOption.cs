using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using System;

public class ObstacleListMenuOption : PracticeMenuOption, IPointerClickHandler
{
    public static event Action<PracticeMenuOption> OnObstacleListOptionClick;
    private ObstacleTypeDefinition obstacle;
    private TextMeshProUGUI text;
    private PracticeObstacleOption _visual;

    public ObstacleTypeDefinition Obstacle => obstacle;
    //public Color exitColor = Color.white;

    public void Setup(ObstacleTypeDefinition def)
    {
        obstacle = def;
        text = GetComponentInChildren<TextMeshProUGUI>();
        text.color = obstacle.textColor;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        OnObstacleListOptionClick?.Invoke(this);
    }

    

    public override void OnEnter()
    {
        base.OnEnter();

        if (_visual == null)
            _visual = GetComponent<PracticeObstacleOption>();

        _visual?.SetSelected(true);
    }

    public override void OnExit()
    {
        base.OnExit();

        if (_visual == null)
            _visual = GetComponent<PracticeObstacleOption>();

        _visual?.SetSelected(false);
    }

    public override void HandleDirectionalInput(Vector2 input)
    {
        // Move to next/previous based on input
        if (input == Vector2.up || input == Vector2.down)
            OnNavigateToOption?.Invoke(input);
    }

    public override void OnConfirm()
    {

    }
}
