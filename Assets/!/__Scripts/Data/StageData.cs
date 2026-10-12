using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class StageData
{
    [Header("Identity")]
    public string stageName;

    [Header("Level Files")]
    public List<TextAsset> normalLevelFiles;
    public TextAsset bossLevelFile;

    [Header("Stage Sequence")]
    public int levelsToPlay = 2;
    public LevelOrderMode levelOrderMode = LevelOrderMode.Random;
    [Tooltip("If true, levels can repeat if there aren't enough unique ones.")]
    public bool allowRepeats = false;

    [Header("Audio")]
    public AudioClip musicClip;
    public AudioClip bossMusicClip;
}