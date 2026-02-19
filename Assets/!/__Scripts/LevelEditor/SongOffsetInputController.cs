using UnityEngine;
using TMPro;
using System.Globalization;

public class SongOffsetInputController : MonoBehaviour
{
    [SerializeField] private TMP_InputField offsetInput;

    private void Start()
    {
        // Initialize from data
        offsetInput.SetTextWithoutNotify(
            LevelEditorData.Instance.SongOffsetSeconds.ToString("F3")
        );
    }


    public void OnOffsetChanged(string _)
    {
        string text = offsetInput.text;

        if (string.IsNullOrWhiteSpace(text))
        {
            Debug.LogError("Offset input cannot be empty!");
            return;
        }

        if (!float.TryParse(
                text,
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out float offset))
        {
            Debug.LogError($"Invalid Offset input: '{text}'");
            return;
        }

        LevelEditorData.Instance.SetSongOffset(offset);

        UIToast.Show($"Song Offset set to: {offset:F3}s");

        EditorPlaybackController.Instance.SyncMusicToTime();
    }
}
