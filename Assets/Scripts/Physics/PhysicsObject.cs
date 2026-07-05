using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


[RequireComponent(typeof(DeterministicTransform)), ExecuteAlways]
public class PhysicsObject : MonoBehaviour
{
    [SerializeReference, SubclassSelector]
    public Shape shape;
    public int mask;
    public int layer;
    public bool isActive = true;
    public bool isStatic = false;
    public ObjectType objectType;
    /// <summary>
    /// Emitted when this object has been entered by another object
    /// </summary>
    public event Action<PhysicsObject, PhysicsObject> Overlapping;

    public enum ObjectType
    {
        TriggerBox,
        CollisionObject
    }
    /// <summary>
    /// Call this to emit the Overlapping Event- when this is the trigger and detects other.
    /// </summary>
    /// <param name="other"></param>
    public void OnOverlap(PhysicsObject other)
    {
        Overlapping?.Invoke(this, other);
    }

    public void Start()
    {
        if (physicsShapeRenderer.Instance != null)
        {
            PhysicsServer.Instance.RegisterObject(this);
        }
    }
}