public class GoldenArrowAbilityChargeModifier :IRuntimeModifier,IGoldenArrowCatchEffect
{
    private int chargeAmount;

    public int Priority => 0;

    public GoldenArrowAbilityChargeModifier(int chargeAmount)
    {
        this.chargeAmount = chargeAmount;
    }

    public void OnActivate()
    {
        GoldenArrowManager.Instance.AddCatchEffect(this);
    }

    public void Destroy()
    {
        GoldenArrowManager.Instance.RemoveCatchEffect(this);
    }

    public void OnGoldenArrowCaught(ArrowResolvedData data)
    {
        Player.Instance.AbilityCharge += chargeAmount;
    }

    public IRuntimeModifier Clone()
    {
        return new GoldenArrowAbilityChargeModifier(chargeAmount);
    }
}