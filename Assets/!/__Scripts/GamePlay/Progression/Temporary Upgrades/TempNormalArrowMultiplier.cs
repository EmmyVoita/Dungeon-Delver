public class TempNormalArrowMultiplier : INormalHitValueModifier
{
    private float amount;

    public TempNormalArrowMultiplier(float amount)
    {
        this.amount = amount;
    }

    public float ModifyNormalHitValue(float baseScore)
    {
        return baseScore * amount;
    }
}
