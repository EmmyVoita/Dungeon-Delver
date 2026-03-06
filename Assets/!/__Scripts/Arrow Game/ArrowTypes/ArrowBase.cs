using UnityEngine;
using System;
using System.Collections;
using UnityEngine.Audio;



public struct ArrowResolvedData
{
    public Goal.GoalType goalType;
    public ArrowStatus status;
}


public abstract class ArrowBase : MonoBehaviour
{
    public static event Action<ArrowResolvedData> OnArrowResolved;

    [Header("Arrow Stats")]
    //public int scoreValue = 100;
    public float health = 1f;
    public bool invincible = false;
    public bool flip180 = false;
    public bool useVariablePitch = false;

    [Header("VFX / SFX")]
    public Sprite empowerSprite;
    public AudioClip destroySound;
    public AudioClip criticalDestroySound;
    public ParticleSystem hitEffect;
    public GameObject destroyEffect;
    public float criticalPitchIncrease = 0.2f;
    public float arrowPitchVariation = 0.0f;
    public float criticalHitPitch = 1.2f;
    public float basePitch = 1.0f;
    public float directionalPitchFactor = 1f;


    [Header("Audio EQ Settings")]
    [Range(0f, 1f)] public float directionalEQStrength = 0.5f;
    [SerializeField] private float hpfNeutral = 20f;
    [SerializeField] private float lpfNeutral = 22000f;
    [SerializeField] private float midNeutral = 0f;


    [Header("Movement")]
    protected Vector2 direction;
    protected float speed;
    protected Rigidbody2D rb;

    [Header("Flags")]
    public ArrowStatus status;
    //[SerializeField] private bool isEmpowered = false;
    //[SerializeField] private bool isFrozen = false;

    [Header("State Effects")]
    public GameObject empowerEffect;
    public AudioClip empowerSound;
    public AudioClip empowerDestroySound;

    public GameObject freezeEffect;
    public AudioClip freezeDestroySound;
    public AudioClip freezeEffectSound;

    // --- Freeze state ---
    private float freezeTimer = 0f;
    private float frozenSpeed = 0f;

    public bool IsGolden => status.HasFlag(ArrowStatus.Golden);
    public bool IsRecoveryArrow => status.HasFlag(ArrowStatus.Recovery);

    


    public void SetRecoveryArrow()
    {
        AddStatus(ArrowStatus.Recovery);

        var sRend = GetComponentInChildren<SpriteRenderer>();
        sRend.color = new Color(0.8f, 1f, 0.8f, 1f); // light green tint   
    }

    public void SetGolden()
    {
        AddStatus(ArrowStatus.Golden);

        //scoreValue = Mathf.RoundToInt(scoreValue * multiplier);
        if (empowerEffect != null)
        {
            GameObject effect = Instantiate(empowerEffect, transform.position, Quaternion.identity);
            effect.transform.SetParent(this.transform);
        }

        AudioHelpers.PlayMyClipAtPoint(empowerSound, AudioChannel.SFX, transform.position);

        var sRend = GetComponentInChildren<SpriteRenderer>();
        if (sRend != null && empowerSprite != null)
        {
            sRend.sprite = empowerSprite;
        }
    }

    public void Freeze(float duration)
    {
        if (HasStatus(ArrowStatus.Frozen)) return;

        AddStatus(ArrowStatus.Frozen);
        freezeTimer = duration;
        frozenSpeed = speed;

        // Stop movement
        rb.linearVelocity = Vector2.zero;

        AudioHelpers.PlayMyClipAtPoint(freezeEffectSound, AudioChannel.SFX, transform.position);

        /*
        // Spawn freeze VFX
        if (freezeEffect != null)
        {
            GameObject fx = Instantiate(freezeEffect, transform.position, Quaternion.identity, transform);
        }
        */

        // Tint sprite for feedback
        var rend = GetComponentInChildren<SpriteRenderer>();
        if (rend != null)
            rend.color = new Color(0.6f, 0.9f, 1f, 1f); // soft cyan tint
    }

    public void Unfreeze()
    {
        if (!HasStatus(ArrowStatus.Frozen)) return;

        RemoveStatus(ArrowStatus.Frozen);
        rb.linearVelocity = -direction * frozenSpeed;

        // Reset tint
        var rend = GetComponentInChildren<SpriteRenderer>();
        if (rend != null)
            rend.color = Color.white;
    }

    protected float GetDirectionPitch(Vector2 dir)
    {
        if (dir == Vector2.up) return +0.08f;
        if (dir == Vector2.down) return -0.08f;
        if (dir == Vector2.right) return +0.04f;
        if (dir == Vector2.left) return -0.04f;

        // Diagonals
        if (dir == new Vector2(1, 1).normalized) return +0.10f;
        if (dir == new Vector2(-1, 1).normalized) return +0.02f;
        if (dir == new Vector2(1, -1).normalized) return -0.02f;
        if (dir == new Vector2(-1, -1).normalized) return -0.10f;

        return 0f;
    }



    void OnEnable()
    {
        ArrowSpawner.OnClearArrows += GameOverEffect;   
        UIManager.OnGameOver += GameOverEffect;
        GameStateManager.OnStateChanged += HandleRoundEnd;
    }
    


    void OnDisable()
    {
        ArrowSpawner.OnClearArrows -= GameOverEffect;
        UIManager.OnGameOver -= GameOverEffect;
        GameStateManager.OnStateChanged -= HandleRoundEnd;
    }

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        ArrowManager.Instance.RegisterArrow(this);
    }

    protected virtual void OnDestroy()
    {
        ArrowManager.Instance.UnregisterArrow(this);
    }

    void HandleRoundEnd(GameState previousState, GameState newState)
    {
        if (newState != GameState.RoundResultsTally) return;
        Die();
    }

    // Initialize arrow direction and movement
    public virtual void Fire(Vector2 direction, float speed)
    {
        this.direction = direction.normalized;
        this.speed = speed;
        rb.linearVelocity = -direction * speed;
        OrientArrow(direction);
    }

    public virtual void OrientArrow(Vector2 direction)
    {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        float offset = flip180 ? 90f : -90f;
        transform.rotation = Quaternion.Euler(0, 0, angle + offset);
    }

    // 🔹 Called when something hits the arrow (not necessarily killing it)
    public virtual void OnArrowHit(float damage = 1f, Goal.GoalType goalType = Goal.GoalType.Normal, Vector2 hitDirection = default)
    {
        if (invincible) return;

        health -= damage;

        // Optional: spawn a small impact VFX
        if (hitEffect != null)
            Instantiate(hitEffect, transform.position, transform.rotation);

        //OnArrowHitGlobal?.Invoke(this, goalType);

        if (health <= 0f)
            Die(goalType, hitDirection: hitDirection);
    }

    public virtual void KillArrow()
    {
        // Instantly die, bypassing health
        health = 0f;
        Die(Goal.GoalType.Miss, true); // Miss type since killed externally
    }

    protected virtual void Update()
    {
        if (HasStatus(ArrowStatus.Frozen))
        {
            freezeTimer -= Time.deltaTime;
            if (freezeTimer <= 0f)
                Unfreeze();

            return; // skip any per-frame updates while frozen
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!HasStatus(ArrowStatus.Frozen)) return;

        ArrowBase incoming = other.GetComponent<ArrowBase>();
        if (incoming != null && !incoming.HasStatus(ArrowStatus.Frozen) && HasStatus(ArrowStatus.Frozen))
        {
            // 💥 Shatter both arrows
            ShatterFrozenPair(incoming);
        }
    }

    private void ShatterFrozenPair(ArrowBase other)
    {
        // Optional effect: play icy explosion
        //if (destroyEffect != null)
            //Instantiate(destroyEffect, transform.position, Quaternion.identity);

        AudioHelpers.PlayMyClipAtPoint(freezeDestroySound, AudioChannel.SFX, transform.position);

        if (freezeEffect != null)
            Instantiate(freezeEffect, transform.position, Quaternion.identity);

        // Give player a small reward (optional)
        //ScoreManager.Instance.AddScore(scoreValue / 2);

        other.Die(Goal.GoalType.Normal);
        Die(Goal.GoalType.Normal);  
    }




    // 🔹 Centralized death handler
    protected virtual void Die(Goal.GoalType goalType = Goal.GoalType.Normal, bool invokeDeathEvent = true, Vector2 hitDirection = default)
    {
        if(destroyEffect != null)
            Instantiate(destroyEffect, transform.position, transform.rotation);

        if (invokeDeathEvent)
        {
            ArrowResolvedData data = new ArrowResolvedData
            {
                goalType = goalType,
                status = this.status
            };
            
            OnArrowResolved?.Invoke(data);  
        }

        float directionPitch = GetDirectionPitch(direction) * directionalPitchFactor;

        float pitch =
            goalType == Goal.GoalType.Critical
                ? criticalHitPitch
                : basePitch;

        pitch += directionPitch;
        pitch = Mathf.Clamp(pitch, 0.85f, 1.15f);

        AudioClip clip =
            goalType == Goal.GoalType.Critical
                ? criticalDestroySound
                : destroySound;

        PlayArrowHit(
            clip,
            transform.position,
            volume: 1f,
            pitch: pitch
        );

        Destroy(gameObject);
    }

    public void PlayArrowHit(
        AudioClip clip,
        Vector3 position,
        float volume = 1f,
        float pitch = 1f)
    {
        if(AudioSettingsManager.Instance.mixer == null)
            return;
            
        AudioMixer mixer = AudioSettingsManager.Instance.mixer;

        float strength = directionalEQStrength;

        // Target values (full effect)
        float hpfTarget = hpfNeutral;
        float lpfTarget = lpfNeutral;
        float midTarget = midNeutral;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // UP – bright / airy
        if (angle > 45f && angle < 135f)
        {
            hpfTarget = 400f;
            midTarget = +1.5f;
        }
        // DOWN – heavy / warm
        else if (angle < -45f && angle > -135f)
        {
            lpfTarget = 3000f;
        }
        // RIGHT – punchy / present
        else if (Mathf.Abs(angle) <= 45f)
        {
            midTarget = +2.0f;
        }
        // LEFT – hollow / scooped
        else
        {
            midTarget = -3.0f;
        }

        // Blend neutral → target
        float hpf = Mathf.Lerp(hpfNeutral, hpfTarget, strength);
        float lpf = Mathf.Lerp(lpfNeutral, lpfTarget, strength);
        float midGain = Mathf.Lerp(midNeutral, midTarget, strength);

        // Apply to mixer
        mixer.SetFloat("Arrow_HPF", hpf);
        mixer.SetFloat("Arrow_LPF", lpf);
        mixer.SetFloat("Arrow_MidGain", midGain);



 

        AudioHelpers.PlayClipWithVariation(
            clip,
            AudioSettingsManager.Instance.arrowHitsGroup,
            position,
            basePitch: pitch,
            pitchRange: 0f,
            volume: volume
        );

        // Reset quickly so it only affects this hit
        StartCoroutine(ResetArrowEQ(mixer));
    }

    IEnumerator ResetArrowEQ(AudioMixer mixer)
    {
        yield return null; // one frame is enough
        mixer.SetFloat("Arrow_HPF", 20f);
        mixer.SetFloat("Arrow_LPF", 22000f);
        mixer.SetFloat("Arrow_MidGain", 0f);
    }





    protected virtual void GameOverEffect()
    {
        StartCoroutine(GameOverEffectCoroutine());
    }

    private IEnumerator GameOverEffectCoroutine()
    {
        //rb.linearVelocity = Vector2.zero;

        float randomDestroyDelay = UnityEngine.Random.Range(0f, 0.5f);
        yield return new WaitForSeconds(randomDestroyDelay);
        // Optional: play a different effect on game over
        if (destroyEffect != null)
            Instantiate(destroyEffect, transform.position, transform.rotation);

        Destroy(gameObject);
    }



    // Arrow Status Helpers
    // ------------------------------------------------------------------

    public bool HasStatus(ArrowStatus s)
    {
        return status.HasFlag(s);
    }

    public bool HasAny(ArrowStatus mask)
    {
        return (status & mask) != 0;
    }

    public bool HasAll(ArrowStatus mask)
    {
        return (status & mask) == mask;
    }

    // ---------- Mutations ----------

    public void AddStatus(ArrowStatus s)
    {
        status |= s;
    }

    public void RemoveStatus(ArrowStatus s)
    {
        status &= ~s;
    }

    public void ClearStatus()
    {
        status = ArrowStatus.None;
    }

    public void SetStatus(ArrowStatus s)
    {
        status = s;
    }

    public ArrowStatus GetStatus()
    {
        return status;
    }
}
