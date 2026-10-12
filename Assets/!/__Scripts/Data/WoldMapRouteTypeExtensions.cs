public static class WorldMapRouteTypeExtensions
{
    public static bool IsShop(this WorldMapRouteType route)
    {
        return route switch
        {
            WorldMapRouteType.SupportShop => true,
            WorldMapRouteType.AbilityShop => true,
            WorldMapRouteType.CurrencyShop => true,
            WorldMapRouteType.ChallengeShop => true,
            WorldMapRouteType.DefenseShop => true,
            WorldMapRouteType.GeneralShop => true,
            WorldMapRouteType.ShopUtility => true,

            _ => false
        };
    }
}