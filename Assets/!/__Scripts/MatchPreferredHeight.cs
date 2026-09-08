using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
[RequireComponent(typeof(LayoutElement))]
public class MatchPreferredHeight : MonoBehaviour
{
    [SerializeField] private RectTransform target;
    [SerializeField] private float additionalHeight = 0f;

    private LayoutElement _layoutElement;
    

    private void Awake()
    {
        _layoutElement = GetComponent<LayoutElement>();
    }

    private void OnEnable()
    {
        _layoutElement = GetComponent<LayoutElement>();
        UpdateWidth();
    }

    private void LateUpdate()
    {
        UpdateWidth();
    }

    private void UpdateWidth()
    {
        if (target == null || _layoutElement == null)
            return;

        float targetPreferredHeight = LayoutUtility.GetPreferredHeight(target);

        _layoutElement.preferredHeight = targetPreferredHeight + additionalHeight;

        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform);
    }
}