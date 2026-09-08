public class RerollHealModifier : IShopRerollEffect
{
    private readonly float healChance;
    private readonly int healAmount;

    public int Priority => 0;

    public RerollHealModifier(float healChance, int healAmount = 1)
    {
        this.healChance = healChance;
        this.healAmount = healAmount;
    }

    public void OnActivate()
    {
        //ShopManager.Instance.AddRerollEffect(this);
    }

    public void OnDestroy()
    {
        //ShopManager.Instance.RemoveRerollEffect(this);
    }

    public void OnShopRerolled()
    {
        if (UnityEngine.Random.value > healChance)
            return;

        Player.Instance.HealPlayer(healAmount);
    }

    public IRuntimeModifier Clone()
    {
        return new RerollHealModifier(
            healChance,
            healAmount
        );
    }
}