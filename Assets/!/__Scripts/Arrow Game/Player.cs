using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class Player : MonoBehaviour
{
    public static Player Instance { get; private set; }
    public static event System.Action OnJumpInput;
    public static event System.Action<Vector2> OnJumped;
    public static event System.Action OnAbilityFilled;
    public static event System.Action<int, int, int> OnAbilityChargeChanged;
    public static event System.Action OnMaxAbilityChargeChanged;
    public static event System.Action OnAbilityUsed;
    public static event System.Action<int> OnDamageTaken;
    public static event System.Action<int> OnHeal;
    public static event System.Action OnMaxHealthChanged;
    public static event System.Action<int> OnPreDamageTaken;
    public static event System.Action<PlayerControlState> OnControlStateChanged;


    public enum PlayerControlState
    {
        Normal,
        Shooter,
        LockedShooter,
    }


    [Header("Set in Inspector")]
    public GameObject goal;
    public GameObject spriteObj;
    [SerializeField] private int _maxHealth = 10;


    [Header("Ability Charge Settings")]
    //[SerializeField] private AudioClip abilitySound;
    [SerializeField] private SoundEffect abilityChargedSoundEffect;
    [SerializeField] private float _critWindow = 0.2f;
    [SerializeField] private int _abilityChargeGain = 1;
    
  
    [Header("Rotation Settings")]
    public float goalRotateSpeed = 10f;
    

    [Header("On Damage Settings")]
    [SerializeField] private AudioClip damageSound;
    [SerializeField] private GameObject damageEffectPrefab;
    [SerializeField] private float invincibilityDuration = 0.5f;
    [SerializeField] private float hitShakeStrength = 0.05f;
    [SerializeField] private float hitShakeDuration = 0.15f;


    [Header("Heal Settings")]
    [SerializeField] private AudioClip healSound;
    [SerializeField] private ParticleSystem healParticleSystem;


    [Header("Boost Settings")]
    public PlayerWings wings;
    [SerializeField] private float boostDistance = 3f;
    [SerializeField] private float boostDuration = 0.15f;
    [SerializeField] private float returnDuration = 0.25f;
    [SerializeField] private float boostCooldown = 0.75f;
    [SerializeField] private AudioClip boostSound;


    [Header("Jump Settings")]
    public AudioClip jumpSound;
    public float jumpPitch = 2.0f;
    public float maxJumpHoldTime = 0.5f;
    public float jumpForce = 7f;           // initial jump velocity
    public float fallMultiplier = 2.5f;    // stronger gravity when falling
    public float lowJumpMultiplier = 2f;   // extra gravity if jump key is released early


    [Header("Projectile Settings")]
    [SerializeField] private float projectileSpawnOffset = 0.5f;
    [SerializeField] private SoundEffect controlModeSwitchSound;



    [Header("Set Dynamically")]
    [SerializeField] private int _health;
    [SerializeField] private AbilityBase _currentAbility;
    [SerializeField] private bool _useEightDirections = false;




    //public JumpInputMode jumpInputMode { get; private set; } = JumpInputMode.Normal;
    public PlayerControlState playerControlState { get; private set; } = PlayerControlState.Normal;


    private List<UpgradeEffectBase> activeUpgrades = new List<UpgradeEffectBase>();
    private int dirHeld = -1;
    private int facing = 1;
    private bool invincible = false;
    private int _abilityCharge = 0;
    private int _maxAbilityCharge = 10;
    private float invincibileDone = 0;
    private SpriteRenderer sRend;
    private bool _isRotating = false;
    private float _rotateStartTime;
    private Tween activeShakeTween;
    private Vector3 baseLocalPos;
    private bool canJump = false;
    private bool obstaclesActive = false;
    public bool lockInput = false;
    private Quaternion targetRotation;
    private Vector2 lastFacingDir = Vector2.up;
    private bool isBoosting = false;
    private bool boostOnCooldown = false;
    private Vector3 centerPosition;
    private Coroutine boostRoutine;
    private Rigidbody2D rb;


    private Vector2 jumpAxis; // direction for current jump
    private bool isJumping = false;
    private float jumpElapsedTime = 0f;



    public Vector2 LastFacingDirection => lastFacingDir;
    public bool IsRotating => _isRotating;
    public float RotateStartTime => _rotateStartTime;
    public float CritWindow => _critWindow;
    public bool FullAbilityCharge => AbilityCharge >= MaxAbilityCharge; 

    public bool FullyLocked { get; private set; } = false;

    private bool _invincible;
    public bool Invincible
    {
        get => _invincible;
        set
        {
            if (_invincible == value) return;
            _invincible = value;
            sRend.color = _invincible ? Color.red : Color.white;
        }
    }


    public bool UseEightDirections
    {
        get { return _useEightDirections; }
        set { _useEightDirections = value; }
    }

    public int Health
    {
        get { return _health; }
        set { _health = value; }
    }

    public int MaxHealth
    {
        get => _maxHealth;
        set
        {
            _maxHealth = Mathf.Max(1, value);
            OnMaxHealthChanged?.Invoke();
        }
    }

    public int MaxAbilityCharge
    {
        get { return UpgradeManager.Instance != null ? (int) UpgradeManager.Instance.ModifyAbilityCost(_maxAbilityCharge) : 0; }
        set { _maxAbilityCharge = value; }
    }   

    public int AbilityCharge
    {
        get => _abilityCharge;
        set
        {
            int previous = _abilityCharge;

            int attemptedDelta = value - previous;

            int clamped = Mathf.Clamp(value, 0, MaxAbilityCharge);
            int appliedDelta = clamped - previous;

            _abilityCharge = clamped;

            // Feedback / juice / UI
            OnAbilityChargeChanged?.Invoke(previous, attemptedDelta, appliedDelta);

            // Filled event
            if (previous < MaxAbilityCharge && clamped >= MaxAbilityCharge)
            {
                AudioHelpers.PlaySoundEffect(abilityChargedSoundEffect, Camera.main.transform.position);
                OnAbilityFilled?.Invoke();
            }
        }
    }

    
    public AbilityBase CurrentAbility
    {
        get { return _currentAbility; }
        set
        {
            _currentAbility = value;
            MaxAbilityCharge = _currentAbility != null ? _currentAbility.abilityBaseCost : 0;
        }
    }

    
    void OnEnable()
    {
        RoundManager.OnRoundStart += HandleRoundStart;
        RoundManager.OnRoundEnd += HandleRoundEnd;

        // ✅ Subscribe to ObstacleManager events
        ObstacleManager.OnFirstObstacleAppeared += EnableJumping;
        ObstacleManager.OnAllObstaclesCleared += DisableJumping;
    }

    void OnDisable()
    {
        RoundManager.OnRoundStart -= HandleRoundStart;
        RoundManager.OnRoundEnd -= HandleRoundEnd;

        // ✅ Unsubscribe safely
        ObstacleManager.OnFirstObstacleAppeared -= EnableJumping;
        ObstacleManager.OnAllObstaclesCleared -= DisableJumping;
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
        targetRotation = Quaternion.identity;
        sRend = spriteObj.GetComponent<SpriteRenderer>();
        Health = MaxHealth;
        centerPosition = transform.position; // cache original center
        baseLocalPos = spriteObj.transform.localPosition;
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        goal.transform.position = transform.position;

        Vector2 dir = Vector2.zero;

        if (Invincible && Time.time > invincibileDone)
            Invincible = false;

        if (FullyLocked)
            return;

        if(!GameStateEffectManager.PlayerInputEnabled) return;

        if (_useEightDirections)
        {
            if (InputBindingManager.Instance.GetKeyInput(InputActionType.MoveUp)) dir += Vector2.up;
            if (InputBindingManager.Instance.GetKeyInput(InputActionType.MoveDown)) dir += Vector2.down;
            if (InputBindingManager.Instance.GetKeyInput(InputActionType.MoveLeft)) dir += Vector2.left;
            if (InputBindingManager.Instance.GetKeyInput(InputActionType.MoveRight)) dir += Vector2.right;

            if (dir.sqrMagnitude > 0.1f)
            {
                dir.Normalize();
                RotateCollider(dir);
                lastFacingDir = dir;
            }
        }
        else
        {
            if (InputBindingManager.Instance.GetKeyInput(InputActionType.MoveUp)) dir = Vector2.up;
            else if (InputBindingManager.Instance.GetKeyInput(InputActionType.MoveDown)) dir = Vector2.down;
            else if (InputBindingManager.Instance.GetKeyInput(InputActionType.MoveLeft)) dir = Vector2.left;
            else if (InputBindingManager.Instance.GetKeyInput(InputActionType.MoveRight)) dir = Vector2.right;

            if (dir != Vector2.zero)
            {
                RotateCollider(dir);
                lastFacingDir = dir;
            }
        }

        goal.GetComponentInChildren<Goal>().SetGoalDirection(dir);

        // Ability usage
        if (InputBindingManager.Instance.GetKeyDown(InputActionType.UseAbility) && 
           FullAbilityCharge
           && !lockInput)
           //&& GameStateEffectManager.PlayerInputEnabled)
           //&& (GameStateManager.Instance.CurrentState == GameState.RoundActive ||
           //GameStateManager.Instance.CurrentState == GameState.Tutorial))
        {
            AbilityCharge -= MaxAbilityCharge;
            OnAbilityUsed?.Invoke();
            SpawnAbility();
        }


        if (InputBindingManager.Instance.GetKeyDown(InputActionType.Jump) && !lockInput)
        {
            if (playerControlState == PlayerControlState.LockedShooter)
            {
                OnJumpInput?.Invoke();
                if (wings != null) wings.PlayFlap();  
                return;
            }

            // Normal jump path
            if (!isBoosting &&
                !boostOnCooldown &&
                canJump &&
                obstaclesActive &&
                !isJumping)
            {
                PerformJump(dir);
            }
        }
        
    }

    void FixedUpdate()
    {
        ApplySmartGravity();
    }

    void PerformJump(Vector2 dir)
    {
        //UIToast.Show("Performing Jump", 1);
        AudioHelpers.PlayMyClipAtPoint(boostSound, AudioChannel.SFX, Camera.main.transform.position);

        // Use last direction if no input
        Vector2 inputDir = dir != Vector2.zero ? dir : lastFacingDir;
        inputDir.Normalize();

        jumpAxis = inputDir;  // store jump direction
        isJumping = true;

        hoverActive = true;        // Hover is allowed at start
        hoverLockedOut = false;    // Not locked yet
        jumpElapsedTime = 0f;

        rb.linearVelocity = jumpAxis * jumpForce;

        if (wings != null)
        wings.PlayFlap();

        AudioHelpers.PlayClipWithVariation(jumpSound, AudioChannel.SFX, Camera.main.transform.position, basePitch: jumpPitch, pitchRange: 0.1f);

        OnJumped?.Invoke(inputDir);
    }

    void OnTriggerEnter2D(Collider2D coll)
    {
        if (coll.gameObject.CompareTag("Player")) return;

        // If arrow is the collider and the arrow is invincible, ignore
        ArrowBase arrow = coll.GetComponent<ArrowBase>();
        if (arrow != null && arrow.invincible) return;

        // If we are invincible, destroy the arrow and return
        if (Invincible)
        {
            if (arrow != null) arrow.KillArrow();
            return;
        }

        // If we hit an arrow, we kill the arrow
        if (arrow != null) arrow.KillArrow();

        DamageEffect dEf = coll.GetComponent<DamageEffect>();
        if (dEf == null)
        {
            Debug.LogWarning("Player hit something without DamageEffect: " + coll.name);
            return;
        }

        DamageSelf(dEf.damage);
    }

    public void ShootProjectile(PlayerProjectile projectilePrefab)
    {
        Vector2 snappedDir = GetSnappedDirection(lastFacingDir, _useEightDirections);

        Vector3 spawnPos = transform.position + (Vector3)(snappedDir * projectileSpawnOffset);

        PlayerProjectile proj = Instantiate(
            projectilePrefab,
            spawnPos,
            Quaternion.identity
        );

        Vector2 normalizedDir = snappedDir.normalized;
        float angle = Mathf.Atan2(normalizedDir.y, normalizedDir.x) * Mathf.Rad2Deg - 90f;
        proj.transform.rotation = Quaternion.Euler(0f, 0f, angle);

        proj.Initialize(snappedDir);
    }

    
    public void SetFullyLocked(bool value)
    {
        FullyLocked = value;
    }

    public void SetPlayerControlState(PlayerControlState newState)
    {
        if (playerControlState == newState) return;

        playerControlState = newState;
        OnControlStateChanged?.Invoke(playerControlState);

        AudioHelpers.PlaySoundEffect(controlModeSwitchSound, this.transform.position);

        if (playerControlState == PlayerControlState.LockedShooter)
        {
            isJumping = false;
            rb.linearVelocity = Vector2.zero;
        }
    }


    public void ResetPositionAndVelocity()
    {
        transform.position = centerPosition;
        rb.linearVelocity = Vector2.zero;
        isJumping = false;
    }

        

    public void AddUpgrade(UpgradeEffectBase effect)
    {
        effect.Apply(this);
        activeUpgrades.Add(effect);
    }

    public void HealPlayer(int amount)
    {
        Health = Mathf.Min(MaxHealth, Health + amount);
        //OnDamageTaken?.Invoke(health);
        Debug.Log($"💖 Player healed by {amount}. Current health: {Health}/{MaxHealth}");
        OnHeal?.Invoke(amount);
        if (healParticleSystem != null)
            healParticleSystem.Play();

        AudioHelpers.PlayMyClipAtPoint(healSound, AudioChannel.SFX, Camera.main.transform.position);
    }





    public void IncreaseMaxHealth(int amount)
    {
        MaxHealth += amount;
        Health = Mathf.Min(Health + amount, MaxHealth);
        OnMaxHealthChanged?.Invoke();
    }

    private void EnableJumping()
    {
        obstaclesActive = true;
        canJump = true;
        wings.ShowWings();
        Debug.Log("🟡 Jumping enabled — obstacle present");
    }

    private void DisableJumping()
    {
        obstaclesActive = false;
        canJump = false;
        wings.HideWings();
        Debug.Log("⚫ Jumping disabled — no obstacles");
    }



    private void HandleRoundEnd()
    {
        canJump = false;
        lockInput = true;
    }

    private void HandleRoundStart()
    {
        canJump = true;
        lockInput = false;
    }

    public void OnCriticalCatch()
    {
        AbilityCharge += _abilityChargeGain;
        Debug.Log($"⚡ Gained {_abilityChargeGain} ability charge from crit catch!");
    }

    // -------------------------------------
    // BOOST LOGIC
    // -------------------------------------

    IEnumerator BoostRoutine(Vector2 dir)
    {
        isBoosting = true;
        boostOnCooldown = true;

        // 🪽 start wing flap
        if (wings != null)
            wings.PlayFlap();

        Vector3 startPos = centerPosition;
        Vector3 targetPos = centerPosition + (Vector3)dir * boostDistance;

        if (boostSound != null)
            AudioHelpers.PlayMyClipAtPoint(boostSound, AudioChannel.SFX, Camera.main.transform.position);

        float t = 0f;
        while (t < boostDuration)
        {
            t += Time.deltaTime;
            float normalized = t / boostDuration;
            float ease = 1f - Mathf.Pow(1f - normalized, 2f);
            transform.position = Vector3.Lerp(startPos, targetPos, ease);
            yield return null;
        }

        StartCoroutine(ReturnToCenter());
        yield return new WaitForSeconds(boostCooldown);
        boostOnCooldown = false;
    }


    IEnumerator ReturnToCenter()
    {
        Vector3 startPos = transform.position;
        float t = 0f;
        while (t < returnDuration)
        {
            t += Time.deltaTime;
            float normalized = t / returnDuration;
            // 🌀 ease-in curve (slow start, quick end)
            float ease = normalized * normalized;
            transform.position = Vector3.Lerp(startPos, centerPosition, ease);
            yield return null;
        }

        transform.position = centerPosition;
        isBoosting = false;
    }



    // -------------------------------------

    private void SpawnAbility()
    {
        // Find the current facing direction the player is allowed to use
        Vector2 snappedDir = GetSnappedDirection(lastFacingDir, _useEightDirections);

        // Compute snapped rotation
        float angle = Mathf.Atan2(snappedDir.y, snappedDir.x) * Mathf.Rad2Deg - 90f;
        Quaternion snappedRotation = Quaternion.Euler(0, 0, angle);

        // Spawn the ability prefab
        //currentAbility.transform.rotation = snappedRotation;
        CurrentAbility.Activate(snappedRotation);
        //GameObject wave = Instantiate(shockwavePrefab, transform.position, snappedRotation);
    }

    /// <summary>
    /// Snaps a direction vector to the nearest cardinal (or diagonal) direction.
    /// </summary>
    private Vector2 GetSnappedDirection(Vector2 inputDir, bool useEight)
    {
        if (inputDir == Vector2.zero)
            return lastFacingDir; // fallback to previous

        // 4 or 8 directions (unit vectors)
        Vector2[] dirs4 = {
            Vector2.up,
            Vector2.right,
            Vector2.down,
            Vector2.left
        };

        Vector2[] dirs8 = {
            Vector2.up,
            new Vector2(1, 1).normalized,
            Vector2.right,
            new Vector2(1, -1).normalized,
            Vector2.down,
            new Vector2(-1, -1).normalized,
            Vector2.left,
            new Vector2(-1, 1).normalized
        };

        Vector2[] pool = useEight ? dirs8 : dirs4;

        // Find direction with smallest angle to input
        float bestDot = -Mathf.Infinity;
        Vector2 bestDir = Vector2.up;

        foreach (Vector2 dir in pool)
        {
            float dot = Vector2.Dot(inputDir.normalized, dir);
            if (dot > bestDot)
            {
                bestDot = dot;
                bestDir = dir;
            }
        }

        return bestDir;
    }



    public void RotateCollider(Vector2 dir)
    {
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        angle -= 90f;
        Quaternion newTarget = Quaternion.Euler(0, 0, angle);

        if (newTarget != targetRotation)
        {
            targetRotation = newTarget;
            _isRotating = true;
            _rotateStartTime = Time.time;
        }
    }

    public void DamageSelf(int damage)
    {
        if (DevCheats.Invincible)
            return;

        if(!GameStateEffectManager.PlayerDamageAllowed)
            return;

        OnPreDamageTaken?.Invoke(damage);

        int finalDamage = UpgradeManager.Instance.ModifyDamageTaken(damage);
        
        Debug.Log($"💥 Player taking {finalDamage} damage (base {damage})");
        Health -= finalDamage;

        OnDamageTaken?.Invoke(Health);

        Invincible = true;
        invincibileDone = Time.time + invincibilityDuration;

        if (damageSound != null)
            AudioHelpers.PlayMyClipAtPoint(damageSound, AudioChannel.SFX, Camera.main.transform.position);

        if(damageEffectPrefab != null)
        {
            Instantiate(damageEffectPrefab, transform.position, Quaternion.identity);
        }

        spriteObj.GetComponent<PlayerSpriteShaker>()?.Shake(hitShakeStrength, hitShakeDuration);
    }

    private Vector2 lastToCenter;
    private Vector2 lastPosition;
    private bool hoverActive;
    private bool hoverLockedOut;

    void ApplySmartGravity()
    {
        if (!isJumping) return;

        // Project onto jump axis
        Vector2 projected = Vector2.Dot(rb.linearVelocity, jumpAxis) * jumpAxis;
        rb.linearVelocity = projected;

        Vector2 currentToCenter = (Vector2)centerPosition - rb.position;
        Vector2 toCenterDir = currentToCenter.normalized;

        float outwardDot = Vector2.Dot(rb.linearVelocity, -toCenterDir);
        //bool jumpHeld = InputBindingManager.Instance.GetKeyInput(InputActionType.Jump);
       

        bool jumpHeld = InputBindingManager.Instance.GetKeyInput(InputActionType.Jump);

        // If they release jump while rising, permanently disable hover
        if (!jumpHeld && hoverActive)
        {
            hoverLockedOut = true;
        }

        // Hover only works if:
        // - jump is held
        // - not locked out
        // - within hold time
        bool canHover = jumpHeld && !hoverLockedOut && jumpElapsedTime <= maxJumpHoldTime;

        if (canHover)
        {
            jumpElapsedTime += Time.deltaTime;
        }

         //UIToast.Show($"Jump Being Held -> {jumpHeld}", 0.05f);

        if (outwardDot < 0f)
        {
            rb.linearVelocity += toCenterDir * fallMultiplier * Time.deltaTime;
        }
        //else if (outwardDot > 0f && (!jumpHeld || jumpElapsedTime > maxJumpHoldTime))
        else if (outwardDot > 0f && !canHover)
        {
            rb.linearVelocity += toCenterDir * lowJumpMultiplier * Time.deltaTime;
        }

        bool distanceCheck = Vector2.Distance(transform.position, centerPosition) <= 0.175f && Vector2.Dot(rb.linearVelocity, currentToCenter) > 0f;
        bool dotCheck = lastToCenter != Vector2.zero && Vector2.Dot(lastToCenter, currentToCenter) <= 0f;

        // 🔒 Only check crossing AFTER lastToCenter is valid
        if (distanceCheck || dotCheck)
        {
            rb.linearVelocity = Vector2.zero;
            rb.position = centerPosition;
            isJumping = false;
            lastToCenter = Vector2.zero;
            hoverActive = false;
            hoverLockedOut = false;
            return;
        }

        lastPosition = rb.position;
        lastToCenter = currentToCenter;
    }







}
