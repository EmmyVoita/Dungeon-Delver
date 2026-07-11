using System;
using UnityEngine;

public abstract class PracticeMenuOption : MonoBehaviour
{
    public static Action<Vector2> OnNavigateToOption;



    public bool IsCurrent()
    {
        return PracticeMenuNavigator.Instance.CurrentOption == this;
    }


    public virtual void OnEnter()
    {
        gameObject.SetActive(true);
    }

    public virtual void OnConfirm() { }

    public abstract void HandleDirectionalInput(Vector2 input);

    public virtual void OnExit()
    {
        //gameObject.SetActive(false);
    }
}
