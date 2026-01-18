using UnityEngine;
using DG.Tweening;

public class Goal : MonoBehaviour
{
    public enum GoalType { Miss, Normal, Critical }

    [Header("Crit Movement Thresholds")]
    [SerializeField] private float critOnAngularSpeed = 35f;
    [SerializeField] private float critOffAngularSpeed = 20f;

   


    [Header("Visual / Audio")]
    [SerializeField] private AudioClip goalSound;
    [SerializeField] private AudioClip goalCritSound;
    [SerializeField] private ParticleSystem critCatchEffect;
    [SerializeField] private float flashDuration = 0.5f;
    [SerializeField] private Sprite defaultGoalSprite;
    [SerializeField] private Sprite normalCatchGoalSprite;
    [SerializeField] private Sprite criticalCatchGoalSprite;

    [Header("Crit Window Visuals")]
    [SerializeField] private Sprite critWindowSprite;

    [Header("Shake Settings")]
    [SerializeField] private float shakeStrength = 0.05f;
    [SerializeField] private float shakeDuration = 0.15f;
    [SerializeField] private int shakeVibrato = 2;



    [Header("Runtime")]
    [SerializeField] private Vector2 _goalDirection = Vector2.up;
    [SerializeField] public SpriteRenderer sRend;
    [SerializeField] private float flashDone = 0;
    [SerializeField] public bool flashColor = false;

    [Header("Golden Harvest Override")]
    public Sprite harvestModeSprite;    // your yellow/golden sprite


    private Tween activeShakeTween;
    private Vector3 baseLocalPos;
    private Quaternion targetRotation;
    private float lastManualRotateTime = -999f; // ✅ New — last time goal was rotated

    private float lastZRotation;
    [SerializeField] private bool isCritWindowActive = false;
    [SerializeField] private bool critArmed = false;
    [SerializeField] private float angularVelocity;


    // Exposed direction property (read-only)
    public Vector2 GoalDirection => _goalDirection;

    void Awake()
    {
        sRend = GetComponentInChildren<SpriteRenderer>();
        sRend.sprite = defaultGoalSprite;
        targetRotation = Quaternion.identity;
    }

    void Start()
    {
        baseLocalPos = transform.localPosition;
    }

    void Update()
    {
        float smoothing = 1f - Mathf.Exp(-Player.Instance.goalRotateSpeed * Time.deltaTime);

        transform.localRotation = Quaternion.Lerp(
            transform.localRotation,
            targetRotation,
            smoothing
        );

           // 🔹 Measure angular velocity
        float currentZ = transform.localEulerAngles.z;
        angularVelocity = Mathf.Abs(Mathf.DeltaAngle(lastZRotation, currentZ)) / Time.deltaTime;
        lastZRotation = currentZ;

        UpdateCritArmedState();
        UpdateCritWindowVisual();
    }
    
    public void EnterHarvestMode()
    {
        if (harvestModeSprite != null)
            sRend.sprite = harvestModeSprite;

        // Optional: pulse the goal a little
        //transform.DOKill();
        //transform.localScale = Vector3.one;
        //transform.DOPunchScale(Vector3.one * 0.15f, 0.25f, 6, 0.4f);
    }

    public void ExitHarvestMode()
    {
        sRend.sprite = defaultGoalSprite;

        //transform.DOKill();
        //transform.localScale = Vector3.one;
    }


    // --------------------------------------------------------
    // 🧭 External control for direction
    // --------------------------------------------------------
    public void SetGoalDirection(Vector2 dir)
    {
        if (dir == Vector2.zero)
            return;

        _goalDirection = dir.normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
        targetRotation = Quaternion.Euler(0, 0, angle);

        // 🕒 Record when rotation happened
        lastManualRotateTime = Time.time;
    }

    // --------------------------------------------------------
    // 🎯 Arrow collision
    // --------------------------------------------------------
    void OnTriggerEnter2D(Collider2D other)
    {
        ArrowBase arrow = other.GetComponent<ArrowBase>();
        if (arrow == null || arrow.invincible)
            return;

        bool isCrit = IsCritWindowActive();

        int score;
        ScorePopupKind popupKind;
        GoalType goalType;

        if (isCrit)
        {
            goalType = GoalType.Critical;
            score = ScoreRules.Instance.CalculateScore(arrow, goalType);

            // 🔑 Decide popup kind here
            if (arrow.IsGolden)
                popupKind = ScorePopupKind.Golden;
            else
                popupKind = ScorePopupKind.CritHit;

            if (critCatchEffect != null)
                critCatchEffect.Play();

            Player.Instance.OnCriticalCatch();
        }
        else
        {
            goalType = GoalType.Normal;
            score = ScoreRules.Instance.CalculateScore(arrow, goalType);
            popupKind = ScorePopupKind.NormalHit;
        }

        int finalAddedScore = ScoreManager.Instance.AddScore(score, ScoreSource.BaseArrow);

        ScoreEvents.OnScorePopupRequested?.Invoke(
            finalAddedScore,
            popupKind
        );

        arrow.OnArrowHit(1, goalType, _goalDirection);
        PlayImpactShake();
    }



    // --------------------------------------------------------
    // 💥 Shake feedback
    // --------------------------------------------------------
    public void PlayImpactShake()
    {
        Debug.Log("🔔 Playing goal impact shake.");
        if (activeShakeTween != null && activeShakeTween.IsActive())
        {
            activeShakeTween.Kill();
            transform.localPosition = baseLocalPos;
        }

        Sequence shakeSeq = DOTween.Sequence();
        shakeSeq.SetUpdate(true);

        for (int i = 0; i < shakeVibrato; i++)
        {
            float intensity = Mathf.Sin(i / (float)shakeVibrato * Mathf.PI) * shakeStrength;
            Vector3 offset = transform.up * (intensity * (i % 2 == 0 ? 1 : -1));

            shakeSeq.Append(transform.DOLocalMove(baseLocalPos + offset, shakeDuration / shakeVibrato / 2).SetEase(Ease.OutSine));
            shakeSeq.Append(transform.DOLocalMove(baseLocalPos, shakeDuration / shakeVibrato / 2).SetEase(Ease.InSine));
        }

        shakeSeq.OnComplete(() =>
        {
            transform.localPosition = baseLocalPos;
            activeShakeTween = null;
        });

        activeShakeTween = shakeSeq;
    }

    bool IsCritWindowActive()
    {
        // Movement-based crit
        if (critArmed) return true;
        
        return false;
    }


    void UpdateCritArmedState()
    {
        float critWindowModifier = UpgradeManager.Instance.ModifyCritWindow(Player.Instance.CritWindow);
        critWindowModifier = Mathf.Max(0.001f, critWindowModifier);

        float speedScale = Player.Instance.CritWindow / critWindowModifier;

        float onThreshold = critOnAngularSpeed * speedScale;
        float offThreshold = critOffAngularSpeed * speedScale;

        // When critWidnowModifier > 1, then speedScale < 1, and the onThreshold will be lower. The off theshold will also be lower.
        // When critWindowModifier < 1, then speedScale > 1, and the onThreshold will be higher. The off threshold will also be higher.

        if (!critArmed)
        {
            // Turn ON only when clearly moving
            if (angularVelocity >= onThreshold)
            {
                critArmed = true;
            }
        }
        else
        {
            // Turn OFF only when clearly stopped
            if (angularVelocity <= offThreshold)
            {
                critArmed = false;
            }
        }
    }



    void UpdateCritWindowVisual()
    {
        bool critActive = IsCritWindowActive();

        if (critActive == isCritWindowActive)
            return;

        isCritWindowActive = critActive;

        if (isCritWindowActive && critWindowSprite != null)
        {
            sRend.sprite = critWindowSprite;
        }
        else
        {
            sRend.sprite = defaultGoalSprite;
        }
    }




}
