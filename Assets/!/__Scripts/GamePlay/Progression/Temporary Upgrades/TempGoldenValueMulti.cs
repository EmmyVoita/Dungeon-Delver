public class TempGoldenValueMulti : IArrowStatusScoreModifier
{
    private float goldenBonus;

    public TempGoldenValueMulti(float goldenBonus)
    {
        this.goldenBonus = goldenBonus;
    }

     public float ModifyStatusMultiplier(
        ArrowStatus status,
        float currentMultiplier)
    {
        if (!status.HasFlag(ArrowStatus.Golden))
            return currentMultiplier;

        return currentMultiplier * (1f + goldenBonus);
    }
}
