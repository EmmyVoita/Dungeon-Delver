using UnityEngine;

public class FullscreenManager : MonoBehaviour
{
    public bool IsFullscreen => Screen.fullScreen;
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
        if (fullscreen)
        {
            Resolution res = Screen.currentResolution;

            Screen.SetResolution(
                res.width,
                res.height,
                true
            );
        }
        else
        {
            Screen.SetResolution(1280, 720, false); // your default windowed size
        }

        PlayerPrefs.SetInt(PREF_KEY, fullscreen ? 1 : 0);
        PlayerPrefs.Save();
    }
}
