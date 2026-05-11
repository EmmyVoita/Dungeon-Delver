using UnityEngine;
using DG.Tweening;

public class LaneMover : MonoBehaviour, IReversible
{
    [SerializeField] private float speed = 3f;
    [SerializeField] private float reverseSpeedMult = 1.5f;

    private int direction = 1;
    private Rigidbody2D rb;

    private float speedMultiplier = 1f;
    private Tween currentTween;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Initialize(int dir)
    {
        direction = dir;
        ApplyVelocity();
    }

    void Update()
    {
        ApplyVelocity();
    }

    private void ApplyVelocity()
    {
        if (rb == null) return;

        rb.linearVelocity = new Vector2(speed * direction * speedMultiplier, 0);
    }

    public void Reverse()
    {
        if (rb == null) return;

        // kill any existing tween
        currentTween?.Kill();

        currentTween = DOTween.Sequence()
            // 🔹 Slow down to 0
            .Append(DOTween.To(
                () => speedMultiplier,
                x => speedMultiplier = x,
                0f,
                0.2f
            ).SetEase(Ease.OutQuad))

            // 🔹 Flip direction
            .AppendCallback(() =>
            {
                direction *= -1;
            })

            // 🔹 Speed back up
            .Append(DOTween.To(
                () => speedMultiplier,
                x => speedMultiplier = x,
                reverseSpeedMult,
                3
            ).SetEase(Ease.InQuad));
    }
}