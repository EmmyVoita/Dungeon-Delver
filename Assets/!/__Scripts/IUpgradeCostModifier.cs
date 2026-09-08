public interface IUpgradeCostModifier : IRuntimeModifier
{
    int ModifyCost(UpgradeBase upgrade, int currentCost);
}
