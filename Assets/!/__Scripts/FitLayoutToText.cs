using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FitLayoutToText : MonoBehaviour
{
    [SerializeField] private TMP_Text text;
    [SerializeField] private LayoutElement layoutElement;
    [SerializeField] private float extraWidth = 24f;

    private void LateUpdate()
    {
        text.ForceMeshUpdate();
        layoutElement.preferredWidth = text.preferredWidth + extraWidth;
    }
}