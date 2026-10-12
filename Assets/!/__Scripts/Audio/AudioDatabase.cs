using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Audio/Audio Database")]
public class AudioDatabase : ScriptableObject
{
    [Header("UI")]
    public SoundEffect navigateMajor;
    public SoundEffect navigateMinor;
    public SoundEffect select;
    public SoundEffect back;
    public SoundEffect negative;
    public SoundEffect purchase;
    public SoundEffect typewriterBlip;

    [Header("Gameplay")]
    public SoundEffect arrowHit;
    public SoundEffect explosion;
    public SoundEffect unlock;
    public SoundEffect rerollGain;

    [Header("Ambience")]
    public SoundEffect ambienceLoop;

    [Header("Tallying")]
    public SoundEffect tallyBase;
    public SoundEffect tallyAccent;

    public List<ArrowHitSoundData> arrowSounds;
}