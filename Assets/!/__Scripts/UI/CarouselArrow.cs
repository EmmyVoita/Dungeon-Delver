using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class CarouselArrow : MonoBehaviour, IPointerClickHandler
{
    public enum CarosuelDirection
    {
        Left,
        Right
    }

    public static event Action<CarosuelDirection> OnCarouselArrowClicked;
    
    [Header("References")]
    [SerializeField] private PlayMenuNavigator selectManager;

    [Header("Input")]
    [SerializeField] private MenuState activeState;
    [SerializeField] private InputActionType inputAction;

    [Header("Setup")]
    [SerializeField] private RectTransform idleRect;
    [SerializeField] private RectTransform movementRect;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Direction")]
    [SerializeField] private CarosuelDirection carosuelDirection;
    [SerializeField] private int direction = 1; // -1 = left, 1 = right

    [Header("Animation")]
    [SerializeField] private float idleDistance = 8f;
    [SerializeField] private float idleDuration = 0.6f;
    [SerializeField] private float disabledAlpha = 0.2f;

    private float baseX;
    private Tween idleTween;
    private bool isEnabled = true;
    private Vector3 baseScale;
    private Vector2 basePos;

    private void Awake()
    {
        if (idleRect == null) idleRect = GetComponent<RectTransform>();
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();

        baseX = movementRect.anchoredPosition.x;

        baseScale = movementRect.localScale;
        basePos = movementRect.anchoredPosition;
    }

    private void OnEnable()
    {
    }

    private void OnDisable()
    {
        KillTweens();
    }



    private void Update()
    {
        if(isEnabled && InputBindingManager.Instance.GetKeyHeld(inputAction) && !selectManager.IsInputLocked)
        {
            PlayPressFeedback();
        }
    }

    // ----------------------------
    // Idle Animation
    // ----------------------------
    private void StartIdle()
    {
        idleRect.DOKill();

        idleTween = idleRect.DOAnchorPosX(baseX + direction * idleDistance, idleDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }

    private void StopIdle()
    {
        if (idleTween != null)
            idleTween.Kill();

        idleRect.anchoredPosition = new Vector2(baseX, idleRect.anchoredPosition.y);
    }

    private void KillTweens()
    {
        idleRect.DOKill();
        if (canvasGroup != null) canvasGroup.DOKill();
    }

    // ----------------------------
    // Public API
    // ----------------------------

    public void SetEnabled(bool enabled, bool force = false)
    {
        if (!force && isEnabled == enabled) return;

        isEnabled = enabled;

        if (enabled)
        {
            canvasGroup.DOKill(); // kill only fade
            canvasGroup.DOFade(1f, 0.2f);

            StartIdle();
        }
        else
        {
            canvasGroup.DOKill();
            canvasGroup.DOFade(disabledAlpha, 0.2f);

            StopIdle();
        }
    }


   public void PlayPressFeedback()
    {
        if (!isEnabled) return;

        movementRect.DOKill();

        // 💥 CRITICAL: reset baseline every time
        movementRect.anchoredPosition = basePos;
        movementRect.localScale = baseScale;

        movementRect.DOPunchScale(baseScale * 0.2f, 0.2f, 10, 1);

        movementRect.DOPunchAnchorPos(
            new Vector2(direction * 10f, 0),
            0.2f,
            10,
            1
        );
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("OnPointerClick");
        
        if (InputModeManager.Instance.CurrentMode
            != InputModeManager.InputMode.Mouse)
            return;

        OnCarouselArrowClicked?.Invoke(carosuelDirection);

        PlayPressFeedback();
    }
}