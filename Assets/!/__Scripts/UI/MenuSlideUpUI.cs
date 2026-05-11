using UnityEngine;
using DG.Tweening;

public class MenuSlideUpUI : MonoBehaviour
{
    [Header("State")]
    [SerializeField] private MenuState targetState;

    [Header("Animation")]
    [SerializeField] private float hiddenY = -300f; // offscreen position
    [SerializeField] private float shownY = 0f;     // visible position
    [SerializeField] private float duration = 0.4f;
    [SerializeField] private Ease ease = Ease.OutBack;

    [SerializeField] private RectTransform rect;


    private void Awake()
    {
        //rect = GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        MenuManager.OnMenuOpened += HandleMenuOpened;

        // Initialize instantly to correct state
        HandleMenuOpened(MenuManager.Instance.CurrentState, true);
    }

    private void OnDisable()
    {
        MenuManager.OnMenuOpened -= HandleMenuOpened;
        rect.DOKill();
    }

    private void HandleMenuOpened(MenuState newState)
    {
        HandleMenuOpened(newState, false);
    }

    private void HandleMenuOpened(MenuState newState, bool instant)
    {
        rect.DOKill();

        if (newState == targetState)
        {

            // Move to visible
            if (instant)
            {
                rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, shownY);
            }
            else
            {
                rect.DOAnchorPosY(shownY, duration)
                    .SetEase(ease);
            }
        }
        else
        {
            // Reset back down
            rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, hiddenY);
        }
    }
}