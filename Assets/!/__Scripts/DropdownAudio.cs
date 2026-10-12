using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections;
using UnityEngine.UI;

public class DropdownAudio : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private TMP_Dropdown dropdown;

    [Header("Audio")]
    [SerializeField] private SoundEffect openSound;
    [SerializeField] private SoundEffect hoverSound;

    private void Awake()
    {
        if (dropdown == null)
            dropdown = GetComponent<TMP_Dropdown>();

        dropdown.onValueChanged.AddListener(OnValueChanged);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        PlaySound(openSound);

        // TMP creates the dropdown options after the click,
        // so wait until the next frame.
        //StartCoroutine(SetupOptions());
    }

    /*
    private IEnumerator SetupOptions()
    {
        yield return null;

        Toggle[] options = GetComponentsInChildren<Toggle>(true);

        foreach (Toggle option in options)
        {
            // Don't add it twice.
            if (option.GetComponent<DropdownOptionAudio>() != null)
                continue;

            DropdownOptionAudio audio = option.gameObject.AddComponent<DropdownOptionAudio>();

            audio.Initialize(this);
        }
    }
    */

    private void OnValueChanged(int index)
    {
        PlaySound(AudioLibrary.Instance.Database.select);
    }

    public void PlayHoverSound()
    {
        PlaySound(hoverSound);
    }

    private void PlaySound(SoundEffect clip)
    {
        AudioHelpers.PlaySoundEffect(
            clip,
            Camera.main.transform.position
        );
    }

    private void OnDestroy()
    {
        dropdown.onValueChanged.RemoveListener(OnValueChanged);
    }
}