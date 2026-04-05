using UnityEngine;
using System.Collections;
using System;
using UnityEngine.VFX;

public class SlowTimeAbilityObject : MonoBehaviour
{
    public static event Action<SlowTimeAbilityObject> OnSlowTimeEnded;
    public static event Action<SlowTimeAbilityObject> OnSlowTimeStarted;

    [Header("Slow Time Settings")]
    public VisualEffect slowTimeEffect;
    public float slowDuration = 2.0f;
    public float slowFactor = 0.5f; // Slow down to 50% speed
    public AudioClip slowSound;

    [Header("Visual Effect")]
    public SpriteRenderer spinSprite;       // assign the child sprite
    [Range(0f, 1f)] public float targetAlpha = 0.8f;
    public float fadeDuration = 0.3f;
    public float rotationSpeed = 180f;

    [Header("State")]
    [SerializeField] private bool slowTime = false;
    [SerializeField] private float slowTimeDone = 0;

    public bool useExpandingWave = false;
    public GameObject schockwavePrefab;
    public int maxCountForFullRange = 10;
    public float minRange = 3f;
    public float maxRange = 10f;

    private Coroutine spinRoutine;
    private int arrowsCaught = 0;

    
    void OnEnable()
    {
        ArrowBase.OnArrowResolved += HandleArrowDeath;
    }

    void OnDisable()
    {
        ArrowBase.OnArrowResolved -= HandleArrowDeath;
    }

    void HandleArrowDeath(ArrowResolvedData data)
    {
        arrowsCaught++;
    }

    void Awake()
    {
        SlowTime();
    }

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

    void Update()
    {
        if (slowTime && Time.time >= slowTimeDone)
        {
            TimeManager.Instance.SetModifier(1, 0.1f);
            slowTime = false;

            OnSlowTimeEnded?.Invoke(this);

            if (spinRoutine != null)
                StopCoroutine(spinRoutine);

            if (spinSprite != null)
                StartCoroutine(FadeOutAndStop());
        }
    }

    void SlowTime()
    {
        if (slowTimeEffect != null)
            slowTimeEffect.SendEvent("OnPlay");

        TimeManager.Instance.SetModifier(slowFactor, 0.1f);

        OnSlowTimeStarted?.Invoke(this);
        
        Time.timeScale = slowFactor;
        Debug.Log("Time slowed down due to crit combo!");
        slowTimeDone = Time.time + slowDuration;
        slowTime = true;

        AudioHelpers.PlayMyClipAtPoint(slowSound, AudioChannel.SFX, Camera.main.transform.position, 1f);

        if (spinRoutine != null)
            StopCoroutine(spinRoutine);

        // 🪄 Detach the spin sprite so it stays in world-space
        if (spinSprite != null)
        {
            spinSprite.transform.SetParent(null); // Detach from player
            spinSprite.transform.position = UnityEngine.Vector3.zero;
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

    private IEnumerator FadeOutAndStop(Action onComplete = null)
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

        onComplete?.Invoke();
    }
}
