using DG.Tweening;
using TMPro;
using UnityEngine;

public class ComboRecoveryGhost : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float fadeDuration;
    [SerializeField] private float fadeInDelay = 0.0f;
    [SerializeField] private GameObject successEffect;
    [SerializeField] private GameObject failEffect;
    [SerializeField] private RectTransform comboGhostRectTransform;
    [SerializeField] private Camera uiCamera;

  

    private void OnEnable()
    {
        RecoveryArrowManager.OnCreateRecoveryArrow += HandleCreateRecoveryArrow;
        RecoveryArrowManager.OnRecoveryArrowResolved += HandleRecoveryArrowResolved;
    }

    private void OnDisable()
    {
        RecoveryArrowManager.OnCreateRecoveryArrow -= HandleCreateRecoveryArrow;
        RecoveryArrowManager.OnRecoveryArrowResolved -= HandleRecoveryArrowResolved;
    }

    private void Awake()
    {
        canvasGroup.alpha = 0;
    }


    private void HandleRecoveryArrowResolved(bool caught)
    {
        FadeOut();

        RectTransform rectT = comboGhostRectTransform;

        Vector3 screenPos = RectTransformUtility.WorldToScreenPoint(
            uiCamera,
            rectT.position
        );

        Vector3 worldPos = uiCamera.ScreenToWorldPoint(
            new Vector3(
                screenPos.x,
                screenPos.y,
                0
            )
        );

        if(caught)
        {
            Instantiate(successEffect,new Vector3(worldPos.x,worldPos.y,0), Quaternion.identity);
        }
        else
        {
             Instantiate(failEffect,new Vector3(worldPos.x,worldPos.y,0), Quaternion.identity);
        }
    }

    private void HandleCreateRecoveryArrow(int amount, int comboCount)
    {
        Initialize(comboCount);
    }

    public void Initialize(int comboValue)
    {
        text.text = $"x{comboValue}";
        FadeIn();
    }

    private void FadeIn()
    {
        canvasGroup.DOKill();

        canvasGroup.DOFade(1f, fadeDuration)
            .SetDelay(fadeInDelay);
    }

    private void FadeOut()
    {
        canvasGroup.DOKill();

        canvasGroup.DOFade(0f, fadeDuration);
    }
}