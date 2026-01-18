using UnityEngine;


[System.Serializable]
public struct BossEffect
{
    public BossEffectType effectType;

    [Tooltip("When the effect starts, in beats from boss start")]
    public float startBeat;

    [Tooltip("How long the effect lasts, in beats")]
    public float durationBeats;
}
