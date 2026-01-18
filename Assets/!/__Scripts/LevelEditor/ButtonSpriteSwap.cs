using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class ButtonSpriteSwap : MonoBehaviour
{
    [Header("References")]
    public Button button;
    public Image targetImage;
    public RectTransform textTransform;

    [Header("Sprites")]
    public Sprite pressedSprite;

    [Header("Timing")]
    public float pressDuration = 0.1f;

    [Header("Text Offset")]
    public float textPressOffsetY = -6f;

    private Sprite originalSprite;
    private Vector2 originalTextPos;
    private Coroutine routine;

    void Awake()
    {
        if (button == null)
            button = GetComponent<Button>();

        if (targetImage == null)
            targetImage = GetComponent<Image>();

        if (textTransform == null)
            textTransform = GetComponentInChildren<TMP_Text>()?.rectTransform;

        originalSprite = targetImage.sprite;
        originalTextPos = textTransform.anchoredPosition;

        button.onClick.AddListener(PlayFeedback);
    }

    void PlayFeedback()
    {
        if (routine != null)
            StopCoroutine(routine);

        routine = StartCoroutine(FeedbackRoutine());
    }

    IEnumerator FeedbackRoutine()
    {
        // Press down
        targetImage.sprite = pressedSprite;
        textTransform.anchoredPosition = originalTextPos + new Vector2(0, textPressOffsetY);

        yield return new WaitForSeconds(pressDuration);

        // Restore
        targetImage.sprite = originalSprite;
        textTransform.anchoredPosition = originalTextPos;
    }
}
