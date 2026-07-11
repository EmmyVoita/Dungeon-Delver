using System;
using UnityEngine;
using UnityEngine.UI;

public class FullAbilityBarBonusIndicator : MonoBehaviour
{

    public static event Action OnStartAnimatOutline;
    public static event Action OnStopAnimatOutline;

    [Header("UI")]
    [SerializeField] private Image indicatorImage;

    private void OnEnable()
    {
        OnStartAnimatOutline += StartAnimation;
        OnStopAnimatOutline += StopAnimation;
    }

    private void OnDisable()
    {
        OnStartAnimatOutline -= StartAnimation;
        OnStopAnimatOutline -= StopAnimation;
    }

    private void Awake()
    {
        indicatorImage.enabled = false;
    }

    public static void RequestStartAnimateOutline()
    {
        OnStartAnimatOutline?.Invoke();
    }

    public static void RequestStopAnimateOutline()
    {
        OnStopAnimatOutline?.Invoke();
    }

    private void StartAnimation()
    {
        indicatorImage.enabled = true;
    }

    private void StopAnimation()
    {
        indicatorImage.enabled = false;
    }
}
