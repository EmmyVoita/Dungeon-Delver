using System.Collections.Generic;
using UnityEngine;

public static class ListExtensions
{
    private static System.Random rng = new System.Random();

    public static void Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            (list[n], list[k]) = (list[k], list[n]);
        }
    }

    public static List<T> PickUnique<T>(this List<T> source, int count)
    {
        List<T> result = new();

        if (source == null || source.Count == 0)
            return result;

        if (source.Count <= count)
        {
            result.AddRange(source);
            return result;
        }

        List<T> pool = new(source);

        for (int i = 0; i < count; i++)
        {
            int index = Random.Range(0, pool.Count);
            result.Add(pool[index]);
            pool.RemoveAt(index);
        }

        return result;
    }

    public static List<T> PickUnique<T>(this List<T> source, int count, List<T> avoidList)
    {
        List<T> result = new();

        if (source == null || source.Count == 0)
            return result;

        if (source.Count <= count)
        {
            result.AddRange(source);
            return result;
        }

        List<T> pool = new();

        foreach(var item in source)
        {
            if(!avoidList.Contains(item))
                pool.Add(item);
        }

        for (int i = 0; i < count; i++)
        {
            int index = Random.Range(0, pool.Count);
            result.Add(pool[index]);
            pool.RemoveAt(index);
        }

        return result;
    }

    public static T GetRandom<T>(this IList<T> list)
    {
        if (list == null || list.Count == 0)
        {
            Debug.LogWarning("Tried to get random element from empty list.");
            return default;
        }

        return list[Random.Range(0, list.Count)];
    }
}
