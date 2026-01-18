using UnityEngine;
using UnityEngine.UI;

public class LevelEditorPlaybackUI : MonoBehaviour
{
    [Header("Buttons")]
    public Button playButton;
    public Button pauseButton;
    public Button stopButton; // optional

    [Header("References")]
    public EditorPlaybackController playback;

    void Start()
    {
        playButton.onClick.AddListener(OnPlay);
        pauseButton.onClick.AddListener(OnPause);

        if (stopButton != null)
            stopButton.onClick.AddListener(OnStop);

        UpdateButtonStates();
    }

    void Update()
    {
        UpdateButtonStates();
    }

    private void OnPlay()
    {
        playback.Play();
        UpdateButtonStates();
    }

    private void OnPause()
    {
        playback.Pause();
        UpdateButtonStates();
    }

    private void OnStop()
    {
        playback.Stop();    // resets time to 0
        UpdateButtonStates();
    }

    private void UpdateButtonStates()
    {
        bool playing = playback.isPlaying;

        playButton.gameObject.SetActive(!playing);
        pauseButton.gameObject.SetActive(playing);

        // stopButton is always shown unless you want auto-hide
    }
}
