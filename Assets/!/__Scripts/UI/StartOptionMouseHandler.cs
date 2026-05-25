using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class StartOptionMouseHandler : MonoBehaviour, 
                                        IPointerClickHandler, 
                                        IPointerEnterHandler,
                                        IPointerExitHandler
{
    public AbilityCardUI cardUI;
    public int index;
    public static event Action<int> OnStartOptionClicked;

    [SerializeField] private TextMeshProUGUI optionText;
    [SerializeField] private Color selectedColor = Color.yellow;
    [SerializeField] private Color defaultColor = Color.white;
    [SerializeField] private float selectedScale = 1.2f;
    [SerializeField] private float transitionSpeed = 8f;

    private bool _pointerInside;

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("OnPointerEnter");

        // Ignore if keyboard mode active
        if (InputModeManager.Instance.CurrentMode
            != InputModeManager.InputMode.Mouse)
            return;

        AudioHelpers.PlaySoundEffect(AudioLibrary.Instance.Database.navigate, transform.position);

        _pointerInside = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log("OnPointerExit");

        // Ignore if keyboard mode active
        if (InputModeManager.Instance.CurrentMode
            != InputModeManager.InputMode.Mouse)
            return;

        

        _pointerInside = false;
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("OnPointerClick");
        
        if (InputModeManager.Instance.CurrentMode
            != InputModeManager.InputMode.Mouse)
            return;

        AudioHelpers.PlaySoundEffect(AudioLibrary.Instance.Database.select, transform.position);

        OnStartOptionClicked?.Invoke(index);
    }

    private void Update()
    {
        if (InputModeManager.Instance.CurrentMode
            != InputModeManager.InputMode.Mouse)
            return;

        UpdateVisuals();
        AnimateSelection();
    }


    void UpdateVisuals()
    {
        optionText.color = _pointerInside ? selectedColor : defaultColor;
    }

    void AnimateSelection()
    {
        float targetScale = _pointerInside ? selectedScale : 1f;
        optionText.transform.localScale = Vector3.Lerp(
            optionText.transform.localScale,
            Vector3.one * targetScale,
            Time.deltaTime * transitionSpeed
        );
    }
}
