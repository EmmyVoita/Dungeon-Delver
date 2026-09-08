using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PracticeObstacleOption : PracticeMenuOption
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private RectTransform nameContainerRect;
    [SerializeField] private RectTransform rootRect;
    [SerializeField] private LayoutElement outlineLayoutElement;
    [SerializeField] private RectTransform outlineRect;

    [Header("Hover")]
    [SerializeField] private float hoverScale = 1.05f;
    [SerializeField] private float hoverDuration = 0.15f;
    [SerializeField] private Ease hoverEase = Ease.OutCubic;


    private ObstacleTypeDefinition _defintion;

    public ObstacleTypeDefinition Obstacle => _defintion;

    public void Initialize(ObstacleTypeDefinition definition)
    {
        _defintion = definition;

        nameText.text = definition.displayName;
        nameText.color = definition.textColor;

        // Make TMP calculate its new preferred size
        nameText.ForceMeshUpdate();

        // Make the layout resize NameContainer based on the new text
        LayoutRebuilder.ForceRebuildLayoutImmediate(nameContainerRect);

        // Now its width should be correct
        outlineLayoutElement.preferredWidth = nameContainerRect.rect.width;
        outlineLayoutElement.preferredHeight = nameContainerRect.rect.height;

        SetSelected(false, false);
    }

    public void SetSelected(bool selected, bool animate = true)
    {
        outlineLayoutElement.preferredWidth = nameContainerRect.rect.width;
        outlineLayoutElement.preferredHeight = nameContainerRect.rect.height;

        // Only show outline while this option is selected
        if (outlineRect)
            outlineRect.gameObject.SetActive(selected);

        if (!rootRect)
            return;

        rootRect.DOKill();

        float targetScale = selected ? hoverScale : 1f;

        if (!animate)
        {
            rootRect.localScale = Vector3.one * targetScale;
            return;
        }

        rootRect.DOScale(
            targetScale,
            hoverDuration
        )
        .SetEase(hoverEase)
        .SetUpdate(true);
    }

    private void OnDestroy()
    {
        rootRect?.DOKill();
    }

    public override void OnEnter()
    {
        SetSelected(true);
    }

    public override void OnExit()
    {
        SetSelected(false);
    }

    public override void HandleDirectionalInput(Vector2 input)
    {
        // Move to next/previous based on input
        if (input == Vector2.up || input == Vector2.down)
            OnNavigateToOption?.Invoke(input);
    }
}