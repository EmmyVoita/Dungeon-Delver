using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class WorldMapRouteMarker : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private RectTransform root;

    [Header("Animation")]
    [SerializeField] private float startScale = 0.4f;
    [SerializeField] private float iconOutDuration = 0.08f;
    [SerializeField] private float iconInDuration = 0.2f;

    private Tween _scaleTween;
    private Sequence _iconSequence;


    private void OnDestroy()
    {
        _scaleTween?.Kill();
    }

    public void SetIcon(Sprite sprite)
    {
        if (icon == null)
            return;

        icon.sprite = sprite;
        icon.SetNativeSize();
        AnimateIcon(sprite);
    }


    private void AnimateIcon(Sprite newSprite)
    {
        if (icon == null || newSprite == null)
            return;

        _iconSequence?.Kill();

        _iconSequence = DOTween.Sequence()
            .SetUpdate(true);

        // Pop old icon out
        _iconSequence.Append(
            root.transform
                .DOScale(0f, iconOutDuration)
                .SetEase(Ease.InBack)
        );

        // Change the sprite while hidden
        _iconSequence.AppendCallback(() =>
        {
            icon.sprite = newSprite;

            root.transform.localScale = Vector3.one * startScale;

            icon.SetNativeSize(); // optional
        });

        // Pop new icon in
        _iconSequence.Append(
            root.transform
                .DOScale(1f, iconInDuration)
                .SetEase(Ease.OutBack)
        );
    }
}