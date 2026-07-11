using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;

[System.Serializable]
public struct InitalizeMaterialData
{
    public Material mat;
    public string propertyName;
    public float propertyValue;
}

public class SceneInitializer : MonoBehaviour
{
    [Header("Optional")]
    [SerializeField] private List<InitalizeMaterialData> matData;

    private void Awake()
    {
        ResetScreenEffects();
        ReleaseInputFocus();
    }

    private void ResetScreenEffects()
    {
        foreach(InitalizeMaterialData data in matData)
        {
            data.mat.SetFloat(data.propertyName,data.propertyValue);
        }
    }

    private void ReleaseInputFocus()
    {
        InputFocusManager.ClearOwner();
    }
}