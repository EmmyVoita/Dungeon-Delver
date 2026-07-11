using System.Collections;
using DG.Tweening;
using UnityEngine;

public class DoomBombBossMechanic : MonoBehaviour
{
    [SerializeField] private SoundEffect destroySound;
    [SerializeField] private GameObject destroyEffect;
    [SerializeField] private int bombDamage = 10;
    [SerializeField] private float minScale = 1.0f;
    [SerializeField] private float maxScale = 2.0f;

    [SerializeField] private float damagePenatly = 0.1f;
    [SerializeField] private float critReduction = 0.1f;
    [SerializeField] private float missPenalty = 0.1f;
    [SerializeField] private float chargeRate = 0.05f;

    [Header("Dynamic")]
    [SerializeField] private float charge = 0f; // 0 = safe, 1 = explode

    private SpriteRenderer sr;
    private Color baseColor;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        baseColor = sr.color;
    }


    void OnEnable()
    {
        ArrowBase.OnArrowResolved += HandleArrowResolved;
        Player.OnDamageTaken += HandleDamageTaken;
    }

    void OnDisable()
    {
        ArrowBase.OnArrowResolved -= HandleArrowResolved;
        Player.OnDamageTaken -= HandleDamageTaken;
    }

    private void HandleDamageTaken(int amount)
    {
        charge += damagePenatly;
        FlashRed();
    }


    public void FlashRed()
    {
        sr.DOKill(); // 🔥 important (prevents stacking issues)

        Sequence seq = DOTween.Sequence();

        seq.Append(sr.DOColor(Color.red, 0.08f));
        seq.Append(sr.DOColor(baseColor, 0.15f));
    }

    public void FlashGreen()
    {
        sr.DOKill(); // 🔥 important (prevents stacking issues)

        Sequence seq = DOTween.Sequence();

        seq.Append(sr.DOColor(Color.green, 0.08f));
        seq.Append(sr.DOColor(baseColor, 0.15f));
    }


    private void HandleArrowResolved(ArrowResolvedData data)
    {
        switch (data.goalType)
        {
            case Goal.GoalType.Critical:
                charge -= critReduction;
                FlashGreen();
                break;

            case Goal.GoalType.Normal:
                break;

            case Goal.GoalType.Miss:
                charge += missPenalty;
                break;
        }
    }


    void Start()
    {
        
    }


    void Update()
    {
        charge = Mathf.Clamp(charge, 0f, 1f);

        charge += chargeRate * Time.deltaTime;

        transform.localScale = Vector3.one * Mathf.Lerp(minScale,maxScale,charge);

        if (charge >= 1f)
        {
            StartCoroutine(Explode());
        }
    }

    IEnumerator Explode()
    {
        ScreenShakeRequest ssRequest = new ScreenShakeRequest(duration: 1.0f,
                                                                magnitude: 0.1f,
                                                                direction: Vector2.up,
                                                                directional: true,
                                                                unscaled: true);
        //AudioHelpers.PlaySoundEffect(destroySound, transform.position);

        yield return new WaitForSeconds(0.2f);

        ScreenShakeManager.Instance.Shake(ssRequest);

        if(destroyEffect)
        {
            Instantiate(destroyEffect,transform.position,Quaternion.identity);
        }

        //Player.Instance.DamageSelf(bombDamage);

        Destroy(gameObject);
    }
}
