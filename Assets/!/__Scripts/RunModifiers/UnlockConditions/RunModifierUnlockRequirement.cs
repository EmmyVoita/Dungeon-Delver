using UnityEngine;

public abstract class RunModifierUnlockRequirement : ScriptableObject
{

    public abstract bool IsUnlocked();
    public abstract string GetRequirementRatioText();
    public abstract string GetUnlockDescriptionText();
}