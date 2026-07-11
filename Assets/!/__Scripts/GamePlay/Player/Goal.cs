using UnityEngine;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.VFX;

public class Goal : MonoBehaviour
{
    public enum GoalType { Miss, Normal, Critical }

    [SerializeField] private SpriteRenderer critRing;
    [SerializeField] private float critActiveAlpha = 0.8f;
    [SerializeField] private float critInactiveAlpha = 0.15f;
    [SerializeField] private float activeScale = 1.05f;

    private Tween critRingTween;


    [Header("Crit Movement Thresholds")]
    [SerializeField] private float critOnAngularSpeed = 35f;
    [SerializeField] private float critOffAngularSpeed = 20f;
    [SerializeField] private List<PlayerControlState> showWindowVisuals;
   


    [Header("Visual / Audio")]
    [SerializeField] private AudioClip goalSound;
    [SerializeField] private AudioClip goalCritSound;
    //[SerializeField] private ParticleSystem critCatchEffect;
    [SerializeField] private VisualEffect critCatchEffect;
    [SerializeField] private float flashDuration = 0.5f;
    //[SerializeField] private Sprite defaultGoalSprite;
    //[SerializeField] private Sprite normalCatchGoalSprite;
    //[SerializeField] private Sprite criticalCatchGoalSprite;

    [Header("Crit Window Visuals")]
    //[SerializeField] private Sprite critWindowSprite;

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


    [SerializeField] private bool isCritWindowActive = false;
    private float critExpireTime = -999f;
    [SerializeField] private Material spriteMat;
    
    [ColorUsage(true, true)] [SerializeField] private Color glowColor;
    //[SerializeField] private bool critArmed = false;
    //[SerializeField] private float angularVelocity;

    private bool prevCritWindowActive = false;

    private Tween ringTween;

    private void SetRingAlpha(float alpha)
    {
        Color color = critRing.color;
        color.a = alpha;
        critRing.color = color;
    }


    // Exposed direction property (read-only)
    public Vector2 GoalDirection => _goalDirection;
    public bool CritWindowActive => Time.time <= critExpireTime;

    void Awake()
    {
        sRend = GetComponentInChildren<SpriteRenderer>();
        targetRotation = Quaternion.identity;
        prevCritWindowActive = CritWindowActive;
    }

    void Start()
    {
        baseLocalPos = transform.localPosition;
    }

    void OnEnable()
    {
        Player.OnControlStateChanged += HandleControlStateChanged;
    }

    void OnDiable()
    {
        Player.OnControlStateChanged -= HandleControlStateChanged;
    }

    private void HandleControlStateChanged(PlayerControlState newState)
    {
        spriteMat.SetColor("_Color", Color.white);
    }

    void Update()
    {
        float smoothing = 1f - Mathf.Exp(-Player.Instance.goalRotateSpeed * Time.deltaTime);

        transform.localRotation = Quaternion.Lerp(
            transform.localRotation,
            targetRotation,
            smoothing
        );
    }

    private void TriggerCritWindowVisual(float critWindow)
    {
        if(!showWindowVisuals.Contains(Player.Instance.playerControlState))
            return;


        critRingTween?.Kill();

        critRing.transform.localScale = Vector3.one * activeScale;

        critRingTween = critRing.transform
            .DOScale(1f, 0.08f)
            .SetDelay(critWindow);
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
        //sRend.sprite = defaultGoalSprite;

        //transform.DOKill();
        //transform.localScale = Vector3.one;
    }


    public void ModifyScale(float scaleMod, float duration)
    {
        StartCoroutine(ModifyScaleSequence(scaleMod,duration));
    }

    IEnumerator ModifyScaleSequence(float scaleMod, float duration)
    {
        Vector3 originalScale = transform.localScale;
        transform.localScale = originalScale * scaleMod;
        yield return new WaitForSeconds(duration);

        transform.localScale = originalScale;
    }


    public void SetGoalDirection(Vector2 dir)
    {
        if (dir == Vector2.zero)
            return;

        dir = dir.normalized;

        if (dir == _goalDirection)
            return;

        _goalDirection = dir.normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
        targetRotation = Quaternion.Euler(0, 0, angle);

        lastManualRotateTime = Time.time;

        float critWindow =
            UpgradeManager.Instance == null
            ? Player.Instance.CritWindow
            : UpgradeManager.Instance.ModifyCritWindow(Player.Instance.CritWindow);

        critExpireTime = Time.time + critWindow;

        TriggerCritWindowVisual(critWindow);
    }

    // --------------------------------------------------------
    // 🎯 Arrow collision
    // --------------------------------------------------------
    void OnTriggerEnter2D(Collider2D other)
    {
        ArrowBase arrow = other.GetComponent<ArrowBase>();
        DamageEffect damageEffect = other.GetComponent<DamageEffect>();
        if (arrow == null || arrow.Invincible)
            return;

        if(arrow.IsInverse)
        {
            ResolveInverseArrowCaught(arrow,damageEffect);
        }
        else
        {
            ResolveArrowCaught(arrow);
        }
    }

    public void ResolveArrowCaught(ArrowBase arrow)
    {
        bool isCrit = CritWindowActive;

        int score;
        ScorePopupKind popupKind;
        GoalType goalType;

        if (isCrit && !arrow.IsInverse)
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

    private void ResolveInverseArrowCaught(ArrowBase arrow, DamageEffect damageEffect)
    {
        Player.Instance.DamageSelf(damageEffect,arrow);
    }



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
}
