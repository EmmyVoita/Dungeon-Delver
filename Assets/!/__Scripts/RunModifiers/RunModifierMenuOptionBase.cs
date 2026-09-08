using System;
using UnityEngine;

public abstract class RunModifierMenuOptionBase : MonoBehaviour
{
    public static Action<Vector2> OnNavigateToOption;

    public bool IsCurrent()
    {
        return RunModifierMenuNavigator.Instance.CurrentOption == this;
    }

    public virtual void OnEnter() {}
    public virtual void OnExit() {}
    public virtual void OnConfirm() {}
    public abstract void HandleDirectionalInput(Vector2 input);
}
