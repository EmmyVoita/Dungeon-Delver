using UnityEngine;
using System.Collections;
using System;
using UnityEngine.VFX;
using DG.Tweening;

public class SlowTimeAbilityObject : AbilityEffectBase
{
    public static event Action<SlowTimeAbilityObject> OnSlowTimeEnded;
    public static event Action<SlowTimeAbilityObject> OnSlowTimeStarted;

    [Header("Slow Time Settings")]
    public VisualEffect slowTimeEffect;
    public float slowFactor = 0.5f; // Slow down to 50% speed
    public float additionalTimeFromArrow = 0.2f;

    [Header("Visual Effect")]
    public SpriteRenderer spinSprite;       // assign the child sprite
    [Range(0f, 1f)] public float targetAlpha = 0.8f;
    public float fadeDuration = 0.3f;
    public float rotationSpeed = 180f;

    [Header("State")]
    [SerializeField] private bool slowTime = false;
    [SerializeField] private float slowTimeDone = 0;

    [Header("Audio")]
    [SerializeField] private SoundEffect critcalSEf;

    public bool useExpandingWave = false;
    public GameObject schockwavePrefab;
    public int maxCountForFullRange = 10;
    public float minRange = 3f;
    public float maxRange = 10f;

    private Coroutine spinRoutine;
    private int _arrowsCaught = 0;
    private int _criticalArrowsCaught = 0;
    private TimeScaleModifier _slowTimeModifier;


    private bool HasCurrencyUpgrade => Context.UpgradeCount >= 1;
    private bool HasDurationUpgrade => Context.UpgradeCount >= 2;

    
    void OnEnable()
    {
        ArrowBase.OnArrowResolved += HandleArrowResolved;
    }

    void OnDisable()
    {
        ArrowBase.OnArrowResolved -= HandleArrowResolved;
    }

    public override void Activate(AbilityEffectContext context)
    {
        _arrowsCaught = 0;
        _criticalArrowsCaught = 0;

        base.Activate(context);
        BeginSlowTime();
    }

    void HandleArrowResolved(ArrowResolvedData data)
    {
        if(!slowTime)
            return;

        if(data.goalType == Goal.GoalType.Miss)
            return;

        if(data.goalType == Goal.GoalType.Critical)
        {
            _criticalArrowsCaught++;

            if(HasCurrencyUpgrade)
                GrantAbilityCurrency();

            if(HasDurationUpgrade)
                ExtendSlowTime();
        }
    }

  
    private void GrantAbilityCurrency()
    {
        int reward = Mathf.RoundToInt(1 * Mathf.Pow(_criticalArrowsCaught, 1.25f));
        CurrencyManager.Instance.AddCurrency(reward, "Ability");
    }

    private void ExtendSlowTime()
    {
        slowTimeDone += additionalTimeFromArrow;
    }





    void Update()
    {
        
        if (slowTime && Time.unscaledTime >= slowTimeDone)
        {
            slowTime = false;

            if (_slowTimeModifier != null)
            {
                DOTween.To(
                    () => _slowTimeModifier.Value,
                    x => _slowTimeModifier.SetValue(x),
                    1f,
                    0.1f
                )
                .SetEase(Ease.InOutSine)
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    TimeManager.Instance.RemoveModifier(_slowTimeModifier.Id);
                    _slowTimeModifier = null;
                });
            }

            OnSlowTimeEnded?.Invoke(this);

            if (spinRoutine != null)
                StopCoroutine(spinRoutine);

            if (spinSprite != null)
                StartCoroutine(FadeOutAndStop());
        }
    }

    void BeginSlowTime()
    {
        if (slowTimeEffect != null)
            slowTimeEffect.Play();

        if (_slowTimeModifier != null)
        {
            TimeManager.Instance.RemoveModifier(_slowTimeModifier.Id);
            _slowTimeModifier = null;
        }

        _slowTimeModifier = new TimeScaleModifier("SlowTimeAbility", 1f);
        TimeManager.Instance.AddModifier(_slowTimeModifier);

        DOTween.To(
            () => _slowTimeModifier.Value,
            x => _slowTimeModifier.SetValue(x),
            slowFactor,
            0.1f
        )
        .SetEase(Ease.InOutSine)
        .SetUpdate(true);

        OnSlowTimeStarted?.Invoke(this);

        Debug.Log("Time slowed down due to crit combo!");

        slowTimeDone = Time.unscaledTime + Mathf.Max(0.1f,Context.Duration);
        slowTime = true;

        if (spinRoutine != null)
            StopCoroutine(spinRoutine);

        if (spinSprite != null)
        {
            spinSprite.transform.SetParent(null);
            spinSprite.transform.position = Vector3.zero;
        }

        spinRoutine = StartCoroutine(SpinAndFadeIn());
    }



    private IEnumerator SpinAndFadeIn()
    {
        if (spinSprite == null)
            yield break;

        spinSprite.gameObject.SetActive(true);

        // Start transparent and no rotation speed
        Color c = spinSprite.color;
        c.a = 0f;
        spinSprite.color = c;

        float timer = 0f;
        float currentRotSpeed = 0f;

        // Fade in + ramp rotation speed
        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(timer / fadeDuration);

            c.a = Mathf.Lerp(0f, targetAlpha, t);
            spinSprite.color = c;

            currentRotSpeed = Mathf.Lerp(0f, rotationSpeed, Mathf.SmoothStep(0f, 1f, t));
            spinSprite.transform.Rotate(Vector3.forward * currentRotSpeed * Time.unscaledDeltaTime);

            yield return null;
        }

        // Maintain rotation while active
        while (slowTime)
        {
            spinSprite.transform.Rotate(Vector3.forward * rotationSpeed * Time.unscaledDeltaTime);
            yield return null;
        }

        // Fade out after slow time ends
        yield return FadeOutAndStop();
    }

    private IEnumerator FadeOutAndStop()
    {
        if (spinSprite == null)
            yield break;

        Color c = spinSprite.color;
        float timer = 0f;
        float startAlpha = c.a;
        float currentRotSpeed = rotationSpeed;

        if(slowTimeEffect != null)
        slowTimeEffect.Stop();

        // Fade out + decelerate rotation
        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(timer / fadeDuration);

            c.a = Mathf.Lerp(startAlpha, 0f, t);
            spinSprite.color = c;

            currentRotSpeed = Mathf.Lerp(rotationSpeed, 0f, Mathf.SmoothStep(0f, 1f, t));
            spinSprite.transform.Rotate(Vector3.forward * currentRotSpeed * Time.unscaledDeltaTime);

            yield return null;
        }

        spinSprite.gameObject.SetActive(false);

        // Optional: reattach to the player hierarchy afterward
        spinSprite.transform.SetParent(Player.Instance.transform);

        //if(HasCurrencyUpgrade)
        //    GrantAbilityCurrency();

        EndEffect();
    }
}



    /*
    Action onFadeComplete
    {
        get
        {
            return () =>
            {
                // Spawn shockwave effect
                if (schockwavePrefab != null && useExpandingWave)
                {
                    float t = Mathf.Clamp01(arrowsCaught / (float)maxCountForFullRange);
                    float radius = Mathf.Lerp(minRange, maxRange, t);
                    GameObject shockwave = Instantiate(schockwavePrefab, Player.Instance.transform.position, Quaternion.identity);
                    var sw = shockwave.GetComponent<ShockwaveEffect>();
                    if (sw != null)
                        sw.Initialize(radius);
                    // Optionally, you can set properties on the shockwave here, e.g., scale based on arrowsCaught
                    Destroy(this.gameObject);
                }
            };
        }
    }
    */
