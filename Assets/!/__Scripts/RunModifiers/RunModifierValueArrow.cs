using UnityEngine;
using UnityEngine.EventSystems;

public class RunModifierValueArrow : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private RunModifierOptionUI option;
    [SerializeField] private int direction;

    public void Initialize(RunModifierOptionUI option,int direction)
    {
        this.option = option;
        this.direction = direction;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        if (option == null)
            return;

        option.ChangeValueFromMouse(direction);
    }
}