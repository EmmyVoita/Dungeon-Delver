using System;
using UnityEngine;
using DG.Tweening;

public class ConveyorItem : MonoBehaviour
{
    public static Action<string, bool> onItemCollected;
    public float fadeOutTime = 0.3f;

    [Header("Runtime")]
    public string itemID;
    public bool isCorrectItem;
    public float speed;
    public Vector2 moveDirection;

    private SpriteRenderer sr;
    private ConveyorBelt parentBelt;

    [Header("Audio / VFX")]
    public AudioClip incorrectCollectSound;
    public ParticleSystem destroyEffect;

    private float destroyDistance;
    private Vector3 spawnPos;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    // ============================================================
    //  Initialization from ConveyorBelt
    // ============================================================
    public void Init(
        string id,
        Sprite sprite,
        bool correct,
        Vector2 direction,
        float moveSpeed,
        ConveyorBelt belt,
        float maxDistance
    )
    {
        itemID = id;
        isCorrectItem = correct;
        moveDirection = direction;
        speed = moveSpeed;

        parentBelt = belt;
        destroyDistance = maxDistance;

        spawnPos = transform.position;

        if (sr != null)
            sr.sprite = sprite;

        // Cute spawn pop
        transform.localScale = Vector3.zero;
        transform.DOScale(Vector3.one, 0.15f).SetEase(Ease.OutBack);
    }

    // ============================================================
    //  Movement
    // ============================================================
    void Update()
    {
        // Optional: elastic spacing check
        int index = parentBelt.activeItems.IndexOf(this);
        if (index > 0)
        {
            ConveyorItem prev = parentBelt.activeItems[index - 1];
            float dist = Vector3.Distance(prev.transform.position, transform.position);

            // If too close, temporarily pause movement
            if (dist < parentBelt.spacingDistance * 0.85f)
                return;
        }

        // Move normally
        transform.position += (Vector3)(moveDirection * speed * Time.deltaTime);

        // Kill when far enough
        if (Vector3.Distance(transform.position, spawnPos) >= destroyDistance)
            DestroySelf();
    }

    // ============================================================
    //  Slow stop when correct item collected
    // ============================================================
    public void SlowToStop(float duration)
    {
        DOTween.To(() => speed, x => speed = x, 0f, duration)
               .SetEase(Ease.OutQuad);
    }

    public void Disable()
    {
        var collider = GetComponent<Collider2D>();
        if (collider != null)
            collider.enabled = false;
    }

    // ============================================================
    //  Destroy with fade
    // ============================================================
    void DestroySelf()
    {
        parentBelt?.DeregisterItem(this);

        if (sr != null)
        {
            sr.DOKill();
            sr.DOColor(new Color(1, 1, 1, 0f), fadeOutTime)
                .OnComplete(() => Destroy(gameObject));
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ============================================================
    //  Picking up items
    // ============================================================
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        // Notify challenge
        onItemCollected?.Invoke(itemID, isCorrectItem);

        // If correct: stop the belt
        if (isCorrectItem)
            parentBelt.SlowStop();

        if(!isCorrectItem)
        {
             // Play sound
            AudioHelpers.PlayMyClipAtPoint(
                incorrectCollectSound,
                AudioChannel.SFX, 
                Camera.main.transform.position
            );
        }
       

        // VFX
        if (destroyEffect != null)
            Instantiate(destroyEffect, transform.position, Quaternion.identity);

        DestroySelf();
    }

    // ============================================================
    //  Forced cleanup (for end of challenge)
    // ============================================================
    public void ForceDestroy()
    {
        parentBelt?.DeregisterItem(this);

        if (destroyEffect != null)
            Instantiate(destroyEffect, transform.position, Quaternion.identity);

        if (sr != null)
        {
            sr.DOKill();
            sr.DOColor(new Color(1, 1, 1, 0f), fadeOutTime)
                .OnComplete(() => Destroy(gameObject));
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
