using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[ExecuteAlways]
[DefaultExecutionOrder(-10000)]

public class PhysicsObjectRegistry : MonoBehaviour
{
    [HideInInspector, System.NonSerialized]
    public List<PhysicsObject> All = new();
    [HideInInspector, System.NonSerialized]
    public HashSet<PhysicsObject> Registered = new();
    public void Awake()
    {
        Reset();
    }
    public int Register(PhysicsObject obj)
    {
        if (Registered.Contains(obj)) return All.IndexOf(obj);
        All.Add(obj);
        Registered.Add(obj);
        return All.Count-1;
    }

    
    public void Reset()
    {
        Debug.Log("resetting");
        All.Clear();
        Registered.Clear();
    }
}
