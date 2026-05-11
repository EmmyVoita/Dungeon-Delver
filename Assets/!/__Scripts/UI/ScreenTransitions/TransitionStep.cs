using DG.Tweening;
using UnityEngine;

[System.Serializable]
public class TransitionStep
{
    public Material material;
    public string transitionProperty;
    public float startValue;
    public float targetValue;
    public float duration = 0.4f;
    public Ease ease = Ease.OutCubic;
    public float holdTime = 0f; // optional

    public bool triggerStateSwitch; // 👈 ADD
}