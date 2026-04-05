using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class RoundProgressUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image fillImage; // Image set to Filled
    private float lastTargetProgress = -1f;
    private Tween fillTween;

    [Header("Tween Settings")]
    [SerializeField] private float tweenDuration = 0.15f;
    [SerializeField] private Ease tweenEase = Ease.OutQuad;

    private void Update()
    {
        var rm = RoundManager.Instance;
        if (rm == null || rm.stats.Spawned == 0)
        {
            fillImage.fillAmount = 0f;
            return;
        }

        float targetProgress = RoundManager.Instance.stats.LevelProgress;

        if(Mathf.Approximately(targetProgress, lastTargetProgress))
            return;

        lastTargetProgress = targetProgress;

        fillTween?.Kill();
        fillTween = fillImage.DOFillAmount(targetProgress, tweenDuration).SetEase(tweenEase);
    }
}
