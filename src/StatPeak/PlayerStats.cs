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

    public static double Get(string name)
    {
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
