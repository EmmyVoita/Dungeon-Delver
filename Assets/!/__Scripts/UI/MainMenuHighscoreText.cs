using TMPro;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class MainMenuHighscoreText : MonoBehaviour
{
    [SerializeField] private string prefix;
    [SerializeField] private TextMeshProUGUI highscoreText;


    void UpdateHighscoreText(int newHighscore)
    {
        int highscore = ScoreManager.Instance.HighScore;
        highscoreText.text = prefix + highscore.ToString("N0");
    }


    void Start()
    {
        if (ScoreManager.Instance != null)
        {
            UpdateHighscoreText(ScoreManager.Instance.HighScore);
        }
        else
        {
            highscoreText.text = "Highscore: N/A";
            Debug.LogWarning("⚠️ ScoreManager instance not found!");
        }
    }
    
    void Update()
    {
        if (ScoreManager.Instance != null)
        {
            UpdateHighscoreText(ScoreManager.Instance.HighScore);
        }
    }
}
