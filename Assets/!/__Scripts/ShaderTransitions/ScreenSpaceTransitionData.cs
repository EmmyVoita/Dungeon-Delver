using UnityEngine;

[System.Serializable]
public struct ScreenSpaceTransitionData
{
    [Header("In Transition")]
    public Material inMaterial;
    public string transitionInPropertyName;
    public float inStartValue;
    public float inTargetValue;
    public float transitionInDuration;
    
    [Header("Out Transition")]
    public Material outMaterial;
    public string transitionOutPropertyName;
    public float outTargetValue;
    public float outStartValue;
    public float transitionOutDuration;

    [Header("Timing")]
    public float outStartDelay;

}