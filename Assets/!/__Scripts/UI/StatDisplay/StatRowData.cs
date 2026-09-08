using TMPro;
using UnityEngine;

[System.Serializable]
public struct StatRowData
{
    public string prefixLabel;
    public StatValueType statValueType;
    public int rowHeight;
    public int nameStatSpacing;

    [Header("Font")]
    public TMP_FontAsset font;
    public int fontSize;
    public TextAlignmentOptions alignment;
    public Color textColor;
}


