public class TempArrowScoreMultiplier : IArrowScoreModifier
{
    private float amount;

    public TempArrowScoreMultiplier(float amount)
    {
        this.amount = amount;
    }

    public float ModifyArrowScore(float baseScore)
    {
        return baseScore * amount;
    }
}
