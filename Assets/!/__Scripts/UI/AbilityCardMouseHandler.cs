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

        AudioHelpers.PlaySoundEffect(AudioLibrary.Instance.Database.select, transform.position);

        OnAbilityCardClicked?.Invoke(cardUI.Card);
    }
}
