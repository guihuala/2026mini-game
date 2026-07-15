using System.Collections.Generic;

public static class ExplorationControlLock
{
    private static readonly HashSet<object> Owners = new HashSet<object>();
    public static bool IsLocked => Owners.Count > 0;
    public static void Acquire(object owner) { if (owner != null) Owners.Add(owner); }
    public static void Release(object owner) { if (owner != null) Owners.Remove(owner); }
    public static void Reset() => Owners.Clear();
}
