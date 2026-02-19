using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using DG.Tweening;

public class GuiPanelArrow : MonoBehaviour
{
    [Header("References")]
    public Transform heartContainer;
    public GameObject heartPrefab;

    [Header("Heart Sprites")]
    public Sprite fullHeart;
    public Sprite halfHeart;
    public Sprite emptyHeart;

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

    void OnHeal(int amount)
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
        foreach (Transform child in heartContainer)
            Destroy(child.gameObject);

        hearts.Clear();

        if(Player.Instance == null)
            return;

        int heartCount = Mathf.CeilToInt(Player.Instance.MaxHealth / 2f);

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
        int remaining = Player.Instance.Health;

        for (int i = hearts.Count - 1; i >= 0; i--)
        {
            Image img = hearts[i];

            if (remaining >= 2)
            {
                img.sprite = fullHeart;
                remaining -= 2;
            }
            else if (remaining == 1)
            {
                img.sprite = halfHeart;
                remaining -= 1;
            }
            else
            {
                img.sprite = emptyHeart;
            }
        }
    }
}
