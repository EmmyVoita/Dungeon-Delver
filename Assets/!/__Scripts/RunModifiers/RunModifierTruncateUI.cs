using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RunModifierTruncateUI : MonoBehaviour
{
    [SerializeField] private string suffix = "MORE";
    [SerializeField] private TextMeshProUGUI nameText;

    public void SetValue(int value)
    {
        nameText.text = $"+{value} {suffix}";
    }
}