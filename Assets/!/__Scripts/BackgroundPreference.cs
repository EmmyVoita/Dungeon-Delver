using System;
using UnityEngine;

public static class BackgroundPreference
{
    private const string PREF_KEY = "simple_backgrounds";

    public static event Action OnBackgroundPreferenceChanged;

    public static bool UseSimpleBackground
    {
        get => PlayerPrefs.GetInt(PREF_KEY, 1) == 1;
        set
        {
            PlayerPrefs.SetInt(PREF_KEY, value ? 1 : 0);
            PlayerPrefs.Save();
            OnBackgroundPreferenceChanged?.Invoke();
        }
    }
}