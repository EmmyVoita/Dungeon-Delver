using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class BossPromptOptionClickHandler :
    MonoBehaviour,
    IPointerClickHandler,
    IPointerEnterHandler,
    IPointerExitHandler
{
    [SerializeField]
    private JumpDirectionModeMenuOption owner;

    [SerializeField]
    private int index;

    
    [SerializeField] private TextMeshProUGUI optionText;
    [SerializeField] private Color selectedColor = Color.yellow;
    [SerializeField] private Color defaultColor = Color.white;
    [SerializeField] private float selectedScale = 1.2f;
    [SerializeField] private float transitionSpeed = 8f;


    private bool _pointerInside;

    public void OnPointerClick(
        PointerEventData eventData)
    {
        Debug.Log("OnPOinterClick!");
        if (InputModeManager.Instance.CurrentMode
            != InputModeManager.InputMode.Mouse)
            return;

        owner.SetIndex(index);
    }

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

    private void Update()
    {
        //Debug.Log($"MenuManager.Instance.ActiveMenu => {MenuManager.Instance.ActiveMenu}");
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