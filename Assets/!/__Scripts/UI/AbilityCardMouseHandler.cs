using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class AbilityCardMouseHandler : MonoBehaviour, IPointerClickHandler
{
    public AbilityCardUI cardUI;
    public static event Action<AbilityData> OnAbilityCardClicked;
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("OnPointerClick");
        
        if (InputModeManager.Instance.CurrentMode
            != InputModeManager.InputMode.Mouse)
            return;

      

        OnAbilityCardClicked?.Invoke(cardUI.Card);
    }
}
