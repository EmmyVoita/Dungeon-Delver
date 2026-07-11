using System.Collections.Generic;

public static class LaneReservationManager
{
    private static HashSet<int> reservedLanes = new();

    public static void ReserveLane(int lane)
    {
        reservedLanes.Add(lane);
    }

    public static void ReleaseLane(int lane)
    {
        reservedLanes.Remove(lane);
    }

    public static bool IsReserved(int lane)
    {
        return reservedLanes.Contains(lane);
    }

    public static List<int> GetAvailableLanes(int maxLanes)
    {
        List<int> available = new();

        for(int i = 0; i < maxLanes; i++)
        {
            if(!reservedLanes.Contains(i))
                available.Add(i);
        }

        return available;
    }

    public static void Clear()
    {
        reservedLanes.Clear();
    }
}