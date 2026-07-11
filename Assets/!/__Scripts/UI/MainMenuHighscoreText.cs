using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class MainMenuHighscoreText : MonoBehaviour
{
    [SerializeField] private string prefix;
    [SerializeField] private TextMeshProUGUI highscoreText;

    private void OnEnable()
    {
        PlayMenuNavigator.OnHoverChanged += UpdateHighscoreText;
    }

    private void OnDisable()
    {
        PlayMenuNavigator.OnHoverChanged -= UpdateHighscoreText;
    }


    void UpdateHighscoreText()
    {
        // Read from ScoreManager save data and build 
        SaveData saveData = ScoreManager.Instance.SaveData;
        AbilityType targetType = PlayMenuNavigator.Instance.ActiveHover;

        if(targetType == AbilityType.ReturnToMenu || targetType == AbilityType.None)
        {
            highscoreText.text = prefix + "0";
            return;
        }

        List<RunRecord> topRuns = LeaderBoardMenuNavigator.GetTopRunsForAbility(targetType, saveData, 1);
        
        int highscore = topRuns[0].score;

        highscoreText.text = prefix + highscore.ToString("N0");
    }

    /*
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
    */
}
