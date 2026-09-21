using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[ExecuteAlways]
[DefaultExecutionOrder(-10000)]

public class PhysicsObjectRegistry : MonoBehaviour
{
    [HideInInspector, System.NonSerialized]
    public List<PhysicsObject> All = new();
    // Trigger boxes, iterated by the trigger pass
    [HideInInspector, System.NonSerialized]
    public List<PhysicsObject> Triggers = new();
    // Collision objects that get moved and pushed out of static ones
    [HideInInspector, System.NonSerialized]
    public List<PhysicsObject> DynamicBodies = new();
    // Collision objects that dynamic bodies collide against
    [HideInInspector, System.NonSerialized]
    public List<PhysicsObject> StaticColliders = new();
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
        // objectType and isStatic are read here, so they must be set before the object registers
        if (obj.objectType == PhysicsObject.ObjectType.TriggerBox)
        {
            Triggers.Add(obj);
        }
        else if (obj.isStatic)
        {
            StaticColliders.Add(obj);
        }
        else
        {
            DynamicBodies.Add(obj);
        }
        return All.Count-1;
    }

    
    public void Reset()
    {
        All.Clear();
        Triggers.Clear();
        DynamicBodies.Clear();
        StaticColliders.Clear();
        Registered.Clear();
    }
}
