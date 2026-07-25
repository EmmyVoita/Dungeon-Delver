using DG.Tweening;
using UnityEngine;

public class FloatingRewardIcon : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Movement")]
    [SerializeField] private float riseDistance = 1.5f;
    [SerializeField] private float riseDuration = 0.8f;
    [SerializeField] private float horizontalDrift = 0.2f;
    [SerializeField] private float endZ = 5f;

    [Header("Scale")]
    [SerializeField] private float startScale = 0.2f;
    [SerializeField] private float targetScale = 1f;
    [SerializeField] private float scaleDuration = 0.35f;

    [Header("Fade")]
    [SerializeField] private float holdDuration = 0.35f;
    [SerializeField] private float fadeDuration = 0.3f;

    [Header("Variation")]
    [SerializeField] private float rotationRange = 12f;

    public void Play(RewardDefinition data = null)
    {   
        if(data != null)
            spriteRenderer.sprite = data.Icon;

        transform.position = new Vector3(transform.position.x,
                                            transform.position.y,
                                            5f);

        transform.localScale = Vector3.one * startScale;

        float drift = Random.Range(-horizontalDrift, horizontalDrift);
        float rotation = Random.Range(-rotationRange, rotationRange);

        transform.rotation = Quaternion.Euler(0f, 0f, rotation);

        Vector3 endPosition =
            transform.position +
            Vector3.up * riseDistance +
            Vector3.right * drift;

        endPosition.z = endZ;

        Sequence sequence = DOTween.Sequence();

        sequence.Join(
            transform.DOMove(endPosition, riseDuration)
                .SetEase(Ease.OutQuad)
        );

        sequence.Join(
            transform.DOScale(targetScale, scaleDuration)
                .SetEase(Ease.OutBack)
        );

        sequence.AppendInterval(holdDuration);

        sequence.Append(
            spriteRenderer.DOFade(0f, fadeDuration)
        );

        sequence.Join(
            transform.DOMoveY(
                endPosition.y + 0.25f,
                fadeDuration
            )
        );

        sequence.OnComplete(() =>
        {
            Destroy(gameObject);
        });
    }

    private void OnDestroy()
    {
        transform.DOKill();

        if (spriteRenderer != null)
            spriteRenderer.DOKill();
    }
}