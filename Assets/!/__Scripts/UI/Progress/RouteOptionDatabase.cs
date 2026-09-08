
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Route Option Database")]
public class RouteOptionDatabase : ScriptableObject
{
    public List<WorldMapRouteOption> routeOptions;
}