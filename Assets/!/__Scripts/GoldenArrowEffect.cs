using UnityEngine;

public class GoldenArrowEffect : IArrowEffect
{
    private int arrowsRemaining;

    public bool IsExpired => arrowsRemaining <= 0;
    public int ArrowsRemaining => arrowsRemaining;

    public GoldenArrowEffect(int initial)
    {
        arrowsRemaining = Mathf.Max(0, initial);
    }

    public void AddArrows(int amount)
    {
        if (amount <= 0)
            return;

        arrowsRemaining += amount;
    }

    public void ApplyToArrow(ArrowBase arrow)
    {
        if (arrowsRemaining <= 0)
            return;

        if (!arrow.IsGolden)
        {
            arrow.SetGolden();
            arrowsRemaining--;
        }
    }
}
