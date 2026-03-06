using UnityEngine;
using DG.Tweening;
using TMPro;

public class BounceOnConfirm : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textElement;

    [Header("Position Bounce")]
    [SerializeField] private float bounceHeight = 28f;
    [SerializeField] private float moveDuration = 0.12f;

    [Header("Scale Punch")]
    [SerializeField] private float scaleAmount = 0.15f;

    [Header("Rotation Jiggle")]
    [SerializeField] private float rotationAmount = 8f;

    private RectTransform rect;
    private Vector2 originalPos;

    private Sequence bounceSequence;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        originalPos = rect.anchoredPosition;

        string key = InputBindingManager.Instance
            .GetKey(InputActionType.Confirm)
            .ToString();

        textElement.text = $"[<color=yellow>{key}</color>]!";

        BuildSequence();
    }

    private void BuildSequence()
    {
        bounceSequence = DOTween.Sequence();

        // --- POSITION ---
        bounceSequence.Append(
            rect.DOAnchorPosY(originalPos.y + bounceHeight, moveDuration)
                .SetEase(Ease.OutCubic)
        );

        bounceSequence.Append(
            rect.DOAnchorPosY(originalPos.y, moveDuration)
                .SetEase(Ease.InCubic)
        );

        // --- SCALE PUNCH ---
        bounceSequence.Join(
            rect.DOPunchScale(
                new Vector3(scaleAmount, -scaleAmount * 0.6f, 0f),
                moveDuration * 2f,
                6,
                0.7f
            )
        );

        // --- ROTATION PUNCH ---
        bounceSequence.Join(
            rect.DOPunchRotation(
                new Vector3(0f, 0f, rotationAmount),
                moveDuration * 2f,
                6,
                0.6f
            )
        );

        bounceSequence.SetAutoKill(false);
        bounceSequence.Pause();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            TriggerBounce();
        }
    }

    public void TriggerBounce()
    {
        // Clean restart without snapping
        bounceSequence.Restart(true);
    }
}