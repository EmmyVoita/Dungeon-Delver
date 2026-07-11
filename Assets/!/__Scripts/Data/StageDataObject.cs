using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Data / Stage Data")]
public class StageDataObject : ScriptableObject
{
    public string stageName;
    public AudioClip musicClip;

    [Header("Level Settings")]
    public int levelsToPlay = 2;
    public List<LevelDataObject> levelFiles;
    public LevelOrderMode levelOrderMode = LevelOrderMode.Random;

    [Tooltip("If true, levels can repeat if there aren't enough unique ones.")]
    public bool allowRepeats = false;


    [Header("Boss")]
    public LevelDataObject bossFile;
}