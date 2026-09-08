using UnityEngine;

[System.Serializable]
public class RunModifierSelection
{
    public RunModifierDefinition definition;
    public float value;

    public int StepCount
    {
        get
        {
            if (definition == null || definition.step <= 0f)
                return 0;

            return Mathf.RoundToInt((value - definition.minValue) / definition.step);
        }
    }
}