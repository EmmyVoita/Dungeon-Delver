using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Data / Stage Data")]
public class StageDataObject : ScriptableObject
{
    [Header("Identity")]
    public string stageName;
    
    [Header("Visuals")]
    public MainBackgroundData backgroundData;

    [Header("Level Files")]
    public List<LevelDataObject> levelFiles;
    public LevelDataObject bossFile;

    [Header("Stage Sequence")]
    public int levelsToPlay = 2;
    [Tooltip("If true, levels can repeat if there aren't enough unique ones.")]
    public bool allowRepeats = false;

    [Header("Audio")]
    public AudioClip musicClip;
    public AudioClip bossMusicClip;
}