using UnityEngine;

public class FullscreenManager : MonoBehaviour
{
    private const string PREF_KEY = "fullscreen_mode"; // 1 = fullscreen, 0 = windowed

    void Start()
    {
        // Load fullscreen pref on startup
        int pref = PlayerPrefs.GetInt(PREF_KEY, 1); // default fullscreen
        bool fullscreen = pref == 1;
        Screen.fullScreen = fullscreen;
    }

    public void ToggleFullscreen(bool fullscreen)
    {
        Screen.fullScreen = fullscreen;

        // Save preference
        PlayerPrefs.SetInt(PREF_KEY, fullscreen ? 1 : 0);
        PlayerPrefs.Save();
    }
}
