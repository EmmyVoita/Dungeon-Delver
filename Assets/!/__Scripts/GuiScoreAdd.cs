using TMPro;
using UnityEngine;

public class GuiScoreAdd : MonoBehaviour
{
    [Header("Set in Inspector")]
    public TextMeshProUGUI scoreText;

    public float popScale = 1.5f;
    public float popDuration = 0.2f;
    public float flashDuration = 0.2f;

    private Vector3 originalScale;
    private Coroutine popRoutine;
    private Coroutine flashRoutine;

    private Color originalColor;

    void Awake()
    {
        originalColor = scoreText.color;
        originalScale = scoreText.transform.localScale;
    }

    void OnEnable()
    {
        ComboManager.AddComboScoreDisplay += UpdateScore;
    }

    void OnDisable()
    {
        ComboManager.AddComboScoreDisplay -= UpdateScore;
    }


    void UpdateScore(int score)
    {
        scoreText.text = FormatScore(score);
        Debug.Log("Score updated");

        
        // Restart the pop animation
        if (popRoutine != null) StopCoroutine(popRoutine);
        popRoutine = StartCoroutine(PopAnimation());
    }

    public static string FormatScore(int value)
    {
        return "+ " + value.ToString();
        if (value >= 1000000)
            return (value / 1000000f).ToString("0.#") + "M";
        else if (value >= 1000)
            return (value / 1000f).ToString("0.#") + "K";
        else
            return value.ToString();
    }


    private System.Collections.IEnumerator PopAnimation()
    {
        float elapsed = 0f;

        // Scale up
        while (elapsed < popDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / popDuration;
            scoreText.transform.localScale = Vector3.Lerp(originalScale, originalScale * popScale, t);
            yield return null;
        }

        // Scale back down
        elapsed = 0f;
        while (elapsed < popDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / popDuration;
            scoreText.transform.localScale = Vector3.Lerp(originalScale * popScale, originalScale, t);
            yield return null;
        }

        scoreText.transform.localScale = originalScale;

        scoreText.text = ""; // Clear text after animation
    }
}
