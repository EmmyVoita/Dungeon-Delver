using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputActionTypeText : MonoBehaviour
{
    [SerializeField] private InputActionType actionType;
    [SerializeField] private TextMeshProUGUI nameText;  
    [SerializeField] private string prefix = "[";
    [SerializeField] private string suffix = "]";

    void Start()
    {
        if(!nameText)
        {
            nameText = GetComponent<TextMeshProUGUI>();
            if(!nameText)
                return;
        }
           

        Key targetKey = InputBindingManager.Instance.GetBoundKey(actionType);
        string keyDisplayName =  targetKey.ToString();//InputBindingManager.Instance.GetKeyDisplayName(targetKey);
        nameText.text = $"{prefix}{keyDisplayName}{suffix}";
    }
}