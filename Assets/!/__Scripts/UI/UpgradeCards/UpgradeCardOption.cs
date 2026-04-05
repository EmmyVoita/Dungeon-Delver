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
    public Sprite Icon => upgrade.baseIcon;
    public string DisplayName => upgrade.displayName;
    public string Description => upgrade.GetDescription();

    public void OnSelected()
    {
        UpgradeManager.Instance.AddUpgrade(upgrade);

        if (UpgradeCardManager.Instance.upgradeIconPrefab == null ||
            UpgradeCardManager.Instance.upgradeIconParent == null)
        {
            Debug.LogError("Upgrade icon prefab or parent missing!");
            return;
        }

        GameObject iconObj = GameObject.Instantiate(
            UpgradeCardManager.Instance.upgradeIconPrefab,
            UpgradeCardManager.Instance.upgradeIconParent
        );

        iconObj.GetComponent<UpgradeIconUI>().Initialize(upgrade);


        UpgradeCardManager.Instance.MarkCardSelected(card);
    }
}
