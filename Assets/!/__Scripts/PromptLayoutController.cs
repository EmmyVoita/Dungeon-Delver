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
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private LayoutElement promptLayout;

    [Header("Sizing")]
    [SerializeField] private float keyButtonPadding = 20f;
    [SerializeField] private float spacingBetweenElements = 10f;
    
    private void Start()
    {
        Refresh();
    }

    private void Update()
    {
        if (Application.isPlaying)
            return;

        Refresh();
    }

    public void Refresh()
    {
        if(!keyText || !descriptionText || !keyButton || !labelContainer || !promptLayout)
        {
            Debug.LogWarning("PromptLayoutController, somthing is missing!");
            return;
        }
            
        keyText.ForceMeshUpdate();
        descriptionText.ForceMeshUpdate();

        LayoutRebuilder.ForceRebuildLayoutImmediate(keyButton);

        float keyButtonWidth = keyButton.rect.width;

        //keyButtonLayout.preferredWidth = keyButtonWidth;

        labelContainer.preferredWidth = descriptionText.preferredWidth;

        float totalWidth =
            keyButtonWidth +
            spacingBetweenElements +
            descriptionText.preferredWidth;

        promptLayout.preferredWidth = totalWidth;
    }
}