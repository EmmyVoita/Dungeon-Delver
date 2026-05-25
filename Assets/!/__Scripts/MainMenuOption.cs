using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class MainMenuOption : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerClickHandler
{
    public static event Action<int> OnMainMenuOptionClicked;


    [SerializeField] private TextMeshProUGUI optionText;
    [SerializeField] private Color selectedColor = Color.yellow;
    [SerializeField] private Color defaultColor = Color.white;
    [SerializeField] private float selectedScale = 1.2f;
    [SerializeField] private float transitionSpeed = 8f;
    [SerializeField] private int index = 0;


    private bool _pointerInside;


    public void OnPointerEnter(PointerEventData eventData)
    {
        if(MenuManager.Instance.ActiveMenuLocked) return;

        // Ignore if keyboard mode active
        if (InputModeManager.Instance.CurrentMode
            != InputModeManager.InputMode.Mouse)
            return;

        AudioHelpers.PlaySoundEffect(AudioLibrary.Instance.Database.navigate, transform.position);

        _pointerInside = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if(MenuManager.Instance.ActiveMenuLocked) return;

        // Ignore if keyboard mode active
        if (InputModeManager.Instance.CurrentMode
            != InputModeManager.InputMode.Mouse)
            return;

        

        _pointerInside = false;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if(MenuManager.Instance.ActiveMenuLocked) return;
        
        if (InputModeManager.Instance.CurrentMode
            != InputModeManager.InputMode.Mouse)
            return;

        AudioHelpers.PlaySoundEffect(AudioLibrary.Instance.Database.select, transform.position);

        OnMainMenuOptionClicked?.Invoke(index);
    }

    private void Update()
    {
        if(MenuManager.Instance.CurrentState != MenuState.Main) return;

        if(MenuManager.Instance.ActiveMenuLocked)
        {
            _pointerInside = false;
        }
        
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