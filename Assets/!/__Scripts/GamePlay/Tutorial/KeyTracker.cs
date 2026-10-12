using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class KeyTracker
{

    private InputActionType actionType;
    private TextMeshProUGUI text;
    private MovementTutorialUI parent;
    private float holdTime = 0f;
    public bool done = false;
    private bool fadingOut = false;
    private AudioSource holdAudio;
    private int keyCounter = 0;
    

    public KeyTracker(InputActionType actionType, TextMeshProUGUI text, MovementTutorialUI parent)
    {
        this.actionType = actionType;
        this.text = text;
        this.parent = parent;

        this.text.text = InputBindingManager.Instance.GetBoundKey(actionType).ToString();

        // Create a local looping audio source
        GameObject audioObj = new GameObject($"HoldSound_{actionType}");
        audioObj.transform.SetParent(parent.transform);
        holdAudio = audioObj.AddComponent<AudioSource>();
        holdAudio.playOnAwake = false;
        holdAudio.loop = true;
        holdAudio.clip = parent.holdSound;
        holdAudio.volume = 0.6f;
    }

    public void Update()
    {
        if (done || fadingOut) return;

        bool isPressed = InputBindingManager.Instance.GetKeyHeld(actionType);

        if (isPressed)
        {
            holdTime += Time.deltaTime;

            // Play hold audio if not already
            if (!holdAudio.isPlaying && parent.holdSound != null)
                holdAudio.Play();

            // Update scale + color
            float t = Mathf.Clamp01(holdTime / parent.holdDuration);
            float scale = Mathf.Lerp(parent.baseScale, parent.maxHoldScale, t);
            text.transform.localScale = Vector3.one * scale;
            text.color = Color.Lerp(parent.baseColor, parent.holdColor, t);

            if (holdTime >= parent.holdDuration)
            {
                Complete();
            }
                
        }
        else
        {
            if (holdTime > 0f)
            {
                holdTime = 0f;
                text.DOColor(parent.baseColor, 0.2f);
                text.transform.DOScale(parent.baseScale, 0.2f);

                if (holdAudio.isPlaying)
                    holdAudio.Stop();
            }
        }
    }

  
    private void Complete()
    {
        done = true;
        fadingOut = true;

        parent.PlayDirectionSound();

        Vector2 dir = Vector2.zero;
        Image indicator = null;

        switch (actionType)
        {
            case InputActionType.MoveUp:
                dir = Vector2.up;
                indicator = parent.topIndicator;
                break;
            case InputActionType.MoveDown:
                dir = Vector2.down;
                indicator = parent.bottomIndicator;
                break;
            case InputActionType.MoveLeft:
                dir = Vector2.left;
                indicator = parent.leftIndicator;
                break;
            case InputActionType.MoveRight:
                dir = Vector2.right;
                indicator = parent.rightIndicator;
                break;
        }

        // Visual pop & fade of key text
        Sequence seq = DOTween.Sequence();
        seq.Append(text.transform.DOScale(parent.completedScale, 0.15f).SetEase(Ease.OutBack));
        seq.Append(text.transform.DOScale(0f, parent.fadeOutDuration).SetEase(Ease.InBack));
        seq.Join(text.DOFade(0f, parent.fadeOutDuration));
        seq.OnComplete(() =>
        {
            text.gameObject.SetActive(false);
            fadingOut = false;
        });
    }
}