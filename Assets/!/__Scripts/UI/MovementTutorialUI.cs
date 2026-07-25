using UnityEngine;
using TMPro;
using DG.Tweening;
using System.Collections;
using UnityEngine.UI;
public class MovementTutorialUI : MonoBehaviour
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
       int count = 0;

    [Header("State")]
    public bool TutorialComplete { get; private set; } = false;

    private KeyTracker up, down, left, right;

    void Start()
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
    }

    void Update()
    {
        if (TutorialComplete) return;

        up.Update();
        down.Update();
        left.Update();
        right.Update();

        if (up.done && down.done && left.done && right.done)
        {
            TutorialComplete = true;
            Debug.Log("✅ Tutorial complete! All directions held.");
        }
    }

    private void PlayDirectionSound()
    {
        if (completeSound == null) return;
        
        count++;
        float pitch = Mathf.Lerp(1f, 1.3f, (count - 1) / 3f);
        AudioHelpers.PlayMyClipAtPoint(completeSound, AudioChannel.SFX, Camera.main.transform.position, 1f, pitch: pitch);
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

    private void StartIdleWobble()
    {
        idleWobbleTween?.Kill();
        idleWobbleTween = tutorialText.rectTransform
            .DOAnchorPosY(basePos.y + 10f, 1.2f)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }

    // -------------------------------------------------------------------
    // 🔹 Nested tracker class — now works with InputBindingManager
    // -------------------------------------------------------------------
    private class KeyTracker
    {
        private InputActionType actionType;
        private TextMeshProUGUI text;
        private MovementTutorialUI parent;
        private float holdTime = 0f;
        public bool done = false;
        private bool fadingOut = false;
        private AudioSource holdAudio;
     

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

            // 🌟 Get direction + matching indicator
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

            // 🔸 Flash indicator
            //if (indicator != null)
            //    parent.FlashIndicator(indicator);

            // 🔹 Spawn tutorial arrows
            //float tutorialSpeed = 4.0f;
            //int prefabIndex = 0;
            //Color tutorialColor = Color.white;
            //parent.StartCoroutine(SpawnTutorialArrows(dir, tutorialSpeed, prefabIndex, tutorialColor));

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




        // 🕓 coroutine to pace tutorial arrows slightly apart
        private IEnumerator SpawnTutorialArrows(Vector2 dir, float speed, int prefabIndex, Color color)
        {
            //ArrowSpawner.Instance.SpawnArrow(dir, speed, prefabIndex, color);
            yield return new WaitForSeconds(0.5f); // short gap between spawns
            //ArrowSpawner.Instance.SpawnArrow(dir, speed, prefabIndex, color);
        }
    }
}
