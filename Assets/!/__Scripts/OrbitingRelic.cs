using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbitingRelic : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float invincibleDuration = 1f;

    [Header("Feedback")]
    [SerializeField] private GameObject breakEffectPrefab;
    [SerializeField] private SoundEffect breakSound;

    [Header("Visuals")]
    [SerializeField] private List<Sprite> sprites;
    [SerializeField] private SpriteRenderer sRend;

    private OrbitingTreasure _owner;

    private int _currencyReward;
    private int _absorbCount;
    private int _absorbedCount;

    private bool _destroyed;

    private float _invincibilityRemaining;
    private bool _isInvincible;

    public int SlotIndex { get; private set; } = -1;

    public void Initialize(
        OrbitingTreasure owner,
        int currencyReward,
        int slotIndex,
        int absorbAmount = 1)
    {
        _owner = owner;
        _currencyReward = Mathf.Max(0, currencyReward);
        _absorbCount = Mathf.Max(1, absorbAmount);
        _absorbedCount = 0;
        _destroyed = false;

        SlotIndex = slotIndex;

        if (sRend != null &&
            sprites != null &&
            sprites.Count > 0)
        {
            sRend.sprite = sprites.GetRandom();
        }

        _invincibilityRemaining =
            Mathf.Max(0f, invincibleDuration);

        _isInvincible =
            _invincibilityRemaining > 0f;
    }

    public void SetOrbitPosition(Vector2 position)
    {
        transform.position = position;
    }

    private void Update()
    {
        if (!_isInvincible)
            return;

        _invincibilityRemaining -= Time.deltaTime;

        if (_invincibilityRemaining > 0f)
            return;

        _invincibilityRemaining = 0f;
        _isInvincible = false;
    }

    public void BlockArrow()
    {
        if (_destroyed)
            return;

        _destroyed = true;

        if (_currencyReward > 0 &&
            CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.AddCurrency(
                _currencyReward
            );
        }

        if (breakEffectPrefab != null)
        {
            Instantiate(
                breakEffectPrefab,
                transform.position,
                Quaternion.identity
            );
        }

        AudioHelpers.PlaySoundEffect(
            breakSound,
            transform.position
        );

        _owner?.NotifyRelicDestroyed(this);

        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_isInvincible || _destroyed)
            return;

        ArrowBase arrow =
            other.GetComponent<ArrowBase>();

        if (arrow == null || arrow.Invincible)
            return;

        _absorbedCount++;

        arrow.KillArrow(
            Goal.GoalType.Critical,
            true
        );

        StartCoroutine(BlockSequence());
    }

    private IEnumerator BlockSequence()
    {
        yield return null;

        if (_absorbedCount >= _absorbCount)
            BlockArrow();
    }
}