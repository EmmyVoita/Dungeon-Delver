using DG.Tweening;
using UnityEngine;

public class UIPanelNudge : MonoBehaviour
{
    [SerializeField] private RectTransform target;

    [Header("Movement")]
    [SerializeField] private float moveDistance = 6f;
    [SerializeField] private float moveDuration = 0.08f;

    [Header("Rotation")]
    [SerializeField] private float rotationAmount = 1.5f;
    [SerializeField] private float returnDuration = 0.12f;

    private Vector2 _startPos;
    private Quaternion _startRot;
    private Sequence _sequence;

    private void Awake()
    {
        if (target == null)
            target = GetComponent<RectTransform>();

        _startPos = target.anchoredPosition;
        _startRot = target.localRotation;
    }

    public void NudgeLeft()
    {
        Nudge(Vector2.left, rotationAmount);
    }

    public void NudgeRight()
    {
        Nudge(Vector2.right, -rotationAmount);
    }

    private void Nudge(Vector2 direction, float angle)
    {
        _sequence?.Kill();

        target.anchoredPosition = _startPos;
        target.localRotation = _startRot;

        _sequence = DOTween.Sequence();

        _sequence.Append(
            target.DOAnchorPos(_startPos + direction * moveDistance, moveDuration)
                  .SetEase(Ease.OutQuad));

        _sequence.Join(
            target.DOLocalRotate(new Vector3(0, 0, angle), moveDuration)
                  .SetEase(Ease.OutQuad));

        _sequence.Append(
            target.DOAnchorPos(_startPos, returnDuration)
                  .SetEase(Ease.OutBack));

        _sequence.Join(
            target.DOLocalRotate(Vector3.zero, returnDuration)
                  .SetEase(Ease.OutBack));
    }
}