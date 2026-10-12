using UnityEngine;
using TMPro;
using DG.Tweening;
using System.Collections;
using UnityEngine.UI;
public class MovementTutorialUI : TutorialBase
{
    [Header("UI References")]
    public TextMeshProUGUI upText;
    public TextMeshProUGUI downText;
    public TextMeshProUGUI leftText;
    public TextMeshProUGUI rightText;
    public Image topIndicator;
    public Image bottomIndicator;
    public Image leftIndicator;
    public Image rightIndicator;
    public TextTypewriter typewriter;
    private Tween idleWobbleTween;
    public TextMeshProUGUI tutorialText;
    private Vector3 basePos;

    [Header("Visual Settings")]
    public Color baseColor = Color.white;
    public Color holdColor = Color.yellow;
    public float baseScale = 1f;
    public float maxHoldScale = 1.4f;
    public float completedScale = 1.5f;
    public float holdDuration = 1.0f;
    public float fadeOutDuration = 0.4f;

    [Header("Audio")]
    public AudioClip holdSound;
    public AudioClip completeSound;
    [SerializeField] private SoundEffect directionSound;
    [SerializeField] private float pitchStep = 0.1f;
    int keyCounter = 0;

    [Header("State")]
   

    private KeyTracker up, down, left, right;

    public override void StartTutorial()
    {
        // Use your InputBindingManager for all movement directions
        up = new KeyTracker(InputActionType.MoveUp, upText, this);
        down = new KeyTracker(InputActionType.MoveDown, downText, this);
        left = new KeyTracker(InputActionType.MoveLeft, leftText, this);
        right = new KeyTracker(InputActionType.MoveRight, rightText, this);

        basePos = tutorialText.rectTransform.anchoredPosition;

        ResetVisual(upText);
        ResetVisual(downText);
        ResetVisual(leftText);
        ResetVisual(rightText);

        StartIdleWobble();
        typewriter.StartTyping("Rotate to face each direction");

        base.StartTutorial();
    }

    void Update()
    {
        if(!_tutorialActive)
            return;

        if (TutorialComplete) 
            return;

        up.Update();
        down.Update();
        left.Update();
        right.Update();

        if (up.done && down.done && left.done && right.done)
        {
            TutorialComplete = true;
        }
    }

    public void PlayDirectionSound()
    {
        float pitch = 1.0f + keyCounter * pitchStep;
        AudioHelpers.PlaySoundEffect(directionSound, Camera.main.transform.position, pitchScalar: pitch);
        keyCounter++;
    }


    void ResetVisual(TextMeshProUGUI text)
    {
        text.color = baseColor;
        text.transform.localScale = Vector3.one * baseScale;
        text.alpha = 1f;
    }

    public void FlashIndicator(Image indicator)
    {
        indicator.gameObject.SetActive(true);
        //indicator.color = Color.yellow;
        indicator.transform.localScale = Vector3.one;

        // build pulse + fade sequence
        Sequence seq = DOTween.Sequence();
        seq.Append(indicator.transform.DOScale(1.3f, 0.25f).SetEase(Ease.OutQuad));
        seq.Append(indicator.transform.DOScale(1f, 0.25f).SetEase(Ease.InOutQuad));
        seq.Append(indicator.transform.DOScale(1.2f, 0.25f).SetEase(Ease.OutQuad));
        seq.Append(indicator.transform.DOScale(1f, 0.25f).SetEase(Ease.InOutQuad));

        seq.Join(indicator.DOFade(0f, 1.0f).SetDelay(0.25f)); // fade during pulses
        seq.OnComplete(() =>
        {
            indicator.gameObject.SetActive(false);
            //indicator.color = new Color(1, 1, 0, 1); // reset to yellow opaque
            indicator.transform.localScale = Vector3.one;
        });
    }
}
