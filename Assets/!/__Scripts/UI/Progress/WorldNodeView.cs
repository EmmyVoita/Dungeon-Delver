using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class WorldNodeView : MonoBehaviour
{
    [SerializeField] private float randomZOffset = 15f;

    [Header("References")]
    [SerializeField] private Image nodeImage;
    [SerializeField] private GameObject bossIcon;

    [Header("Normal Icon")]
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite completedNormalSprite;
    [SerializeField] private Color normalColor = Color.white;

    [Header("Boss Icon")]
    [SerializeField] private Sprite bossSprite;
    [SerializeField] private Sprite completedBossSprite;
    [SerializeField] private Color bossColor = Color.white;

    [Header("Completed Pop")]
    [SerializeField] private bool animateCompletedNodes = true;
    [SerializeField] private float popStartScale = 0.65f;
    [SerializeField] private float popOvershootScale = 1.2f;
    [SerializeField] private float popUpDuration = 0.16f;
    [SerializeField] private float settleDuration = 0.12f;
    [SerializeField] private float delayPerNode = 0.04f;

    public int Index { get; private set; }

    private RectTransform _rectTransform;
    private Vector3 _normalScale;

    private void Awake()
    {
        _rectTransform = transform as RectTransform;
        _normalScale = transform.localScale;

        if (nodeImage == null)
            nodeImage = GetComponent<Image>();
    }

    public void Initialize(
        int index,
        bool isBoss,
        bool completed)
    {
        Index = index;

        transform.DOKill();

        if (nodeImage == null)
        {
            Debug.LogError(
                $"{nameof(WorldNodeView)} requires a UI Image.",
                this
            );

            return;
        }

        nodeImage.color = isBoss ? bossColor : normalColor;

        nodeImage.sprite = completed ? isBoss
                    ? completedBossSprite
                    : completedNormalSprite
                : isBoss
                    ? bossSprite
                    : normalSprite;

        float randomRotation =Random.Range(-randomZOffset,randomZOffset);

        nodeImage.rectTransform.localEulerAngles = new Vector3(0f,0f,randomRotation);

        if (bossIcon != null)
            bossIcon.SetActive(isBoss && !completed);

        if (completed && animateCompletedNodes)
        {
            PlayCompletedPop();
        }
        else
        {
            transform.localScale = _normalScale;
        }
    }

    private void PlayCompletedPop()
    {
        float delay =
            Index * delayPerNode;

        transform.localScale =
            _normalScale * popStartScale;

        Sequence sequence =
            DOTween.Sequence();

        sequence.SetUpdate(true);

        sequence.AppendInterval(delay);

        sequence.Append(
            transform
                .DOScale(
                    _normalScale *
                    popOvershootScale,
                    popUpDuration
                )
                .SetEase(Ease.OutBack)
        );

        sequence.Append(
            transform
                .DOScale(
                    _normalScale,
                    settleDuration
                )
                .SetEase(Ease.OutQuad)
        );
    }

    private void OnDisable()
    {
        transform.DOKill();
        transform.localScale = _normalScale;
    }
}