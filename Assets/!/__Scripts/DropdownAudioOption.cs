using UnityEngine;
using UnityEngine.EventSystems;

public class DropdownOptionAudio : MonoBehaviour, IPointerEnterHandler
{
    private DropdownAudio dropdownAudio;

    public void Initialize(DropdownAudio audio)
    {
        dropdownAudio = audio;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        dropdownAudio.PlayHoverSound();
    }
}