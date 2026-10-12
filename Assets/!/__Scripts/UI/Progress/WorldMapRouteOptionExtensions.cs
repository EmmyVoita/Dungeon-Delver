using UnityEngine;

public static class WorldMapRouteOptionExtensions
{
    public static bool TryGetUpgradeCategory(this WorldMapRouteOption routeOption, out UpgradeCategory upgradeCategory)
    {
        switch(routeOption.type)
        {
            case WorldMapRouteType.SupportShop:
                upgradeCategory = UpgradeCategory.Shop;
                return true;
            case WorldMapRouteType.AbilityShop:
                upgradeCategory = UpgradeCategory.Ability;
                return true;
            case WorldMapRouteType.CurrencyShop:
                upgradeCategory = UpgradeCategory.Currency;
                return true;
            case WorldMapRouteType.ChallengeShop:
                upgradeCategory = UpgradeCategory.Challenge;
                return true;
            case WorldMapRouteType.DefenseShop:
                upgradeCategory = UpgradeCategory.Defensive;
                return true;
            case WorldMapRouteType.GeneralShop:
                upgradeCategory = UpgradeCategory.General;
                return true;
            case WorldMapRouteType.ShopUtility:
                upgradeCategory = UpgradeCategory.Shop;
                return true;
            default:
                upgradeCategory = default;
                return false;
        }
    }
}