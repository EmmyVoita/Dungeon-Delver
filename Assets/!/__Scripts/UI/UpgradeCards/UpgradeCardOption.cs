using UnityEngine;

public class UpgradeCardOption : ICardOption
{
    private UpgradeCard card;
    private UpgradeBase upgrade;

    public UpgradeCardOption(UpgradeCard card)
    {
        this.card = card;
        this.upgrade = card.upgrade as UpgradeBase;
    }

    public UpgradeBase UpgradeData => upgrade;
    public string DisplayName => upgrade.displayName;
    public string Description => upgrade.GetDescription();

    public void OnSelected()
    {
        ShopManager.Instance.MarkCardSelected(upgrade);
    }
}
