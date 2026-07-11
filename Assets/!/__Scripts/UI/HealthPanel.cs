using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using DG.Tweening;

public class HealthPanel : MonoBehaviour
{
    [Header("References")]
    public Transform heartContainer;
    public GameObject heartPrefab;

    [Header("Heart Sprites")]
    public Sprite fullHeart;
    public Sprite fullHeartGlow;
    public Sprite halfHeart;
    public Sprite emptyHeart;

    [Header("Overflow Hearts")]
    [SerializeField] private int visibleHealthCapacity = 10;
    [SerializeField] private Sprite overflowFullHeart;
    [SerializeField] private Sprite overflowFullHeartGlow;
    [SerializeField] private Sprite overflowHalfHeart;


    [Header("Heart Mode")]
    public bool useHalfHearts = true;
    public int defaultAmount = 10;

    [Header("Shared Hearts Material")]
    public Material heartsMaterial;

    [Header("Shader Property")]
    public string colorProperty = "_Color";

    [Header("HDR Colors")]
    [Tooltip("Base HDR color shown normally.")]
    [ColorUsageAttribute(true,true)]
    public Color baseHDRColor = Color.white;

    [Tooltip("HDR color flashed when healing.")]
    [ColorUsageAttribute(true,true)]
    public Color healHDRColor = Color.white;

    [Header("Flash Timing")]
    public float flashInDuration = 0.15f;
    public float holdDuration = 0.25f;
    public float fadeBackDuration = 0.3f;
    public bool performHealEffect = true;

    private readonly List<Image> hearts = new();
    private Tween colorTween;

    private bool _glow = false;

    public bool Glow
    {
        get { return _glow; }
        set 
        { 
            _glow = value; 
            RefreshDisplay();
        }
    }

    // -------------------------------------------------------

    void OnEnable()
    {
        Player.OnDamageTaken += RefreshDisplay;
        Player.OnMaxHealthChanged += RebuildHearts;
        Player.OnHeal += OnHeal;
    }

    void OnDisable()
    {
        Player.OnDamageTaken -= RefreshDisplay;
        Player.OnMaxHealthChanged -= RebuildHearts;
        Player.OnHeal -= OnHeal;
    }

    void Start()
    {
        RebuildHearts();
    }

    // -------------------------------------------------------

    void OnHeal(int amount, bool wasFullHealth)
    {
        if (heartsMaterial == null)
            return;

        if (!performHealEffect)
        {
            RefreshDisplay();
            return;
        }

        DOTween.Kill(heartsMaterial);

        // Start at base
        heartsMaterial.SetColor(colorProperty, baseHDRColor);

        // HDR flash sequence
        colorTween = DOTween.Sequence()
            .Append(heartsMaterial.DOColor(healHDRColor, colorProperty, flashInDuration).SetEase(Ease.OutQuad))
            .AppendInterval(holdDuration)
            .Append(heartsMaterial.DOColor(baseHDRColor, colorProperty, fadeBackDuration).SetEase(Ease.InOutSine));

        RefreshDisplay();
    }

    // -------------------------------------------------------

    private void RebuildHearts()
    {
        //Debug.LogError("REBUILD HEARTS");
        foreach (Transform child in heartContainer)
            Destroy(child.gameObject);

        hearts.Clear();

        int maxHealth = Player.Instance != null ? Player.Instance.MaxHealth : defaultAmount;

        int displayedCapacity = Mathf.Min(maxHealth, visibleHealthCapacity);

        int heartCount = useHalfHearts
            ? Mathf.CeilToInt(displayedCapacity / 2f)
            : displayedCapacity;

        for (int i = 0; i < heartCount; i++)
        {
            GameObject heart = Instantiate(heartPrefab, heartContainer);
            Image img = heart.GetComponentInChildren<Image>();
            hearts.Add(img);

            // Every heart uses the SAME material instance
            if (heartsMaterial != null)
            {
                img.material = heartsMaterial;
            }
            else
            {
                if (i == 0)
                    heartsMaterial = img.material;
                else
                    img.material = heartsMaterial;
            }
        }

        if (heartsMaterial != null)
            heartsMaterial.SetColor(colorProperty, baseHDRColor);

        RefreshDisplay();
    }

    private void RefreshDisplay(int _ = 0)
    {
        int health = Player.Instance != null ? Player.Instance.Health : 0;

        int baseHealth = Mathf.Clamp(health, 0, visibleHealthCapacity);
        int overflowHealth = Mathf.Max(0, health - visibleHealthCapacity);

        for (int i = hearts.Count - 1; i >= 0; i--)
        {
            Image img = hearts[i];

            int slotAmount;

            // Overflow layer takes priority visually
            if (overflowHealth > 0)
            {
                slotAmount = useHalfHearts
                    ? Mathf.Min(overflowHealth, 2)
                    : Mathf.Min(overflowHealth, 1);

                overflowHealth -= slotAmount;

                img.sprite = GetHeartSprite(slotAmount, true);
                continue;
            }

            slotAmount = useHalfHearts
                ? Mathf.Min(baseHealth, 2)
                : Mathf.Min(baseHealth, 1);

            baseHealth -= slotAmount;

            img.sprite = GetHeartSprite(slotAmount, false);
        }
    }

    private Sprite GetHeartSprite(int amount, bool overflow)
    {
        if (useHalfHearts)
        {
            if (amount >= 2)
                return overflow
                    ? (Glow ? overflowFullHeartGlow : overflowFullHeart)
                    : (Glow ? fullHeartGlow : fullHeart);

            if (amount == 1)
                return overflow && overflowHalfHeart != null
                    ? overflowHalfHeart
                    : halfHeart;

            return emptyHeart;
        }

        if (amount >= 1)
            return overflow
                ? (Glow ? overflowFullHeartGlow : overflowFullHeart)
                : (Glow ? fullHeartGlow : fullHeart);

        return emptyHeart;
    }
}
