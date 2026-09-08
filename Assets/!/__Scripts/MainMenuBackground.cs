using System.Collections.Generic;
using UnityEngine;

public class MainMenuBackground : MonoBehaviour
{
    [SerializeField] private BackgroundVisualManager backgroundManager;

    private void Start()
    {
        if(!BackgroundPreference.UseSimpleBackground)
            backgroundManager.PickRandomBackground();
        else
            backgroundManager.SetDefaultBackground();
    }
}