using UnityEngine;

public class GoldenArrowSession : IArrowEffect
{
    public readonly string sourceId;

    private int arrowsRemaining;
    private int extensionRemaining;

    public bool IsExpired => arrowsRemaining <= 0;

    public GoldenArrowSession(
        string sourceId,
        int initialArrows,
        int maxExtensions)
    {
        this.sourceId = sourceId;
        arrowsRemaining = initialArrows;
        extensionRemaining = maxExtensions;
    }

    public bool TryConsumeArrow(ArrowBase arrow)
    {
        if (arrowsRemaining <= 0)
            return false;

        if (!arrow.IsGolden)
        {
            arrow.SetGolden();
            arrowsRemaining--;
            return true;
        }

        return false;
    }

    public bool TryExtend(int amount)
    {
        if (extensionRemaining <= 0)
            return false;

        int applied = Mathf.Min(amount, extensionRemaining);
        arrowsRemaining += applied;
        extensionRemaining -= applied;

        return applied > 0;
    }

    public void InvalidateExtensions()
    {
        extensionRemaining = 0;
    }

    public void ApplyToArrow(ArrowBase arrow)
    {
        TryConsumeArrow(arrow);
    }
}
