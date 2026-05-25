using UnityEngine;

[CreateAssetMenu(menuName = "Audio/Audio Database")]
public class AudioDatabase : ScriptableObject
{
    [Header("UI")]
    public SoundEffect navigate;
    public SoundEffect select;
    public SoundEffect back;
    public SoundEffect negative;
    public SoundEffect typewriterBlip;

    [Header("Gameplay")]
    public SoundEffect arrowHit;
    public SoundEffect explosion;
    public SoundEffect unlock;

    [Header("Ambience")]
    public SoundEffect ambienceLoop;

    [Header("Tallying")]
    public SoundEffect tallyBase;
    public SoundEffect tallyAccent;
}