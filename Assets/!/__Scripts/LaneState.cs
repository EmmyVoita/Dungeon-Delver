using System.Collections.Generic;

public static class LaneState
{
    public static int MaxLanes { get; private set; }
    public static float LaneSpacing { get; private set; }

    private static HashSet<int> collapsedLanes = new();

    public static bool IsLaneCollapsed(int lane)
    {
        return collapsedLanes.Contains(lane);
    }

    public static void CollapseLane(int lane)
    {
        collapsedLanes.Add(lane);
    }

    public static void RestoreLane(int lane)
    {
        collapsedLanes.Remove(lane);
    }

    public static void Clear()
    {
        MaxLanes = 0;
        LaneSpacing = 0;
        collapsedLanes.Clear();
    }

    public static void Set(int maxLanes,float spacing)
    {
        MaxLanes=maxLanes;
        LaneSpacing=spacing;
    }
}