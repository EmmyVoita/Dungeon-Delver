using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class PauseMenuOptionClickHandler :
    MonoBehaviour,
    IPointerClickHandler,
    IPointerEnterHandler,
    IPointerExitHandler
{
   
    [SerializeField] private TextMeshProUGUI optionText;
    [SerializeField] private Color selectedColor = Color.yellow;
    [SerializeField] private Color defaultColor = Color.white;
    [SerializeField] private float selectedScale = 1.2f;
    [SerializeField] private float transitionSpeed = 8f;


     private PauseMenuNavigator _owner;
    private bool _pointerInside;
    private int _index;

    public void Initialize(PauseMenuNavigator owner, int index)
    {
        _owner = owner;
        _index = index;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (InputModeManager.Instance.CurrentMode
            != InputModeManager.InputMode.Mouse)
            return;

        _owner.SetIndex(_index);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Ignore if keyboard mode active
        if (InputModeManager.Instance.CurrentMode
            != InputModeManager.InputMode.Mouse)
            return;

        AudioHelpers.PlaySoundEffect(AudioLibrary.Instance.Database.navigateMajor, transform.position);

        _pointerInside = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Ignore if keyboard mode active
        if (InputModeManager.Instance.CurrentMode
            != InputModeManager.InputMode.Mouse)
            return;

        _pointerInside = false;
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
            Time.unscaledDeltaTime * transitionSpeed
        );
    }
}