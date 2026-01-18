using UnityEngine;
using DG.Tweening;
using System.Collections;

[RequireComponent(typeof(SpriteRenderer), typeof(BoxCollider2D))]
public class RectangleObstacle : MonoBehaviour
{
    [Header("Flash Settings")]
     [SerializeField] private float startDelay = 1.25f;
    [SerializeField] private float flashDuration = 0.1f;
    [SerializeField] private int whiteFlashCount = 2;
    [SerializeField] private float redFlashDuration = 0.15f;
    [SerializeField] private float delayBetweenFlashes = 0.1f;
    [SerializeField] private bool killOnFinish = true;

    [Header("Active Phase")]
    [SerializeField] private float activeDuration = 0.5f; // how long the collider stays active
    [SerializeField] private GameObject childRenderer; // optional child renderer to flash

    [Header("Auto Rotation")]
    [SerializeField] private bool autoRotate = false;       // 🔹 Toggle rotation on/off
    [SerializeField] private float rotationSpeed = 90f;     // 🔹 Degrees per second

    [Header("Audio")]
    public AudioClip activationSound;
    public AudioClip whiteFlashSound;
    public AudioClip redFlashSound;

    private SpriteRenderer rend;
    private BoxCollider2D col;
    private Sequence flashSequence;

    void Awake()
    {
        rend = GetComponent<SpriteRenderer>();
        col = GetComponent<BoxCollider2D>();
        col.enabled = false; // disabled by default
        rend.color = new Color(1, 1, 1, 0); // start invisible

        if (childRenderer != null)
            childRenderer.SetActive(false);

        ObstacleManager.Instance.RegisterObstacle(gameObject);

        if (autoRotate)
        {
            float randomZ = Random.Range(0f, 360f);
            transform.rotation = Quaternion.Euler(0f, 0f, randomZ);

            // 🆕 Randomly choose clockwise or counterclockwise (50/50 chance)
            rotationSpeed *= Random.value < 0.5f ? 1f : -1f;
        }

        StartCoroutine(StartFlashCoroutine());
    }

    void Update()
    {
        // 🔹 Auto-rotate smoothly each frame if enabled
        if (autoRotate)
            transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);
    }

    private IEnumerator StartFlashCoroutine()
    {
        yield return new WaitForSeconds(startDelay);
        PlayWarningFlash();
    }

    public void PlayWarningFlash()
    {
        if (flashSequence != null && flashSequence.IsActive())
            flashSequence.Kill();

        flashSequence = DOTween.Sequence();

        // 🔹 White flashes
        for (int i = 0; i < whiteFlashCount; i++)
        {
            if (i == whiteFlashCount - 1)
            {
                flashSequence.AppendCallback(() =>
                {
                    if (redFlashSound != null)
                    {
                        AudioHelpers.PlayClipWithVariation(
                            redFlashSound,
                            AudioChannel.SFX,
                            Camera.main.transform.position,
                            1.0f,
                            0.05f,
                            1.0f
                        );
                    }
                });
            }

            flashSequence.Append(rend.DOColor(Color.white, flashDuration));
            flashSequence.AppendInterval(delayBetweenFlashes);
            flashSequence.Append(rend.DOColor(new Color(1, 1, 1, 0), flashDuration));
            flashSequence.AppendInterval(delayBetweenFlashes);
        }

        // 🔹 Red flash
        flashSequence.Append(rend.DOColor(Color.red, redFlashDuration));
        flashSequence.AppendInterval(delayBetweenFlashes);
        flashSequence.Append(rend.DOColor(new Color(1, 1, 1, 0), flashDuration));
        flashSequence.AppendInterval(delayBetweenFlashes);

        // 🔹 Enable collider at the end
        flashSequence.OnComplete(() =>
        {
            StartCoroutine(EnableColliderTemporarily());
        });
    }

    private IEnumerator EnableColliderTemporarily()
    {
        col.enabled = true;

        if (childRenderer != null)
        {
            childRenderer.SetActive(true);
            var childColor = childRenderer.GetComponent<SpriteRenderer>().color;
            childColor.a = 1f;
            childRenderer.GetComponent<SpriteRenderer>().color = childColor;
        }

        if (activationSound != null)
            AudioHelpers.PlayClipWithVariation(activationSound, AudioChannel.SFX, Camera.main.transform.position, 1.0f, 0.1f, 1.0f);

        yield return new WaitForSeconds(activeDuration);
        col.enabled = false;

        if (childRenderer != null)
            childRenderer.GetComponent<SpriteRenderer>().DOFade(0f, 0.3f);

        if (killOnFinish)
        {
            ObstacleManager.Instance.UnregisterObstacle(gameObject);
            Destroy(gameObject, 0.3f);
        }
    }

    private void OnDisable()
    {
        flashSequence?.Kill();
    }
}
