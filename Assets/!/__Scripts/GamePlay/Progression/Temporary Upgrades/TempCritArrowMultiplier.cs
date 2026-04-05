public class TempCritArrowMultiplier : ICritHitValueModifier
{
    private float amount;

    public TempCritArrowMultiplier(float amount)
    {
        this.amount = amount;
    }

    public float ModifyCritHitValue(float baseScore)
    {
        return baseScore * amount;
    }
}
