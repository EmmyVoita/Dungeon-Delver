using System;
using DG.Tweening;
using UnityEngine;

public class BackgroundVisualManager : MonoBehaviour
{
    public static event Action OnFlareBottomRequested;

    [Header("Textures")]
    public Texture2D defaultBackground;
    public Texture2D worldMapBackground;
    public Texture2D gameOverBackground;

    [Header("Colors")]
    public Color defaultColor;
    public Color worldMapColor;
    public Color gameOverColor;

    public Material mat;
    public string texturePropertyName = "_MainTexture";
    public string colorPropertyName = "_Color";
    public string bottomGradientName = "_GradientEdge2";
    public float defaultGradientEdge = 0.23f;
    public float flaredGradientEdge = 0.3f;
    public float flareDuration = 0.5f;

    private void OnEnable()
    {
        SetTexture(defaultBackground);
        SetColor(defaultColor);
        
        GameStateManager.OnStateChanged += HandleStateChanged;
        OnFlareBottomRequested += HandleFlareBottom;
    }

    private void OnDisable()
    {
        GameStateManager.OnStateChanged -= HandleStateChanged;
        OnFlareBottomRequested -= HandleFlareBottom;
    }

    public static void FlareBottom()
    {
        OnFlareBottomRequested?.Invoke();
    }

    private void HandleFlareBottom()
    {
        mat.DOKill();

        float value = defaultGradientEdge;


        DOTween.To(
            () => value,
            x =>
            {
                value = x;
                mat.SetFloat(bottomGradientName, value);
            },
            flaredGradientEdge,
            flareDuration
        )
        .SetEase(Ease.OutQuad)
        .OnComplete(() =>
        {
            DOTween.To(
                () => value,
                x =>
                {
                    value = x;
                    mat.SetFloat(bottomGradientName, value);
                },
                defaultGradientEdge,
                flareDuration * 1.5f
            )
            .SetEase(Ease.OutBack);
        });
    }

    private void HandleStateChanged(GameState previousState, GameState newState)
    {
        switch(newState)
        {
            case GameState.WorldMapView:
                SetTexture(worldMapBackground);
                SetColor(worldMapColor);
                break;
            case GameState.WorldMapViewEnd:
                SetTexture(worldMapBackground);
                SetColor(worldMapColor);
                break;
            case GameState.GameOverTally:
                SetTexture(gameOverBackground);
                SetColor(gameOverColor);
                break;
            case GameState.GameOverResults:
                SetTexture(gameOverBackground);
                SetColor(gameOverColor);
                break;
            default:
                SetTexture(defaultBackground);
                SetColor(defaultColor);
                break;
        }
    }

    private void SetTexture(Texture2D texture)
    {
        mat.SetTexture(texturePropertyName, texture);
    }

    private void SetColor(Color color)
    {
        mat.SetColor(colorPropertyName, color);
    }

    private void OnDestroy()
    {
        SetTexture(defaultBackground);
        SetColor(defaultColor);
    }
}
