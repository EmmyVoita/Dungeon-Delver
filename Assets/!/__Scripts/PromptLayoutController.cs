using TMPro;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class PromptLayoutController : MonoBehaviour
{
    [Header("Key Button")]
    [SerializeField] private TMP_Text keyText;
    [SerializeField] private RectTransform keyButton;

    [Header("Prompt Container")]
    [SerializeField] private LayoutElement labelContainer;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private LayoutElement promptLayout;

    [Header("Sizing")]
    [SerializeField] private float keyButtonPadding = 20f;
    [SerializeField] private float spacingBetweenElements = 10f;


    private bool _initialized = false;
    private KeyPrompt _keyPrompt;
    

    public void Initialize(KeyPrompt keyPrompt)
    {
        _keyPrompt = keyPrompt;
        _initialized = true;
        keyText.text = InputBindingManager.Instance.GetKeyDisplayName(InputBindingManager.Instance.GetBoundKey(keyPrompt.input));
        
        Refresh();
    }

    private void Update()
    {
        if (!_initialized)
            return;

        Refresh();
    }

    public void Refresh()
    {
        if(!keyText || !nameText || !keyButton || !labelContainer || !promptLayout)
        {
            Debug.LogWarning("PromptLayoutController, something is missing!");
            return;
        }

        RefreshText();
            
        keyText.ForceMeshUpdate();
        nameText.ForceMeshUpdate();

        LayoutRebuilder.ForceRebuildLayoutImmediate(keyButton);

        float keyButtonWidth = keyButton.rect.width;


        // So the goal here is to set the width of the containers to the size of 
        // their text elements + some padding
        // To do this we get the preferredWidth of the text element
        // and we use a layout element on the container and set it to that 
        labelContainer.preferredWidth = nameText.preferredWidth;

        float totalWidth =
            keyButtonWidth +
            spacingBetweenElements +
            nameText.preferredWidth;

        promptLayout.preferredWidth = totalWidth;
    }

    private void RefreshText()
    {
        string displayName = _keyPrompt.name;

        switch (_keyPrompt.valueType)
        {
            case KeyPromptValueType.Rerolls:
                displayName += $": {RunStateManager.Instance.ShopRerollsRemaining}";
                break;
            case KeyPromptValueType.None:
                break;
        }

        nameText.text = displayName;
    }
}