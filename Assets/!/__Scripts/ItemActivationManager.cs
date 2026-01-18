using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine.UI;

public class ItemActivationManager : MonoBehaviour
{
    public static ItemActivationManager Instance { get; private set; }

    [Header("UI References")]
    public Image itemDisplayImage;
    public ParticleSystem burstParticles;

    [Header("Animation Settings")]
    public float showDuration = 1.2f;
    public float fadeDuration = 0.3f;
    public float popScale = 1.3f;
    public float tiltAngle = 15f;
    public float tiltSpeed = 8f;

    // 🔹 Queue system
    private readonly Queue<(Sprite, AudioClip)> activationQueue = new Queue<(Sprite, AudioClip)>();
    private bool isPlayingQueue = false;

    public static event System.Action OnAllItemActivationsComplete;

    private void Awake()
    {
        Instance = this;
        if (itemDisplayImage != null)
            itemDisplayImage.gameObject.SetActive(false);
    }

    public bool IsActive => isPlayingQueue || activationQueue.Count > 0;

    // 🎯 Public entry point — add a new activation to the queue
    public void EnqueueItemActivation(Sprite itemSprite, AudioClip activationSound = null)
    {
        activationQueue.Enqueue((itemSprite, activationSound));

        // Start the queue processor if it isn't already running
        if (!isPlayingQueue)
            StartCoroutine(ProcessQueue());
    }

    private IEnumerator ProcessQueue()
    {
        isPlayingQueue = true;

        while (activationQueue.Count > 0)
        {
            (Sprite sprite, AudioClip sound) = activationQueue.Dequeue();
            yield return StartCoroutine(PlayItemActivationInternal(sprite, sound));
        }

        // Queue is empty
        isPlayingQueue = false;
        OnAllItemActivationsComplete?.Invoke();
    }

    // 🧩 Internal single activation handler
    private IEnumerator PlayItemActivationInternal(Sprite itemSprite, AudioClip activationSound)
    {
        if (itemDisplayImage == null)
            yield break;

        itemDisplayImage.sprite = itemSprite;
        itemDisplayImage.color = new Color(1, 1, 1, 0);
        itemDisplayImage.gameObject.SetActive(true);

        RectTransform t = itemDisplayImage.rectTransform;
        t.localScale = Vector3.zero;
        t.localRotation = Quaternion.identity;
        t.anchoredPosition = Vector2.zero;

        // --- Fade in + pop ---
        itemDisplayImage.DOFade(1f, 0.2f);
        t.DOScale(popScale, 0.3f).SetEase(Ease.OutBack);

        // --- Tilt shake ---
        Sequence tiltSeq = DOTween.Sequence();
        float halfShake = 0.5f / tiltSpeed;
        int shakeCount = Mathf.RoundToInt(showDuration * tiltSpeed);

        for (int i = 0; i < shakeCount; i++)
        {
            float angle = (i % 2 == 0) ? tiltAngle : -tiltAngle;
            tiltSeq.Append(t.DOLocalRotate(new Vector3(0, 0, angle), halfShake).SetEase(Ease.InOutSine));
        }
        tiltSeq.Append(t.DOLocalRotate(Vector3.zero, halfShake).SetEase(Ease.InOutSine));

        if (burstParticles != null)
            burstParticles.Play();
        if (activationSound != null)
            AudioHelpers.PlayMyClipAtPoint(activationSound, AudioChannel.SFX, Camera.main.transform.position);

        yield return new WaitForSeconds(showDuration);

        // --- Fade out + shrink ---
        itemDisplayImage.DOFade(0f, fadeDuration);
        t.DOScale(0f, fadeDuration).SetEase(Ease.InBack);
        yield return new WaitForSeconds(fadeDuration);

        itemDisplayImage.gameObject.SetActive(false);
    }
}
    