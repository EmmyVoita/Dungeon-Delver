using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class StageData
{
    public string stageName;
    public AudioClip musicClip;
    public List<TextAsset> normalLevelFiles;

    public LevelOrderMode levelOrderMode = LevelOrderMode.Random;
    

    [Header("Boss")]
    public TextAsset bossLevelFile;

    public int levelsToPlay = 2;

    [Tooltip("If true, levels can repeat if there aren't enough unique ones.")]
    public bool allowRepeats = false;
}