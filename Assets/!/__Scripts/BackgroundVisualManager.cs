using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using DG.Tweening;
using UnityEngine;

public class BackgroundVisualManager : MonoBehaviour
{
    public static event Action OnFlareBottomRequested;

    [SerializeField] private List<StageDataObject> stages;

    [Header("Textures")]
    [SerializeField] private MainBackgroundData defaultBackgroundData;

    [Header("Colors")]
    public Color defaultColor;
    public Color worldMapColor;
    public Color gameOverColor;

    [Header("Material References")]
    public Material mat;
    public string mainTexturePropertyName = "_MainTexture";
    public string secondaryTexturePropertyName = "_SecondaryTexture";

    public string mainTilingPropertyName = "_Tiling";
    public string secondaryTilingPropertyName = "_SecondaryTiling";

    public string mainSpeedPropertyName = "_Speed";
    public string secondarySpeedPropertyName = "_SecondarySpeed";

    public string mainDirectionPropertyName = "_Direction";
    public string secondaryDirectionPropertyName = "_SecondaryDirection";



    public string colorPropertyName = "_Color";
    public string bottomGradientName = "_GradientEdge2";

    private bool _useSimpleBackground;

    

    private void OnEnable()
    {
        BackgroundPreference.OnBackgroundPreferenceChanged += HandlePreferenceChanged;
    }

    private void OnDisable()
    {
        BackgroundPreference.OnBackgroundPreferenceChanged -= HandlePreferenceChanged;
    }

    private void HandlePreferenceChanged()
    {
        _useSimpleBackground = BackgroundPreference.UseSimpleBackground;
        if(_useSimpleBackground)
            ChangeDefaultBackground(defaultBackgroundData);
        else
            PickRandomBackground();
    }

    public void PickRandomBackground()
    {
        StageDataObject stageData = stages.GetRandom();
        ChangeDefaultBackground(stageData.backgroundData);
    }

    public void SetDefaultBackground()
    {
        ChangeDefaultBackground(defaultBackgroundData);
    }


    public void ChangeDefaultBackground(MainBackgroundData backgroundData)
    {
        mat.SetTexture(mainTexturePropertyName, backgroundData.mainBackground);
        mat.SetTexture(secondaryTexturePropertyName, backgroundData.secondaryBackground);

        mat.SetVector(mainTilingPropertyName, backgroundData.mainTiling);
        mat.SetVector(secondaryTilingPropertyName, backgroundData.secondaryTiling);

        mat.SetFloat(mainSpeedPropertyName, backgroundData.mainSpeed);
        mat.SetFloat(secondarySpeedPropertyName, backgroundData.secondarySpeed);

        mat.SetVector(mainDirectionPropertyName, backgroundData.mainDirection);
        mat.SetVector(secondaryDirectionPropertyName, backgroundData.secondaryDirection);
    }

    private void SetTexture(Texture2D texture)
    {
        mat.SetTexture(mainTexturePropertyName, texture);
    }

    private void SetColor(Color color)
    {
        mat.SetColor(colorPropertyName, color);
    }
}
