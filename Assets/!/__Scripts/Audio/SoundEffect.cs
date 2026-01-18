using UnityEngine;



[System.Serializable]
public struct SoundEffect
{
    public AudioClip clip;
    public AudioChannel channel;
    public bool affectedByTimeScale;


    [Header("Volume Settings")]
    [Range(0f, 1f)] public float volume;
   

    [Header("Pitch Settings")]
    public float pitch;
    public bool usePitchVariation;
    public float pitchVariation;

    public bool IsValid => clip != null;
}
