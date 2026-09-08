using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
[RequireComponent(typeof(LayoutElement))]
public class MatchPreferredWidth : MonoBehaviour
{
    [SerializeField] private RectTransform target;
    [SerializeField] private float additionalWidth = 0f;

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

        float targetPreferredWidth = LayoutUtility.GetPreferredWidth(target);

        _layoutElement.preferredWidth = targetPreferredWidth + additionalWidth;

        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform);
    }
}