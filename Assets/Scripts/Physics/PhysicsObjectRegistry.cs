using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class PhysicsObjectRegistry
{
    public static List<PhysicsObject> All = new();
    public static HashSet<PhysicsObject> Registered = new();
    public static int Register(PhysicsObject obj)
    {
        if (Registered.Contains(obj)) return All.IndexOf(obj);
        All.Add(obj);
        Registered.Add(obj);
        return All.Count-1;
    }

    
    public static void Reset()
    {
        Debug.Log("resetting");
        All.Clear();
        Registered.Clear();
    }
}
