using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

public class ScreenDimmerManager : MonoBehaviour
{
    public static ScreenDimmerManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private SpriteRenderer dimSprite;
    [SerializeField] private SpriteRenderer fullDimSprite;
    [SerializeField] private float fadeDuration = 0.4f;
    [SerializeField, Range(0, 1f)] private float baseDimAlpha = 0.4f;

    private Tween fadeTween;
    private readonly HashSet<string> dimSources = new(); // e.g. "obstacle", "upgrade", "cutscene"

    // --------------------------------------------------
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void OnEnable()
    {
        // ✅ subscribe to obstacle events
        ObstacleManager.OnFirstObstacleAppeared += HandleObstacleDim;
        ObstacleManager.OnAllObstaclesCleared += HandleObstacleUndim;
    }

    void OnDisable()
    {
        ObstacleManager.OnFirstObstacleAppeared -= HandleObstacleDim;
        ObstacleManager.OnAllObstaclesCleared -= HandleObstacleUndim;
    }

    // --------------------------------------------------
    // 🔹 Dim-source system
    // --------------------------------------------------
    public void AddDimSource(string key, float? customAlpha = null)
    {
        Debug.Log($"Dim Manager Add: {key}");
        
        if (dimSources.Add(key))
        {
            float alpha = customAlpha ?? baseDimAlpha;
            UpdateDimState(alpha);
        }
    }

    public void RemoveDimSource(string key)
    {
        Debug.Log($"Dim Manager Remove: {key} \n" +
                  $"Dim Manager contains key ? => {dimSources.Contains(key)}"
        );

        if (!dimSources.Contains(key)) return;
        

        dimSources.Remove(key);

        if (dimSources.Count == 0)
            UpdateDimState(1f); // undim when no active sources
    }

    public bool HasDimSource(string key) => dimSources.Contains(key);
    public bool AnyDimActive => dimSources.Count > 0;

    // --------------------------------------------------
    // 🔹 Visual fading
    // --------------------------------------------------
    private void UpdateDimState(float targetAlpha)
    {
        fadeTween?.Kill();
        fadeTween = DOTween.To(GetAlpha, SetAlpha, targetAlpha, fadeDuration)
            .SetEase(Ease.InOutSine);
    }

    private float GetAlpha() => dimSprite.color.a;
    private void SetAlpha(float a)
    {
        Color c = dimSprite.color;
        c.a = a;
        dimSprite.color = c;
    }

    // --------------------------------------------------
    // 🔹 Full-screen cinematic dim
    // --------------------------------------------------
    public void FadeFullScreen(float targetAlpha, float? customDuration = null)
    {
        float duration = customDuration ?? fadeDuration;
        fadeTween?.Kill();
        fadeTween = DOTween.To(() => fullDimSprite.color.a, a =>
        {
            Color c = fullDimSprite.color;
            c.a = a;
            fullDimSprite.color = c;
        }, targetAlpha, duration).SetEase(Ease.InOutSine);
    }

    // --------------------------------------------------
    // 🔹 Obstacle event handlers
    // --------------------------------------------------
    private void HandleObstacleDim()
    {
        if(ObstacleManager.Instance == null) return;
        AddDimSource("obstacle");
    }

    private void HandleObstacleUndim()
    {
        if(ObstacleManager.Instance == null) return;
        RemoveDimSource("obstacle");
    }
}
