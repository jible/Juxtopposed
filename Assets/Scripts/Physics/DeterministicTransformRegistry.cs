using System.Collections.Generic;

// Every enabled deterministic transform, so the tick manager can push positions to unity transforms after ticking.
// Static is safe here since syncing is display only, not simulation state
public static class DeterministicTransformRegistry
{
    // Sync order does not matter, and a set ignores double registration
    private static readonly HashSet<DeterministicTransform> transforms = new();

    public static void Add(DeterministicTransform dt) => transforms.Add(dt);
    public static void Remove(DeterministicTransform dt) => transforms.Remove(dt);

    public static void SyncAll()
    {
        foreach (var dt in transforms)
        {
            dt.UpdateNormalTransformPosition();
        }
    }
}
