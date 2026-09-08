using System;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class PracticeMenuOption : MonoBehaviour, 
                                         IPointerClickHandler, 
                                         IPointerExitHandler
{
    public static event Action<PracticeMenuOption> OnPracticeOptionClick;

    public static Action<Vector2> OnNavigateToOption;

    public bool IsCurrent()
    {
        return PracticeMenuNavigator.Instance.CurrentOption == this;
    }

    public virtual void OnPointerClick(PointerEventData eventData)
    {
        if (InputModeManager.Instance.CurrentMode
            != InputModeManager.InputMode.Mouse)
            return;

        OnPracticeOptionClick?.Invoke(this);
    }

    public virtual void OnPointerExit(PointerEventData eventData)
    {
        
    }



    public virtual void OnEnter() {}
    public virtual void OnExit() {}
    public virtual void OnConfirm() {}
    public abstract void HandleDirectionalInput(Vector2 input);
}
