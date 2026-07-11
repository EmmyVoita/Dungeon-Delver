using UnityEngine;

[RequireComponent(typeof(Canvas))]
public class CardCanvasSorting : MonoBehaviour
{
    [SerializeField] private string sortingLayerName = "UI & Overlays";
    [SerializeField] private int normalOrder = 0;
    [SerializeField] private int highlightedOrder = 100;

    private Canvas _canvas;

    private void Awake()
    {
        _canvas = GetComponent<Canvas>();
        _canvas.overrideSorting = true;
        _canvas.sortingLayerName = sortingLayerName;
        _canvas.sortingOrder = normalOrder;
    }

    public void SetHighlighted(bool highlighted)
    {
        _canvas.sortingOrder = highlighted ? highlightedOrder : normalOrder;
    }
}