using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class TimelineBoxSelect : MonoBehaviour,
    IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [SerializeField] private RectTransform content;
    [SerializeField] private TimelineMarkerController controller;
    [SerializeField] private Image selectionBoxImage;
    [SerializeField] private RectTransform viewport;
    [SerializeField] private ScrollRect scrollRect;


    private RectTransform selectionBox;
    private Vector2 dragStartLocal;
    private Vector2 currentLocal;
    private bool isSelecting;
    private Rect lastBox;

    void Awake()
    {
        selectionBox = selectionBoxImage.rectTransform;
        selectionBox.gameObject.SetActive(false);
        selectionBox.SetParent(content, false);

    }

    public void OnPointerDown(PointerEventData e)
    {
        if (e.button != PointerEventData.InputButton.Left)
            return;

        if (e.pointerEnter?.GetComponentInParent<TimelineMarker>() != null)
            return;

        scrollRect.enabled = false; // 🔒 STOP SCROLLING

        dragStartLocal = ScreenToContentLocal(e.position);
        currentLocal = dragStartLocal;

        controller.ClearSelectionPreview();

        isSelecting = true;
        selectionBox.gameObject.SetActive(true);
    }


    public void OnDrag(PointerEventData e)
    {
        if (!isSelecting)
            return;

        currentLocal = ScreenToContentLocal(e.position);
    }

    void Update()
    {
        if (!isSelecting)
            return;

        UpdateSelectionBox(dragStartLocal, currentLocal);

        Rect box = GetSelectionRect();
        if (box == lastBox)
            return;

        lastBox = box;
        ApplySelectionPreview(box);
    }

    public void OnPointerUp(PointerEventData e)
    {
        if (!isSelecting)
        return;
        
        isSelecting = false;
        selectionBox.gameObject.SetActive(false);

        scrollRect.enabled = true;
    }

    private void UpdateSelectionBox(Vector2 a, Vector2 b)
    {
        float xMin = Mathf.Min(a.x, b.x);
        float xMax = Mathf.Max(a.x, b.x);
        float yMin = Mathf.Min(a.y, b.y);
        float yMax = Mathf.Max(a.y, b.y);

        selectionBox.anchoredPosition = new Vector2(xMin, yMax);
        selectionBox.sizeDelta = new Vector2(xMax - xMin, yMax - yMin);
    }

    private Rect GetSelectionRect()
    {
        return new Rect(selectionBox.anchoredPosition, selectionBox.sizeDelta);
    }

    private void ApplySelectionPreview(Rect box)
    {
        foreach (var marker in controller.AllMarkers)
        {
            if (MarkerOverlaps(marker, box))
                controller.SelectMarker(marker, SelectionMode.Add);
        }
    }

    private bool MarkerOverlaps(TimelineMarker marker, Rect box)
    {
        RectTransform rt = marker.Rect;
        Rect markerRect = new Rect(rt.anchoredPosition, rt.rect.size);
        return box.Overlaps(markerRect);
    }

    private Vector2 ScreenToContentLocal(Vector2 screenPos)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            viewport,
            screenPos,
            null,
            out Vector2 localViewport
        );

        Debug.Log($"localViewport: {localViewport.x}, content.anchoredPosition.x: {content.anchoredPosition.x}");

        return new Vector2(
            localViewport.x - content.anchoredPosition.x,
            localViewport.y
        );
    }

}
