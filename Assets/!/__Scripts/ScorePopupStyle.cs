using UnityEngine;
using TMPro;
using DG.Tweening;

public enum ScorePopupKind { Default, NormalHit, CritHit, Combo, AbilityOverflow, Golden }


[CreateAssetMenu(menuName = "UI/Score Popup Style")]
public class ScorePopupStyle : ScriptableObject
{

    [Header("Text")]
    public Color color = Color.white;
    public float scale = 1f;
    public TMP_FontAsset font;
    public Material fontMaterial;

    [Header("Motion")]
    public float flyTime = 0.6f;
    public Ease moveEase = Ease.OutSine;
    public Ease fadeEase = Ease.InQuad;

    [Header("Optional Flair")]
    public bool punchScale;
    public float punchStrength = 0.15f;
}
