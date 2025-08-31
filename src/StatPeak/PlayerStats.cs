using System.Collections.Generic;

namespace StatPeak;

internal static class PlayerStats
{
    private readonly static Dictionary<string, double> stats = new(2);
        
    public static void Increment(string name, double amount = 1)
    {
        if (!stats.TryGetValue(name, out double current))
        {
            Set(name, amount);
            return;
        }

        stats[name] = current + amount;
    }

    public static void Set(string name, double value)
    {
        if (!stats.ContainsKey(name))
        {
            stats.Add(name, value);
            return;
        }

        stats[name] = value;
    }

    // If there is data stored at key {name}, then will return that data, otherwise will return a default value of double
    public static double Get(string name)
    {
        if (!stats.ContainsKey(name))
        {
            return default;
        }

        return stats[name];
    }

    public static void Reset()
    {
        stats.Clear();
    }

    public static Dictionary<string, double> GetAll()
    {
        return stats;
    }
}
