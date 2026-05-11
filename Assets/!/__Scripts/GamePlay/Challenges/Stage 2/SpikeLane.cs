using UnityEngine;
using DG.Tweening;

public class SpikeLane : MonoBehaviour
{
    public int LaneIndex { get; private set; }

    [Header("References")]
    [SerializeField] private Transform spikeVisual;
    [SerializeField] private SpriteRenderer sRend;

    [Header("Positions")]
    [SerializeField] private float idleOffset = 1.5f;
    [SerializeField] private float windupOffset = 2.2f;
    [SerializeField] private float attackOffset = 0.2f;

    [Header("Timing")]
    [SerializeField] private float windupDuration = 0.4f;
    [SerializeField] private float attackDuration = 0.1f;
    [SerializeField] private float extendDuration = 0.5f;
    [SerializeField] private float returnDuration = 0.2f;

    private Sequence spikeSequence;
    private int direction = 1; // 1 = right side, -1 = left side

    public void SetLaneIndex(int index)
    {
        LaneIndex = index;
    }

    public void SetDirection(int dir)
    {
        direction = dir;

        Vector3 scale = sRend.transform.localScale;
        sRend.transform.localScale = dir == 1 ? new Vector3(scale.x * -1,scale.y,scale.z) : scale;
    }

    // -------------------------------------
    // TELEGRAPH (WINDUP)
    // -------------------------------------
    public void SetTelegraph(bool isOpen, bool isSpecial)
    {
        if (isOpen)
        {
            // stay idle if safe lane
            MoveToIdle();
            return;
        }

        sRend.DOColor(Color.white,0.3f);

        spikeSequence?.Kill();

        spikeSequence = DOTween.Sequence();

        // Pull back (windup)
        spikeSequence.Append(
            spikeVisual.DOLocalMoveX(direction * windupOffset, windupDuration)
                       .SetEase(Ease.OutQuad)
        );
    }

    // -------------------------------------
    // ATTACK (SHOOT OUT)
    // -------------------------------------
    public void SetActiveState(bool spikeOut)
    {
        if (!spikeOut) return;

        sRend.DOColor(Color.white,0.3f);

        spikeSequence?.Kill();

        spikeSequence = DOTween.Sequence();
        

        // Snap forward fast
        spikeSequence.Append(
            spikeVisual.DOLocalMoveX(direction * attackOffset, attackDuration)
                       .SetEase(Ease.OutQuad)
        );

        spikeSequence.AppendInterval(extendDuration);

        // Return to idle
        spikeSequence.Append(
            spikeVisual.DOLocalMoveX(direction * idleOffset, returnDuration)
                       .SetEase(Ease.InQuad)
        );
    }

    // -------------------------------------
    // RESET
    // -------------------------------------
    public void ResetState()
    {
        spikeSequence?.Kill();
        MoveToIdle();
    }

    void MoveToIdle()
    {
        sRend.DOColor(Color.clear,0.3f);
        spikeVisual.DOLocalMoveX(direction * idleOffset, 0.15f);
    }
}