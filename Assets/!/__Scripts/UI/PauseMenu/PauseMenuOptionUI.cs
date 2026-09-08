using TMPro;
using UnityEngine;

public class PauseMenuOptionUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform rootRect;
    [SerializeField] private TextMeshProUGUI text;

    [Header("Default")]
    [SerializeField] private Color defaultColor = Color.white;

    [Header("OnSelected")]
    [SerializeField] private float selectedScale = 1.2f;
    [SerializeField] private Color selectedColor = Color.yellow;

    [Header("Animation")]
    [SerializeField] private float transitionSpeed = 8.0f;
    
    private bool _isSelected = false;
    private PauseMenuOption _menuOption;

    public PauseMenuOption MenuOption => _menuOption;

    public void Initialize(PauseMenuOption menuOption)
    {
        text.text = menuOption.ToString();
        _menuOption = menuOption;
    }

    public void SetSelected(bool selected)
    {
        _isSelected = selected;
        UpdateVisuals();
    }

    private void Update()
    {
        AnimateSelection();
    }

    private void UpdateVisuals()
    {
        float currentAlpha = text.color.a;

        Color targetColor = _isSelected ? selectedColor : defaultColor;
        targetColor.a = currentAlpha;

        text.color = targetColor;
    }

    void AnimateSelection()
    {
        float dt = Time.unscaledDeltaTime;

        float targetScale = _isSelected ? selectedScale : 1f;

        rootRect.localScale = Vector3.Lerp(
            rootRect.localScale,
            Vector3.one * targetScale,
            dt * transitionSpeed
        );
    }
}