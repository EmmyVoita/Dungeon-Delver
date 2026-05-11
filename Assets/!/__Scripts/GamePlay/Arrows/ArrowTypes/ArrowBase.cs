using UnityEngine;
using System;
using System.Collections;
using UnityEngine.Audio;



public struct ArrowResolvedData
{
    public Goal.GoalType goalType;
    public ArrowStatus status;
}

public enum ArrowCatchRule
{
    Catch,
    Avoid
}


public abstract class ArrowBase : MonoBehaviour
{
    public static event Action<ArrowResolvedData> OnArrowResolved;

    [Header("Arrow Stats")]
    [SerializeField] private float health = 1f;

    [Header("Type Settings")]
    [SerializeField] private ArrowCatchRule catchRule = ArrowCatchRule.Catch;

    [Header("Sprite")]
    [SerializeField] private bool flip180 = false;


    [Header("Audio Base Settings")]
    [SerializeField] private AudioClip arrowHitSound;
    [SerializeField] private float normalHitPitch = 1.0f;
    [SerializeField] private float criticalHitPitch = 1.2f;
    [Range(0f, 1f),SerializeField] private float directionalPitchFactor = 1f;


    [Header("Audio EQ Settings")]
    [Range(0f, 1f)] public float directionalEQStrength = 0.5f;
    [SerializeField] private float hpfNeutral = 20f;
    [SerializeField] private float lpfNeutral = 22000f;
    [SerializeField] private float midNeutral = 0f;


    [Header("Movement")]
    protected Vector2 direction;
    protected float speed;
    protected Rigidbody2D rb;


    //[Header("State Effects")]
    //public GameObject empowerEffect;
    //public AudioClip empowerSound;
    //public AudioClip empowerDestroySound;

    //public GameObject freezeEffect;
    //public AudioClip freezeDestroySound;
    //public AudioClip freezeEffectSound;

    [Header("Dynamic")]
    [SerializeField] protected bool invincible = false;
    public ArrowStatus status;

    // --- Freeze state ---
    private float freezeTimer = 0f;
    private float frozenSpeed = 0f;
    protected bool _isDead = false;
    float _expectedArrivalTime;

    public bool IsGolden => status.HasFlag(ArrowStatus.Golden);
    public bool IsRecoveryArrow => status.HasFlag(ArrowStatus.Recovery);
    public bool Invincible => invincible;
    public ArrowCatchRule CatchRule => catchRule;
    public bool IsInverse => catchRule == ArrowCatchRule.Avoid;
    public Vector2 Direction => direction;

    protected float spawnTime;
    protected float arrivalTime;
    protected Vector2 startPos;
    protected Vector2 endPos;
    private float testStartTime;



    public void SetRecoveryArrow()
    {
        AddStatus(ArrowStatus.Recovery);

        var sRend = GetComponentInChildren<SpriteRenderer>();
        sRend.color = new Color(0.8f, 1f, 0.8f, 1f); // light green tint   
    }

    public void SetGolden()
    {
        AddStatus(ArrowStatus.Golden);
        
        var sRend = GetComponentInChildren<SpriteRenderer>();
        sRend.color = new Color(1f, 1f, 0f, 1f);

        /*
        //scoreValue = Mathf.RoundToInt(scoreValue * multiplier);
        if (empowerEffect != null)
        {
            GameObject effect = Instantiate(empowerEffect, transform.position, Quaternion.identity);
            effect.transform.SetParent(this.transform);
        }

        AudioHelpers.PlayMyClipAtPoint(empowerSound, AudioChannel.SFX, transform.position);
        */
    }

    public void Freeze(float duration)
    {
        if (HasStatus(ArrowStatus.Frozen)) return;

        AddStatus(ArrowStatus.Frozen);
        freezeTimer = duration;
        frozenSpeed = speed;

        // Stop movement
        //rb.linearVelocity = Vector2.zero;

        //AudioHelpers.PlayMyClipAtPoint(freezeEffectSound, AudioChannel.SFX, transform.position);

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
        //rb.linearVelocity = -direction * frozenSpeed;

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
    public virtual void Init(Vector2 direction, float speed, float spawnTime, float arrivalTime, Vector3 startPos, Vector3 endPos)
    {
        this.direction = direction.normalized;
        this.speed = speed;
        this.spawnTime = spawnTime;
        this.arrivalTime = arrivalTime;
        this.startPos = startPos;
        this.endPos = endPos;

        testStartTime = Time.time - spawnTime;

        //rb.linearVelocity = -direction * speed;
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
        /*
        // Optional: spawn a small impact VFX
        if (hitEffect != null)
            Instantiate(hitEffect, transform.position, transform.rotation);
        */

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
        if (_isDead) return;

        float rawTime = (float)MusicManager.Instance.ScaledElapsedTime;
        float scaledTime = rawTime * TimeManager.Instance.GetCurrentScale();

        float elapsed = (float)MusicManager.Instance.ScaledElapsedTime;

        Vector2 targetPos;

        if (elapsed <= arrivalTime)
        {
            // Normal movement to goal
            float t = Mathf.InverseLerp(spawnTime, arrivalTime, elapsed);
            targetPos = Vector2.Lerp(startPos, endPos, t);
        }
        else
        {
            // AFTER goal → continue to center
            float extraTime = elapsed - arrivalTime;

            float postTravelDuration = 0.2f; // tweak this

            float t = Mathf.Clamp01(extraTime / postTravelDuration);

            targetPos = Vector2.Lerp(endPos, Vector2.zero, t);
        }

        SmoothTranslate(targetPos);
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

    protected void SmoothTranslate(Vector3 targetPos)
    {
        // 👇 Smooth movement toward correct DSP position
        transform.position = Vector2.Lerp(
            transform.position,
            targetPos,
            1f - Mathf.Exp(-20f * Time.deltaTime)
        );
    }

    private void ShatterFrozenPair(ArrowBase other)
    {
        // Optional effect: play icy explosion
        //if (destroyEffect != null)
            //Instantiate(destroyEffect, transform.position, Quaternion.identity);
        /*
        AudioHelpers.PlayMyClipAtPoint(freezeDestroySound, AudioChannel.SFX, transform.position);

        if (freezeEffect != null)
            Instantiate(freezeEffect, transform.position, Quaternion.identity);

        // Give player a small reward (optional)
        //ScoreManager.Instance.AddScore(scoreValue / 2);
        */

        other.Die(Goal.GoalType.Normal);
        Die(Goal.GoalType.Normal);  
    }




    // 🔹 Centralized death handler
    protected virtual void Die(Goal.GoalType goalType = Goal.GoalType.Normal, bool invokeDeathEvent = true, Vector2 hitDirection = default)
    {   
        if(_isDead) return;

        _isDead = true;

        if (invokeDeathEvent)
        {
            ArrowResolvedData data = new ArrowResolvedData
            {
                goalType = goalType,
                status = this.status
            };
            
            OnArrowResolved?.Invoke(data);  
        }

        //AudioSettingsManager.Instance.PlayArrowHitSound();

        PlayAudio(goalType);

        Destroy(gameObject);
    }

    public void PlayAudio(Goal.GoalType goalType)
    {
        float directionPitch = GetDirectionPitch(direction) * directionalPitchFactor;

        float pitch =
            goalType == Goal.GoalType.Critical
                ? criticalHitPitch
                : normalHitPitch;

        pitch += directionPitch;

        PlayArrowHit(
            arrowHitSound,
            transform.position,
            volume: 1f,
            pitch: pitch
        );
    }

    public void PlayArrowHit(
        AudioClip clip,
        Vector3 position,
        float volume = 1f,
        float pitch = 1f)
    {
        if(AudioSettingsManager.Instance.mixer == null)
            return;

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

        float midVolume = Mathf.Pow(10f, midGain / 20f);

        AudioHelpers.PlayArrowAudio(
            clip,
            position,
            pitch,
            volume * midVolume,
            hpf,
            lpf
        );
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
        /*
        if (destroyEffect != null)
            Instantiate(destroyEffect, transform.position, transform.rotation);
        */

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
