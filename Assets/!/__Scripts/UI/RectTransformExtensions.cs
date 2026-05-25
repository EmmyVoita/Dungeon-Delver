using DG.Tweening;
using UnityEngine;

public static class RectTransformExtensions
{
    public static Sequence PlayJumpDip(
        this RectTransform rect,
        float baseY,
        float dipAmount = 15f,
        float dipDuration = 0.15f,
        float returnDuration = 0.25f
    )
    {
        rect.DOKill();

        Sequence seq = DOTween.Sequence();

        seq.Append(
            rect.DOAnchorPosY(baseY - dipAmount, dipDuration)
                .SetEase(Ease.OutQuad)
        );

        seq.Append(
            rect.DOAnchorPosY(baseY, returnDuration)
                .SetEase(Ease.OutBack)
        );

        return seq;
    }
}