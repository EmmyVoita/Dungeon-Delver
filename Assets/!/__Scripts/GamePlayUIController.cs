using UnityEngine;

public class GameplayUIController : MonoBehaviour
{
    public static GameplayUIController Instance { get; private set; }

    [SerializeField] private InputPopup inputPopup;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void ShowInputPopup(InputActionType input, bool hideOnPress, string prefixText, string suffixText, Color textColor)
    {
        inputPopup.Initialize(input, hideOnPress, prefixText, suffixText,textColor);
    }

    public void HideInputPopup()
    {
        inputPopup.Hide();
    }
}