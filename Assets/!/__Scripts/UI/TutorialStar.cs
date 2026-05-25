using System;
using DG.Tweening;
using UnityEngine;

public class TutorialStar : MonoBehaviour
{
    public static event Action OnStarCollected;

    [SerializeField] private SoundEffect collectSound;

    [Header("Collect Animation")]
    [SerializeField] private float growScale = 1.3f;
    [SerializeField] private float growDuration = 0.12f;
    [SerializeField] private float shrinkDuration = 0.15f;

    private bool collected = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (collected) return;

        if (!other.CompareTag("Player"))
            return;

        collected = true;

        OnStarCollected?.Invoke();

        AudioHelpers.PlaySoundEffect(collectSound, transform.position);

        transform
            .DOScale(growScale, growDuration)
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                transform
                    .DOScale(0f, shrinkDuration)
                    .SetEase(Ease.InBack)
                    .OnComplete(() =>
                    {
                        Destroy(gameObject);
                    });
            });
    }
}