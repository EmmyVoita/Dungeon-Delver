using TMPro;
using UnityEngine;

public class KeyBindLabel : MonoBehaviour
{
    [SerializeField] private InputActionType action;
    [SerializeField] private TextMeshProUGUI text;

    void Start()
    {
        text.text = InputBindingManager.Instance.GetKeyDisplayName(InputBindingManager.Instance.GetBoundKey(action));
    }
}