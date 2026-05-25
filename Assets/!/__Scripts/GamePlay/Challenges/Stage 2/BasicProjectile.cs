using UnityEngine;
using DG.Tweening;
using System;

[RequireComponent(typeof(Collider2D))]
public class BasicProjectile : MonoBehaviour, IProjectile
{
    public static event Action OnProjectileHit;

    private enum DestroyType
    {
        FadeOut,
        Instant
    }


    [Header("References")]
    [SerializeField] private SpriteRenderer sRend;
    [SerializeField] private GameObject destroyEffectPrefab;
    
    [Header("Death Settings")]
    [SerializeField] private DestroyType destroyType = DestroyType.Instant;
    [SerializeField] private bool destroyOnHit = true;    
    [SerializeField] private float fadeOutDuration = 0.2f;      // Quick fade time

    [Header("Audio")]
    [SerializeField] private SoundEffect hitSound;          
    [SerializeField] private SoundEffect destroySound; 
    
    
    private Collider2D _col;
    private bool _hasBeenHit = false;



    void Awake()
    {
        _col = GetComponent<Collider2D>();
        _hasBeenHit = false;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
       // Prevent multiple triggers & only care about hitting the player
        if (!other.CompareTag("Player") || _hasBeenHit) return;

        OnProjectileHit?.Invoke();

        AudioHelpers.PlaySoundEffect(hitSound, transform.position);

        // Destroy after fade & particle delay
        if(destroyOnHit)
            DestroyProjectile();
    }

    public void DestroyProjectile(bool silent = false)
    {
        // Prevent multiple triggers
        if (_hasBeenHit) return; 

        _hasBeenHit = true;

        // Disable Collider Immediately
        _col.enabled = false;

        if(silent)
        {
            Destroy(gameObject);
            return;
        }

        if(destroyType == DestroyType.Instant)
        {
            AudioHelpers.PlaySoundEffect(destroySound,transform.position);

            if(destroyEffectPrefab != null)
            {
                Instantiate(destroyEffectPrefab, transform.position, transform.rotation);
            }

            Destroy(gameObject);
        }
        else
        {
            // Fade transparency using DOTween
            sRend?.DOFade(0f, fadeOutDuration)
            .SetEase(Ease.OutQuad)
            .SetLink(gameObject)
            .OnComplete(() =>
            {
                AudioHelpers.PlaySoundEffect(destroySound,transform.position);

                if(destroyEffectPrefab != null)
                {
                    Instantiate(destroyEffectPrefab, transform.position, transform.rotation);
                }

                // Destroy after fade
                Destroy(gameObject);
            });
        }
    }

    /*
    public void FadeOut()
    {
        // Prevent multiple triggers
        if (_hasBeenHit) return; 

        _hasBeenHit = true;

        // Disable Collider Immediately
        _col.enabled = false;

        if(destroyEffectPrefab != null)
        {
            Instantiate(destroyEffectPrefab, transform.position, transform.rotation);
        }
    }
    */
}
