using UnityEngine;

[CreateAssetMenu(menuName = "Run Modifiers/Modifier")]
public class RunModifierDefinition : ScriptableObject
{
    
    public string displayName;
    [TextArea]public string description;
    public Sprite icon;

    [Header("Summary UI")]
    public string summaryName;
    public Sprite summaryIcon;




    public RunModifierType type;
    public RunModifierCategory category;

    [Header("Value")]
    public float minValue = 0;
    public float maxValue = 5;
    public float step = 1;

    [Header("Score")]
    [Tooltip("Score multiplier change for each step away from zero.")]
    public float scoreMultiplierPerStep = 0.05f;
    public string valueSuffix;

    [Header("UnlockCondition")]
    public RunModifierUnlockRequirement unlockRequirement;


    public bool IsUnlocked => unlockRequirement == null || unlockRequirement.IsUnlocked();
    public string UnlockRequirementText => unlockRequirement == null ? string.Empty : unlockRequirement.GetUnlockDescriptionText();
    public string UnlockRatioText => unlockRequirement == null ? string.Empty : unlockRequirement.GetRequirementRatioText();
    

    public float ClampValue(float value)
    {
        return Mathf.Clamp(value, minValue, maxValue);
    }
}